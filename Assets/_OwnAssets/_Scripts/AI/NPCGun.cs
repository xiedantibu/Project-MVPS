﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SealTeam4
{
    public class NPCGun : MonoBehaviour
    {
        [SerializeField] private Transform firingPt;
        [SerializeField] private GameObject hitEffect_Prefab;

        [SerializeField] private MuzzleFlash muzzleFlashEffect;
        private NetworkAnimator gunNetworkAnim;
        private NetworkedAudioSource networkedAudioSource;

        private float timeToNextShot = 0;

        private float minVerticalDispersion = 0.15f;
        private float minHorizontalDispersion = 0.07f;

        private List<Vector3> hitPoints = new List<Vector3>();
        private void Start()
        {
            gunNetworkAnim = GetComponent<NetworkAnimator>();
            networkedAudioSource = GetComponent<NetworkedAudioSource>();
        }

        private void Update()
        {
            foreach (Vector3 point in hitPoints)
            {
                Debug.DrawLine(firingPt.position, point, Color.green);
            }

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    FireGun(GlobalEnums.GunAccuracy.HIGH);
            //}
        }

        public void FireGun(GlobalEnums.GunAccuracy accuracy)
        {
            float horizontalOffset = 0;
            float verticalOffset = 0;

            switch (accuracy)
            {
                case GlobalEnums.GunAccuracy.HIGH:
                    horizontalOffset = minHorizontalDispersion * 1f;
                    verticalOffset = minVerticalDispersion * 1f;
                    break;
                case GlobalEnums.GunAccuracy.MID:
                    horizontalOffset = minHorizontalDispersion * 1.3f;
                    verticalOffset = minVerticalDispersion * 1.3f;
                    break;
                case GlobalEnums.GunAccuracy.LOW:
                    horizontalOffset = minHorizontalDispersion * 1.6f;
                    verticalOffset = minVerticalDispersion * 1.6f;
                    break;
            }

            Vector3 offsetAmt =
                new Vector3(
                    Random.Range(-horizontalOffset, horizontalOffset),
                    Random.Range(-verticalOffset, verticalOffset),
                    0);


            int layerToHit = ~(1 << LayerMask.NameToLayer("UI"));

            Ray ray = new Ray(firingPt.position, firingPt.forward + offsetAmt);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerToHit))
            {
                IDamageable iDamagable = hitInfo.transform.root.GetComponent<IDamageable>();

                hitPoints.Add(hitInfo.point);

                if (iDamagable != null)
                {
                    iDamagable.OnHit(hitInfo.collider, GlobalEnums.WeaponType.PISTOL);
                }
                else
                {
                    // Spawn bullet hole
                    Transform bulletHole = Instantiate(hitEffect_Prefab, hitInfo.point, Quaternion.identity).GetComponent<Transform>();
                    
                    Destroy(bulletHole.gameObject, 120);
                }

                //Debug.Log(hitInfo.transform.name + " | " + hitInfo.transform.root.name);
                //Debug.Log("Bullet Offset " + offsetAmt);
                //Debug.Log("Bullet Hit");
            }


            // All the Effects are here. If anything breaks, comment it out.
            if (muzzleFlashEffect)
                muzzleFlashEffect.Activate();

            if (networkedAudioSource)
                networkedAudioSource.DirectPlay();

            if (gunNetworkAnim)
                gunNetworkAnim.SetTrigger("AI_Fire");
        }
    }
}
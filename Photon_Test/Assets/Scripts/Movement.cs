using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Movement : MonoBehaviour
{
    private CharacterController controller;
    private new Transform transform;
    private Animator animator;
    private new Camera camera;

    //가상의 Plane에 레이캐스팅하기 위한 변수
    private Plane plane;
    private Ray ray;
    private Vector3 hitPoint;

    //이동속도
    public float moveSpeed = 10.0f;
    private PhotonView pv;
    private CinemachineVirtualCamera virtualCamera;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        camera = Camera.main;

        pv = GetComponent<PhotonView>();
        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
        // 자신의 캐릭터일 경우 시네머신 카메라를 연결
        if(pv.IsMine){
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }

        // 가상의 바닥을 기준으로 주인공 위치를 생성
        plane = new Plane(transform.up,transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // 자신의 캐릭터(네트워크 객체)만 컨트롤
        if(pv.IsMine){
            Move();
            Turn();
        }
    }
    float h=>Input.GetAxis("Horizontal");
    float v=>Input.GetAxis("Vertical");

    private void Move(){
        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;

        //이동할 방향 백터 계산
        Vector3 moveDir = (cameraForward * v ) + (cameraRight * h);
        moveDir.Set(moveDir.x, 0.0f, moveDir.z);
        // 캐릭터 이동 처리
        controller.SimpleMove(moveDir * moveSpeed);

        // 캐릭터 애니메이션 처리
        float forward = Vector3.Dot(moveDir, transform.forward);
        float strafe = Vector3.Dot(moveDir, transform.right);

        animator.SetFloat("Forward", forward);
        animator.SetFloat("strafe", strafe);


    }
    // 회전 처리하는 함수
    private void Turn(){
        // 마우스의 2차원 좌푯값을 이용해 3차원 레이를 생서
        ray = camera.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        // 가상의 바닥에 ray를 발사해 충동 지점의 거리를 enter 변수로 반환
        plane.Raycast(ray, out enter);
        // 가상의 바닥에 레이가 충돌한 좌푯값을 추출
        hitPoint = ray.GetPoint(enter);

        // 회전해야 할 방향의 벡터를 계산
        Vector3 lookDir = hitPoint - transform.position;
        lookDir.y = 0.0f;
        // 캐릭터의 회전값 지정
        transform.localRotation = Quaternion.LookRotation(lookDir);


    }
}

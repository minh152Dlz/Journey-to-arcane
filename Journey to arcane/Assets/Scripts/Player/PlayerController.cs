using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump System")]
    
    [SerializeField] float jumpHeight;
    [SerializeField] float fallSpeed;
    [SerializeField] float jumpTime;
    [SerializeField] float jumpBuffer;
    Vector2 vecGravity;
  
    //public AudioSource jumpSound;
    //public AudioSource landingSound;
    private float move;
    public float maxSpeed;
    public float groundCheckRadius;
    bool facingRight;
    
    public Transform groundCheck;
    public LayerMask groundLayer;
    bool isJumping = false;
    float jumpCounter;

    //dash
    private bool isDashing;
    private float dashTimeLeft;
    private float lastImageXpos;
    private float lastDash = -100f;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCoolDown;

    [Header("Wall Jump System")]

    public Transform wallCheck;
    bool isSliding;
    public float wallSlidingSpeed;
    public float wallJumpDuration;
    public Vector2 wallJumpForce;
    bool wallJumping;

    private Rigidbody2D myBody;
    private Animator myAnim;
    
    // Start is called before the first frame update
    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        myBody = GetComponent<Rigidbody2D> ();
        myAnim = GetComponent<Animator> ();
        
        facingRight = true;
    
    }

    void Update()
    {
        CheckInput();
        CheckDash();
    }

    private void FixedUpdate()
    {
        Movement();
        if(move>0 && !facingRight){
            flip();
        }else if (move<0 && facingRight){
            flip();
        }

        if(myBody.velocity.x !=0)
            myAnim.SetBool("isMove", true);
        else
            myAnim.SetBool("isMove", false);

        Jump();
        WallJump();
        
        
    }

    private void CheckInput()
    {
        move = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Dash"))
        {
            if(Time.time >= (lastDash + dashCoolDown))
            AttempToDash();
        }
    }

    private void Movement()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            maxSpeed = 14;
            myAnim.SetBool("isRun", true);
        }
        else
        {
            maxSpeed = 8;
            myAnim.SetBool("isRun", false);
        }
        myBody.velocity = new Vector2(move*maxSpeed, myBody.velocity.y);

    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded() && !isJumping)
        {
            myBody.velocity = new Vector2(myBody.velocity.x, jumpHeight);
            isJumping = true;
            jumpCounter = 0;
        }else if (isSliding)
        {
            // wallJumping = true;
            // Invoke("StopWallJump", wallJumpDuration);
        }

        if(myBody.velocity.y>0 && isJumping)
        {
            jumpCounter += Time.deltaTime;
            if(jumpCounter > jumpTime)  isJumping = false;

            float t =  jumpCounter / jumpTime;
            float currentJumpB = jumpBuffer;
            if(t>0.5)
            {
                currentJumpB = jumpBuffer * (1-t);
            }

            myBody.velocity += vecGravity * currentJumpB * Time.deltaTime; 
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            jumpCounter =0;
            if(myBody.velocity.y >0)
            {
                myBody.velocity = new Vector2(myBody.velocity.x, myBody.velocity.y * 0.6f);
            }
        }

        if(myBody.velocity.y < 0)
        {
            myBody.velocity -= vecGravity * fallSpeed * Time.deltaTime;
        }
    }

    private void WallJump()
    {
        if(isWallTouch() && !isGrounded() && move != 0)
        {
            isSliding = true;
        }else
        {
            isSliding = false;
        }

        if(isSliding)
        {
            myBody.velocity = new Vector2(myBody.velocity.x, Mathf.Clamp(myBody.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }

        // if(wallJumping)
        // {
        //     myBody.velocity = new Vector2(-move * wallJumpForce.x, wallJumpForce.y);
        // }
        // else
        // {
        //     myBody.velocity = new Vector2(move*maxSpeed, myBody.velocity.y);
        // }
    }

    // private void StopWallJump()
    // {
    //     wallJumping = false;
    // }

    private void AttempToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    private void CheckDash()
    {
        if(isDashing)
        {
            if(dashTimeLeft>0)
            {
                myBody.velocity = new Vector2(dashSpeed, myBody.velocity.y);
                dashTimeLeft -= Time.deltaTime;

                if(Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }
            if(dashTimeLeft <= 0 || isWallTouch())
            {
                isDashing =false;
            }
        }
    }

    void flip(){
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale; 
    }

    bool isGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1.5f, 0.3f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }

    bool isWallTouch()
    {
        return Physics2D.OverlapBox(wallCheck.position, new Vector2(0.4f, 1.6f), 0, groundLayer);
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius)  ;
    }

    IEnumerator Co_CoyoteTimer()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        //grounded = false;
    }
}

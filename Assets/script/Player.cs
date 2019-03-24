using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float accele = 0.5f;

    [SerializeField]
    private float friction = 0.85f;

    [SerializeField]
    private float jumpPower = 8.0f;

    [SerializeField]
    private float gravity = 0.5f;

    [SerializeField]
    private Collider2D collision;

    [SerializeField]
    private Tilemap tilemap;

    private Vector2 speed;
    private Vector2 nextPos;

    private bool isGround;

    private const int CELL_SIZE = 16;

    void Start()
    {
        speed = new Vector2();
        isGround = false;
        nextPos = transform.position;
    }

    public void Update()
    {
        KeyInput();

        isGround = false;
        Vector3 position = transform.position;

        // speed -----

        speed.x *= friction;
        if (Mathf.Abs(speed.x) <= 0.1) speed.x = 0;

        speed.y -= gravity;

        // collision -----
        Vector3Int[] cellPositions = GetCollisionCellPositions();

        // y -----
        nextPos.y += speed.y;
        foreach (Vector3Int pos in cellPositions)
        {
            hitCheckY(pos);
        }
        position.y = nextPos.y;

        // x -----
        nextPos.x += speed.x;
        foreach (Vector3Int pos in cellPositions)
        {
            hitCheckX(pos);
        }
        position.x = nextPos.x;

        transform.position = position;
    }

    private void KeyInput()
    {
        if (Input.GetKey(KeyCode.A))
        {
            speed.x -= accele;
        }
        if (Input.GetKey(KeyCode.D))
        {
            speed.x += accele;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGround)
            {
                isGround = false;
                speed.y += jumpPower;
            }
        }
    }

    private Vector3Int[] GetCollisionCellPositions()
    {
        Bounds bns = collision.bounds;
        int cmaxX = (int)Mathf.Ceil(bns.max.x / CELL_SIZE);
        int cmaxY = (int)Mathf.Ceil(bns.max.y / CELL_SIZE);
        int cminX = (int)Mathf.Floor(bns.min.x / CELL_SIZE);
        int cminY = (int)Mathf.Floor(bns.min.y / CELL_SIZE);

        Vector3Int max = new Vector3Int(cmaxX + 1, cmaxY + 1, 0);
        Vector3Int min = new Vector3Int(cminX - 1, cminY - 1, 0);

        List<Vector3Int> list = new List<Vector3Int>();

        for (int cy = min.y; cy <= max.y; cy++)
        {
            for (int cx = min.x; cx <= max.x; cx++)
            {
                Vector3Int pos = new Vector3Int(cx, cy, 0);
                Tile.ColliderType type = tilemap.GetColliderType(pos);
                if (type == Tile.ColliderType.Sprite)
                {
                    list.Add(pos);
                }
            }
        }

        return list.ToArray();
    }

    private void hitCheckY(Vector3Int pos)
    {
        Bounds bns1 = collision.bounds;
        bns1.center = nextPos;
        Bounds bns2 = tilemap.GetSprite(pos).bounds;
        bns2.center = tilemap.GetCellCenterWorld(pos);

        bool touchX = bns1.max.x - 0 > bns2.min.x && bns1.min.x + 0 < bns2.max.x;
        bool touchY = bns1.max.y - 0 > bns2.min.y && bns1.min.y + 0 < bns2.max.y;

        if (touchY && touchX)
        {
            nextPos.y = Mathf.Sign(-speed.y) * (bns1.extents.y + bns2.extents.y) + bns2.center.y;

            if (touchY)
            {
                if (speed.y < 0)
                {
                    isGround = true;
                }
                speed.y = 0;
            }
        }
    }

    private void hitCheckX(Vector3Int pos)
    {
        Bounds bns1 = collision.bounds;
        bns1.center = nextPos;
        Bounds bns2 = tilemap.GetSprite(pos).bounds;
        bns2.center = tilemap.GetCellCenterWorld(pos);

        bool touchX = bns1.max.x - 0 > bns2.min.x && bns1.min.x + 0 < bns2.max.x;
        bool touchY = bns1.max.y - 0 > bns2.min.y && bns1.min.y + 0 < bns2.max.y;

        if (touchY && touchX)
        {
            nextPos.x = Mathf.Sign(-speed.x) * (bns1.extents.x + bns2.extents.x) + bns2.center.x;
        }
    }
}
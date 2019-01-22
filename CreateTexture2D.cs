using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 创建模型贴图修改模型形状
/// </summary>
public class CreateTexture2D : MonoBehaviour
{
   
    //用于shader 的贴图
    public Texture2D texture;

    int imgWeight = 128;
    int imgHeight = 128;

    IEnumerator Start()
    {
        texture = new Texture2D(imgWeight, imgHeight);
        for (int i = 0; i < imgWeight; i++)
        {
            yield return null;
            for (int j = 0; j < imgHeight; j++)
            {
                texture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }

        texture.Apply();
        yield return new WaitForEndOfFrame();
        GetComponent<Renderer>().materials[1].SetTexture("_lineTex", texture);

    }
    [HideInInspector]
    public Vector2 meshBegin = new Vector2(-5.0195f, 1.4725f);
    [HideInInspector]
    public Vector2 meshEnd =  new Vector2(-5.4472f, 1.1917f);
    /// <summary>
    /// 线条偏移量
    /// </summary>
    int offset = 0;
    /// <summary>
    /// 线条宽度
    /// </summary>
    int width = 10;


    List<Vector2> indexList = new List<Vector2>();
    /// <summary>
    /// 在模型上增高对应传入线段的模型部位
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public IEnumerator AddHitPoint(Vector2 startPos, Vector2 endPos, float time)
    {
        //print("timeStart: " + Time.time);
        Vector2 startIndex = new Vector2((int)(imgWeight * (startPos.x - meshBegin.x) / (meshEnd.x - meshBegin.x)), (int)(imgHeight * (startPos.y - meshBegin.y) / (meshEnd.y - meshBegin.y)));
        Vector2 endIndex = new Vector2((int)(imgWeight * (endPos.x - meshBegin.x) / (meshEnd.x - meshBegin.x)), (int)(imgHeight * (endPos.y - meshBegin.y) / (meshEnd.y - meshBegin.y)));
        print(startIndex+" , "+endIndex);
        Vector2 oldstart = startIndex;
        Vector2 oldend = endIndex;


        //起始点邻近的话，往后退一点
        for (int i = 0; i < indexList.Count; i++)
        {
            if (Vector2.Distance(indexList[i], startIndex) < width / 2)
            {
                startIndex += (endIndex - startIndex).normalized * width;
            }
   
        }

        indexList.Add(oldstart);
        indexList.Add(oldend);

        yield return null;
        int newI;
        int newJ;


        //循环起点值
        if (startIndex.x <= endIndex.x)
        {
            newI = (int)startIndex.x - width;
        }
        else
        {
            newI = (int)startIndex.x + width;
        }

        if (startIndex.y <= endIndex.y)
        {
            newJ = (int)startIndex.y - width;
        }
        else
        {
            newJ = (int)startIndex.y + width;
        }

        //起点到终点，在X轴上移动更多还是Y轴
        bool isLongerInX = Mathf.Abs(startIndex.x - endIndex.x) > Mathf.Abs(startIndex.y - endIndex.y);

        if (isLongerInX) //X轴上移动
        {
            int count = 0;
            float repeatTime = 2 * time / ((int)Mathf.Abs(endIndex.x - startIndex.x) + 1 + width * 2);
            float singleSpeed = 0.2f + 0.2f * (Mathf.Clamp01((repeatTime * 100 - 8) / 20));
            for (int i = newI; ;)
            {
                count++;

                for (int j = newJ; ;)
                {
                    float dis = pointToLine(new Vector2(i, j), startIndex, endIndex);
                    dis = SectionType(dis, singleSpeed);
                    texture.SetPixel(i, j, texture.GetPixel(i, j) + new Color(dis, 0, 0, 1));

                    if (startIndex.y <= endIndex.y)
                    {
                        j++;

                        if (j > endIndex.y + width)
                        {
                            break;
                        }
                    }
                    else
                    {
                        j--;

                        if (j < endIndex.y - width)
                        {
                            break;
                        }
                    }
                }
                texture.Apply();

                GetComponent<Renderer>().materials[1].SetTexture("_lineTex", texture);

                if (count % 2 == 0)
                {
                    yield return new WaitForSecondsRealtime(repeatTime);
                }


                if (startIndex.x <= endIndex.x)
                {
                    i++;
                    if (i > endIndex.x + width)
                    {
                        break;
                    }
                }
                else
                {
                    i--;
                    if (i < endIndex.x - width)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            int count = 0;
            float repeatTime = 2 * time / ((int)Mathf.Abs(endIndex.y - startIndex.y) + 1 + width * 2);
            float singleSpeed = 0.2f + 0.2f * (Mathf.Clamp01((repeatTime * 100 - 8) / 20));
            for (int j = newJ; ;)
            {
                count++;
                for (int i = newI; ;)
                {
                    float dis = pointToLine(new Vector2(i, j), startIndex, endIndex);
                    dis = SectionType(dis, singleSpeed);
                    texture.SetPixel(i, j, texture.GetPixel(i, j) + new Color(dis, 0, 0, 1));

                    if (startIndex.x <= endIndex.x)
                    {
                        i++;
                        if (i > endIndex.x + width)
                        {
                            break;
                        }
                    }
                    else
                    {
                        i--;
                        if (i < endIndex.x - width)
                        {
                            break;
                        }
                    }

                }
                texture.Apply();
                GetComponent<Renderer>().materials[1].SetTexture("_lineTex", texture);


                if (count % 2 == 0)
                {
                    yield return new WaitForSecondsRealtime(repeatTime);
                }

                if (startIndex.y <= endIndex.y)
                {
                    j++;

                    if (j > endIndex.y + width)
                    {
                        break;
                    }
                }
                else
                {
                    j--;

                    if (j < endIndex.y - width)
                    {
                        break;
                    }
                }
            }
            
        }
        print(startIndex + " , " + endIndex);
        //Debug.Log("timeEnd: " + Time.time);
        //Debug.Log("生成新线条");
    }


    /// <summary>
    /// 截面形状
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="v">每像素耗时*100</param>
    /// <returns></returns>
    float SectionType(float distance, float v)
    {
        float x = Mathf.Clamp01(distance / width);//0-1 距离越近越靠近0

        return (1 - Mathf.Pow(x, 2)) * v;//每次加深(0.2f-0.4f),取值根据每像素耗时*100(v)，（8-28秒）,8秒的时候加深0.2,28秒以上加深0.5f,
    }




    /// <summary>
    /// 点到直线距离
    /// </summary>
    /// <param name="point">点坐标</param>
    /// <param name="linePoint1">直线上一个点的坐标</param>
    /// <param name="linePoint2">直线上另一个点的坐标</param>
    /// <returns></returns>
    private static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
    {
        Vector3 vec1 = point - linePoint1;
        Vector3 vec2 = linePoint2 - linePoint1;
        Vector3 vecProj = Vector3.Project(vec1, vec2);
        float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
        return dis;
    }
    /// <summary>
    /// 点到线段的距离
    /// </summary>
    /// <param name="position"></param>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    private float pointToLine(Vector2 position, Vector2 point1, Vector2 point2)//point1和point2为线的两个端点
    {
        float space = 0;
        float a, b, c;
        a = Vector2.Distance(point1, point2);// 线段的长度      
        b = Vector2.Distance(point1, position);// position到点point1的距离      
        c = Vector2.Distance(point2, position);// position到point2点的距离 
        if (c <= 0.000001 || b <= 0.000001)
        {
            space = 0;
            return space;
        }
        if (a <= 0.000001)
        {
            space = b;
            return space;
        }
        if (c * c >= a * a + b * b)
        {
            space = b;
            return space;
        }
        if (b * b >= a * a + c * c)
        {
            space = c;
            return space;
        }
        float p = (a + b + c) / 2;// 半周长      
        float s = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));// 海伦公式求面积      
        space = 2 * s / a;// 返回点到线的距离（利用三角形面积公式求高）      
        return space;
    }


}

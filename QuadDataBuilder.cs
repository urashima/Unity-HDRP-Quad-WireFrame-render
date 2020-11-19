// Based on: https://catlikecoding.com/unity/tutorials/advanced-rendering/flat-and-wireframe-shading/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(MeshFilter))]
public class QuadDataBuilder : MonoBehaviour
{
    private void Reset()
    {
        GenerateMeshData();
    }

    /// <summary>
    /// We will assign a color to each Vertex in a Triangle on the object's mesh
    /// </summary>
    void GenerateMeshData()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        SplitMesh(mesh);
        SetVertexColors(mesh);
    }

    /// <summary>
    /// For this approach, we need to make sure there are not shared vertices
    /// on the mesh, that's why we use this method to split the mesh. 
    /// This will increase the number of vertices, so less optimized.
    /// </summary>
    /// <param name="mesh"></param>
    void SplitMesh(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] Verts;
        int n = triangles.Length;
        Verts = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            Verts[i] = verts[triangles[i]];
            triangles[i] = i;
        }

        mesh.vertices = Verts;
        mesh.triangles = triangles;
    }

    /// <summary>
    /// We paint the vertex color
    /// </summary>
    /// <param name="mesh"></param>
    void SetVertexColors(Mesh mesh)
    {
        int dataCount = mesh.triangles.Length;
        Vector3[] Vertices = new Vector3[dataCount];
        List<int[]> Triangles = new List<int[]>();

        Vector3[] mVertices = mesh.vertices;
        Color32[] vertexColors = new Color32[mesh.vertices.Length];

        int tIndex = 0;
        int dataIndex = -1;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int vCount = mesh.GetTriangles(i).Length;
            int[] T = new int[vCount];
            int[] mT = mesh.GetTriangles(i);

            for (int j = 0; j < vCount; j++)
            {
                int index = mT[j];

                //Vertices
                Vertices[++dataIndex] = mVertices[index];

                //Triangles
                T[j] = tIndex++;
            }

            Triangles.Add(T);
        }

        //Generate Quad Data
        for (int i = 0; i < Triangles.Count; i++)
        {
            for (int j = 0; j < Triangles[i].Length; j += 3)
            {
                Vector3 v1, v2, v3;

                float d1 = Vector3.Distance(Vertices[Triangles[i][j]], Vertices[Triangles[i][j + 1]]);
                float d2 = Vector3.Distance(Vertices[Triangles[i][j + 1]], Vertices[Triangles[i][j + 2]]);
                float d3 = Vector3.Distance(Vertices[Triangles[i][j + 2]], Vertices[Triangles[i][j]]);

                Vector3 offset = Vector3.zero;
                if (d1 > d2 && d1 > d3)
                    offset.y = 1;
                else if (d2 > d3 && d2 > d1)
                    offset.x = 1;
                else
                    offset.z = 1;

                v1 = new Vector3(1, 0, 0) + offset;
                v2 = new Vector3(0, 0, 1) + offset;
                v3 = new Vector3(0, 1, 0) + offset;

                Color[] colorCoords = new[]
                {
                         new Color(v1.x,v1.y,v1.z),
                         new Color(v2.x,v2.y,v2.z),
                         new Color(v3.x,v3.y,v3.z),
                 };

                vertexColors[i * 3 + j    ] = colorCoords[0];
                vertexColors[i * 3 + j + 1] = colorCoords[1];
                vertexColors[i * 3 + j + 2] = colorCoords[2];
            }
        };

        mesh.colors32 = vertexColors;
    }

}

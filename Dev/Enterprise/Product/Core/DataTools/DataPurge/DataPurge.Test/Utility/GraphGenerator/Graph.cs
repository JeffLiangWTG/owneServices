using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.DataPurge.Utility
{
	public class Graph
	{
		public Graph(SqlUtility sqlUtility, ZGuid companyId)
		{
			this.sqlUtility = sqlUtility;
			this.companyId = companyId;
		}

		public IEnumerable<string> GenerateSqlRowsByDFS(Node root)
		{
			var result = new List<string>();
			var stack = new Stack<Node>();
			var isVisited = new HashSet<Node>();
			stack.Push(root);
			isVisited.Add(root);
			while (stack.Any())
			{
				Node next = null;
				var currentNode = stack.Peek();
				LinkedList<Node> adjacency = null;
				graph.TryGetValue(currentNode, out adjacency);
				if (adjacency != null)
				{
					foreach (var node in adjacency)
					{
						if (!isVisited.Contains(node))
						{
							next = node;
							break;
						}
					}
				}
				else
				{
					var nodes = new List<Node>(stack);
					nodes.Reverse();
					var nodePathSql = sqlUtility.ConvertNodePathToSql(nodes, companyId);
					result.AddRange(nodePathSql);
				}

				if (next != null)
				{
					stack.Push(next);
					isVisited.Add(next);
				}
				else
				{
					stack.Pop();
				}
			}

			return result;
		}

		public void AddNodeRecursively(Node currentNode)
		{
			var fkInfos = sqlUtility.GetForeignKeysReferToTable(currentNode.PKTableName);
			foreach (var fkInfo in fkInfos)
			{
				var pkName = sqlUtility.GetPrimaryKeyNameByTableName(fkInfo.FKTableName);
				var targetNode = new Node(fkInfo.FKTableName, currentNode.PKTableName, pkName, fkInfo.FKName);
				AddEdge(currentNode, targetNode);
				if (fkInfo.FKTableName != currentNode.PKTableName)
				{
					AddNodeRecursively(targetNode);
				}
			}
		}

		void AddEdge(Node fromNode, Node toNode)
		{
			if (!graph.ContainsKey(fromNode))
			{
				graph.Add(fromNode, new LinkedList<Node>());
			}

			graph[fromNode].AddFirst(toNode);
		}

#if DEBUG
		public Dictionary<Node, LinkedList<Node>> GetGraphNodes_ForTestOnly() => graph;
#endif
		readonly Dictionary<Node, LinkedList<Node>> graph = new Dictionary<Node, LinkedList<Node>>();
		readonly SqlUtility sqlUtility;
		readonly ZGuid companyId;
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public class RecursiveTreeViewSearcher : ITreeViewSearcher
	{
		public RecursiveTreeViewSearcher()
		{
		}

		public ISearchResults Search(TreeView treeView, string criteria, StringComparison comparison)
		{
			var flattenedNodes = IterateForwardBreadthFirst(treeView);
			var cache = new CachedEnumerableWrapper<TreeNode>(flattenedNodes);

			return new RecursiveSearchResults(treeView, cache, criteria, comparison);
		}

		static IEnumerable<TreeNode> IterateForwardBreadthFirst(TreeView treeView)
		{
			var expandedNodes = new HashSet<string>();
			var uncheckedNodes = new Queue<TreeNode>();
			EnqueueAll(treeView.Nodes, uncheckedNodes);

			var expandingNodeTree = treeView as ITreeViewWithExpandingNodes;
			while (uncheckedNodes.Count > 0)
			{
				var node = uncheckedNodes.Dequeue();
				yield return node;

				if (expandedNodes.Add(node.Text))
				{
					expandingNodeTree?.ExpandNodeIfNeeded(node);
					EnqueueAll(node.Nodes, uncheckedNodes);
				}
			}
		}

		static void EnqueueAll(TreeNodeCollection nodes, Queue<TreeNode> queue)
		{
			foreach (TreeNode node in nodes)
			{
				queue.Enqueue(node);
			}
		}
	}
}
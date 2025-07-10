using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

namespace Enterprise.Registry.GUI
{
	internal class OverridesLevelSearchResults : NonRecursiveSearchResults
	{
		public OverridesLevelSearchResults(TreeView treeView, List<TreeViewPath> matches, int offset)
			: this(offset)
		{
			Total = matches.Count;
			this.matches = matches;
			this.treeView = treeView;
		}

		readonly TreeView treeView;
		readonly List<TreeViewPath> matches;

		OverridesLevelSearchResults(int offset) : base(offset)
		{
		}

		protected override TreeNode GetTreeNode(int index)
		{
			TreeNode result = null;

			var path = matches[index];
			var nodePaths = path.GetFullPaths().ToList();

			var treeNodeCollection = treeView.Nodes;

			for (var i = 0; i < nodePaths.Count; i++)
			{
				var nodePath = nodePaths[i];
				if (Find(treeNodeCollection, nodePath) is TreeNode treeNode)
				{
					if (i == nodePaths.Count - 1)
					{
						result = treeNode;
					}
					else
					{
						treeNode.Expand();
						treeNodeCollection = treeNode.Nodes;
					}
				}
			}

			return result;
		}

		TreeNode Find(TreeNodeCollection nodes, TreeViewPath path)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Text == path.Content && node.Index == path.IndexOfParent)
				{
					return node;
				}
			}

			return null;
		}
	}
}

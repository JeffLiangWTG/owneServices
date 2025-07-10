using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public sealed class LazyTreeViewSearcher : ITreeViewSearcher
	{
		public LazyTreeViewSearcher()
		{
			predicate = (node, criteria, comparison) => node.Text.IndexOf(criteria, comparison) >= 0;
		}

		public LazyTreeViewSearcher(Func<TreeNode, string, StringComparison, bool> predicate)
		{
			this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public ISearchResults Search(TreeView tree, string criteria, StringComparison comparison)
		{
			if (tree.Nodes.Count == 0)
			{
				return EmptySearchResults.Instance;
			}

			var startNode = tree.SelectedNode ?? tree.Nodes[0];
			return new DepthFirstSearchResults(tree, startNode, node => predicate(node, criteria, comparison));
		}

		readonly Func<TreeNode, string, StringComparison, bool> predicate;

		sealed class DepthFirstSearchResults : ISearchResults
		{
			public DepthFirstSearchResults(TreeView tree, TreeNode startNode, Func<TreeNode, bool> predicate)
			{
				this.tree = tree;
				this.startNode = startNode;
				this.predicate = predicate;
			}

			public int Total => -1;

			public TreeNode Next() => MoveCurrent(true);

			public TreeNode Previous() => MoveCurrent(false);

			TreeNode MoveCurrent(bool next)
			{
				tree.BeginUpdate();

				do
				{
					if (current == null)
					{
						current = startNode;
					}
					else
					{
						current = next ? GetNextOrWrapAround() : GetPreviousOrWrapAround();
						if (current == startNode)
						{
							current = null;
						}
					}
				}
				while (current != null && !predicate(current));

				tree.EndUpdate();

				return current;
			}

			TreeNode GetNextOrWrapAround()
			{
				ExpandNodeIfNeeded(current);
				if (current.Nodes.Count > 0)
				{
					return current.Nodes[0];
				}

				for (var node = current; node != null; node = node.Parent)
				{
					var collection = node.Parent?.Nodes ?? tree.Nodes;
					if (collection.Count > node.Index + 1)
					{
						return collection[node.Index + 1];
					}
				}

				return tree.Nodes[0];
			}

			TreeNode GetPreviousOrWrapAround()
			{
				if (current.Index > 0)
				{
					var collection = current.Parent?.Nodes ?? tree.Nodes;
					return DeepestLastChild(collection[current.Index - 1]);
				}

				return current.Parent ?? DeepestLastChild(tree.Nodes[tree.Nodes.Count - 1]);

				TreeNode DeepestLastChild(TreeNode node)
				{
					ExpandNodeIfNeeded(node);
					while (node.Nodes.Count > 0)
					{
						node = node.Nodes[node.Nodes.Count - 1];
					}

					return node;
				}
			}

			void ExpandNodeIfNeeded(TreeNode node)
			{
				if (tree is ITreeViewWithExpandingNodes treeViewWithExpandingNodes)
				{
					treeViewWithExpandingNodes.ExpandNodeIfNeeded(node);
				}
			}

			TreeNode current;
			readonly TreeView tree;
			readonly TreeNode startNode;
			readonly Func<TreeNode, bool> predicate;
		}
	}
}

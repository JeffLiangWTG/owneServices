using System;
using System.Collections.Generic;
using System.Windows.Forms;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public class NonRecursiveTreeViewSearcher : ITreeViewSearcher
	{
		List<TreeNode> treeViewFlattened;

		public event EventHandler<TreeViewEventArgs> BeforeIteratingChildren;
		public bool Cont;

		public NonRecursiveTreeViewSearcher()
		{
		}

		public ISearchResults Search(TreeView treeView, string criteria, StringComparison comparison) => SearchCore(treeView, criteria, comparison);

		protected virtual ISearchResults SearchCore(TreeView treeView, string criteria, StringComparison comparison)
		{
			var matches = new List<TreeNode>();

			if (treeViewFlattened == null)
			{
				FlattenTreeDepthFirst(treeView);
			}

			var selectedIndex = treeViewFlattened.IndexOf(treeView.SelectedNode);
			var index = 0;
			var offset = 0;
			foreach (var node in treeViewFlattened)
			{
				if (node.Text.Contains(criteria, comparison))
				{
					matches.Add(node);

					if (index >= selectedIndex)
					{
						offset = Cont ? matches.Count - 1 : 0;
						selectedIndex = Int32.MaxValue;
					}
				}

				index++;
			}

			return new NonRecursiveSearchResults(matches, offset);
		}

		void FlattenTreeDepthFirst(TreeView treeView)
		{
			treeViewFlattened = new List<TreeNode>();

			foreach (TreeNode node in treeView.Nodes)
			{
				AddChildNodesToList(node);
			}

			void AddChildNodesToList(TreeNode treeNode)
			{
				treeViewFlattened.Add(treeNode);
				BeforeIteratingChildren?.Invoke(this, new TreeViewEventArgs(treeNode));

				foreach (TreeNode node in treeNode.Nodes)
				{
					AddChildNodesToList(node);
				}
			}
		}
	}
}

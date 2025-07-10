using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	class RecursiveSearchResults : ISearchResults
	{
		public int Total => -1;

		readonly TreeView treeView;
		readonly CachedEnumerableWrapper<TreeNode> nodes;
		readonly string criteria;
		readonly StringComparison comparison;

		int lastMatchIndex;

		public RecursiveSearchResults(TreeView treeView, CachedEnumerableWrapper<TreeNode> nodes, string criteria, StringComparison comparison)
		{
			this.treeView = treeView;
			this.nodes = nodes;
			this.criteria = criteria;
			this.comparison = comparison;
		}

		bool MatchesCriteria(TreeNode node)
			=> node.Text.IndexOf(criteria, comparison) >= 0;

		public TreeNode Previous()
		{
			treeView.BeginUpdate();
			try
			{
				nodes.Flush();

				for (var i = lastMatchIndex - 1; i != lastMatchIndex; i--)
				{
					try
					{
						if (MatchesCriteria(nodes[i]))
						{
							lastMatchIndex = i;
							return nodes[lastMatchIndex];
						}
					}
					catch (ArgumentOutOfRangeException)
					{
						i = nodes.CacheCount;
					}
				}

				return null;
			}
			finally
			{
				treeView.EndUpdate();
			}
		}

		public TreeNode Next()
		{
			treeView.BeginUpdate();
			try
			{
				for (var i = lastMatchIndex + 1; i != lastMatchIndex; i++)
				{
					try
					{
						if (MatchesCriteria(nodes[i]))
						{
							lastMatchIndex = i;
							return nodes[lastMatchIndex];
						}
					}
					catch (ArgumentOutOfRangeException)
					{
						i = -1;
					}
				}

				return null;
			}
			finally
			{
				treeView.EndUpdate();
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.Registry.GUI
{
	class RegistryTreeNodeComparer : IComparer, IComparer<TreeNode>
	{
		public RegistryTreeNodeComparer()
		{
		}

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return Compare((TreeNode)x, (TreeNode)y);
		}

		#endregion

		#region IComparer<TreeNode> Members

		public int Compare(TreeNode x, TreeNode y)
		{
			int result;
			bool xHasNodes = (x.Nodes.Count > 0);
			bool yHasNodes = (y.Nodes.Count > 0);

			if (xHasNodes && !yHasNodes)
			{
				result = 1;
			}
			else if (!xHasNodes && yHasNodes)
			{
				result = -1;
			}
			else
			{
				result = string.Compare(x.Text, y.Text);
			}

			return result;
		}

		#endregion
	}
}

using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public class NonRecursiveSearchResults : ISearchResults
	{
		List<TreeNode> nodes { get; set; }
		public int Index { get; private set; }
		public int Total { get; set; }
		public int Offset { get; set; }

		public NonRecursiveSearchResults(int offset)
		{
			Index = -1;
			Offset = offset;
		}

		public NonRecursiveSearchResults(List<TreeNode> nodes, int offset)
			: this(offset)
		{
			Total = nodes.Count;
			this.nodes = nodes;
		}

		public TreeNode Previous()
		{
			Index = (Index - 1 < 0) ? Total - 1 : Index - 1;

			return GetTreeNode(GetOffsetIndex());
		}

		public TreeNode Next()
		{
			Index = (Index + 1 >= Total) ? 0 : Index + 1;

			return GetTreeNode(GetOffsetIndex());
		}

		public int GetOffsetIndex()
		{
			return ((Index + Offset % Total + Total) % Total);
		}

		protected virtual TreeNode GetTreeNode(int index) => nodes[index];
	}
}

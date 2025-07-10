using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Registry.GUI
{
	internal class TreeViewPath
	{
		public TreeViewPath(string path, TreeViewPath parent = null)
		{
			Content = path;
			Parent = parent;
			AddToParent();
		}

		public TreeViewPath Parent { get; private set; }

		public string Content { get; }

		public int IndexOfParent { get; private set; }

		public IEnumerable<TreeViewPath> GetFullPaths()
		{
			var result = new List<TreeViewPath>();
			var parentPath = this;
			while (!string.IsNullOrEmpty(parentPath?.Content))
			{
				result.Add(parentPath);
				parentPath = parentPath.Parent;
			}

			if (result.Count > 1)
			{
				result.Reverse();
			}

			return result;
		}

		public IEnumerable<TreeViewPath> Children => children;

		void AddToParent()
		{
			IndexOfParent = Parent?.Children.Count() ?? -1;
			Parent?.children.Add(this);
		}

		readonly IList<TreeViewPath> children = new List<TreeViewPath>();
	}
}

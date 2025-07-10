using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	class MenuTraverser
	{
		public IEnumerable<MenuItem> GetPath(MenuItem menuItem)
		{
			var stack = new Stack<MenuItem>();
			var parent = menuItem;
			while (parent != null)
			{
				stack.Push(parent);
				parent = parent.Parent as MenuItem;
			}
			return stack;
		}
	}
}

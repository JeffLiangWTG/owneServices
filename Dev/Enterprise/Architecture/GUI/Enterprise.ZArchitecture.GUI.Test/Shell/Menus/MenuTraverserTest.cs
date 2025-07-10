using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MenuTraverserTest : TestCase
	{
		public void TestTraverseSingleLayer()
		{
			var menu = new ZMainMenu();
			var menuItem = menu.MenuItems.Add("Menu1");
			var path = new MenuTraverser().GetPath(menuItem).ToArray();
			AssertEquals(1, path.Length);
			AssertEquals(menuItem, path[0]);
		}

		public void TestTraverseMultiLayer()
		{
			var menu = new ZMainMenu();
			var menuItem = menu.MenuItems.Add("Menu1");
			menu.MenuItems.Add("Menu2");
			menu.MenuItems.Add("Menu3");
			menuItem.MenuItems.Add("SubMenu1");
			menuItem.MenuItems.Add("SubMenu2");
			var sub3 = menuItem.MenuItems.Add("SubMenu3");
			sub3.MenuItems.Add("Hello");
			var subGoodbye = sub3.MenuItems.Add("Goodbye");
			var path = new MenuTraverser().GetPath(subGoodbye).ToArray();
			AssertEquals(3, path.Length);
			AssertEquals(menuItem, path[0]);
			AssertEquals(sub3, path[1]);
			AssertEquals(subGoodbye, path[2]);
		}
	}
}

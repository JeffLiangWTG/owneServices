using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class MenuItemCollectionExtensionsTest : TestCase
	{
		public void TestFindByText()
		{
			MenuItem menu = new ZMenuItem();
			var item1 = menu.MenuItems.Add("Item1");
			var item2 = menu.MenuItems.Add("Ite&m2");
			AssertEquals(item1, menu.MenuItems.FindByText("Item1"));
			AssertEquals(item2, menu.MenuItems.FindByText("Item2"));
			AssertEquals(null, menu.MenuItems.FindByText("NoMatch"));
		}
	}
}

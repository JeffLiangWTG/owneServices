using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class MenuItemCollectionExtensionsTest : TestCaseWithFactory
	{
		public void TestAddIfNotNull()
		{
			var mainMenuItem = new ZMenuItem((NoResString)("Test Main Menu Item"));
			AssertEquals("Pre-Condition", mainMenuItem.MenuItems.Count, 0);

			mainMenuItem.MenuItems.AddIfNotNull(null);
			AssertEquals("Add null menu item", mainMenuItem.MenuItems.Count, 0);

			mainMenuItem.MenuItems.AddIfNotNull(new ZMenuItem((NoResString)"Test Sub Menu Item"));
			AssertEquals("Add non-null menu item", mainMenuItem.MenuItems.Count, 1);
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	sealed class MapTreeMenuManagerTest : TestCaseWithFactory
	{
		public void TestMenuItemsGetAdded()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var menuManager = new MapTreeMenuManager(orgHeader.DocumentSupporter);
			var addedMenuItem = menuManager.TopLevelMenuItem;
			AssertMultilineASCIIEquals("addedMenuItem.MenuItems", "", SerialiseMenuItems(addedMenuItem.MenuItems));
			addedMenuItem.PerformSelect();
			AssertMultilineASCIIEquals("addedMenuItem.MenuItems", @"GenericFreightJob
.OrgHeader
".Trim(), SerialiseMenuItems(addedMenuItem.MenuItems));
		}

		string SerialiseMenuItems(Menu.MenuItemCollection menuItems)
		{
			var results = new ZStringBuilder();

			foreach (MenuItem menuItem in menuItems)
			{
				results.Append(menuItem.Text);
			}

			return results.ToStringWithNewLineBetweenAppends();
		}
	}
}

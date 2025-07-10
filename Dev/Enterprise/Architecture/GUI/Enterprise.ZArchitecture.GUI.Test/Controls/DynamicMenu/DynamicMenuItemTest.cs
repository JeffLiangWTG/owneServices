using System;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DynamicMenuItemTest : TestCase
	{
		public void TestIDynamicMenu()
		{
			using (var menuItem = new DynamicMenuItem())
			{
				var iDynamicMenuItem = (IDynamicMenu)menuItem;

				AssertNotNull("Precondition", menuItem.MenuItems);
				AssertEquals("iDynamicMenuItem.MenuItems", menuItem.MenuItems, iDynamicMenuItem.MenuItems);

				menuItem.MenuItems.Add("Test");

				var testString = "NoChange";
				iDynamicMenuItem.Opening += delegate
				{ testString = "Changed"; };

				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals("Adding delegate through interface should work", "Changed", testString);
			}
		}
	}
}

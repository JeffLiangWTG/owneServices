using System;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DynamicToolStripMenuItemTest : TestCase
	{
		public void TestIMenuItem()
		{
			using (var menuItem = new DynamicToolStripMenuItem())
			{
				var iDynamicMenu = (IDynamicMenu)menuItem;

				AssertNotNull("Precondition", menuItem.DropDownItems);
				AssertEquals("iDynamicMenu.MenuItems", menuItem.DropDownItems, iDynamicMenu.MenuItems);

				menuItem.DropDownItems.Add("Test");

				var testString = "NoChange";
				iDynamicMenu.Opening += delegate
				{ testString = "Changed"; };

				menuItem.OnOpening(EventArgs.Empty);

				AssertEquals("Adding delegate through interface should work", "Changed", testString);
			}
		}
	}
}

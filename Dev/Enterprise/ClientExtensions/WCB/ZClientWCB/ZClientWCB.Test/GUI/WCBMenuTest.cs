using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.GUI;

namespace Enterprise.Client.WCB.GUI.Testing
{
	internal class WCBMenuTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			using (EDIMenu menu = WCBMenu.New())
			{
				AssertEquals("Returns WCBMenu", typeof(WCBMenu), menu.GetType());
			}
		}

		public void TestInitialise()
		{
			Assert("Constructor initilised, should not be null", !(WCBMenu.IsOverridableNewDelegateNull));
		}

		public void TestSetupTopLevelMenu()
		{
			using (WCBMenu menu = (WCBMenu)WCBMenu.New())
			{
				int menuItemCount = menu.dataMenuItemInternal.MenuItems.Count;
				menu.SetupTopLevelMenuInternal();
				AssertEquals("Menu was added, Menu Item Count should be one extra", menuItemCount + 1, menu.dataMenuItemInternal.MenuItems.Count);
				bool isFound = false;
				foreach (MenuItem currentMenuItem in menu.dataMenuItemInternal.MenuItems)
				{
					if (currentMenuItem.Text == WCBMenu.ImportWCBInvoiceFormatText)
					{
						isFound = true;
					}
				}

				Assert("Should find WCB import menu item", isFound);
			}
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.GUI;

namespace Enterprise.Client.AUS.GUI
{
	public class CommercialInvoiceMenuTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			using (EDIMenu menu = CommercialInvoiceMenu.New())
			{
				AssertEquals("Returns AUS Commercial Invoice Menu", typeof(CommercialInvoiceMenu), menu.GetType());
			}
		}

		public void TestInitialise()
		{
			Assert("Constructor initilised, should not be null", !CommercialInvoiceMenu.IsOverridableNewDelegateNull);
		}

		public void TestSetupTopLevelMenu()
		{
			using (CommercialInvoiceMenu menu = (CommercialInvoiceMenu)CommercialInvoiceMenu.New())
			{
				int menuItemCount = menu.InternalDataMenuItem.MenuItems.Count;
				menu.InternalSetupTopLevelMenu();
				AssertEquals("Menu was added, Menu Item Count should be one extra", menuItemCount + 1, menu.InternalDataMenuItem.MenuItems.Count);
				bool isFound = false;
				foreach (MenuItem currentMenuItem in menu.InternalDataMenuItem.MenuItems)
				{
					if (currentMenuItem.Text == CommercialInvoiceMenu.ImportAUSInvoiceFormatText)
					{
						isFound = true;
					}
				}

				Assert("Should find AUS Commercial Invoice Import menu item", isFound);
			}
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.HEN.GUI
{
	public class HENMenuTest : TestCaseWithFactory
	{
		public void TestMenuItem()
		{
			using (HENMenu menu = (HENMenu)HENMenu.New())
			{
				int menuItemCount = menu.InternalDataMenuItem.MenuItems.Count;
				menu.InternalSetupTopLevelMenu();
				AssertEquals("Should be 1 new DataMenuItem", menuItemCount + 1, menu.InternalDataMenuItem.MenuItems.Count);
				MenuItem nissanImportMenuItem = null;
				foreach (MenuItem item in menu.InternalDataMenuItem.MenuItems)
				{
					if (item.Text == HENMenu.NissanImportMenuItemText)
					{
						nissanImportMenuItem = item;
						break;
					}
				}

				AssertNotNull("Nissan Menu Item was not found", nissanImportMenuItem);
			}
		}
	}
}

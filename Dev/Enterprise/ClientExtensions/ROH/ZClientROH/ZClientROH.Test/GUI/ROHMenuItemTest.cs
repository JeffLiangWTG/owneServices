using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.Rohlig.GUI
{
	public class ROHMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItem()
		{
			using (ROHMenuItem menu = (ROHMenuItem)ROHMenuItem.New())
			{
				int menuItemCount = menu.InternaldataMenuItem.MenuItems.Count;
				menu.SetupTopLevelMenuForTest();
				AssertEquals("Should be 1 new DataMenuItem", menuItemCount + 1, menu.InternaldataMenuItem.MenuItems.Count);
				MenuItem harleyDavidsonImportMenuItem = null;
				foreach (MenuItem item in menu.InternaldataMenuItem.MenuItems)
				{
					if (item.Text == ROHMenuItem.HarleyDavidsonImportMenuItemText)
					{
						harleyDavidsonImportMenuItem = item;
						break;
					}
				}

				AssertNotNull("HarleyDavidson Menu Item was not found", harleyDavidsonImportMenuItem);
			}
		}
	}
}

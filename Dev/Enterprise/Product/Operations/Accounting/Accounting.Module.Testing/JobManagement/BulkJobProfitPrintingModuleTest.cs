using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing
{
	public class BulkJobProfitPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (ZFilterGridModule module = new JobManagementModule())
			{
				MenuItem actions = module.FormActionMenu.FindByText("&Actions");
				foreach (MenuItem childMenuItem in actions.MenuItems)
				{
					if (childMenuItem == testMenuItem)
					{
						childMenuItem.PerformClick();
						break;
					}
				}
			}
		}
	}
}

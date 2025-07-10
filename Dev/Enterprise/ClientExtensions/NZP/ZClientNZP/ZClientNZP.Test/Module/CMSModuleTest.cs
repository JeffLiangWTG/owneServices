using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.NZP.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.NZP.Module.Testing
{
	public class CMSModuleTest : TransactionedTestCase
	{
		public void TestCMSAccountingExport_Click()
		{
			NZPDataRegistry.Instance.CMSLastDateExported = ZDateTime.Now;
			MenuItem exportMenuItem = null;
			using (CMSModule module = new CMSModule())
			{
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					foreach (MenuItem item in menuItem.MenuItems)
					{
						if (item.Text.EndsWith(Constants.CMSMenuItem))
						{
							exportMenuItem = item;
							break;
						}
					}
				}

				AssertNotNull(exportMenuItem);
				exportMenuItem.PerformClick();
			}

			AssertEquals(typeof(CMSFlatFileXmlExportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}

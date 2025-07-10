using System;
using System.Windows.Forms;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Client.DFD.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Module
{
	[TestedType(typeof(DFDARTransactionModule))]
	public class DFDARTransactionModuleTest : ARTransactionModuleStripTest
	{
		public void TestMenuItems()
		{
			using (DFDARTransactionModule module = (DFDARTransactionModule)ZModuleFactory.Instance.Create(ModuleIDs.ARTransaction))
			{
				MenuItem exportToXmlMenuItem = null;
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					foreach (MenuItem item in menuItem.MenuItems)
					{
						if (item.Text == "Export to XML (DFD)")
						{
							exportToXmlMenuItem = item;
							break;
						}
					}
				}

				AssertNotNull(exportToXmlMenuItem);
				exportToXmlMenuItem.PerformClick();
				AssertEquals("DFDFlatFileXmlExportForm shown when selecting menu item", typeof(DFDFlatFileXmlExportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public new void TestMenuItemAddedCorrectly()
		{
			using (TestTransactionModule)
			{
				using (var form = new ZForm())
				{
					var menu = ((DFDARTransactionModule)TestTransactionModule).InternalGetNewActionMenuItems();
					var menuitem = menu.FindByText("Create Compliance Document Records");
					AssertNull(menuitem);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var menu = ((DFDARTransactionModule)TestTransactionModule).InternalGetNewActionMenuItems();
				var menuitem = menu.FindByText("Create Compliance Document Records");
				AssertNotNull(menuitem);
				var subMenuItem1 = menuitem.MenuItems.FindByText("Roll-up by Charge Code");
				AssertNotNull(subMenuItem1);
				var subMenuItem2 = menuitem.MenuItems.FindByText("No Roll-up");
				AssertNotNull(subMenuItem2);
			}
		}
	}
}

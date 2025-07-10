using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.CLE.Modules.Testing
{
	[TestedType(typeof(CLEOrdersModule))]
	public class CLEOrdersModuleTest : OrdersModule_Test
	{
		public void TestImportOrdersMenuItem()
		{
			using (CLEOrdersModule module = new CLEOrdersModule())
			{
				MenuItem importMenuItem = MenuAssertion.AssertHasMenu("Menu item should exist", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import CLE Orders");
				importMenuItem.PerformClick();
				AssertEquals("Data Import form was shown", typeof(CLEDataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}
}

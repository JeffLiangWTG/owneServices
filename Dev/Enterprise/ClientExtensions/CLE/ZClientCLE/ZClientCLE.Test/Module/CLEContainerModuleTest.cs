using System.Windows.Forms;
using Enterprise.Freight.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.CLE.Modules.Testing
{
	public class CLEContainerModuleTest : ContainersModuleMenuTest
	{
		public void TestImportGMCMenuItem()
		{
			using (CLEContainerModule module = new CLEContainerModule())
			{
				MenuItem importMenuItem = MenuAssertion.AssertHasMenu("Menu item should exist", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import &GMC Dehire from CSV");
				importMenuItem.PerformClick();
				AssertEquals("Data Import form was shown", typeof(CLEDataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}
	}
}

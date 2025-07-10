using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InvoiceLineFiscalReferencesUserControlTest : TestCaseWithFactory
	{
		public void TestFiscalReferencesUserControlDock()
		{
			using (var control = new InvoiceLineFiscalReferencesUserControl())
			{
				AssertEquals(System.Windows.Forms.DockStyle.Fill, control.FindSingleOrDefault<FiscalReferencesUserControl>("FiscalReferencesUserControl").Dock);
			}
		}
	}
}

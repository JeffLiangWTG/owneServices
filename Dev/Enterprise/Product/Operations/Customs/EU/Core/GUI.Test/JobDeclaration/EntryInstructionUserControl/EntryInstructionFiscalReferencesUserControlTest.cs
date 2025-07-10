using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionFiscalReferencesUserControlTest : TestCaseWithFactory
	{
		public void TestFiscalReferencesUserControlDock()
		{
			using (var control = new EntryInstructionFiscalReferencesUserControl())
			{
				AssertEquals(System.Windows.Forms.DockStyle.Fill, control.FindSingleOrDefault<FiscalReferencesUserControl>("FiscalReferencesUserControl").Dock);
			}
		}
	}
}

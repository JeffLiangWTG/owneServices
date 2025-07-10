using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class PreviousProcedureExportATZLPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new PreviousProcedureExportATZLPanel("FilteredInvoiceLines"))
			{
				AssertEquals("localReferenceTextBox", "FilteredInvoiceLines.PreviousProcedureMaster.CSI_ReferenceNumber2", control.LocalReferenceTextBox.BindTo);
				AssertEquals("authorizationNumberTextBox", "FilteredInvoiceLines.PreviousProcedureMaster.AuthorizationNumber", control.AuthorizationNumberDropDown.BindTo);
			}
		}

		public void TestLocalReferenceTextBoxAllowsLowerCaseCharacters()
		{
			using (var control = new PreviousProcedureExportATZLPanel("FilteredInvoiceLines"))
			{
				AssertEquals("localReferenceTextBox CharacterCasing is set to Normal", CharacterCasing.Normal, control.LocalReferenceTextBox.CharacterCasing);
			}
		}
	}
}

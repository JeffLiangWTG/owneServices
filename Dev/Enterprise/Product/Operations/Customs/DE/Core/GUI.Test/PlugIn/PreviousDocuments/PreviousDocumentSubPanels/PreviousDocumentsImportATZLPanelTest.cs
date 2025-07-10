using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class PreviousDocumentsImportATZLPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new PreviousDocumentsImportATZLPanel("FilteredInvoiceLines"))
			{
				AssertEquals("localReferenceTextBox", "FilteredInvoiceLines.PreviousDocumentMaster.CSI_ReferenceNumber2", control.FindSingle<ZTextBox>("localReferenceTextBox").BindTo);
				AssertEquals("authorizationNumberTextBox", "FilteredInvoiceLines.PreviousDocumentMaster.AuthorizationNumber", control.FindSingle<ZDropEdit>("authorizationNumberTextBox").BindTo);
			}
		}
	}
}

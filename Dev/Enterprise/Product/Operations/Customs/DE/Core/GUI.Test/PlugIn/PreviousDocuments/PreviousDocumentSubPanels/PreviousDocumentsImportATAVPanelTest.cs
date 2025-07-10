using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class PreviousDocumentsImportATAVPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new PreviousDocumentsImportATAVPanel("FilteredInvoiceLines"))
			{
				AssertEquals("SimplifiedGrantAuthorizationCheckBox", "FilteredInvoiceLines.PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag", control.FindSingle<ZCheckBox>("SimplifiedGrantAuthorizationCheckBox").BindTo);
				AssertEquals("MonitoringCustomsOfficeFindBox", "FilteredInvoiceLines.PreviousDocumentMaster.CSI_CustomsOffice", control.FindSingle<ZCodeFindBox>("MonitoringCustomsOfficeFindBox").BindTo);
				AssertEquals("AuthorizationNumberDropEdit", "FilteredInvoiceLines.PreviousDocumentMaster.AuthorizationNumber", control.FindSingle<ZDropEdit>("AuthorizationNumberDropEdit").BindTo);
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class PreviousProcedureExportATAVPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new PreviousProcedureExportATAVPanel("FilteredInvoiceLines"))
			{
				AssertEquals("SimplifiedGrantAuthorizationCheckBox", "FilteredInvoiceLines.PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag", control.FindSingle<ZCheckBox>("SimplifiedGrantAuthorizationCheckBox").BindTo);
				AssertEquals("MonitoringCustomsOfficeFindBox", "FilteredInvoiceLines.PreviousProcedureMaster.CSI_CustomsOffice", control.FindSingle<ZCodeFindBox>("MonitoringCustomsOfficeFindBox").BindTo);
				AssertEquals("AuthorizationNumberDropEdit", "FilteredInvoiceLines.PreviousProcedureMaster.AuthorizationNumber", control.FindSingle<ZDropEdit>("AuthorizationNumberDropEdit").BindTo);
			}
		}
	}
}

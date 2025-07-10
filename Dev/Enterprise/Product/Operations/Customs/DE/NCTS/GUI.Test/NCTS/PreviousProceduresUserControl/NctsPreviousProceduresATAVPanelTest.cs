using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class NctsPreviousProceduresATAVPanelTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			using (var control = new NctsPreviousProceduresATAVPanel())
			{
				AssertEquals("SimplifiedGrantAuthorizationCheckBox", "PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag", control.SimplifiedGrantAuthorizationCheckBox.BindTo);
				AssertEquals("MonitoringCustomsOfficeFindBox", "PreviousProcedureMaster.CSI_CustomsOffice", control.MonitoringCustomsOfficeFindBox.BindTo);
				AssertEquals("AuthorizationNumberDropEdit", "PreviousProcedureMaster.AuthorizationNumber", control.AuthorizationNumberDropEdit.BindTo);
			}
		}
	}
}

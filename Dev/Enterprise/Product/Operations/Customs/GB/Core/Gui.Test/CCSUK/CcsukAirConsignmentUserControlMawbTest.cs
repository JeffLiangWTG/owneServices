using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Testing
{
	class CcsukAirConsignmentUserControlMawbTest : TestCaseWithFactory
	{
		public void TestRemovalsTabsVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "12387654321";
			consol.JK_RL_NKDischargePort = "GBLHR";

			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "12387654321";
			mAWB.Profile = "CUKAIR98LHRABC";
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					var control = plugin.UserControl;
					AssertEquals(1, control.Controls.Find("IsrsTabsPage", true).Length);
					AssertEquals(0, control.Controls.Find("TsrTabPage", true).Length);
					AssertEquals(0, control.Controls.Find("IarTabPage", true).Length);
					AssertEquals(0, control.Controls.Find("FallbackTabPage", true).Length);
					mAWB.Profile = "CUKFFW98000ABC";
					AssertEquals(0, control.Controls.Find("IsrsTabsPage", true).Length);
					AssertEquals(1, control.Controls.Find("TsrTabPage", true).Length);
					AssertEquals(1, control.Controls.Find("IarTabPage", true).Length);
					AssertEquals(1, control.Controls.Find("FallbackTabPage", true).Length);
				}
			}
		}
	}
}

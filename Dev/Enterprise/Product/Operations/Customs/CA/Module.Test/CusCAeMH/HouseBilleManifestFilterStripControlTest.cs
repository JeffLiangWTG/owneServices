using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class HouseBilleManifestFilterStripControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridFields()
		{
			var coll = new CusCAeMHMasterCollection(Factory);
			var filterBO = new HouseBilleManifestFilterStripBusinessObject();
			using (var filterControl = new HouseBilleManifestFilterStripControl(coll, filterBO))
			{
				filterControl.Show();
				AssertNotNull("Message reference", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_MessageReference));
				AssertNotNull("Discharge Port", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_CBSADischargePortName));
				AssertNotNull("Discharge Sub-Location", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_CBSADischargeSubLocationName));
				AssertNotNull("Customs Status", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_CustomsStatusDescription));
				AssertNotNull("Message Status", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_MessageStatusDescription));
				AssertNotNull("Port of Discharge", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_RL_NKDiscPortName));
				AssertNotNull("Carrier", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_CBSACarrierName));
				AssertNotNull("Amendment", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_AmendReasonCode));
				AssertNotNull("ATA", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_ATA));
				AssertNotNull("Branch", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_GB_Branch));
				AssertNotNull("Master Bill", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_MasterBill));
				AssertNotNull("Master House Bill", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_MasterHouseBill));
				AssertNotNull("Master House CCN", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_MasterHouseCCN));
				AssertNotNull("Primary CCN", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_PrimaryCCN));
				AssertNotNull("Master Latest D4 Notice", filterControl.FilteredGrid.GetColumnStyle(CusCAeMHMaster.Schema.BP_D4MessageStatus));
				AssertNotNull("Master Latest D4 Notice Description", filterControl.FilteredGrid.GetColumnStyle("BP_D4MessageStatusDescription"));
				AssertNotNull("House Latest D4 Notice", filterControl.FilteredGrid.GetColumnStyle("HouseBillLatestD4MessageStatus"));
				AssertNotNull("House Latest D4 Notice Description", filterControl.FilteredGrid.GetColumnStyle("HouseBillLatestD4MessageStatusDescription"));
			}
		}
	}
}

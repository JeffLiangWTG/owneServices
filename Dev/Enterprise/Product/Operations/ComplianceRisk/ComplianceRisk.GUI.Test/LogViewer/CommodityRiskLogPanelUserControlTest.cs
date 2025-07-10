using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class CommodityRiskLogPanelUserControlTest : ComplianceRiskHelperTest
	{
		public void TestControls()
		{
			using var control = new CommodityRiskLogPanelUserControl();
			AssertType<CommodityRiskStatusLogUserControl>(control.Controls.Find("CommodityAssessmentRiskLogUserControl", searchAllChildren: true).Single());
			AssertType<ZGroupBox>(control.Controls.Find("CommodityDetailsGroupBox", searchAllChildren: true).Single());
			AssertType<CommodityRiskLogGrid>(control.Controls.Find("CommodityRiskLogGrid", searchAllChildren: true).Single());
			AssertType<KSplitContainer>(control.Controls.Find("CommodityAssessmentSplitContainer", searchAllChildren: true).Single());
		}

		public void TestSnapshotData_CommodityAssessment()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = "WCO";
			complianceCommodityDetail.CCD_HarmonizedCode = "963258";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControl())
			{
				userControl.SetBindingMember(".");

				form.Controls.Add(userControl);
				form.Show();

				var logsCollection = userControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				userControl.ComplianceRiskStatusChangesLogsGrid.
					SelectSingleElement(logsCollection.Cast<ComplianceRiskStatusChangeLog>().
					Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear));

				var snapshotCommoditiesGrid = (ZGrid)userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single();
				AssertNotNull(snapshotCommoditiesGrid.GetColumnStyle("NomenclatureCondition"));
				AssertNotNull(snapshotCommoditiesGrid.GetColumnStyle("SpecificCondition"));

				var commodity = snapshotCommoditiesGrid.ListManager.Current as ComplianceCommodityRiskLog;
				AssertEquals("963258", commodity.Code);

				var commodityRiskStatus = (ZTextBox)userControl.Controls.Find("CommodityRiskStatus", searchAllChildren: true).Single();
				AssertEquals(commodity.RiskStatusDescription, commodityRiskStatus.Text);
				AssertNotNull(snapshotCommoditiesGrid.GetColumnStyle("SpecificCondition"));
			}
		}
	}
}

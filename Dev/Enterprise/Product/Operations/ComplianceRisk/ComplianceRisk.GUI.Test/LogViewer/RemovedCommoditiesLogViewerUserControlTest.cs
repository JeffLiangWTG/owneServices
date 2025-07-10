using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(RemovedCommoditiesLogViewerUserControl))]
	public class RemovedCommoditiesLogViewerUserControlTest : ComplianceRiskHelperTest
	{
		public void TestReloadCollectionOnFactorySaved_WhenCommoditiesDeletedWithInitialized()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.InitializeAssessmentWorkflow();

			using var zForm = new ZForm(shipment);
			using var userControl = new RemovedCommoditiesLogViewerUserControl();
			userControl.SetBindingMember(".");
			zForm.Controls.Add(userControl);
			zForm.Show();

			var grid = typeof(RemovedCommoditiesLogViewerUserControl)
				.GetField("RemovedCommoditiesLogsGrid", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(userControl) as ZGrid;

			AssertEquals(0, grid.ListManager.Count);

			var snapshot = new RemovedCommoditiesSnapshot();
			snapshot.Commodities.Add(new Commodity { Code = "C1", Conditions = "C2", HsCodeDescription = "D1", RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk, Source = "S1", CommoditySource = "C3", Notes = "N1" });
			var provider = shipment as IComplianceItemRiskStatusProvider;
			var eventLog = Factory.New<StmComplianceEvent>();
			eventLog.SCE_ParentID = provider.ParentID;
			eventLog.SCE_ParentTableCode = provider.ParentTableCode;
			eventLog.SCE_EventType = ComplianceEventList.EventType.ComplianceCommodityInteraction;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.CommodityLineDeleted;
			eventLog.SCE_EventTimeOffset = new ZDateTimeOffset(new ZDateTime(2024, 8, 7, 10, 51, 09));
			eventLog.SCE_SystemCreateTimeUtc = new ZDateTime(2024, 8, 7, 10, 52, 01);
			eventLog.SCE_SystemCreateUser = "CC1";
			eventLog.SCE_Snapshot = JsonConvert.SerializeObject(snapshot);
			AssertEquals(0, grid.ListManager.Count);

			Factory.Save();
			AssertEquals("Collection Reloaded", 1, grid.ListManager.Count);
		}

		public void TestShowCommodityRiskPanelUserControl()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var zForm = new ZForm(shipment))
			using (var userControl = new RemovedCommoditiesLogViewerUserControl())
			{
				userControl.SetBindingMember(".");
				zForm.Controls.Add(userControl);
				zForm.Show();

				var panelUserControl = userControl.Controls.Find("CommodityAssessmentRiskLogUserControl", searchAllChildren: true).Single();
				var grid = userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single() as ZGrid;
				AssertEquals(true, panelUserControl.Visible);
				AssertEquals(true, grid.Visible);
				Assert(grid.GetColumnStyle("DateAddedUtc").IsVisible);
			}
		}
	}
}

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskLogViewerUserControlTest : ComplianceRiskHelperTest
	{
		public void TestComplianceRiskStatusBackgroundLabelColor_WhenTakeSnapshotAndSetToOverrideClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			using (var form = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskStatusCodeList.Codes.PotentialRisk), userControl.ExposedPartyRiskLabel.BackColor);
				AssertEquals(ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskStatusCodeList.Codes.Clear), userControl.ExposedLocationRiskLabel.BackColor);
				AssertEquals(ComplianceRiskColorHelper.GetColorForRiskStatus(ComplianceRiskStatusCodeList.Codes.Incomplete), userControl.ExposedCommodityRiskLabel.BackColor);
			}
		}

		public void TestReloadCollectionVisibleChange_WhenTakeSnapshotAndSetToOverrideClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			using (var form = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				form.Controls.Add(userControl);
				userControl.Visible = false;
				form.Show();
				AssertNull(userControl.ComplianceRiskStatusChangesLogsGrid.ListManager);

				userControl.Visible = true;
				AssertNotNull(userControl.ComplianceRiskStatusChangesLogsGrid.ListManager);
				AssertEquals("Collection Reloaded", 1, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);

				var collection = userControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				collection.RemoveAndDeleteAll();
				userControl.Visible = false;
				AssertEquals("Collection not Reloaded when visible is false", 0, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);
			}
		}

		public void TestReloadCollectionOnFactorySaved_WhenTakeSnapshotAndSetToOverrideClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			using (var zForm = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				zForm.Controls.Add(userControl);
				zForm.Show();
				AssertEquals(1, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);

				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				AssertEquals(1, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);

				Factory.Save();
				AssertEquals("Collection Reloaded", 2, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);
			}
		}

		public void TestBindDataToGrids_WhenTakeSnapshotAndSetToOverrideClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "963258";
			complianceCommodityDetail.Conditions = "CONDITIONS";
			complianceCommodityDetail.Description = "DESCRIPTION";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			using (var form = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(1, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);
				AssertEquals(1, userControl.SnapshotPartiesGrid.ListManager.Count);
				AssertEquals(1, userControl.SnapshotLocationsGrid.ListManager.Count);
				var snapshotCommoditiesGrid = (ZGrid)userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single();
				AssertEquals(1, snapshotCommoditiesGrid.ListManager.Count);

				AssertEquals(false, userControl.InnerSplitContainer1.Panel2Collapsed);
				AssertEquals(false, userControl.InnerSplitContainer2.Panel2Collapsed);
				AssertEquals(false, userControl.InnerSplitContainer3.Panel2Collapsed);
			}
		}

		public void TestCollaspedPanels_WhenTakeSnapshotAndSetToOverrideClear()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "963258";
			complianceCommodityDetail.Conditions = "CONDITIONS";
			complianceCommodityDetail.Description = "DESCRIPTION";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				form.Controls.Add(userControl);
				form.Show();

				var logsCollection = userControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				userControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logsCollection.Cast<ComplianceRiskStatusChangeLog>().
					Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear));

				AssertEquals(2, userControl.ComplianceRiskStatusChangesLogsGrid.ListManager.Count);
				AssertEquals(1, userControl.SnapshotPartiesGrid.ListManager.Count);
				AssertEquals(1, userControl.SnapshotLocationsGrid.ListManager.Count);
				var snapshotCommoditiesGrid = (ZGrid)userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single();
				AssertEquals(1, snapshotCommoditiesGrid.ListManager.Count);

				AssertEquals(false, userControl.InnerSplitContainer1.Panel2Collapsed);
				AssertEquals(false, userControl.InnerSplitContainer2.Panel2Collapsed);
				AssertEquals(false, userControl.InnerSplitContainer3.Panel2Collapsed);

				userControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logsCollection.Cast<ComplianceRiskStatusChangeLog>().
					Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.PotentialRisk));

				AssertEquals(0, userControl.SnapshotPartiesGrid.ListManager.Count);
				AssertEquals(0, userControl.SnapshotLocationsGrid.ListManager.Count);
				AssertEquals(0, snapshotCommoditiesGrid.ListManager.Count);

				AssertEquals(true, userControl.InnerSplitContainer1.Panel2Collapsed);
				AssertEquals(true, userControl.InnerSplitContainer2.Panel2Collapsed);
				AssertEquals(true, userControl.InnerSplitContainer3.Panel2Collapsed);

				userControl.Dispose();
				form.Dispose();
			}
		}

		public void TestAssessmentGroupVisibilityFalse_WhenTakeSnapshotSetToOverrideClearAndJobConsolidationDirectionIsInternational()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<IForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)consol);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(consol as BusinessObject);
			pluginBizO.RefreshData();

			Factory.Save();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "International job direction is Import"));

			using (var form = new ZForm(consol))
			using (var logUserControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				logUserControl.SetBindingMember(".");
				form.Controls.Add(logUserControl);
				form.Show();

				var logsCollection = logUserControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				var logDetails = logsCollection.Cast<ComplianceRiskStatusChangeLog>().
					Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear);
				logUserControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logDetails);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, logDetails.OverallRisk);
				AssertEquals(true, logDetails.ComplianceJobDirection.IsInternational);
				AssertEquals("Import", logDetails.ComplianceJobDirection.Direction);
				AssertEquals(true, logUserControl.ExposedCommoditiesGroupBox.Visible);
			}
		}

		public void TestAssessmentGroupVisibilityTrue_WhenTakeSnapshotSetToOverrideClearAndJobShipmentDirectionIsInternational()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "International job direction is Import"));

			using (var form = new ZForm(shipment))
			using (var logUserControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				logUserControl.SetBindingMember(".");
				form.Controls.Add(logUserControl);
				form.Show();

				var logsCollection = logUserControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				var logDetails = logsCollection.Cast<ComplianceRiskStatusChangeLog>().
					Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear);
				logUserControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logDetails);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, logDetails.OverallRisk);
				AssertEquals(true, logDetails.ComplianceJobDirection.IsInternational);
				AssertEquals("Import", logDetails.ComplianceJobDirection.Direction);
				AssertEquals(true, logUserControl.ExposedCommoditiesGroupBox.Visible);
			}
		}

		public void TestAssessmentGroupVisibilityFalse_WhenTakeSnapshotSetToOverrideClearAndJobShipmentDirectionIsDomestic()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Domestic job direction"));

			using (var form = new ZForm(shipment))
			using (var logUserControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				logUserControl.SetBindingMember(".");
				form.Controls.Add(logUserControl);
				form.Show();

				var logsCollection = logUserControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				var logDetails = logsCollection.Cast<ComplianceRiskStatusChangeLog>().Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear);
				logUserControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logDetails);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, logDetails.OverallRisk);
				AssertEquals(false, logDetails.ComplianceJobDirection.IsInternational);
				AssertEquals("Domestic", logDetails.ComplianceJobDirection.Direction);
				AssertEquals(false, logUserControl.ExposedCommoditiesGroupBox.Visible);
			}
		}

		public void TestAssessmentGroupVisibilityFalse_WhenTakeSnapshotSetToOverrideClearAndJobShipmentDirectionIsUnknown()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			Factory.Save();

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Unknown job direction"));

			using (var form = new ZForm(shipment))
			using (var logUserControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				logUserControl.SetBindingMember(".");
				form.Controls.Add(logUserControl);
				form.Show();

				var logsCollection = logUserControl.ComplianceRiskStatusChangesLogsGrid.DataSource as ComplianceRiskStatusChangeLogCollection;
				var logDetails = logsCollection.Cast<ComplianceRiskStatusChangeLog>().Single(u => u.OverallRisk == ComplianceRiskStatusCodeList.Codes.OverrideClear);
				logUserControl.ComplianceRiskStatusChangesLogsGrid.SelectSingleElement(logDetails);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, logDetails.OverallRisk);
				AssertEquals(false, logDetails.ComplianceJobDirection.IsInternational);
				AssertEquals("Unknown", logDetails.ComplianceJobDirection.Direction);
				AssertEquals(false, logUserControl.ExposedCommoditiesGroupBox.Visible);
			}
		}

		public void TestOpenForm()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			Factory.Save();

			using (var zForm = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				zForm.Controls.Add(userControl);
				zForm.Show();

				userControl.ExposedOpenForm(new CompliancePartyRiskLog(new Party { PK = Guid.NewGuid(), TableCode = "OH" }, null, null), typeof(OrgHeader));
				AssertEquals("The selected party is no longer exists.", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.ExposedOpenForm(new CompliancePartyRiskLog(new Party { PK = header.PK.ToGuid(), TableCode = "OH" }, null, null), typeof(OrgHeader));
				using (var form = ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull(form);
					AssertEquals("Logs", form.OrganisationsTabControl.SelectedTab.Text);
					AssertEquals("Denied Party Screening Logs", (form.OrganisationsTabControl.SelectedTab.Controls[0].Controls[0] as ZTabControl).SelectedTab.Text);
				}

				userControl.ExposedOpenForm(new ComplianceLocationRiskLog(new Business.Country { Code = "DUM" }, null), typeof(RefCountry));
				AssertEquals("The selected country is no longer exists.", UnitTestUserNotification.Instance.LastMessage.Text);

				userControl.ExposedOpenForm(new ComplianceLocationRiskLog(new Business.Country { Code = "AU" }, null), typeof(RefCountry));
				using (var form = ZApplication.GetOpenForms().OfType<RefCountryForm>().SingleOrDefault())
				{
					AssertNotNull(form);
					AssertEquals("Logs", form.Controls.OfType<ZTemplateTabControl>().FirstOrDefault()?.SelectedTab.Text);
				}
			}
		}

		public void TestComplianceLogsTabPage_ShouldNotThrow_NotImplementedException()
		{
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var dpsLogsUserControl = new ZUserControl();
			var complianceLogUserControl = new ComplianceLogUserControl(dpsLogsUserControl);

			var tariffView = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "930390", "Test Conditions");
			var commodityDetail = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = tariffView.ZZ1_TariffCode;
			commodityDetail.Conditions = tariffView.Conditions[0].ConditionValues[0].ZX3_Value;

			AssertNoExceptionThrown(() =>
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
				using (var logsTabPage = new ZLogsTabPage())
				{
					form.TabControl.TabPages.Add(logsTabPage);
					ComplianceLogTabHelper.AddLogTabIfNeeded((BusinessObject)shipment, logsTabPage, complianceLogUserControl);
					form.Show();

					var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
					form.FireSaveButton();
					form.Dispose();
				}
			});
		}

		public void TestShowCommodityRiskPanelUserControl()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = CreateNewShipment;
			shipment.JS_OH_ExportBroker = header.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			Factory.Save();

			complianceRiskStatus.GetEventLogs().ForEach(e => e.Delete());

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment as BusinessObject);
			pluginBizO.RefreshData();

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Unknown job direction"));

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var zForm = new ZForm(shipment))
			using (var userControl = new ComplianceRiskLogViewerUserControlForTest())
			{
				userControl.SetBindingMember(".");
				zForm.Controls.Add(userControl);
				zForm.Show();

				var panelUserControl = userControl.Controls.Find("CommodityAssessmentRiskLogUserControl", searchAllChildren: true).Single();
				var grid = userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single() as ZGrid;
				AssertEquals(true, panelUserControl.Visible);
				AssertEquals(true, grid.Visible);
				Assert(!grid.GetColumnStyle("DateAddedUtc").IsVisible);
			}
		}

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}

		class ComplianceRiskLogViewerUserControlForTest : ComplianceRiskLogViewerUserControl
		{
			public ZLabel ExposedPartyRiskLabel => PartyRiskLabel;
			public ZLabel ExposedLocationRiskLabel => LocationRiskLabel;
			public ZLabel ExposedCommodityRiskLabel => CommodityRiskLabel;
			public ZGroupBox ExposedCommoditiesGroupBox => CommoditiesGroupBox;

			public void ExposedOpenForm(CompliancePartyRiskLog partyLog, Type type) => OpenForm(partyLog, type);
			public void ExposedOpenForm(ComplianceLocationRiskLog locationLog, Type type) => OpenForm(locationLog, type);
		}
	}
}

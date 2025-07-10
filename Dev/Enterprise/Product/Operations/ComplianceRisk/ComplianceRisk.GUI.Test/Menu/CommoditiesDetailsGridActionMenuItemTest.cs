using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	[TestedType(typeof(CommoditiesDetailsGridActionMenuItem))]
	public class CommoditiesDetailsGridActionMenuItemTest : ComplianceRiskHelperTest
	{
		public void TestCommodityGridWhenAssessmentNotInitializedOrEmpty_ShouldShowActionMenuItemsDefaultSettings()
		{
			var shipment = CreateInternationalShipment();
			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				CombineAssertions("Commodities Grid should show actions menu items(", () =>
				{
					var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
					AssertNotNull("CommodityRiskGrid should exists.", commodityGrid);

					var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);
					AssertNotNull("Actions menu should exists.", actionsMenu);
					AssertNotNull("Set Commodity Risk Status to Blocked menu item should exists.", blockedMenu);
					AssertNotNull("Set Commodity Risk Status to Released menu item should exists.", releaseMenu);
					AssertEquals("Set Commodity Risk Status to Blocked menu item should be disabled.", expected: false, blockedMenu.Enabled);
					AssertEquals("Set Commodity Risk Status to Released menu item should be disabled.", expected: false, releaseMenu.Enabled);
				});

				form.Dispose();
			}
		}

		public void TestCommodityGridWhenAssessmentInitialized_SetCommodityRiskStatusToBlocked()
		{
			var shipment = CreateInternationalShipment();
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var tariff = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "072311");
			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = tariff.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				initializeAssessmentMenu.PerformClick();

				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				commodityGrid.Select(0);
				AssertEquals("Selected elements count should include anything selected", 1, commodityGrid.SelectedElements.Length);
				AssertEquals("Commodity Risk Status should be Clear", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);

				commodityGrid.PerformMouseDownForTest(commodityGrid.CurrentRowIndex, 1);
				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);
				AssertEquals("Set Commodity Risk Status to Blocked menu item should be enabled.", expected: true, blockedMenu.Enabled);
				AssertEquals("Set Commodity Risk Status to Released menu item should be enabled.", expected: true, releaseMenu.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				blockedMenu.PerformClick();
				AssertEquals("Commodity Risk Status should be Blocked", ComplianceRiskStatusCodeList.Codes.Blocked, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().FirstOrDefault().CCD_RiskStatus);
				AssertEquals("Compliance Commodity Risk Status should be Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);

				form.Dispose();
			}
		}

		public void TestCommodityGridWhenAssessmentInitialized_SetCommodityRiskStatusToReleased()
		{
			var shipment = CreateInternationalShipment();
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var tariff = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "930390", "Test Conditions");

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_CountryOrGrouping = tariff.ZZ1_ZZZ_NKDataGrouping;
				commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;
				Factory.Save();

				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				initializeAssessmentMenu.PerformClick();

				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				commodityGrid.Select(0);
				AssertEquals("Selected elements count should include anything selected", 1, commodityGrid.SelectedElements.Length);
				AssertEquals("Commodity Risk Status should be Potential Risk", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);

				commodityGrid.PerformMouseDownForTest(commodityGrid.CurrentRowIndex, 1);
				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);
				AssertEquals("Set Commodity Risk Status to Blocked menu item should be enabled.", expected: true, blockedMenu.Enabled);
				AssertEquals("Set Commodity Risk Status to Released menu item should be enabled.", expected: true, releaseMenu.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				releaseMenu.PerformClick();
				AssertEquals("Commodity Risk Status should be Release", ComplianceRiskStatusCodeList.Codes.Released, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().FirstOrDefault().CCD_RiskStatus);
				AssertEquals("Compliance Commodity Risk Status should be Clear", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);

				form.Dispose();
			}
		}

		public void TestCommodityGridWhenAssessmentInitialized_SetCommodityRiskStatusVariations()
		{
			var shipment = CreateInternationalShipment();
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var tariff1 = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "072311");
				var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity1.CCD_CountryOrGrouping = tariff1.ZZ1_ZZZ_NKDataGrouping;
				commodity1.CCD_HarmonizedCode = tariff1.ZZ1_TariffCode;

				var tariff2 = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "930390", "Test Conditions");
				var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
				commodity2.CCD_CountryOrGrouping = tariff2.ZZ1_ZZZ_NKDataGrouping;
				commodity2.CCD_HarmonizedCode = tariff2.ZZ1_TariffCode;
				commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

				Factory.Save();

				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				initializeAssessmentMenu.PerformClick();

				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				commodityGrid.SelectAllElements();
				AssertEquals("Commodity1 Risk Status should be Clear", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("Commodity2 Risk Status should be Potential Risk", ComplianceRiskStatusCodeList.Codes.PotentialRisk, commodity2.CCD_RiskStatus);

				commodityGrid.PerformMouseDownForTest(1, 2);
				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);
				AssertEquals("Set Commodity Risk Status to Blocked menu item should be enabled.", expected: true, blockedMenu.Enabled);
				AssertEquals("Set Commodity Risk Status to Released menu item should be enabled.", expected: true, releaseMenu.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				releaseMenu.PerformClick();
				AssertEquals("Commodity1 Risk Status should be Release", ComplianceRiskStatusCodeList.Codes.Released, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().FirstOrDefault().CCD_RiskStatus);
				AssertEquals("Commodity2 Risk Status should be Release", ComplianceRiskStatusCodeList.Codes.Released, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().LastOrDefault().CCD_RiskStatus);
				AssertEquals("Compliance Commodity Risk Status should be Clear", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				blockedMenu.PerformClick();
				AssertEquals("Commodity1 Risk Status should be Blocked", ComplianceRiskStatusCodeList.Codes.Blocked, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().FirstOrDefault().CCD_RiskStatus);
				AssertEquals("Commodity2 Risk Status should be Blocked", ComplianceRiskStatusCodeList.Codes.Blocked, commodityGrid.SelectedElements.Cast<ComplianceCommodityDetail>().LastOrDefault().CCD_RiskStatus);
				AssertEquals("Compliance Commodity Risk Status should be Blocked", ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_CommodityRisk);

				form.Dispose();
				form.Close();
			}
		}

		public void TestCommodityGridActionsMenuItems()
		{
			var shipment = CreateInternationalShipment();
			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);
				AssertNotNull("Actions menu should exist", actionsMenu);
				AssertNotNull("Set Commodity Risk Status to Blocked menu item should exist", blockedMenu);
				AssertNotNull("Set Commodity Risk Status to Released menu item should exist", releaseMenu);
			}
		}

		public void TestCommodityGridWhenSecurityRights_ShipmentsComplianceEditComplianceAssessmentNotGranted()
		{
			var shipment = CreateInternationalShipment();
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var tariff = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "072311");
			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = tariff.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;

			Factory.Save();

			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceEditComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				initializeAssessmentMenu.PerformClick();

				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				commodityGrid.Select(0);
				AssertEquals("Selected elements count should include anything selected", 1, commodityGrid.SelectedElements.Length);

				commodityGrid.PerformMouseDownForTest(commodityGrid.CurrentRowIndex, 1);
				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);

				UnitTestUserNotification.Instance.ClearMessages();
				releaseMenu.PerformClick();
				AssertEquals(securityCore.ShipmentsComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				blockedMenu.PerformClick();
				AssertEquals(securityCore.ShipmentsComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				form.Dispose();
			}
		}

		public void TestCommodityGrid_WhenJobHasChangesShowMessage()
		{
			var shipment = CreateInternationalShipment();
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			var tariff = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "072311");
			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = tariff.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;

			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				initializeAssessmentMenu.PerformClick();

				shipment.JS_RL_NKDestination = "DEHAM";

				var commodityGrid = form.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				commodityGrid.Select(0);
				commodityGrid.PerformMouseDownForTest(commodityGrid.CurrentRowIndex, 1);

				var (actionsMenu, blockedMenu, releaseMenu) = GetCommodityGridMenuItems(commodityGrid);

				UnitTestUserNotification.Instance.ClearMessages();
				blockedMenu.PerformClick();
				AssertEquals("Please save the form before updating the Commodity Risk Status to Blocked.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				releaseMenu.PerformClick();
				AssertEquals("Please save the form before updating the Commodity Risk Status to Released.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.Dispose();
			}
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawComplianceWiseRegistryBusinessObject;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawComplianceWiseRegistryBusinessObject = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true));
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawComplianceWiseRegistryBusinessObject);
		}

		(ZMenuItem ActionsMenu, ZMenuItem BlockedMenu, ZMenuItem ReleaseMenu) GetCommodityGridMenuItems(ZGrid commodityGrid)
		{
			var actionsMenu = commodityGrid.ContextMenu.MenuItems.FindByText("Actions", findSubitems: true) as ZMenuItem;
			var blockedMenu = actionsMenu?.MenuItems.FindByText("Set Commodity Risk Status to Blocked", findSubitems: true) as ZMenuItem;
			var releasedMenu = actionsMenu?.MenuItems.FindByText("Set Commodity Risk Status to Released", findSubitems: true) as ZMenuItem;

			return (actionsMenu, blockedMenu, releasedMenu);
		}

		IForwardingShipment CreateInternationalShipment()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			(shipment as CommonShipment).ConsignorPK = consignor.PK;
			(shipment as CommonShipment).ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			eventLog.SCE_ParentID = shipment.PK;

			return shipment;
		}
	}
}

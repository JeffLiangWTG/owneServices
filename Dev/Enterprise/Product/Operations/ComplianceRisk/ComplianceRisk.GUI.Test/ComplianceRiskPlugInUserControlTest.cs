using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskPlugInUserControlTest : ComplianceRiskHelperTest
	{
		public void TestSetRiskProvidersVisible_WithoutCommodityProviders()
		{
			var shipment = (Factory.New<ShipmentWithoutCommodityProvider>() as BusinessObject);
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			using (var form = new ZForm())
			using (var userControl = new ComplianceRiskPlugInUserControl(pluginBizO))
			{
				form.Controls.Add(userControl);
				form.Show();
				var locationsBlockVisibility = form.Controls.Find("LocationGroupBox", true).FirstOrDefault().Visible;
				var partiesBlockVisibility = form.Controls.Find("PartyGroupBox", true).FirstOrDefault().Visible;
				var commoditiesBlockVisibility = form.Controls.Find("CommodityGroupBox", true).FirstOrDefault().Visible;
				AssertEquals(true, locationsBlockVisibility);
				AssertEquals(true, partiesBlockVisibility);
				AssertEquals(false, commoditiesBlockVisibility);
			}
		}

		public void TestSetRiskProvidersVisible_OnlyWithCommodityProviders()
		{
			var shipment = (Factory.New<ShipmentOnlyWithCommodityProvider>() as BusinessObject);
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			using (var form = new ZForm())
			using (var userControl = new ComplianceRiskPlugInUserControl(pluginBizO))
			{
				form.Controls.Add(userControl);
				form.Show();
				var locationsBlockVisibility = form.Controls.Find("LocationGroupBox", true).FirstOrDefault().Visible;
				var partiesBlockVisibility = form.Controls.Find("PartyGroupBox", true).FirstOrDefault().Visible;
				var commoditiesBlockVisibility = form.Controls.Find("CommodityGroupBox", true).FirstOrDefault().Visible;
				AssertEquals(false, locationsBlockVisibility);
				AssertEquals(false, partiesBlockVisibility);
				AssertEquals(true, commoditiesBlockVisibility);
			}
		}

		public void TestSetRiskStatusBackgroundColor()
		{
			var shipment = (CreateNewShipment as BusinessObject);
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			var complianceRiskStatus = pluginBizO.ComplianceRiskStatus;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			using (var form = new ZForm())
			using (var userControl = new ComplianceRiskPlugInUserControl(pluginBizO))
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.SetRiskLabelBackColor();

				AssertEquals("1.OverallRiskLabel.BackColor", Color.FromArgb(255, 179, 179), userControl.OverallRiskLabel.BackColor);
				AssertEquals("1.PartyRiskLabel.BackColor", Color.FromArgb(198, 236, 198), userControl.PartyRiskLabel.BackColor);
				AssertEquals("1.LocationRiskLabel.BackColor", Color.FromArgb(198, 236, 198), userControl.LocationRiskLabel.BackColor);
				AssertEquals("1.CommodityRiskLabel.BackColor", Color.FromArgb(255, 179, 179), userControl.CommodityRiskLabel.BackColor);

				complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

				userControl.SetRiskLabelBackColor();

				AssertEquals("2.OverallRiskLabel.BackColor", Color.FromArgb(139, 154, 239), userControl.OverallRiskLabel.BackColor);
				AssertEquals("2.PartyRiskLabel.BackColor", Color.FromArgb(198, 236, 198), userControl.PartyRiskLabel.BackColor);
				AssertEquals("2.LocationRiskLabel.BackColor", Color.FromArgb(198, 236, 198), userControl.LocationRiskLabel.BackColor);
				AssertEquals("2.CommodityRiskLabel.BackColor", Color.FromArgb(255, 179, 179), userControl.CommodityRiskLabel.BackColor);

				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;

				userControl.SetRiskLabelBackColor();

				AssertEquals("3.CommodityRiskLabel.BackColor", Color.FromArgb(255, 179, 179), userControl.CommodityRiskLabel.BackColor);

				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PossibleRisk;

				userControl.SetRiskLabelBackColor();

				AssertEquals("4.CommodityRiskLabel.BackColor", Color.FromArgb(255, 210, 165), userControl.CommodityRiskLabel.BackColor);

				form.Dispose();
			}
		}

		public void TestAddUserControl()
		{
			var shipment = CreateNewShipment as BusinessObject;
			var consol = CreateNewConsolidation as BusinessObject;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
				using (var form = new ZForm())
				using (var userControl = new ComplianceRiskPlugInUserControl(pluginBizO))
				{
					form.Controls.Add(userControl);
					form.Show();

					AssertType<PartyRiskUserControl>("PartyRiskUserControl should exist.", userControl.PartyTableLayoutPanel.Controls.Find("PartyRiskUserControl", searchAllChildren: false).Single());
					AssertType<LocationRiskUserControl>("LocationRiskUserControl should exist.", userControl.LocationTableLayoutPanel.Controls.Find("LocationRiskUserControl", searchAllChildren: false).Single());
					Assert("CommodityRiskUserControl should exist.", userControl.CommodityTableLayoutPanel.Controls.Find("CommodityRiskUserControl", searchAllChildren: true).Single() is ICommodityRiskUserControl);
					AssertNotNull("CommodityAssessmentPanelUserControl should exist.", userControl.CommodityTableLayoutPanel.Controls.Find("CommodityAssessmentPanelUserControl", searchAllChildren: true).FirstOrDefault());
					AssertEquals("AssessmentInitializeButton should visible.", true, userControl.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).Single().Visible);
					AssertEquals("CommoditySpinnerIndicator should hidden.", false, userControl.Controls.Find("CommoditySpinnerIndicator", searchAllChildren: true).Single().Visible);

					form.Dispose();
				}

				pluginBizO = new ComplianceRiskPlugInBusinessObject(consol);
				using (var form = new ZForm())
				using (var userControl = new ComplianceRiskPlugInUserControl(pluginBizO))
				{
					form.Controls.Add(userControl);
					form.Show();

					AssertType<PartyRiskUserControl>("PartyRiskUserControl should exist.", userControl.PartyTableLayoutPanel.Controls.Find("PartyRiskUserControl", searchAllChildren: false).Single());
					AssertType<LocationRiskUserControl>("LocationRiskUserControl should exist.", userControl.LocationTableLayoutPanel.Controls.Find("LocationRiskUserControl", searchAllChildren: false).Single());
					AssertNotNull("CommodityAssessmentPanelUserControl should exist.", userControl.CommodityTableLayoutPanel.Controls.Find("CommodityAssessmentPanelUserControl", searchAllChildren: true).FirstOrDefault());
					AssertEquals("AssessmentInitializeButton should hidden.", false, userControl.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).Single().Visible);
					AssertEquals("CommoditySpinnerIndicator should hidden.", false, userControl.Controls.Find("CommoditySpinnerIndicator", searchAllChildren: true).Single().Visible);

					form.Dispose();
				}
			}
		}

		public void TestInitiateAssessmentButtonClick()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var button = form.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
				AssertEquals("AssessmentInitializeButton should be enabled", true, button.Enabled);

				button.PerformClick();

				AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
				AssertEquals("AssessmentInitializeButton should be disabled", false, button.Enabled);
			}
		}

		public void TestInitiateAssessmentButtonClick_WhenSecurityCheckpointNotGranted_ErrorMessageShown()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			var securityCore = new ComplianceRiskHelperTest().CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessages();

				form.Show();

				var pluginBizO = (form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin) as ComplianceRiskPlugIn).GetBusinessObjectForPlugin;
				var button = form.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				button.PerformClick();

				CombineAssertions("Security: Allow Commodity Risk Check should show error message", () =>
				{
					AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
					AssertEquals("AssessmentInitializeButton should be enabled", true, button.Enabled);
					AssertEquals("Compliance Initialize Assessment should be False", false, pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized);
					AssertEquals(securityCore.ShipmentsComplianceAllowComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestInitializeComplianceAssessmentMenuClick()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var button = form.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				var menu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				menu.PerformClick();

				AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
				AssertEquals("AssessmentInitializeButton should be disabled", false, button.Enabled);

				AssertEquals("Initialize Compliance Assessment is visible", true, form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true).Visible);
				AssertEquals("Initialize Compliance Assessment is disabled", false, form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true).Enabled);

				AssertEquals("Decline Compliance Assessment is visible", true, form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true).Visible);
				AssertEquals("Decline Compliance Assessment is disabled", false, form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true).Enabled);
			}
		}

		public void TestDeclineComplianceAssessmentMenuAndThenInitializeComplianceAssessmentMenuClick()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var button = form.Controls.Find("AssessmentInitializeButton", searchAllChildren: true).FirstOrDefault() as ZButton;
				var initializeAssessmentMenu = form.Menu.MenuItems.FindByText("Initialize Compliance Assessment", true);
				var declineAssessmentMenu = form.Menu.MenuItems.FindByText("Decline Compliance Assessment", true);

				declineAssessmentMenu.PerformClick();

				CombineAssertions("Click Decline Compliance Assessment Menu:", () =>
				{
					AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
					AssertEquals("AssessmentInitializeButton should be enabled", true, button.Enabled);

					AssertEquals("Initialize Compliance Assessment is visible", true, initializeAssessmentMenu.Visible);
					AssertEquals("Initialize Compliance Assessment is enabled", true, initializeAssessmentMenu.Enabled);

					AssertEquals("Decline Compliance Assessment is visible", true, declineAssessmentMenu.Visible);
					AssertEquals("Decline Compliance Assessment is disabled", false, declineAssessmentMenu.Enabled);
				});

				initializeAssessmentMenu.PerformClick();

				CombineAssertions("Click Initialize Compliance Assessment Menu:", () =>
				{
					AssertEquals("AssessmentInitializeButton should be visible", true, button.Visible);
					AssertEquals("AssessmentInitializeButton should be disabled", false, button.Enabled);

					AssertEquals("Initialize Compliance Assessment is visible", true, initializeAssessmentMenu.Visible);
					AssertEquals("Initialize Compliance Assessment is disabled", false, initializeAssessmentMenu.Enabled);

					AssertEquals("Decline Compliance Assessment is visible", true, declineAssessmentMenu.Visible);
					AssertEquals("Decline Compliance Assessment is disabled", false, declineAssessmentMenu.Enabled);
				});
			}
		}

		public void TestCommoditySpinnerIndicatorVisibility()
		{
			var shipment = CreateNewShipment as BusinessObject;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

				using var form = new ZForm();
				using var userControl = new ComplianceRiskPlugInUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();

				var spinnerIndicatorControl = userControl.Controls.Find("CommoditySpinnerIndicator", searchAllChildren: true).Single();
				AssertEquals("CommoditySpinnerIndicatorVisibility: False", expected: false, spinnerIndicatorControl.Visible);

				pluginBizO.ComplianceRiskSpinnerIndicatorVisibility.Invoke(true);
				AssertEquals("CommoditySpinnerIndicatorVisibility: True", expected: true, spinnerIndicatorControl.Visible);

				pluginBizO.ComplianceRiskSpinnerIndicatorVisibility.Invoke(false);
				AssertEquals("CommoditySpinnerIndicatorVisibility: False", expected: false, spinnerIndicatorControl.Visible);
			}
		}

		public void TestCheckboxAvailability()
		{
			var shipment = CreateNewShipment as BusinessObject;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

				using var form = new ZForm();
				using var userControl = new ComplianceRiskPlugInUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();

				var partyHideCheckBox = userControl.PartyHideCheckBox;
				var locationHideCheckBox = userControl.LocationHideCheckBox;
				var commodityHideCheckBox = userControl.CommodityHideCheckBox;
				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);
				AssertEquals("CommodityHideCheckBoxEnabled: True", expected: true, commodityHideCheckBox.Enabled);

				partyHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);
				AssertEquals("CommodityHideCheckBoxEnabled: True", expected: true, commodityHideCheckBox.Enabled);

				locationHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);
				AssertEquals("CommodityHideCheckBoxEnabled: False", expected: false, commodityHideCheckBox.Enabled);

				locationHideCheckBox.Checked = false;
				commodityHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: False", expected: false, locationHideCheckBox.Enabled);
				AssertEquals("CommodityHideCheckBoxEnabled: True", expected: true, commodityHideCheckBox.Enabled);

				partyHideCheckBox.Checked = false;
				locationHideCheckBox.Checked = true;
				commodityHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: False", expected: false, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);
				AssertEquals("CommodityHideCheckBoxEnabled: True", expected: true, commodityHideCheckBox.Enabled);

				// when commodity is not visible (domestic)
				userControl.SplitContainer.Panel2Collapsed = true;

				partyHideCheckBox.Checked = false;
				locationHideCheckBox.Checked = false;

				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);

				partyHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: True", expected: true, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: False", expected: false, locationHideCheckBox.Enabled);

				partyHideCheckBox.Checked = false;
				locationHideCheckBox.Checked = true;
				AssertEquals("PartyHideCheckBoxEnabled: False", expected: false, partyHideCheckBox.Enabled);
				AssertEquals("LocationHideCheckBoxEnabled: True", expected: true, locationHideCheckBox.Enabled);
			}
		}

		public void TestSplitterHeight()
		{
			var shipment = CreateNewShipment as BusinessObject;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

				using var form = new ZForm();
				using var userControl = new ComplianceRiskPlugInUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();
				var partyHideCheckBox = userControl.PartyHideCheckBox;
				var locationHideCheckBox = userControl.LocationHideCheckBox;
				var commodityHideCheckBox = userControl.CommodityHideCheckBox;
				var partyAndLocationSplitter = userControl.SubSplitContainer;
				var commodityAndOtherSplitter = userControl.SplitContainer;

				AssertEquals($"SubSplitter.Panel1MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel1MinSize);
				AssertEquals($"SubSplitter.Panel2MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel2MinSize);
				AssertEquals($"SplitContainer.Panel2MinSize should be {userControl.defaultCommodityPanelMinHeight}", expected: userControl.defaultCommodityPanelMinHeight, commodityAndOtherSplitter.Panel2MinSize);

				partyHideCheckBox.Checked = true;
				AssertEquals($"SubSplitter.Panel1MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, partyAndLocationSplitter.Panel1MinSize);
				AssertEquals($"SubSplitter.Panel2MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel2MinSize);
				AssertEquals($"SplitContainer.Panel2MinSize should be {userControl.defaultCommodityPanelMinHeight}", expected: userControl.defaultCommodityPanelMinHeight, commodityAndOtherSplitter.Panel2MinSize);

				locationHideCheckBox.Checked = true;
				AssertEquals($"SubSplitter.Panel1MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, partyAndLocationSplitter.Panel1MinSize);
				AssertEquals($"SubSplitter.Panel2MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, partyAndLocationSplitter.Panel2MinSize);
				AssertEquals($"SplitContainer.Panel2MinSize should be {userControl.defaultCommodityPanelMinHeight}", expected: userControl.defaultCommodityPanelMinHeight, commodityAndOtherSplitter.Panel2MinSize);

				locationHideCheckBox.Checked = false;
				commodityHideCheckBox.Checked = true;
				AssertEquals($"SubSplitter.Panel1MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, partyAndLocationSplitter.Panel1MinSize);
				AssertEquals($"SubSplitter.Panel2MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel2MinSize);
				AssertEquals($"SplitContainer.Panel2MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, commodityAndOtherSplitter.Panel2MinSize);

				partyHideCheckBox.Checked = false;
				AssertEquals($"SubSplitter.Panel1MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel1MinSize);
				AssertEquals($"SubSplitter.Panel2MinSize should be {userControl.defaultPanelMinHeight}", expected: userControl.defaultPanelMinHeight, partyAndLocationSplitter.Panel2MinSize);
				AssertEquals($"SplitContainer.Panel2MinSize should be {userControl.collapsedPanelMinHeight}", expected: userControl.collapsedPanelMinHeight, commodityAndOtherSplitter.Panel2MinSize);
			}
		}

		public void TestShipmentTypeHighVolumeLowValue_HideCommoditySpinnerIndicator()
		{
			var shipment = (CreateNewShipment as BusinessObject);
			(shipment as CommonShipment).JS_RL_NKOrigin = "AUSYD";
			(shipment as CommonShipment).JS_RL_NKDestination = "USORD";

			AssertShipmentTypeHighVolumeLowValue(Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue);
			AssertShipmentTypeHighVolumeLowValue(Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValueMaster);

			void AssertShipmentTypeHighVolumeLowValue(string type)
			{
				(shipment as CommonShipment).JS_ShipmentType = type;

				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

				using var form = new ZForm();
				using var userControl = new ComplianceRiskPlugInUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();
				var locationsBlockVisibility = form.Controls.Find("LocationGroupBox", true).FirstOrDefault().Visible;
				var partiesBlockVisibility = form.Controls.Find("PartyGroupBox", true).FirstOrDefault().Visible;
				var commoditiesBlockVisibility = form.Controls.Find("CommodityGroupBox", true).FirstOrDefault().Visible;
				AssertEquals(true, locationsBlockVisibility);
				AssertEquals(true, partiesBlockVisibility);
				AssertEquals(true, commoditiesBlockVisibility);
				AssertEquals("CommoditySpinnerIndicator should hidden.", false, userControl.Controls.Find("CommoditySpinnerIndicator", searchAllChildren: true).Single().Visible);
			}
		}

		public void TestCommodityFilterDefaultHide_WhenClickShowMenu_CommodityFilterShouldShow()
		{
			var shipment = CreateNewShipment as BusinessObject;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

				using var form = new ZForm();
				using var userControl = new ComplianceRiskPlugInUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();

				var commodityRiskUserControl = userControl.Controls.Find("CommodityAssessmentPanelUserControl", searchAllChildren: true).FirstOrDefault() as CommodityAssessmentPanelUserControl;
				var commodityFilterStrip = commodityRiskUserControl.CommodityFilterStrip;
				AssertEquals("CommodityFilterDefaultVisibility: False", expected: false, commodityFilterStrip.Visible);

				var hideShowMenu = commodityRiskUserControl.HideShowToolStripItem;
				hideShowMenu.PerformClick();
				AssertEquals("CommodityFilterDefaultVisibility: True", expected: true, commodityFilterStrip.Visible);
			}
		}
	}
}

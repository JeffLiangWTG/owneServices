using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskPlugInTest : ComplianceRiskHelperTest
	{
		public void TestComplianceRiskPlugin_ZAlwaysLoadPluginType()
		{
			using (var plugIn = new ComplianceRiskPlugIn((IBusiness)CreateNewShipment))
			{
				AssertEquals(typeof(ZAlwaysLoadPlugIn), plugIn.GetType().BaseType);
			}
		}

		public void TestComplianceRiskPlugin_NewShipmentWithCorrectComplianceRiskStatuses()
		{
			var shipment = CreateShipmentWithValidData();

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				form.FireSaveButton();

				CombineAssertions(() =>
				{
					var complianceRiskStatus = new BusinessObjectFactory().LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
					AssertEquals(shipment.PK, complianceRiskStatus.COR_ParentID);
					AssertEquals((shipment as BusinessObject).TablePrefix, complianceRiskStatus.COR_ParentTableCode);
					AssertEquals("Job compliance status", ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
					AssertEquals("Parties risk status", ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
					AssertEquals("Locations risk status", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
					AssertEquals("Commodities risk status", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceRiskPlugin_NewShipmentWithOverrideClearJobComplianceStatus()
		{
			var shipment = CreateShipmentWithValidData();

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Unknown;
			complianceRiskStatus.CopyFromBooking = true;
			complianceRiskStatus.CopyFromBookingFirstLoaded = true;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = (shipment as BusinessObject).TablePrefix;

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				plugIn.OnUserControlShown();

				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRiskPlugin_NotSynchronizeWhenFormShowAndJobIsNotCurrent()
		{
			var shipment = CreateShipmentWithValidData();

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			shipment.JS_E_ARV = ZDateTime.Now.AddDays(-10);

			Factory.Save();
			AssertEquals("Pre-requisite", false, ((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();
				CombineAssertions("Compliance Risk Status has not been resynchronized.", () =>
				{
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
				});

				form.TabControl.SelectNextTabPage();
				CombineAssertions("Compliance Risk Status has been resynchronized when switch to Compliance Tab.", () =>
				{
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				});
			}
		}

		public void TestComplianceWithDefaultValueWhenFormShowAndJobIsNotCurrent()
		{
			var shipment = CreateShipmentWithValidData();
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(-10);

			Factory.Save();
			AssertEquals("Pre-requisite", false, ((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
				AssertEquals("Shipment compliance risk status has been created", true, complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		public void TestComplianceRiskPlugIn_UserControl()
		{
			using (var plugIn = new ComplianceRiskPlugInForTest((IBusiness)CreateNewShipment))
			{
				AssertType<ComplianceRiskPlugInUserControl>(plugIn.UserControl);
			}
		}

		public void TestComplianceRiskPlugIn_IsDockedToFill()
		{
			using (var plugIn = new ComplianceRiskPlugInForTest((IBusiness)CreateNewShipment))
			{
				AssertEquals(DockStyle.Fill, plugIn.UserControl.Dock);
			}
		}

		public void TestGetJobUniqueRef()
		{
			var shipment = CreateNewShipment;
			shipment.JS_UniqueConsignRef = "JS_UniqueConsignRef";

			using (var plugIn = new ComplianceRiskPlugIn((IBusiness)shipment))
			{
				AssertEquals("JS_UniqueConsignRef", plugIn.GetJobUniqueRef());
			}
		}

		#region Menus

		public void TestTopLevelMenu()
		{
			AssertCreatedMenu(CreateNewShipment, new[]
				{
					"Override Compliance Risk",
					"Resynchronize Compliance Risk Status",
					"View Party Risk",
					"Initialize Compliance Assessment",
					"Decline Compliance Assessment",
					"Legacy Screening Status"
				});
		}

		public void TestTopLevelMenu_WithoutCommodityProvider()
		{
			AssertCreatedMenu(Factory.New<ShipmentWithoutCommodityProvider>(), new[]
				{
					"Override Compliance Risk",
					"Resynchronize Compliance Risk Status",
					"View Party Risk",
					"Legacy Screening Status"
				});
		}

		public void TestTopLevelMenu_WithoutPartyProvider()
		{
			AssertCreatedMenu(Factory.New<ShipmentOnlyWithCommodityProvider>(), new[]
				{
					"Override Compliance Risk",
					"Resynchronize Compliance Risk Status",
					"Initialize Compliance Assessment",
					"Decline Compliance Assessment",
					"Legacy Screening Status"
				});
		}

		public void TestTopLevelMenu_Declaration()
		{
			AssertCreatedMenu(Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>(), new[]
			{
				"Override Compliance Risk",
				"Resynchronize Compliance Risk Status",
				"View Party Risk",
				"Initialize Compliance Assessment",
				"Decline Compliance Assessment",
				"Legacy Screening Status"
			});
		}

		void AssertCreatedMenu(object datasource, string[] expectedVisibleMenuNames)
		{
			using (var form = new ComplianceRiskPluginParentFormForTest(datasource))
			{
				form.Show();
				var complianceRisk = form.TopLevelMenu.MenuItems.FindByText("Compliance Risk", true);

				AssertEquals(6, complianceRisk.MenuItems.Count);
				AssertEquals("Compliance Risk", complianceRisk.Text);
				AssertContainsExactElementsInAnyOrder(expectedVisibleMenuNames, complianceRisk.MenuItems.Cast<MenuItem>().Where(o => o.Visible).Select(u => u.Text));
			}
		}

		public void TestComplianceAssessmentMenu_WithCommodityScreeningFeature()
		{
			AssertComplianceAssessmentMenu_WithCommodityScreeningFeature(true, true);
			AssertComplianceAssessmentMenu_WithCommodityScreeningFeature(false, false);

			void AssertComplianceAssessmentMenu_WithCommodityScreeningFeature(bool enableCommodityScreening, bool expected)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				Factory.Save();

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
				{
					form.Show();
					var initializeMenu = form.TopLevelMenu.MenuItems.FindByText("Initialize Compliance Assessment", true);
					var declineMenu = form.TopLevelMenu.MenuItems.FindByText("Decline Compliance Assessment", true);

					AssertNotNull(initializeMenu);
					AssertNotNull(declineMenu);
					AssertEquals(expected, initializeMenu.Enabled);
					AssertEquals(expected, declineMenu.Enabled);
				}
			}
		}

		public void TestLegacyDPSMenuItem_Click()
		{
			var booking = CreateNewBookingQuick;
			Factory.Save();

			using (var form = new ComplianceRiskPluginParentFormForTest(booking))
			{
				form.Show();

				(booking.ForwardingShipment as IScreeningStatusProvider).ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				form.TopLevelMenu.MenuItems.FindByText("Legacy Screening Status", true).PerformClick();
				AssertEquals($"Screening Status of {CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)booking)} is CLR - Clear", UnitTestUserNotification.Instance.LastMessage.Text);

				(booking.ForwardingShipment as IScreeningStatusProvider).ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				form.TopLevelMenu.MenuItems.FindByText("Legacy Screening Status", true).PerformClick();
				AssertEquals($"Screening Status of {CodePropertyAttribute.CodeFromBusinessObject(booking.ForwardingShipment)} is UNK - Unknown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldHideTopLevelMenuWithTabIsFalse()
		{
			using (var plugIn = new ComplianceRiskPlugIn((IBusiness)CreateNewShipment))
			{
				AssertEquals("This one must be false, we don't want to hide the actions menu when compliance risk tab hidden", expected: false, plugIn.ShouldHideTopLevelMenuWithTab);
			}
		}

		public void TestCreateHelperRegisterInteractEventIfNeeded()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			using (var plugIn = new ComplianceRiskPlugIn((IBusiness)CreateNewShipment))
			{
				var pluginBizO = plugIn.GetBusinessObjectForPlugin;
				AssertNull(pluginBizO.AssessmentHelper);
				AssertNull(pluginBizO.GetSupportInteractionWithComplianceWiseCommodities);
			}

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			using (var plugIn = new ComplianceRiskPlugIn(CreateNewBaseJobDeclaration))
			{
				var pluginBizO = plugIn.GetBusinessObjectForPlugin;
				AssertNotNull(pluginBizO.AssessmentHelper);
				AssertNotNull(pluginBizO.GetSupportInteractionWithComplianceWiseCommodities);
			}
		}

		#endregion

		public void TestComplianceRiskPlugIn_SynchronizeAndSaveWhenPlugInLoad()
		{
			var shipment = CreateNewShipment;
			Factory.Save();

			using (_ = new ComplianceRiskPlugInForTest((IBusiness)shipment))
			{
				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, (shipment as BusinessObject).PK));
				Assert("Shipment compliance risk status must be created", complianceRiskStatus.IsInDatabase);
			}
		}

		public void TestComplianceRiskPlugIn_SynchronizeOnSaving()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			(shipment as CommonShipment).ConsigneePK = consignee.PK;

			Factory.Save();
			TestConnection.ExecuteNonQuery($@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'");

			using (var form = new ComplianceRiskPluginParentFormForTest((IBusiness)shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, (shipment as BusinessObject).PK));
				Assert("Shipment compliance risk status must be created", complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;
				consignor.OH_Code = "Consignor";
				(shipment as CommonShipment).ConsignorPK = consignor.PK;

				plugIn.OnSaving();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
			}
		}

		public void TestComplianceRiskPlugIn_SynchronizeOnUserControlShown()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			(shipment as CommonShipment).ConsigneePK = consignee.PK;

			Factory.Save();
			TestConnection.ExecuteNonQuery($@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'");

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, (shipment as BusinessObject).PK));
				Assert("Shipment compliance risk status must be created", complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;
				consignor.OH_Code = "Consignor";
				(shipment as CommonShipment).ConsignorPK = consignor.PK;

				plugIn.OnUserControlShown();
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
			}
		}

		public void TestTabPageOnInitializeImageIsRedAlamBell_WhenOverallRiskIsPotentialRisk()
		{
			AssertTabPageRedAlamBellIsVisible("PSK");
		}

		public void TestTabPageOnInitializeImageIsRedAlamBell_WhenOverallRiskIsBlocked()
		{
			AssertTabPageRedAlamBellIsVisible("BLK");
		}

		public void TestTabPageOnInitializeImageIsRedAlamBell_WhenOverallRiskIsHeld()
		{
			AssertTabPageRedAlamBellIsVisible("HLD");
		}

		void AssertTabPageRedAlamBellIsVisible(string status)
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

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = TariffDummy.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = TariffDummy.ZZ1_TariffCode;

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var tabPage = new ZTabPage())
			using (var tabControl = new ZTemplateTabControl())
			using (var form = new ZForm(shipment))
			{
				tabControl.Controls.Add(tabPage);
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				form.Show();

				complianceRiskStatus.COR_OverallRisk = status;
				AssertEquals("Compliance Risk tab image is Red Alarm Bell icon.", Icons.GetImageIndex(IconTypes.RedAlarmBell), tabControl.TabPages[1].ImageIndex);

				form.Close();
			}
		}

		public void TestTabPageOnInitializeImageIsEmpty_WhenOverallRiskIsClear()
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

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = TariffDummy.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = TariffDummy.ZZ1_TariffCode;

			Factory.Save();

			TestConnection.ExecuteNonQuery($@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignor.PK}'");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var tabControl = new ZTemplateTabControl())
			using (var form = new ZForm(shipment))
			{
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				form.Show();

				((ZTabPagePlugIn)tabControl.SelectedTab).PlugIn.OnUserControlShown();

				AssertEquals("Compliance Risk tab image is empty.", -1, tabControl.TabPages[0].ImageIndex);
			}
		}

		public void TestTabPageOnShownImageIsEmpty_WhenDataChangesInDB()
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

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = TariffDummy.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = TariffDummy.ZZ1_TariffCode;

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				AssertEquals("Compliance Risk tab image is Red Alarm Bell icon.", Icons.GetImageIndex(IconTypes.RedAlarmBell), ((ZTabPagePlugIn)form.TabControl.SelectedTab).ImageIndex);

				TestConnection.ExecuteNonQuery($@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignor.PK}'");

				form.PlugIns.Instances[0].OnUserControlShown();

				AssertEquals("Compliance Risk tab image is empty.", -1, ((ZTabPagePlugIn)form.TabControl.SelectedTab).ImageIndex);
				AssertEquals("Overall risk is Clear.", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestTabPageOnShownImageIsEmpty_WhenMenuOverrideClear()
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

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Incomplete;

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("Compliance Risk tab image is Red Alarm Bell icon.", Icons.GetImageIndex(IconTypes.RedAlarmBell), ((ZTabPagePlugIn)form.TabControl.SelectedTab).ImageIndex);
					AssertEquals("Overall risk is Potential Risk.", ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.TopLevelMenu.MenuItems.FindByText("Override Compliance Risk", true).PerformClick();

				AssertEquals("Compliance Risk tab image is empty.", -1, ((ZTabPagePlugIn)form.TabControl.SelectedTab).ImageIndex);
				AssertEquals("Overall risk is Override Clear.", ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestOnDataChangedInDB_WithSynchronizeDataAndNoDuplicateStmALog()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneePK = consignee.PK;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_JobEndDate = ZDateTimeOffset.Today;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_CountryOrGrouping = TariffDummy.ZZ1_ZZZ_NKDataGrouping;
			commodity.CCD_HarmonizedCode = TariffDummy.ZZ1_TariffCode;

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			Factory.Save();

			CombineAssertions("Pre-Condition:", () =>
			{
				var stmALogs = GetLogs();
				AssertEquals("Initial Compliance risk logs should not exist", 0, stmALogs.Length);
				AssertEquals("Initial Party risk should not exist", 0, stmALogs.Where(l => l.SL_Reference.Contains("TYP=Party risk status")).Count());
				// Change to Job compliance status in WI00828592 - CPW: Handle new risk status - Stored Procedure
				AssertEquals("Initial Overall risk should not exist", 0, stmALogs.Where(l => l.SL_Reference.Contains("TYP=Overall compliance risk status")).Count());
			});

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ZForm(shipment))
			{
				var newHeaderFactory = new BusinessObjectFactory();
				var headerInNewFactory = newHeaderFactory.Load<OrgHeader>(shipment.ConsigneePK);
				headerInNewFactory.OH_ScreeningStatus = "CLR";
				headerInNewFactory.Factory.Save();

				ScreeningStatusUpdater.UpdateRelatedJobs(headerInNewFactory);

				CombineAssertions("Organization has been updated in new BusinessObjectFactory:", () =>
				{
					var stmALogs = GetLogs();
					AssertEquals("New Party risk created and log count 1", 1, stmALogs.Where(l => l.SL_Reference.Contains("TYP=Party risk status")).Count());
					AssertEquals("New Overall risk created and log count 1", 1, stmALogs.Where(l => l.SL_Reference.Contains("TYP=Job compliance status")).Count());
					AssertEquals("Total Compliance risk log count 2", 2, stmALogs.Length);
				});

				shipment.JS_HouseBill = "CRT27062023";

				form.PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin).OnSaving();

				Factory.Save();
				form.Close();
			}

			CombineAssertions("Shipment has been updated:", () =>
			{
				var stmALogs = GetLogs();
				AssertEquals("Should update house bill", "CRT27062023", shipment.JS_HouseBill);
				AssertEquals("After job data changed should not create logs", 4, stmALogs.Length);
			});

			StmALog[] GetLogs()
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, shipment.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "|MST=Compliance Risk");
				return new BusinessObjectFactory().Load<StmALog>(query);
			}
		}

		public void TestOnDataChangedInDB_WithNoSaveConcurrencyExceptionThrown()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);

			(shipment as CommonShipment).ConsigneePK = consignee.PK;

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				TestConnection.ExecuteNonQuery($@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'");

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new ZForm(shipment))
				{
					shipment.JS_HouseBill = "CRT23062023";

					form.PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
					form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin).OnSaving();
					form.FireSaveButton();

					Factory.Save();
					form.Close();
				}
			});
		}

		public void TestBookingSpotQuote_WhenConverToBookingWithQuote_NoExceptionThrown()
		{
			var bookingSpotQuote = CreateNewBookingSpotQuote;

			Factory.Save();

			var booking = Factory.New<CommonShipment>();
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsCFSRegistered = false;
			booking.JS_IsShipping = false;
			booking.JS_IsBooking = true;

			AssertNoExceptionThrown(() =>
			{
				var bookingWithQuote = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(((BusinessObject)bookingSpotQuote).PK, booking.PK, Factory);
				using (var form = new ComplianceRiskPluginParentFormForTest(bookingWithQuote as IBusiness))
				{
					form.Show();
					form.PlugIns.Instances[0].OnSaving();
				}
			});
		}

		public void TestCommodityGroupBoxVisibilityAndEnabledOnUserControlShown_Shipment_Domestic()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				plugIn.OnUserControlShown();
				AssertEquals("CommodityGroupBox is Invisible", false, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);
				AssertEquals("SplitContainer.Panel2Collapsed is True", true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);
				AssertEquals("CommodityRiskUserControl is Enabled", true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Find(u => u.Name == "CommodityRiskUserControl").First().Enabled);

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		public void TestCommoditySectionVisible_WithCommodityScreeningFeature()
		{
			AssertCommoditySectionVisible(false, false, false);
			AssertCommoditySectionVisible(true, false, true);
			AssertCommoditySectionVisible(false, true, true);
			AssertCommoditySectionVisible(true, true, true);

			void AssertCommoditySectionVisible(bool enableCommodityScreening, bool isAssessmentUsed, bool expected)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				if (isAssessmentUsed)
				{
					var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
					complianceRiskStatus.COR_ParentID = shipment.PK;
					complianceRiskStatus.COR_ParentTableCode = "JS";

					StmComplianceEventHelperTest.CreateAssessmentEvent(shipment as BusinessObject, ComplianceEventList.Codes.AssessmentInitialized);
				}

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
				{
					form.Show();
					var plugIn = form.PlugIns.Instances[0];
					plugIn.OnUserControlShown();

					AssertEquals(expected, !((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);

					var button = ((ComplianceRiskPlugInUserControl)plugIn.UserControl).AssessmentInitializeButton;
					AssertEquals("AssessmentInitializeButton: ", expected, button.Visible);
					AssertEquals("Commodity Section: ", expected, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);
				}
			}
		}

		public void TestCommoditySectionVisible_ConsolWithCommodityScreeningFeature()
		{
			AssertConsolCommoditySectionVisible(false, false, false);
			AssertConsolCommoditySectionVisible(true, false, true);
			AssertConsolCommoditySectionVisible(false, true, true);
			AssertConsolCommoditySectionVisible(true, true, true);

			void AssertConsolCommoditySectionVisible(bool enableCommodityScreening, bool hasShipmentAssessment, bool expected)
			{
				var consol = (ForwardingConsol)CreateNewConsolidation;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				if (hasShipmentAssessment)
				{
					var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
					var shipmentRiskStatus = Factory.New<ComplianceRiskStatus>();
					shipmentRiskStatus.COR_ParentID = shipment.PK;
					shipmentRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
					consol.Shipments.Add(shipment);

					StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
				}

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				using (var form = new ComplianceRiskPluginParentFormForTest(consol))
				{
					form.Show();
					var plugIn = form.PlugIns.Instances[0];
					plugIn.OnUserControlShown();

					AssertEquals(expected, !((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);

					var button = ((ComplianceRiskPlugInUserControl)plugIn.UserControl).AssessmentInitializeButton;
					AssertEquals(false, button.Visible);
					AssertEquals("Commodity Section: ", expected, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);
				}
			}
		}

		public void TestCommodityGroupBoxVisibilityAndEnabledOnUserControlShown_Shipment_International()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				plugIn.OnUserControlShown();
				AssertEquals(true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);
				AssertEquals(false, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);
				AssertEquals(true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Find(u => u.Name == "CommodityRiskUserControl").First().Enabled);

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		public void TestImportAlertsForExportDeclaration()
		{
			AssertImportAlertsForExportDeclaration(true, true, CommodityRiskCalculateFactor.Export, true);
			AssertImportAlertsForExportDeclaration(false, true, CommodityRiskCalculateFactor.Export, false);
			AssertImportAlertsForExportDeclaration(true, false, CommodityRiskCalculateFactor.Export, false);
			AssertImportAlertsForExportDeclaration(true, true, CommodityRiskCalculateFactor.All, false);
			AssertImportAlertsForExportDeclaration(true, true, CommodityRiskCalculateFactor.Import, false);

			void AssertImportAlertsForExportDeclaration(bool enableFeature, bool enableShowImportAlerts, CommodityRiskCalculateFactor factor, bool expectShowImportAlerts)
			{
				var declaration = Factory.New<DeclarationWithProvider>();
				declaration.SetRiskCalculateFactor = factor;

				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(enableFeature))
				using (OrganisationsDataRegistry.Instance.CustomsShowImportAlertsOnExportDeclarations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableShowImportAlerts))
				using (var form = new ComplianceRiskPluginParentFormForTest(declaration))
				{
					form.Show();
					var plugIn = form.PlugIns.Instances[0];
					plugIn.OnUserControlShown();

					var commodityRiskGrid = plugIn.UserControl.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
					AssertNotNull(commodityRiskGrid);

					var importAlertsColumn = commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == "ImportAlertsForExportJobDescription");
					AssertEquals(expectShowImportAlerts, importAlertsColumn != null);
					if (expectShowImportAlerts)
					{
						AssertEquals(true, importAlertsColumn.IsVisible);
					}
				}
			}
		}

		public void TestCommodityGroupBoxVisibilityAndEnabledOnUserControlShown_Consolidation_Domestic()
		{
			var consol = CreateNewConsolidation;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";

			using (var form = new ComplianceRiskPluginParentFormForTest(consol))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				plugIn.OnUserControlShown();
				AssertEquals(false, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, consol.PK));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		public void TestCommodityGroupBoxVisibilityAndEnabledOnUserControlShown_Consolidation_International()
		{
			var consol = CreateNewConsolidation;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			using (var form = new ComplianceRiskPluginParentFormForTest(consol))
			{
				form.Show();
				var plugIn = form.PlugIns.Instances[0];
				plugIn.OnUserControlShown();
				AssertEquals("CommodityGroupBox is Visible", true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Visible);
				AssertEquals("SplitContainer.Panel2Collapsed is False", false, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);
				AssertEquals("CommodityRiskUserControl is Enabled", true, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).CommodityGroupBox.Find(u => u.Name == "CommodityRiskUserControl").First().Enabled);

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, consol.PK));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		public void TestCommodityAssessmentPanelUserControlShown_JobDirection_DomesticAndInternational()
		{
			AssertOnUserControlShown("AUSYD", "AUMEL", expecetdPanelCollapsed: true, "Domestic Job:");
			AssertOnUserControlShown("AUSYD", "USLAX", expecetdPanelCollapsed: false, "International Job:");

			void AssertOnUserControlShown(string origin, string destination, bool expecetdPanelCollapsed, string extraMessage)
			{
				var shipment = CreateNewShipment;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;

				using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
				{
					form.Show();
					var plugIn = form.PlugIns.Instances[0];
					plugIn.OnUserControlShown();
					AssertEquals(extraMessage, expecetdPanelCollapsed, ((ComplianceRiskPlugInUserControl)plugIn.UserControl).SplitContainer.Panel2Collapsed);
				}
			}
		}

		public void TestRiskFactorCheckboxCheckedValue_WhenPointPairChanges()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			using (var plugIn = new ComplianceRiskPlugIn((IBusiness)shipment))
			{
				var userControl = ((ComplianceRiskPlugInUserControl)plugIn.UserControl);
				plugIn.OnUserControlShown();

				userControl.PartyHideCheckBox.Checked = true;

				plugIn.OnUserControlShown();

				AssertEquals("PartyHideCheckBox should be checked.", true, userControl.PartyHideCheckBox.Checked);
				AssertEquals("LocationHideCheckBox should NOT be checked.", false, userControl.LocationHideCheckBox.Checked);
				AssertEquals("CommodityHideCheckBox should NOT be checked.", false, userControl.CommodityHideCheckBox.Checked);

				userControl.LocationHideCheckBox.Checked = true;
				userControl.CommodityHideCheckBox.Checked = true;

				plugIn.OnUserControlShown();

				AssertEquals("PartyHideCheckBox should be checked.", true, userControl.PartyHideCheckBox.Checked);
				AssertEquals("LocationHideCheckBox should be checked.", true, userControl.LocationHideCheckBox.Checked);
				AssertEquals("CommodityHideCheckBox should be checked.", true, userControl.CommodityHideCheckBox.Checked);

				// update Domestic to International
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				plugIn.OnUserControlShown();

				AssertEquals("PartyHideCheckBox should NOT be checked.", false, userControl.PartyHideCheckBox.Checked);
				AssertEquals("LocationHideCheckBox should NOT be checked.", false, userControl.LocationHideCheckBox.Checked);
				AssertEquals("CommodityHideCheckBox should NOT be checked.", false, userControl.CommodityHideCheckBox.Checked);
			}
		}

		public void Test_NewShipment_OnFactorySaved_UpdateJobSourceNumberIfNeeded()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, shipment.PK));
				var tariff = complianceRiskStatus.CommodityDetailCollection.AddNew();
				tariff.CCD_HarmonizedCode = "123456";
				complianceRiskStatus.CommodityDetailCollection.Load();
				AssertEquals(ZString.Empty, tariff.Source);

				Factory.Save();
				AssertEquals(shipment.JS_UniqueConsignRef, tariff.Source);
			}
		}

		public void TestOnFormLoadInitializedComplianceMaterialChangeSnapshot()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ComplianceRiskPluginParentFormForTest(shipment))
			{
				form.Show();
				var pluginBizO = ((ComplianceRiskPlugIn)form.PlugIns.Instances[0]).GetBusinessObjectForPlugin;
				AssertEquals(typeof(ComplianceCheckRequestModel), pluginBizO.ComplianceMaterialChangesSnapshot.GetType());
			}
		}

		public void TestHookEventsProcessMaterialChangeIfNeeded()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_HarmonizedCode = "123456";

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("CCD_RiskStatus: NCH", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);
				AssertEquals("CCD_NomenclatureCondition: false", false, commodity.CCD_NomenclatureCondition);
				AssertEquals("CCD_SpecificCondition: false", false, commodity.CCD_SpecificCondition);
			});

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", commoditySpecificConditionsApply: true, nomenclatureWideConditionsApply: false, Factory)))
			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();
				form.FireSaveButton();
				AssertEquals("Commodity risk status HSK", ComplianceRiskStatusCodeList.Codes.HighRisk, commodity.CCD_RiskStatus);
			}
		}

		public void TestComplianceAssessmentInitializedByComplianceRule()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "1234";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity1 = Factory.New<ComplianceCommodityDetail>();
			commodity1.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity1.CCD_HarmonizedCode = "123456";
			var commodity2 = Factory.New<ComplianceCommodityDetail>();
			commodity2.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity2.CCD_HarmonizedCode = "654321";

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("CCD_RiskStatus: NCH", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals("CCD_RiskStatus: NCH", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			});

			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("654321", commoditySpecificConditionsApply: false, nomenclatureWideConditionsApply: false, Factory)))
			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();
				form.FireSaveButton();
				AssertEquals(true, complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity2.CCD_RiskStatus);
			}
		}

		#region BorderWise Check

		public void TestTriggerCommodityRiskStatusCheck_OnFormLoad_OnUserControlShown()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_HarmonizedCode = "123456";

			StmComplianceEventHelperTest.CreateAssessmentEvent((BusinessObject)shipment, ComplianceEventList.Codes.AssessmentInitialized);

			Factory.Save();

			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", false, false, Factory)))
			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();
				AssertEquals("Trigger status check on form loaded", ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);

				shipment.JS_RL_NKDestination = "NZAKL"; // Invalid response cache
				using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", true, false, Factory)))
				{
					form.TabControl.SelectNextTabPage();
					AssertEquals("Trigger status check on user control show", ComplianceRiskStatusCodeList.Codes.HighRisk, commodity.CCD_RiskStatus);
				}
			}
		}

		public void TestTriggerValidation_OnFormLoad_OnUserControlShown()
		{
			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			var complianceEvent = Factory.NewWithValidTestData<StmComplianceEvent>();
			complianceEvent.SCE_ParentID = shipment.PK;
			complianceEvent.SCE_ParentTableCode = "JS";
			complianceEvent.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			complianceEvent.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;

			var complianceRiskStatus = CreateNewComplianceRiskStatus((BusinessObject)shipment);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var commodity = Factory.New<ComplianceCommodityDetail>();
			commodity.CCD_COR_ComplianceRisk = complianceRiskStatus.PK;
			commodity.CCD_HarmonizedCode = "123456";

			Factory.Save();
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.Clear, commodity.CCD_RiskStatus);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BorderWiseApiHelper.SetResponse(CommodityRiskStatusBorderWiseCheckerTest.GetResponse("123456", true, false, Factory, null, null, false)))
			using (var form = new DummyFormForBorderWiseTest(shipment))
			{
				form.Show();
				AssertNoRowErrorContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);

				form.TabControl.SelectNextTabPage();
				AssertHasRowWarningContaining(commodity, ComplianceCommodityDetailValidationReal.GetMessages.UnsupportedHarmonizedMessage);
			}
		}

		class DummyFormForBorderWiseTest : ZForm
		{
			public DummyFormForBorderWiseTest(object dataSource) : base(dataSource)
			{
			}

			public ZTemplateTabControl TabControl { get; } = new ZTemplateTabControl();

			protected override ZTabControl TopLevelTabControl => TabControl;

			protected override void InitialiseForm()
			{
				base.InitialiseForm();

				var tabPage1 = new ZTabPage();
				tabPage1.Name = "DummyFirstTab";
				TopLevelTabControl.TabPages.Add(tabPage1);

				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				Controls.Add(TabControl);
			}
		}

		#endregion

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			TariffDummy = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "072311");
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}

		TariffView TariffDummy;

		Forwarding.IForwardingShipment CreateShipmentWithValidData()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = CreateNewShipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			(shipment as CommonShipment).ConsigneePK = consignee.PK;
			(shipment as CommonShipment).ConsignorPK = consignor.PK;

			return shipment;
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukConsolPluginTestsUsingMultiMawbPlugin : TestCaseWithFactory
	{
		public void TestPluginToConsolDoestShowEmptySplitsTabOrHouseTabForDirect()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "12387654321";
			consol.JK_RL_NKDischargePort = "GBLHR";
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "12387654321";
			mAWB.Profile = "CUKAIR98LHRXXX";
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab is visible for a basic", 1, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab not visible, no splits exist", 0, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
					AssertEquals("Houses tab visible, this is a basic in the DB but it's not locked so we can add a house", 1, plugin.UserControl.Controls.Find("HousesTabPage", true).Length);
				}
			}

			mAWB.Splits.AddNew();
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab is visible for a basic with splits", 1, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab visible, splits exist", 1, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
					AssertEquals("Houses tab not visible, this is a basic with splits", 0, plugin.UserControl.Controls.Find("HousesTabPage", true).Length);
				}
			}

			mAWB.Splits.RemoveAndDeleteAll();
			mAWB.ChildBills.AddNew();
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab is invisible for a consolidation", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab not visible, no splits exist", 0, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
					AssertEquals("Houses tab visible, this is a consol with one house", 1, plugin.UserControl.Controls.Find("HousesTabPage", true).Length);
				}
			}

			mAWB.ChildBills.RemoveAndDeleteAll();
			mAWB.SetCustomsActionCode("CW", ZDateTime.BrettsBirthday);
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab is visible for a locked basic", 1, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab not visible, no splits exist", 0, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
					AssertEquals("Houses tab is invisible, this basic has status 3", 0, plugin.UserControl.Controls.Find("HousesTabPage", true).Length);
				}
			}

			mAWB.Profile = "CUKFFW98000YYY";
			Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab is hidden for an agent basic", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
				}
			}
		}

		//public void TestMakeNewMawb()
		//{
		//    consol = Factory.New<ForwardingConsol>();
		//    consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
		//    consol.JK_RL_NKLoadPort = "AUSYD";
		//    consol.JK_RL_NKDischargePort = "GBMAN";
		//    var shipment = consol.Shipments.AddNew();

		//    using (var consolForm = new ConsolForm(consol))
		//    {
		//        consolForm.Show();
		//        using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
		//        {
		//            TestPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
		//            AssertNotNull("Master bill loaded for consol plugin", TestPlugIn.CusMawbExposed);
		//            AssertEquals(consol.PK, TestPlugIn.CusMawbExposed.CM_JK);
		//            GBCustomsDataRegistry.Instance.CcsukAutoPopulateHawbsOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		//            TestPlugIn.OnGUIShown();
		//            AssertEquals("Autocreate disabled in rego, no hawb created", 0, TestPlugIn.CusMawbExposed.ChildBills.Count);
		//            GBCustomsDataRegistry.Instance.CcsukAutoPopulateHawbsOnConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		//            consol.JK_AgentType = "DRT";
		//            TestPlugIn.OnGUIShown();
		//            AssertEquals("No hawb created for direct consol's fake shipment", 0, TestPlugIn.CusMawbExposed.ChildBills.Count);
		//            consol.JK_AgentType = "AGT";
		//            TestPlugIn.OnGUIShown();
		//            AssertEquals("One hawb autocreated", 1, TestPlugIn.CusMawbExposed.ChildBills.Count);
		//            AssertEquals(shipment.PK, TestPlugIn.CusMawbExposed.ChildBills[0].CS_JS);
		//            TestPlugIn.OnGUIShown();
		//            AssertEquals("Not recreated", 1, TestPlugIn.CusMawbExposed.ChildBills.Count);
		//        }
		//    }
		//}

		public void TestPluginShownOnlyWhenRegistrySet()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBMAN";
			consol.JK_MasterBillNum = "12387654321";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertEquals("Should be plugged in", true, consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory).Enabled);
			}
		}

		public void TestSyncShedFromConsol()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
			Factory.Save();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCTO.OH_RL_NKClosestPort = "GBLHR";

			var addressLHR = arrivalCTO.Addresses.AddNewMainAddress();
			addressLHR.OA_Address1 = "HEATHROW";
			addressLHR.OA_City = "CTY";
			addressLHR.OA_PostCode = "PC";
			addressLHR.OA_State = "ST";

			var addressLGW = arrivalCTO.Addresses.AddNewMainAddress();
			addressLGW.OA_Address1 = "GATWICK";
			addressLGW.OA_City = "CTY";
			addressLGW.OA_PostCode = "PC";
			addressLGW.OA_State = "ST";

			var customsCodeShedAtLHR = arrivalCTO.CustomsCodes.AddNew();
			customsCodeShedAtLHR.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			customsCodeShedAtLHR.OK_CodeType = OrgCusCode.UnitedKingdomCodeTypes.CTOShed;
			customsCodeShedAtLHR.OK_OA_PremisesAddress = addressLHR.PK;
			customsCodeShedAtLHR.OK_CustomsRegNo = "BAC";

			var customsCodeShedAtLGW = arrivalCTO.CustomsCodes.AddNew();
			customsCodeShedAtLGW.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			customsCodeShedAtLGW.OK_CodeType = OrgCusCode.UnitedKingdomCodeTypes.CTOShed;
			customsCodeShedAtLGW.OK_OA_PremisesAddress = addressLGW.PK;
			customsCodeShedAtLGW.OK_CustomsRegNo = "XXX";

			consol.JK_OA_ArrivalCTOAddress = addressLGW.PK;
			consol.JK_MasterBillNum = "Test";
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.CM_MAWB = "";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					Assert("Pre-req", plugin.Enabled);
					AssertEquals("Pre-req", "", mAWB.CM_MAWB);
					AssertEquals("Pre-req", "", mAWB.CargoTerminalOperator);
					plugin.OnSaving();
					AssertEquals("Test", mAWB.CM_MAWB);
					AssertEquals("", mAWB.CargoTerminalOperator);
				}
			}

			var consol2 = (ForwardingConsol)consol.Clone();
			consol2.JK_OA_ArrivalCTOAddress = addressLHR.PK;
			consol2.JK_MasterBillNum = "Test2";
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol2.PK;
			mAWB.CM_MAWB = "";
			using (var consolForm = new ConsolForm(consol2))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					Assert("Pre-req", plugin.Enabled);
					AssertEquals("Pre-req", "", mAWB.CM_MAWB);
					AssertEquals("Pre-req", "", mAWB.CargoTerminalOperator);
					plugin.OnSaving();
					AssertEquals("Test2", mAWB.CM_MAWB);
					AssertEquals("BAC", mAWB.CargoTerminalOperator);
				}
			}
		}

		public void TestNoPluginWhenConsolIsDomestic()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "GBMAN";
			AssertEquals("Pre-req: consol is domestic", true, consol.IsDomestic());
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "12387654321";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					Assert("No active plugin", !plugin.Enabled);
				}
			}

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBMAN";
			AssertEquals("Pre-req: consol is not domestic", false, consol.IsDomestic());
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					Assert("Plugin is active", plugin.Enabled);
				}
			}
		}

		public void TestGetNewTopLevelMenuWhenViewOnly()
		{
			consol = Factory.New<ForwardingConsol>();
			mAWB = Factory.New<CusMAWB>();
			consol.JK_TransportMode = "AIR";
			mAWB.CM_MAWB = "12387654321";
			consol.JK_MasterBillNum = "12387654321";
			consol.JK_RL_NKDischargePort = "GBLHR";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					var menu = (CcsukMenu)plugin.TopLevelMenu;
					menu.RefreshMenu();
					AssertEquals(1, menu.MenuItems.Count);
					consolForm.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit;
					menu.RefreshMenu();
					Assert(menu.MenuItems.Count > 1);
				}
			}
		}

		public void TestOnSaving()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "GBYYY";
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "Ziggy";
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.CM_MAWB = "Stardust";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					mAWB.CM_MAWB = "";
					plugin.OnSaving();
					AssertEquals("Ziggy", mAWB.CM_MAWB);
				}
			}
		}

		CusMAWB mAWB;
		ForwardingConsol consol;
	}
}

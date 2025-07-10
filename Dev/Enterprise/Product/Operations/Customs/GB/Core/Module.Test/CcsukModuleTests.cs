using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Customs.GB.Module.CcsukAirInventoryCombinedMasterAndHouse.Testing;
using Enterprise.Customs.Module.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	partial class Mawb
	{
		class CcsukAirInventoryUFOControllerTest : ZControllerTest
		{
			public void TestModuleId()
			{
				var c = new CcsukAirInventoryControllerUFO();
				AssertEquals(ModuleIDs.Customs.EU.GB.CcsukAirInventory, c.ModuleID);
			}
		}

		[TestedType(typeof(CcsukAirInventoryControllerUFO))]
		class CcsukAirInventoryUFOControllerBasherTest : ZControllerBasherTest
		{
			public override void TestViewForm()
			{
				Assert("UFOs are not viewed via this controller, they are only created", true);
			}

			public override void TestDeleteForm()
			{
				Assert("UFOs are not deleted via this controller, they are only created", true);
			}

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CusMAWB);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukAirInventoryUFO;
			}
		}

		class CcsukAirInventoryControllerTest : ZControllerTest
		{
			public void TestSecurityCheckpoints()
			{
				var controller = new CcsukAirInventoryController();
				var bizO = Factory.New<CusMAWB>();
				AssertEquals(Env.Security.AirCcsukMasterNew, controller.GetCheckPointForNew(bizO));
				AssertEquals(Env.Security.AirCcsukMasterEdit, controller.GetCheckPointForEdit(bizO));
				AssertEquals(Env.Security.AirCcsukMasterView, controller.GetCheckPointForView(bizO));
				AssertEquals(Env.Security.AirCcsukMasterDelete, controller.GetCheckPointForDelete(bizO));
			}

			public void TestModuleId()
			{
				var c = new CcsukAirInventoryController();
				AssertEquals(ModuleIDs.Customs.EU.GB.CcsukAirInventory, c.ModuleID);
			}

			public void TestShowNew()
			{
				var controller = new CcsukAirInventoryController();
				using (var form = controller.ShowNewForm())
				{
					AssertEquals("CcsukAirInventoryForm", form.GetType().Name);
				}
			}

			public void TestLicenceDeterminesPlugin()
			{
				var consol = Factory.New<ForwardingConsol>();
				var controller = new CcsukAirInventoryControllerForTest();
				using (var plugin = controller.GetPlugin(consol))
				{
					AssertType("With shed enabled we see the one-to-many plugin", typeof(CcsukConsolMultiMawbPlugin), plugin);
				}
			}

			class CcsukAirInventoryControllerForTest : CcsukAirInventoryController
			{
				internal ZPlugIn GetPlugin(IBusiness businessEntity)
				{
					return base.GetPlugIn(businessEntity);
				}
			}
		}

		class CcsukAirInventoryInConsolControllerTest : ZControllerTest
		{
			public void TestModuleId()
			{
				var c = new CcsukAirInventoryInConsolController();
				AssertEquals(ModuleIDs.Customs.EU.GB.CcsukAirInventory, c.ModuleID);
			}

			public void TestShowNew()
			{
				var controller = new CcsukAirInventoryInConsolController();
				using (var form = controller.ShowNewForm())
				{
					AssertEquals("ConsolForm", form.GetType().Name);
				}
			}

			public void TestGetForm()
			{
				var controller = new CcsukAirInventoryInConsolControllerForTest();
				var mawb = Factory.New<CusMAWB>();
				var consol = Factory.New<ForwardingConsol>();
				using (var form = controller.GetForm(mawb))
				{
					AssertEquals("CcsukAirInventoryForm", form.GetType().Name);
				}
				mawb.CM_JK = consol.PK;
				using (var form = controller.GetForm(mawb))
				{
					AssertEquals("ConsolForm", form.GetType().Name);
				}
			}

			public void TestShowFormForDelete()
			{
				var controller = new CcsukAirInventoryInConsolControllerForTest();
				var mawb = Factory.New<CusMAWB>();
				var consol = Factory.New<ForwardingConsol>();
				mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
				Factory.Save();
				using (var form = controller.ShowDeleteForm(mawb))
				{
					AssertEquals("The delete form is the MAWB form for standalone jobs", "CcsukAirInventoryForm", form.GetType().Name);
				}
				mawb.CM_JK = consol.PK;
				Factory.Save();
				using (var form = controller.ShowDeleteForm(mawb))
				{
					AssertEquals("The delete form is the MAWB form for consol-linked jobs", "CcsukAirInventoryForm", form.GetType().Name);
				}
				mawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
				using (var form = controller.ShowDeleteForm(mawb))
				{
					AssertNotNull(form);
				}
				mawb.Messages.AddNew();

				using (var form = controller.ShowDeleteForm(mawb))
				{
					AssertNull(form);
				}
			}

			class CcsukAirInventoryInConsolControllerForTest : CcsukAirInventoryInConsolController
			{
				public new IZForm GetForm(IBusiness bizO)
				{
					return base.GetForm(bizO);
				}
			}
		}

		class CcsukAirInventoryModuleTest : ZModuleTest
		{
			public void TestGetNewStandardMenuItems()
			{
				using (var module = new CcsukAirInventoryModule())
				{
					module.SetupAndGetGrid();
					AssertEquals(2, module.NewMenuItem.MenuItems.Count);
					AssertEquals("New UFO", module.NewMenuItem.MenuItems[1].Text);
				}
			}

			public void TestGetRightControllerForAwbsLinkedToForwarding()
			{
				var mawb = Factory.New<CusMAWB>();
				var consol = Factory.New<ForwardingConsol>();
				using (var module = new CcsukAirInventoryModuleForTest())
				{
					module.SetupAndGetGrid();
					var controller = module.GetNewControllerExposed(mawb);
					AssertType(typeof(CcsukAirInventoryController), controller);
					mawb.CM_JK = consol.PK;
					controller = module.GetNewControllerExposed(mawb);
					AssertType(typeof(CcsukAirInventoryInConsolController), controller);
				}
			}

			class CcsukAirInventoryModuleForTest : CcsukAirInventoryModule
			{
				public ZController GetNewControllerExposed(BusinessObject selectedBusinessObject)
				{
					return base.GetNewController(selectedBusinessObject);
				}
			}
		}

		[TestedType(typeof(CcsukAirInventoryFilterStripBusinessObject))]
		internal class CcsukAirInventoryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
		{
			public void TestBranchAndCompanyLimit()
			{
				CcsukAirInventoryCombinedMasterAndHouseFilterStripBusinessObjectTest.RunTestBranchAndCompanyLimit<CcsukAirInventoryFilterStripBusinessObject, CusMAWB>(
					delegate
					{
						return Factory.New<CusMAWB>();
					}, Factory, true);
			}

			public void TestAllSimpleFilters()
			{
				// These properties can be tested easily because they are all text properties and have ZPropertyInfos on the mawb.  Other, more complex properties will have their own test methods.
				TestMakeAnyWorkerQueryString("Goods description", mawb.DescriptionOfGoodsInfo);
				TestMakeAnyWorkerQueryString("MAWB", mawb.CM_MAWBInfo);
				TestMakeAnyWorkerQueryString("Shed", mawb.CargoTerminalOperatorAirportAndShedInfo);
				TestMakeAnyWorkerQueryString("Agent", mawb.AgentBadgeInfo);
				TestMakeAnyWorkerQueryString("Discharge airport", mawb.AirportOfDestinationInfo);
				TestMakeAnyWorkerQueryString("Arrival airport", mawb.AirportOfArrivalInfo);
				TestMakeAnyWorkerQueryString("Origin airport", mawb.AirportOfOriginInfo);
				TestMakeAnyWorkerQueryString("Goods description", mawb.DescriptionOfGoodsInfo);
				TestMakeAnyWorkerQueryString("Profile", mawb.ProfileInfo);
				TestMakeAnyWorkerQueryString("Presence on Network", mawb.PresenceOnNetworkStatusInfo);
			}

			public void TestIsUFO()
			{
				RunMediumDifficultyFilterTest("UFO", (CusMAWB m) => m.InitialiseUFO());
			}

			public void TestP5()
			{
				RunMediumDifficultyFilterTest("P5 (auto-created)", (CusMAWB m) => m.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc);
			}

			public void TestStatus2()
			{
				RunMediumDifficultyFilterTest("Status 2", (CusMAWB m) => m.Status2Granted = false, false);
			}

			void RunMediumDifficultyFilterTest(string filterName, Action<CusMAWB> changeToPositiveValue, bool initalFlagValue = true)
			{
				Factory.Save();
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var flagsFilter = (ModuleFlagsFilter)stripBO[filterName];
				AssertNotNull(flagsFilter);
				flagsFilter.IsActive = true;
				flagsFilter.Property0 = initalFlagValue;
				AssertNull(Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				changeToPositiveValue(mawb);
				Factory.Save();
				var found = Factory.LoadTop1<CusMAWB>(stripBO.Filter);
				AssertNotNull(found);
				AssertEquals(mawb.PK, found.PK);
			}

			public void TestIsFilterContainsConditionCM_GBAndApplicationCode()
			{
				mawb.Factory.Save();
				var strip = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				AssertContains(mawb.CM_GB.ToString(), strip.Filter.LiteralTextSqlFormatted);
				AssertContains(mawb.CM_ApplicationCode, strip.Filter.LiteralTextSqlFormatted);
			}

			public void TestHasSplitsQuery()
			{
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				HasSplitsQueryTestRunner<CusMAWB>(mawb, stripBO);
			}

			public static void HasSplitsQueryTestRunner<T>(ICcsukCusAwb awb, FilterStripBusinessObject stripBO) where T : BusinessObject
			{
				awb.Factory.Save();
				var textFilter = (ModuleFlagsFilter)stripBO["Has splits?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = false;
				AssertEquals(awb.PK, awb.Factory.LoadTop1<T>(stripBO.Filter).PK);
				awb.Splits.AddNew();
				awb.Factory.Save();
				AssertNull("Has splits?", awb.Factory.LoadTop1<T>(stripBO.Filter));
				textFilter.Property0 = true;
				AssertEquals(awb.PK, awb.Factory.LoadTop1<T>(stripBO.Filter).PK);
			}

			public void TestIsBasicQueryDoesntCareForOrphanedAuHawbs()
			{
				var orphanedAuHawbY = Factory.New<Customs.Business.CusHAWB>();
				orphanedAuHawbY.CS_IsMasterHouse = true;
				var orphanedAuHawbN = Factory.New<Customs.Business.CusHAWB>();
				orphanedAuHawbN.CS_IsMasterHouse = false;
				Factory.Save();
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Basic?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = true;
				AssertEquals(mawb.PK, Factory.LoadTop1<CusMAWB>(stripBO.Filter).PK);
				mawb.ChildBills.AddNew();
				Factory.Save();
				AssertNull("Basic?", Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				textFilter.Property0 = false;
				AssertEquals(mawb.PK, Factory.LoadTop1<CusMAWB>(stripBO.Filter).PK);
				AssertEquals("ZQuery does not complain about a nullable FK", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			}

			public void TestIsAgentQuery()
			{
				Factory.Save();
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Is Agent?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = true;
				AssertNull("Is Agent?", Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				mawb.Profile = "CUKFFW98NRW";
				Factory.Save();
				AssertNotNull("Is Agent?", Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				AssertEquals(mawb.PK, Factory.LoadTop1<CusMAWB>(stripBO.Filter).PK);
			}

			public void TestIsShedQuery()
			{
				Factory.Save();
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Is Shed?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = true;
				AssertNull("Is Shed?", Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				mawb.Profile = "CUKAIR98NRW";
				Factory.Save();
				AssertNotNull("Is Shed?", Factory.LoadTop1<CusMAWB>(stripBO.Filter));
				AssertEquals(mawb.PK, Factory.LoadTop1<CusMAWB>(stripBO.Filter).PK);
			}

			public void TestCustomsActionCodeQuery()
			{
				var strip = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				CustomsActionCodeTestRunner<CusMAWB>(mawb, strip);
			}

			public static void CustomsActionCodeTestRunner<T>(ICcsukCusAwb awb, FilterStripBusinessObject stripBO) where T : BusinessObject
			{
				awb.Factory.Save();
				var textFilter = (ModuleTextFilter)stripBO["Customs Action Code"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property = "";
				AssertEquals(awb.PK, awb.Factory.LoadTop1<T>(stripBO.Filter).PK);
				awb.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
				awb.Factory.Save();
				AssertEquals(awb.PK, awb.Factory.LoadTop1<T>(stripBO.Filter).PK);
				textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
				AssertNull("Customs Action Code", awb.Factory.LoadTop1<T>(stripBO.Filter));
				textFilter.Property = "CA";
				textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				AssertEquals(awb.PK, awb.Factory.LoadTop1<T>(stripBO.Filter).PK);
			}

			public void TestGetCompletedConsolQuery()
			{
				Factory.Save();
				var stripPositive = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var filterPositive = (ModuleFlagsFilter)stripPositive[CcsukAirInventoryFilterStripBusinessObject.FilterConstants.ConsolidationOfAllClosedWholeHouses];
				AssertNotNull(filterPositive);
				var stripNegative = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var filterNegative = (ModuleFlagsFilter)stripNegative[CcsukAirInventoryFilterStripBusinessObject.FilterConstants.ConsolidationOfAllClosedWholeHouses];
				filterPositive.IsActive = true;
				filterPositive.Property0 = true;
				filterNegative.IsActive = true;
				filterNegative.Property0 = false;

				AssertNull("Not a consol", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertNull("No match on the opposite filter, because that still demands a consol", Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				var hawb = mawb.ChildBills.AddNew();
				Factory.Save();
				AssertNull("A consol, but has a house that is open", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				hawb.SetCustomsActionCode("CA", ZDateTime.Now);
				Factory.Save();
				AssertNull("A consol, but this house is open, or at least not finalised", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				hawb.SetCustomsActionCode("CX", ZDateTime.Now);
				Factory.Save();
				AssertNull("A consol, but this house is open, or at least not finalised", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				hawb.SetCustomsActionCode("CC", ZDateTime.Now);
				Factory.Save();
				AssertEquals("A consol, all its houses are finalised, we get a match", mawb, Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertNull("A consol, all its houses are finalised, we get no NEGATIVE match", Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				var hawb2 = mawb.ChildBills.AddNew();
				Factory.Save();
				AssertNull("A new (open) house means that not all the houses are closed", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				hawb2.SetCustomsActionCode("CB", ZDateTime.Now);
				Factory.Save();
				AssertEquals("A consol, all its houses are finalised, get a match", mawb, Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertNull("A consol, all its houses are finalised, get no NEGATIVE match", Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				var hawb3 = mawb.ChildBills.AddNew();
				var split1 = hawb3.Splits.AddNew();
				Factory.Save();
				AssertNull("The third hawb is open (no splits have a CAC either), no match", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));

				split1.SetCustomsActionCode("CC", ZDateTime.Now);
				Factory.Save();
				AssertNull("The third hawb is split and one split has a CAC, so the hawb's CAC is '--', we cannot be sure whether the house is closed, so no match. Besides, this test is for WHOLE houses.", Factory.LoadTop1<CusMAWB>(stripPositive.Filter));
				AssertEquals("Match on the opposite filter", mawb, Factory.LoadTop1<CusMAWB>(stripNegative.Filter));
			}

			void TestMakeAnyWorkerQueryString(string filterName, ZPropertyInfo prop)
			{
				var stripBO = (CcsukAirInventoryFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)stripBO[filterName];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				mawb[prop.Name] = goodString;
				Factory.Save();
				textFilter.Property = goodString;
				AssertEquals(mawb.PK, Factory.LoadTop1<CusMAWB>(stripBO.Filter).PK);
				mawb[prop.Name] = badString;
				Factory.Save();
				AssertNull(filterName, Factory.LoadTop1<CusMAWB>(stripBO.Filter));
			}

			public void TestPresenceOnNetworkFilterStripsAddedByDefault()
			{
				var strip = new CcsukAirInventoryFilterStripBusinessObject();
				strip.LoadLayout(null);
				AssertCollectionContains(strip.ActiveModuleFilters, filter => filter is ModuleTextFilter f &&
					f.Description.StartsWith("Presence on Network") &&
					f.SqlComparisonOperator == SQLComparisonOperator.NotEqual && f.Property == PresenceOnNetworkList.Codes.ArchivedOnCcsuk);
				AssertCollectionContains(strip.ActiveModuleFilters, filter => filter is ModuleTextFilter f &&
					f.Description.StartsWith("Presence on Network") &&
					f.SqlComparisonOperator == SQLComparisonOperator.NotEqual && f.Property == PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			}

			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			{
				return new CcsukAirInventoryFilterStripBusinessObject(false);
			}

			protected override void SetUp()
			{
				base.SetUp();

				var ukCompany = Factory.New<GlbCompany>();
				ukCompany.GC_Code = "DAN";
				ukCompany.GC_RN_NKCountryCode = "GB";
				ukBranch = ukCompany.Branches.AddNew();
				ukBranch.GB_RL_NKHomePort = "GBLON";
				ukBranch.GB_Code = "DAN";
				Factory.Save();
				tempEnvironment = DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid());
				mawb = Factory.New<CusMAWB>();
			}

			protected override void TearDown()
			{
				base.TearDown();
				tempEnvironment.Dispose();
			}

			IDisposable tempEnvironment;
			CusMAWB mawb;
			GlbBranch ukBranch;
			readonly ZString goodString = "DAN";
			readonly ZString badString = "BAD";
		}

		[TestedType(typeof(CcsukAirInventoryModule))]
		class CcsukAirInventoryModuleBasherTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.CcsukAirInventory;
			}

			protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		}

		[TestedType(typeof(CcsukAirInventoryController))]
		class CcsukAirInventoryControllerBasherTest : BaseAirCargoControllerBasherTest
		{
			public override Type ControllerToBashType
			{
				get { return typeof(CcsukAirInventoryController); }
			}

			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CusMAWB);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukAirInventory;
			}

			protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			{
				var mawb = Factory.NewWithValidTestData<CusMAWB>();
				mawb.Profile = "xxx";
				mawb.CM_MAWB = "123";
				mawb.ChildBills.AddNew();
				Factory.Save();
				return mawb;
			}
		}

		[TestedType(typeof(CcsukAirInventoryInConsolController))]
		class CcsukAirInventoryInConsolControllerBasherTest : BaseAirCargoControllerBasherTest
		{
			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CusMAWB);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukAirInventoryInConsol;
			}

			protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			{
				var consol = Factory.New<ForwardingConsol>();
				var mawb = Factory.NewWithValidTestData<CusMAWB>();
				mawb.CM_JK = consol.PK;
				mawb.Profile = "xxx";
				mawb.CM_MAWB = "123";
				mawb.ChildBills.AddNew();
				Factory.Save();
				return mawb;
			}

			public override void TestSaveFormWithCustomsPlugIns()
			{
				Assert(true);
			}
		}
	}

	partial class Hawb
	{
		[TestedType(typeof(CcsukAirInventoryHouseFilterStripBusinessObject))]
		class CcsukAirInventoryHouseFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
		{
			public void TestBranchAndCompanyLimit()
			{
				CcsukAirInventoryCombinedMasterAndHouseFilterStripBusinessObjectTest.RunTestBranchAndCompanyLimit<CcsukAirInventoryHouseFilterStripBusinessObject, CusHAWB>(
					delegate
					{
						return Factory.New<CusMAWB>().ChildBills.AddNew();
					}, Factory, true);
			}

			public void TestP5()
			{
				RunMediumDifficultyFilterTest("P5 (auto-created)", (CusHAWB h) => h.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc);
			}

			public void TestStatus2()
			{
				RunMediumDifficultyFilterTest("Status 2", (CusHAWB h) => h.Status2Granted = false, false);
			}

			void RunMediumDifficultyFilterTest(string filterName, Action<CusHAWB> changeToPositiveValue, bool initalFlagValue = true)
			{
				Factory.Save();
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var flagsFilter = (ModuleFlagsFilter)stripBO[filterName];
				AssertNotNull(flagsFilter);
				flagsFilter.IsActive = true;
				flagsFilter.Property0 = initalFlagValue;
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				changeToPositiveValue(hawb);
				Factory.Save();
				var found = Factory.LoadTop1<CusHAWB>(stripBO.Filter);
				AssertNotNull(found);
				AssertEquals(hawb.PK, found.PK);
			}

			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
			{
				return new CcsukAirInventoryHouseFilterStripBusinessObject(false);
			}

			public void TestIsFilterContainsConditionCM_GBAndApplicationCode()
			{
				Factory.Save();
				var strip = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				AssertContains(hawb.MAWB.CM_GB.ToString(), strip.Filter.LiteralTextSqlFormatted);
				AssertContains(hawb.CS_ApplicationCode, strip.Filter.LiteralTextSqlFormatted);
			}

			public void TestAllSimpleFilters()
			{
				hawb.Profile = "CUKFFW98000ABC";
				MakeAnyWorkerQueryStringTestRunnerString("Airport+Shed", hawb.CS_WarehouseLocationInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Agent", hawb.CS_ResponsiblePartyIDInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Goods description", hawb.CS_GoodsDescriptionInfo);
				MakeAnyWorkerQueryStringTestRunnerDecimal("Weight", hawb.CS_WeightInfo);
				MakeAnyWorkerQueryStringTestRunnerShort("NPR", hawb.CS_PiecesLandedInfo);
				MakeAnyWorkerQueryStringTestRunnerShort("NPX", hawb.CS_PiecesManifestedInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Airport of Arrival", hawb.CS_RL_NKDestinationInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Airport of Origin", hawb.CS_RL_NKLoadPortInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Airport of Destination", hawb.CS_RL_NKDischargePortInfo);
				MakeAnyWorkerQueryStringTestRunnerString("Presence on Network", hawb.PresenceOnNetworkStatusInfo);
			}

			public void TestHasSplitsQuery()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				Mawb.CcsukAirInventoryFilterStripBusinessObjectTest.HasSplitsQueryTestRunner<CusHAWB>(hawb, stripBO);
			}

			public void TestIsAgentQuery()
			{
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_FolioReference = "XXX";
				Factory.Save();
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Is Agent?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = true;
				AssertNull("Is Agent?", Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				hawb.CS_FolioReference = "CUKFFW98NRW";
				Factory.Save();
				AssertNotNull("Is Agent?", Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
			}

			public void TestIsShedQuery()
			{
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_FolioReference = "XXX";
				Factory.Save();
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Is Shed?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = true;
				AssertNull("Is Shed?", Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				hawb.CS_FolioReference = "CUKAIR98NRW";
				Factory.Save();
				AssertNotNull("Is Shed?", Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
			}

			public void TestCustomsActionCodeQuery()
			{
				var strip = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				Mawb.CcsukAirInventoryFilterStripBusinessObjectTest.CustomsActionCodeTestRunner<CusHAWB>(hawb, strip);
			}

			public void TestHasEntryLinked()
			{
				Factory.Save();
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleFlagsFilter)stripBO["Has entry linked?"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property0 = false;
				AssertEquals(hawb.PK, hawb.Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				var dec = Factory.New<JobDeclaration>();
				hawb.CS_JE_CustomsFormalEntry = dec.PK;
				hawb.Factory.Save();
				AssertNull("Has entry linked?", hawb.Factory.LoadTop1<CusHAWB>(stripBO.Filter));
				textFilter.Property0 = true;
				AssertEquals(hawb.PK, hawb.Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
			}

			public void TestMasterBranchFilter()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)stripBO["HAWB"];
				textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb.CS_HAWB = goodString;
				Factory.Save();
				textFilter.Property = goodString;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				mawb.CM_GB = (new GlbBranch.Loader(Factory).LoadAllBranchesInThisCountry("AU", true))[0].PK;
				Factory.Save();
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			public void TestMawbNumberFilter()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)stripBO["MAWB"];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb.MAWB.CM_MAWB = goodString;
				Factory.Save();
				textFilter.Property = goodString;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb.MAWB.CM_MAWB = badString;
				Factory.Save();
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			public void TestHawbNumberFilter()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)stripBO["HAWB"];
				textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb.CS_HAWB = goodString;
				Factory.Save();
				textFilter.Property = goodString;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb.CS_HAWB = badString;
				Factory.Save();
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			public void TestCustomsActionDate()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var dateFilter = (ModuleDateFilter)stripBO["Customs Action Date"];
				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				AssertNotNull(dateFilter);
				dateFilter.IsActive = true;
				hawb.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
				Factory.Save();
				dateFilter.Property1 = ZDateTime.BrettsBirthday;
				dateFilter.Property2 = ZDateTime.BrettsBirthday;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				dateFilter.Property1 = ZDateTime.Now;
				dateFilter.Property2 = ZDateTime.Now;
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			public void TestMawbArrivalDateFilter()
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var dateFilter = (ModuleDateFilter)stripBO["Mawb arrival date"];
				dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				AssertNotNull(dateFilter);
				dateFilter.IsActive = true;
				hawb.MAWB.CM_ArrivalDate = ZDateTime.BrettsBirthday;
				Factory.Save();
				dateFilter.Property1 = ZDateTime.BrettsBirthday;
				dateFilter.Property2 = ZDateTime.BrettsBirthday;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb.MAWB.CM_ArrivalDate = ZDateTime.Now;
				Factory.Save();
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			public void TestStatusOneFilter()
			{
				hawb.Profile = "CUKFFW98000ABC";
				hawb.CS_PiecesLanded = 6;
				hawb.CS_PiecesManifested = 6;
				Factory.Save();
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var filter = (ModuleFlagsFilter)stripBO["Status 1 (NPR=NPX)"];
				AssertNotNull(filter);
				filter.IsActive = true;
				filter.Property0 = true;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb.CS_PiecesLanded = 7;
				Factory.Save();
				AssertNull(Factory.LoadTop1<CusHAWB>(stripBO.Filter));

				filter.Property0 = false;
				AssertNoExceptionThrown(() => hawb.MatchesFilter(filter.Query));
			}

			public void TestPresenceOnNetworkFilterStripsAddedByDefault()
			{
				var strip = new CcsukAirInventoryHouseFilterStripBusinessObject();
				strip.LoadLayout(null);
				AssertCollectionContains(strip.ActiveModuleFilters, filter => filter is ModuleTextFilter f &&
					f.Description.StartsWith("Presence on Network") &&
					f.SqlComparisonOperator == SQLComparisonOperator.NotEqual && f.Property == PresenceOnNetworkList.Codes.ArchivedOnCcsuk);
				AssertCollectionContains(strip.ActiveModuleFilters, filter => filter is ModuleTextFilter f &&
					f.Description.StartsWith("Presence on Network") &&
					f.SqlComparisonOperator == SQLComparisonOperator.NotEqual && f.Property == PresenceOnNetworkList.Codes.NotOnCommDbDeleted);
			}

			void MakeAnyWorkerQueryStringTestRunnerString(string filterName, ZPropertyInfo prop)
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)stripBO[filterName];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb[prop.Name] = goodString;
				Factory.Save();
				textFilter.Property = goodString;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb[prop.Name] = badString;
				Factory.Save();
				AssertNull(filterName, Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			void MakeAnyWorkerQueryStringTestRunnerDecimal(string filterName, ZPropertyInfo zprop)
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleNumberRangeFilter)stripBO[filterName];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb[zprop.Name] = 66m;
				Factory.Save();
				textFilter.Property1 = 66m;
				textFilter.Property2 = 66m;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb[zprop.Name] = 99m;
				Factory.Save();
				AssertNull(filterName, Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			void MakeAnyWorkerQueryStringTestRunnerShort(string filterName, ZPropertyInfo zprop)
			{
				var stripBO = (CcsukAirInventoryHouseFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleNumberRangeFilter)stripBO[filterName];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				hawb[zprop.Name] = new ZShort(66);
				Factory.Save();
				textFilter.Property1 = 66;
				textFilter.Property2 = 66;
				AssertEquals(hawb.PK, Factory.LoadTop1<CusHAWB>(stripBO.Filter).PK);
				hawb[zprop.Name] = new ZShort(99);
				Factory.Save();
				AssertNull(filterName, Factory.LoadTop1<CusHAWB>(stripBO.Filter));
			}

			protected override void SetUp()
			{
				base.SetUp();
				var ukCompany = Factory.New<GlbCompany>();
				ukCompany.GC_Code = "DAN";
				ukCompany.GC_RN_NKCountryCode = "GB";
				ukBranch = ukCompany.Branches.AddNew();
				ukBranch.GB_RL_NKHomePort = "GBLON";
				ukBranch.GB_Code = "DAN";
				Factory.Save();
				tempEnvironment = DisposableEnvironment.ForBranch(ukBranch.PK.ToGuid());
				mawb = Factory.New<CusMAWB>();
				hawb = mawb.ChildBills.AddNew();
				Factory.Save();
			}

			protected override void TearDown()
			{
				base.TearDown();
				tempEnvironment.Dispose();
			}

			IDisposable tempEnvironment;
			CusMAWB mawb;
			GlbBranch ukBranch;
			CusHAWB hawb;
			readonly ZString goodString = "DAN";
			readonly ZString badString = "BAD";
		}

		class CcsukAirInventoryHouseControllerTest : ZControllerTest
		{
			public void TestSecurityCheckpoints()
			{
				var controller = new CcsukAirInventoryHouseController();
				var bizO = Factory.New<CusHAWB>();
				AssertEquals(Env.Security.AirCcsukHouseNew, controller.GetCheckPointForNew(bizO));
				AssertEquals(Env.Security.AirCcsukHouseEdit, controller.GetCheckPointForEdit(bizO));
				AssertEquals(Env.Security.AirCcsukHouseView, controller.GetCheckPointForView(bizO));
				AssertEquals(Env.Security.AirCcsukHouseDelete, controller.GetCheckPointForDelete(bizO));
			}

			public void TestShowNew()
			{
				var controller = new CcsukAirInventoryHouseController();
				using (var form = controller.ShowNewForm())
				{
					AssertEquals("CcsukAirInventoryFormHouse", form.GetType().Name);
				}
			}

			public void TestModuleId()
			{
				var c = new CcsukAirInventoryHouseController();
				AssertEquals(ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse, c.ModuleID);
			}

			public void TestShowDeleteForm_WithCorrectNotDummyController()
			{
				var controller = new CcsukAirInventoryHouseController();
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
				Factory.Save();
				using (var form = controller.ShowDeleteForm(hawb))
				{
					AssertEquals("CcsukAirInventoryFormHouse", form.GetType().Name);
				}
			}

			public void TestShedEnabledSwitchDeterminesPlugin()
			{
				var shipment = Factory.New<ForwardingShipment>();
				var controller = new CcsukAirInventoryControllerHouseForTest();
				using (var plugin = controller.GetPlugin(shipment))
				{
					AssertType("With shed enabled we see the one-to-many plugin", typeof(CcsukShipmentMultiHawbPlugin), plugin);
				}
			}

			class CcsukAirInventoryControllerHouseForTest : CcsukAirInventoryHouseController
			{
				internal ZPlugIn GetPlugin(IBusiness businessEntity)
				{
					return base.GetPlugIn(businessEntity);
				}
			}
		}

		class CcsukAirInventoryHouseInShipmentControllerTest : ZControllerTest
		{
			public void TestModuleId()
			{
				var c = new CcsukAirInventoryHouseInShipmentController();
				AssertEquals(ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse, c.ModuleID);
			}

			public void TestGetForm()
			{
				var controller = new CcsukAirInventoryHouseInShipmentControllerForTest();
				var mawb = Factory.New<CusMAWB>();
				var consol = Factory.New<ForwardingConsol>();
				var hawb = mawb.ChildBills.AddNew();
				var shipment = consol.Shipments.AddNew();
				using (var form = controller.GetForm(hawb))
				{
					AssertEquals("CcsukAirInventoryFormHouse", form.GetType().Name);
				}
				mawb.CM_JK = consol.PK;
				hawb.CS_JS = shipment.PK;
				using (var form = controller.GetForm(hawb))
				{
					AssertEquals("ShipmentForm", form.GetType().Name);
				}
			}

			public void TestShowDeleteForm_WithCorrectNotDummyController()
			{
				var controller = new CcsukAirInventoryHouseInShipmentControllerForTest();
				var mawb = Factory.New<CusMAWB>();
				var consol = Factory.New<ForwardingConsol>();
				var hawb = mawb.ChildBills.AddNew();
				var shipment = consol.Shipments.AddNew();
				hawb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
				Factory.Save();
				using (var form = controller.ShowDeleteForm(hawb))
				{
					AssertEquals("The delete form is the HAWB form for standalone jobs", "CcsukAirInventoryFormHouse", form.GetType().Name);
				}
				mawb.CM_JK = consol.PK;
				hawb.CS_JS = shipment.PK;
				Factory.Save();
				using (var form = controller.ShowDeleteForm(hawb))
				{
					AssertEquals("The delete form is the HAWB form for shipment-linked jobs", "CcsukAirInventoryFormHouse", form.GetType().Name);
				}
			}

			class CcsukAirInventoryHouseInShipmentControllerForTest : CcsukAirInventoryHouseInShipmentController
			{
				public new IZForm GetForm(IBusiness bizO)
				{
					return base.GetForm(bizO);
				}
			}
		}

		[TestedType(typeof(CcsukAirInventoryHouseInShipmentController))]
		class CcsukAirInventoryHouseInShipmentControllerBasherTest : ZControllerBasherTest
		{
			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CusHAWB);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment;
			}
		}

		[TestedType(typeof(CcsukAirInventoryHouseController))]
		class CcsukAirInventoryHouseControllerBasherTest : ZControllerBasherTest
		{
			protected override string CountryCode
			{
				get { return Core.Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CusHAWB);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukAirInventoryHouse;
			}
		}

		class CcsukAirInventoryHouseModuleTest : ZModuleTest
		{
			public void TestAllowNew()
			{
				using (var module = new CcsukAirInventoryHouseModule())
				{
					Assert("Thou shalt not create houses independently of thy master", !module.AllowNew);
				}
			}

			public void TestGetRightControllerForAwbsLinkedToForwarding()
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				var shipment = Factory.New<ForwardingShipment>();
				using (var module = new CcsukAirInventoryHouseModuleForTest())
				{
					module.SetupAndGetGrid();
					var controller = module.GetNewControllerExposed(hawb);
					AssertType(typeof(CcsukAirInventoryHouseController), controller);
					hawb.CS_JS = shipment.PK;
					controller = module.GetNewControllerExposed(hawb);
					AssertType(typeof(CcsukAirInventoryHouseInShipmentController), controller);
				}
			}

			class CcsukAirInventoryHouseModuleForTest : CcsukAirInventoryHouseModule
			{
				public ZController GetNewControllerExposed(BusinessObject selectedBusinessObject)
				{
					return base.GetNewController(selectedBusinessObject);
				}
			}
		}

		[TestedType(typeof(CcsukAirInventoryHouseModule))]
		class CcsukAirInventoryHouseModuleBasherTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse;
			}

			protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		}
	}

	partial class SplitTests
	{
		[TestedType(typeof(CcsukSplitBasicController))]
		class CcsukSplitBasicControllerBasherTest : ZControllerBasherTest
		{
			protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			{
				var basic = Factory.New<CusMAWB>();
				var splitBasic = basic.Splits.AddNew();
				Factory.Save();
				return splitBasic;
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukSplitBasicController;
			}

			public override void TestNewForm()
			{
				Assert("There is no NEW form, it is handled via the NonPersistentSplit form", true);
			}

			public override void TestDeleteForm()
			{
				Assert("There is no DELETE form, it is handled via the NonPersistentSplit form", true);
			}

			public void TestSecurityCheckpoints()
			{
				var controller = new CcsukSplitBasicController();
				var bizO = Factory.New<CusMAWB>().Splits.AddNew();
				AssertEquals(Env.Security.AirCcsukMasterNew, controller.GetCheckPointForNew(bizO));
				AssertEquals(Env.Security.AirCcsukMasterEdit, controller.GetCheckPointForEdit(bizO));
				AssertEquals(Env.Security.AirCcsukMasterView, controller.GetCheckPointForView(bizO));
				AssertEquals(Env.Security.AirCcsukMasterDelete, controller.GetCheckPointForDelete(bizO));
			}
		}

		[TestedType(typeof(CcsukSplitBasicModule))]
		class CcsukSplitBasicModuleTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.CcsukSplitBasic;
			}

			public override void TestBoundListsAreNotLoadedOnAccess()
			{
				Assert("The module doesn't have an enquiry screen, nothing is grid loaded.", true);
			}

			public override void TestModuleShowsAndCanSearch()
			{
				Assert("This module is not yet visible and no searches can be performed", true);
			}

			protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		}

		[TestedType(typeof(CcsukSplitHouseController))]
		class CcsukSplitHouseControllerBasherTest : ZControllerBasherTest
		{
			public void TestSecurityCheckpoints()
			{
				var controller = new CcsukSplitHouseController();
				var bizO = Factory.New<CusHAWB>().Splits.AddNew();
				AssertEquals(Env.Security.AirCcsukHouseNew, controller.GetCheckPointForNew(bizO));
				AssertEquals(Env.Security.AirCcsukHouseEdit, controller.GetCheckPointForEdit(bizO));
				AssertEquals(Env.Security.AirCcsukHouseView, controller.GetCheckPointForView(bizO));
				AssertEquals(Env.Security.AirCcsukHouseDelete, controller.GetCheckPointForDelete(bizO));
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CcsukSplitHouseController;
			}

			protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			{
				var mawb = Factory.New<CusMAWB>();
				var house = mawb.ChildBills.AddNew();
				var splitHouse = house.Splits.AddNew();
				Factory.Save();
				return splitHouse;
			}

			public override void TestNewForm()
			{
				Assert("There is no NEW form, it is handled via the NonPersistentSplit form", true);
			}

			public override void TestDeleteForm()
			{
				Assert("There is no DELETE form, it is handled via the NonPersistentSplit form", true);
			}
		}

		[TestedType(typeof(CcsukSplitHouseModule))]
		class CcsukSplitHouseModuleTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.CcsukSplitHouse;
			}

			public override void TestBoundListsAreNotLoadedOnAccess()
			{
				Assert("The module doesn't have an enquiry screen, nothing is grid loaded.", true);
			}

			public override void TestModuleShowsAndCanSearch()
			{
				Assert("This module is not yet visible and no searches can be performed", true);
			}

			protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
		}
	}

	[TestedType(typeof(CcsukReportsModule))]
	class CcsukReportsModuleBasherTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.GB.GbCcsukReports;
		}
	}
}

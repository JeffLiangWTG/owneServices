using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.CcsukAirInventoryCombinedMasterAndHouse.Testing
{
	class CcsukAirInventoryCombinedMasterAndHouseControllerTest : ZControllerTest
	{
		public void TestGetIdAndNameForOpenedFormCache()
		{
			var controller = new CcsukAirInventoryCombinedMasterAndHouseControllerForTest();
			var mawb = Factory.New<CusMAWB>();
			var realHouse = mawb.ChildBills.AddNew();
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			AssertEquals("HAWB BOID", realHouse.PK, controller.GetIDForBusinessEntityExposed(realHouse));
			AssertEquals("HAWB FormID", ControllerIDs.Customs.GB.CcsukAirInventoryHouse.ToString(), controller.GetIDForFormCacheExposed(realHouse));
			AssertEquals("MAWB BOID", mawb.PK, controller.GetIDForBusinessEntityExposed(mawb.MasterLevelHouseHelper));
			AssertEquals("MAWB FormID", ControllerIDs.Customs.GB.CcsukAirInventory.ToString(), controller.GetIDForFormCacheExposed(mawb.MasterLevelHouseHelper));

			var consol = Factory.New<ForwardingConsol>();
			mawb.CM_JK = consol.PK;
			var shipment = consol.Shipments.AddNew();
			realHouse.CS_JS = shipment.PK;
			AssertEquals("Shipment BOID", shipment.PK, controller.GetIDForBusinessEntityExposed(realHouse));
			AssertEquals("Shipment FormID", ControllerIDs.JobShipment.ToString(), controller.GetIDForFormCacheExposed(realHouse));
			AssertEquals("Consol BOID", consol.PK, controller.GetIDForBusinessEntityExposed(mawb.MasterLevelHouseHelper));
			AssertEquals("Consol FormID", ControllerIDs.JobConsol.ToString(), controller.GetIDForFormCacheExposed(mawb.MasterLevelHouseHelper));
		}

		public void TestModuleId()
		{
			var c = new CcsukMasterAndHouseCombinedController();
			AssertEquals(ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined, c.ModuleID);
		}

		public void TestSecurity()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var c = new CcsukMasterAndHouseCombinedController();
			AssertEquals(Env.Security.AirCcsukAllAWBsView, c.GetCheckPointForView(mawb));
			AssertEquals(Env.Security.AirCcsukAllAWBsView, c.GetCheckPointForView(hawb));
		}

		public void TestGetFormAndReload_Hawb()
		{
			GetFormAndReload("HAWB", aHawb: true, forwarding: false, "CcsukAirInventoryFormHouse");
		}

		public void TestGetFormAndReload_Mawb()
		{
			GetFormAndReload("MAWB", aHawb: false, forwarding: false, "CcsukAirInventoryForm");
		}

		public void TestGetFormAndReload_HawbInShipment()
		{
			GetFormAndReload("HAWB/Shipment", aHawb: true, forwarding: true, "ShipmentForm");
		}

		public void TestGetFormAndReload_MawbInConsol()
		{
			GetFormAndReload("MAWB/Consol", aHawb: false, forwarding: true, "ConsolForm");
		}

		void GetFormAndReload(string message, bool aHawb, bool forwarding, string formTypeName)
		{
			var controller = new CcsukAirInventoryCombinedMasterAndHouseControllerForTest();
			var mawb = Factory.New<CusMAWB>();
			var hawb = aHawb ? mawb.ChildBills.AddNew() : mawb.MasterLevelHouseHelper;
			var reloadPk = aHawb ? hawb.PK : mawb.PK;
			if (forwarding)
			{
				var consol = Factory.New<ForwardingConsol>();
				mawb.CM_JK = consol.PK;
				if (aHawb)
				{
					var shipment = consol.Shipments.AddNew();
					hawb.CS_JS = shipment.PK;
					reloadPk = shipment.PK;
				}
				else
				{
					reloadPk = consol.PK;
				}
			}
			Factory.Save();

			using var form = controller.GetFormExposed(hawb);
			AssertForm("GetForm " + message, form, hawb.PK);

			using var reloadedForm = ((ZForm)form).ReloadForm();
			CombineAssertions("ReloadForm " + message, () =>
			{
				AssertNotNull("Form", reloadedForm);
				AssertNullOrEmpty("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			});
			AssertForm("ReloadForm " + message, reloadedForm, reloadPk);

			void AssertForm(string message, IZForm form, ZGuid expectedIdentifier)
			{
				CombineAssertions(message, () =>
				{
					AssertContains("Type", formTypeName, form.GetType().FullName);
					AssertEquals("Identifier", expectedIdentifier, form.IdentifierForPersistingForm);
				});
			}
		}

		class CcsukAirInventoryCombinedMasterAndHouseControllerForTest : CcsukMasterAndHouseCombinedController
		{
			public IZForm GetFormExposed(CusHAWB businessEntity)
			{
				return base.ShowLoadedForm(businessEntity, FormAction.View);
				//return base.GetForm(BusinessEntity);
			}

			public string GetIDForFormCacheExposed(IBusiness workerOrRealHouse)
			{
				return base.GetIDForFormCache(workerOrRealHouse);
			}

			public Guid GetIDForBusinessEntityExposed(IBusiness workerOrRealHouse)
			{
				return base.GetIDForBusinessEntity(workerOrRealHouse);
			}
		}
	}

	[TestedType(typeof(CcsukMasterAndHouseCombinedController))]
	class CcsukAirInventoryCombinedMasterAndHouseControllerBasherTest : ZControllerBasherTest
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
			return ControllerIDs.Customs.GB.CcsukMasterAndHouseCombined;
		}
	}

	[TestedType(typeof(CcsukMasterAndHouseCombinedModule))]
	class CcsukAirInventoryMasterAndHouseCombinedModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var bizo = (CusHAWB)factory.NewWithValidTestData(businessObjectType);
			bizo.CS_GoodsDescription = "MODULE BASHER";

			return bizo;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Goods description"];
			filter.IsActive = true;
			filter.Property = "MODULE BASHER";
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}

	class CcsukAirInventoryMasterAndHouseCombinedModuleTest : ZModuleTest
	{
	}

	[TestedType(typeof(CcsukMasterAndHouseCombinedFilterStripBusinessObject))]
	class CcsukAirInventoryCombinedMasterAndHouseFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestAllQueries()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "11122222222";
			var trueHawb1 = mawb.ChildBills.AddNew();
			trueHawb1.CS_HAWB = "HOUSE001";
			var trueHawb2 = mawb.ChildBills.AddNew();
			trueHawb2.CS_HAWB = "HOUSE002";
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11155555555";
			var duffMawb = Factory.New<CusMAWB>();
			duffMawb.CM_MAWB = "99900000000";
			Factory.Save();

			var stripBO = (CcsukMasterAndHouseCombinedFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var mawbTextFilter = (ModuleTextFilter)stripBO["MAWB"];
			AssertNotNull(mawbTextFilter);
			mawbTextFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			mawbTextFilter.IsActive = true;
			mawbTextFilter.Property = "111";
			var loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(trueHawb1, loadedRecords);
			AssertCollectionContains(trueHawb2, loadedRecords);
			AssertCollectionNotContains(duffMawb.MasterLevelHouseHelper, loadedRecords);

			var hawbTextFilter = (ModuleTextFilter)stripBO["HAWB"];
			AssertNotNull(hawbTextFilter);
			hawbTextFilter.IsActive = true;
			hawbTextFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			hawbTextFilter.Property = "H";
			loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionNotContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionNotContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(trueHawb1, loadedRecords);
			AssertCollectionContains(trueHawb2, loadedRecords);
			AssertCollectionNotContains(duffMawb.MasterLevelHouseHelper, loadedRecords);

			hawbTextFilter.IsActive = false;
			mawbTextFilter.IsActive = false;
			var splitsFilter = (ModuleFlagsFilter)stripBO["Has splits?"];
			AssertNotNull(splitsFilter);
			splitsFilter.IsActive = true;
			splitsFilter.Property0 = false;
			loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(trueHawb1, loadedRecords);
			AssertCollectionContains(trueHawb2, loadedRecords);
			AssertCollectionContains(duffMawb.MasterLevelHouseHelper, loadedRecords);

			splitsFilter.Property0 = true;
			loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionNotContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionNotContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionNotContains(trueHawb1, loadedRecords);
			AssertCollectionNotContains(trueHawb2, loadedRecords);
			AssertCollectionNotContains(duffMawb.MasterLevelHouseHelper, loadedRecords);

			basic.Splits.AddNew();
			trueHawb2.Splits.AddNew();
			Factory.Save();
			loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionNotContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionNotContains(trueHawb1, loadedRecords);
			AssertCollectionContains(trueHawb2, loadedRecords);
			AssertCollectionNotContains(duffMawb.MasterLevelHouseHelper, loadedRecords);

			splitsFilter.Property0 = false;
			loadedRecords = Factory.Load<CusHAWB>(stripBO.Filter);
			AssertCollectionContains(mawb.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionNotContains(basic.MasterLevelHouseHelper, loadedRecords);
			AssertCollectionContains(trueHawb1, loadedRecords);
			AssertCollectionNotContains(trueHawb2, loadedRecords);
			AssertCollectionContains(duffMawb.MasterLevelHouseHelper, loadedRecords);
		}

		public void TestPresenceOnNetworkFilterStripsAddedByDefault()
		{
			var strip = new CcsukMasterAndHouseCombinedFilterStripBusinessObject();
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
			return new CcsukMasterAndHouseCombinedFilterStripBusinessObject(false);
		}

		public void TestBranchAndCompanyLimit()
		{
			RunTestBranchAndCompanyLimit<CcsukMasterAndHouseCombinedFilterStripBusinessObject, CusHAWB>(
				delegate
				{
					return Factory.New<CusMAWB>().ChildBills.AddNew();
				}, Factory, true);
		}

		public static void RunTestBranchAndCompanyLimit<TFilter, TRealRow>(Func<TRealRow> makeRow, BusinessObjectFactory factory, bool doContainsSearchNotPkMatch)
				where TFilter : FilterStripBusinessObject, new()
				where TRealRow : BusinessObject
		{
			var companyA = factory.New<GlbCompany>();
			companyA.GC_Code = "AAA";
			companyA.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branchA = companyA.Branches.AddNew();
			branchA.GB_Code = "AAA";
			branchA.GB_RL_NKHomePort = "GBDTE";
			var companyB = factory.New<GlbCompany>();
			companyB.GC_Code = "BBB";
			companyB.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branchB = companyB.Branches.AddNew();
			branchB.GB_Code = "BBB";
			branchB.GB_RL_NKHomePort = "GBLHR";
			factory.Save();
			TRealRow rowA = default(TRealRow);
			TRealRow rowB = default(TRealRow);
			using (DisposableEnvironment.ForBranch(branchA.PK.ToGuid()))
			{
				rowA = makeRow();
			}
			using (DisposableEnvironment.ForBranch(branchB.PK.ToGuid()))
			{
				rowB = makeRow();
			}
			factory.Save();
			using (DisposableEnvironment.ForBranch(branchA.PK.ToGuid()))
			{
				var loadedRecords = factory.Load<TRealRow>(new TFilter().Filter);
				if (doContainsSearchNotPkMatch)
				{
					AssertCollectionContains(rowA, loadedRecords);
					AssertCollectionNotContains(rowB, loadedRecords);
				}
				else
				{
					AssertEquals(rowA.PK, loadedRecords[0].PK);
					AssertNotEquals(rowB.PK, loadedRecords[0].PK);
				}
			}
			using (DisposableEnvironment.ForBranch(branchB.PK.ToGuid()))
			{
				var loadedRecords = factory.Load<TRealRow>(new TFilter().Filter);
				if (doContainsSearchNotPkMatch)
				{
					AssertCollectionContains(rowB, loadedRecords);
					AssertCollectionNotContains(rowA, loadedRecords);
				}
				else
				{
					AssertEquals(rowB.PK, loadedRecords[0].PK);
					AssertNotEquals(rowA.PK, loadedRecords[0].PK);
				}
			}
		}
	}

	class CcsukAirInventoryCombinedMasterAndHouseFilterStripControlTest : TestCaseWithFactory
	{
		public void TestReferenceNumberAndForwardingLinkedAndFlightColumns()
		{
			var hawbs = new CusHAWBCollectionNonDependentShowMastersToo(Factory);
			var filterBO = new CcsukMasterAndHouseCombinedFilterStripBusinessObject();
			using (var filterControl = new CcsukMasterAndHouseCombinedFilterStripControl(hawbs, filterBO))
			{
				using (var form = new ZForm())
				{
					var foundReference = false;
					var foundShipmentLinked_HopefullyDontFindIt = false;
					var foundForwardingLinked = false;
					var foundFlightNumber = false;
					var foundFlightDate = false;
					var foundSplitDetails = false;
					form.Controls.Add(filterControl);
					form.Show();
					foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
					{
						if (column.ColumnName == CusHAWB.Schema.ReferenceNumber)
						{
							foundReference = true;
							AssertEquals("ReferenceNumber column should be visible", true, column.IsVisible);
						}
						else if (column.ColumnName == CusHAWB.Schema.IsLinkedToForwardingJob)
						{
							foundForwardingLinked = true;
						}
						else if (column.ColumnName == CusHAWB.Schema.IsLinkedToJobShipment)
						{
							foundShipmentLinked_HopefullyDontFindIt = true;
						}
						else if (column.ColumnName.EndsWith(CusMAWB.Schema.CM_FlightNo))
						{
							foundFlightNumber = true;
						}
						else if (column.ColumnName.EndsWith(CusMAWB.Schema.CM_ArrivalDate))
						{
							foundFlightDate = true;
						}
						else if (column.ColumnName == CusHAWB.Schema.SplitReferencesForAllAwbsModuleGrid)
						{
							foundSplitDetails = true;
						}
					}
					Assert("Found correct columns", foundReference && foundForwardingLinked && !foundShipmentLinked_HopefullyDontFindIt && foundFlightDate && foundFlightNumber && foundSplitDetails); // note the NOT on foundShipmentLinked...
				}
			}
		}
	}
}

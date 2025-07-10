using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRBillsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSpecialCargoCodes()
		{
			PrepareRefCusCodeList();

			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var specialCargoCodes = bill.Lookups.SpecialCargoCodes;
			specialCargoCodes.Load();
			var countryOrGroupingKey = Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			Assert(specialCargoCodes.FilterBusinessObjectDefaults.ContainsDefaultFor(countryOrGroupingKey));
			AssertEquals(Core.Constants.CountryCodes.Japan, specialCargoCodes.FilterBusinessObjectDefaults[countryOrGroupingKey].Value);
			AssertEquals(4, specialCargoCodes.Count);
			AssertContainsExactElementsInAnyOrder(specialCargoCodes, ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, ZDateTime.Today));
		}

		public void TestConsignor()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			ConsignorCollection collection = bill.Lookups.Consignors;
			AssertNotNull(collection);
		}

		public void TestConsignees()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			ConsigneeCollection collection = bill.Lookups.Consignees;
			AssertNotNull(collection);
		}

		public void TestOrganisations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			OrgHeaderCollection collection = bill.Lookups.Organisations;
			AssertNotNull(collection);
		}

		public void TestManifestUnitList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "Package types");
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BK", "Basket", startDate, endDate);
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "CG", "Cage", startDate, endDate);
			universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "DP", "DemiJohn, protected", startDate, endDate);
			Factory.Save();
			
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			AssertEquals("BK, CG, DP", (bill.Lookups.ManifestUnitList as CodeDescriptionPairList).CodesAsString);
		}

		public void TestWeightUnitList()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<WeightUnitCodeList>(), bill.Lookups.WeightUnitList);
		}

		public void TestVolumeUnitList()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<VolumeUnitCodeList>(), bill.Lookups.VolumeUnitList);
		}

		public void TestDeliveries()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			RefUNLOCOCollection collection = bill.Lookups.Deliveries;
			AssertNotNull(collection);
		}

		public void TestBillCustomsStatusList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<AFRBillCustomsStatusList>(), bill.Lookups.BillCustomsStatusList);
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<MessageStatusList>(), bill.Lookups.MessageStatusList);
		}

		public void TestGoodsValueCurrencies()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			RefCurrencyCollection collection = bill.Lookups.GoodsValueCurrencies;
			AssertNotNull(collection);
		}

		public void TestTemporaryLandingReasonCodeList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<TemporaryLandingReasonCodeList>(), bill.Lookups.TemporaryLandingReasonCodeList);
		}

		public void TestTransportModeList()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(Factory.GetCachedValue<TransportModeList>(), bill.Lookups.TransportModeList);
		}

		void PrepareRefCusCodeList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			TestCaseHelper.ClearTable(RefCusCodeType.Schema.TableName);

			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "Japan Special Cargo Code");
			var jp1dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "PLQ", startDate, endDate);
			jp1dr.ZZD_Description = "Plant Protection Act ";
			var jp2dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "ARM", startDate, endDate);
			jp2dr.ZZD_Description = "firearms and swords Control Law";
			var jp3dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AVI", startDate, endDate);
			jp3dr.ZZD_Description = "a domestic animal infectious disease prophylaxis";
			var jp4dr = universalReferenceTestHelper.CreateCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "NRC", startDate, endDate);
			jp4dr.ZZD_Description = "drugs and psychotropic drugs Control Law";
			Factory.Save();
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageTypeList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			AssertType<ILDeclarationMessageTypeList>(lookups.MessageTypeList);
		}

		public void TestMessageSubTypeList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			AssertType<ILDeclarationMessageSubTypeList>(lookups.MessageSubTypeList);
		}

		public void TestTransportMeansList()
		{
			var groupingIL = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			var groupingLV = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "DESC1", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "DESC2", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "10", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "20", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "30", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "11", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			var transportMeansList = lookups.TransportMeansList;

			CombineAssertions(() =>
			{
				AssertEquals("List should have 3 items.", 3, transportMeansList.Count);
				AssertEquals("List should have item 10.", true, transportMeansList.ContainsCode("10"));
				AssertEquals("List should have item 20.", true, transportMeansList.ContainsCode("20"));
				AssertEquals("List should have item 30.", true, transportMeansList.ContainsCode("30"));

				AssertEquals("List should NOT have non-IL item.", false, transportMeansList.ContainsCode("11"));

				var newDeclaration = Factory.New<JobDeclaration>();
				AssertSame("List should have been cached.", transportMeansList, newDeclaration.Lookups.TransportMeansList);
			});
		}

		public void TestLocationOfGoodsCollection()
		{
			var groupingIL = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			var groupingLV = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC1", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC2", Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "010", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "020", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "030", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "011", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			var locationOfGoodsCollection = lookups.LocationOfGoodsCollection as BusinessObjectCollection;
			locationOfGoodsCollection.Load(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today.AddDays(-2)));

			CombineAssertions(() =>
			{
				AssertEquals("List should have 3 items.", 3, locationOfGoodsCollection.Count);
				Assert(locationOfGoodsCollection.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "010"));
				Assert(locationOfGoodsCollection.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "020"));
				Assert(locationOfGoodsCollection.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "030"));
				Assert(!locationOfGoodsCollection.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "011"));

				var filters = locationOfGoodsCollection.FilterBusinessObjectDefaults;
				var countryFilter = filters["Country/Region or Grouping:Property"];
				var listTypeFilter = filters["List Type:Property"];
				AssertEquals("IL", countryFilter.Value);
				AssertEquals("FAC", listTypeFilter.Value);

				var newDeclaration = Factory.New<JobDeclaration>();
				AssertSame("List should have been cached.", locationOfGoodsCollection, newDeclaration.Lookups.LocationOfGoodsCollection);
			});
		}

		public void TestRepresentativeOfficeList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			AssertType<OrganisationsFindBoxCollection>(lookups.RepresentativeOfficeList);
		}

		public void TestSellerOfficeList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			AssertType<OrganisationsFindBoxCollection>(lookups.SellerOfficeList);
		}

		public void TestCustomsOfficeList()
		{
			var factory = Factory;
			var groupingIL = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeIT654321 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ILHFA", "נמל חיפה", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var jobDeclaration = factory.New<JobDeclaration>();
			var lookups = jobDeclaration.Lookups;
			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.CustomsOfficeList);
			var customsOfficeList = lookups.CustomsOfficeList;
			AssertSame("List should have been cached.", customsOfficeList, lookups.CustomsOfficeList);
			Assert("Filter by List Type", customsOfficeList.FilterBusinessObjectDefaults.ContainsDefaultFor("List Type:Property"));
			Assert("Filter by Country/Region or Grouping", customsOfficeList.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping:Property"));
			customsOfficeList.Load();
			AssertEquals("List should have 1 item.", 1, customsOfficeList.Count);
			AssertEquals("List should have item ILHFA.", true, customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ILHFA"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		UniversalReferenceTestDataHelper helper;
	}
}

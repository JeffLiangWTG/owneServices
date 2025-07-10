using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusCNClassificationLookupsTest : TestCaseWithFactory
	{
		public void TestEndUseList()
		{
			var classification = Factory.New<CusCNClassification>();
			AssertEquals("There should be 24 items.", 24, classification.Lookups.EndUseList.Count);
			Assert("EndUseList Type", classification.Lookups.EndUseList is UntranslatableCodeDescriptionPairList);
			AssertEquals("EndUseList Descriptions", "\u79cd\u7528\u6216\u7e41\u6b96", classification.Lookups.EndUseList.GetDescriptionFromCode("11"));
		}

		[TestDate(2016, 5, 5)]
		public void TestCIQTariffList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			var ciqTariffTypeUS = helper.CreateNewOrGetExistingTariffType("US", "CIQ");
			var xxxTariffType = helper.CreateNewOrGetExistingTariffType("CN", "XXX");
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			Factory.Save();
			var ciqTariff1 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff2 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 5, 5));
			var ciqTariffUS = helper.LoadOrCreateNewTariff("US", ciqTariffTypeUS.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var xxxTariff = helper.LoadOrCreateNewTariff("CN", xxxTariffType.PK, "10000012999", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000012", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateTariffRelationship(ciqTariff1.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff2.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariffUS.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(xxxTariff.PK, cusTariffType.PK, "10000012");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "10000012";
			var testList = pivot.CNClassificationLookups.CIQTariffList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<TariffView>().Any(tariff => tariff.ZZ1_TariffCode == "10000012001"));
			Assert(testList.Cast<TariffView>().Any(tariff => tariff.ZZ1_TariffCode == "10000012002"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Tariff Code:Property"];
			AssertEquals("10000012", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestCIQOriginStateList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CIQST", "CIQST");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101001", "CIQ State1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101002", "CIQ State2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101003", "CIQ State3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_RN_NKCountryOfOrigin = "US";
			pivot.CountryOfOrigin.RN_IsoNumericUNM49Code = "840";
			var testList = pivot.CNClassificationLookups.CIQOriginStateList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101001"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101002"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("840", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestOrigDistrictList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CNC_OriginRegion = "101011";
			var testList = pivot.CNClassificationLookups.OrigDistrictList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10101"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10102"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Origin Region", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestDestDistrictList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CNC_DestinationRegion = "101011";
			var testList = pivot.CNClassificationLookups.DestDistrictList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10101"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10102"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Destination Region", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestOrigRegionList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CNC_OriginDistrict = "10101";
			var testList = pivot.CNClassificationLookups.OrigRegionList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101011"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101022"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Origin District", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestDestRegionList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CNC_DestinationDistrict = "10101";
			var testList = pivot.CNClassificationLookups.DestRegionList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101011"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101022"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Destination District", "1010", listTypeFilter.Value);
		}

		public void TestUNDGPackageTypes()
		{
			var fullList = Factory.GetCachedValue<UNDGPackageTypeList>();
			var pivot = Factory.New<CusClassPartPivot>();
			var testList = pivot.CNClassificationLookups.UNDGPackageTypes;
			AssertEquals("UNDGPackageTypeList Count", fullList.Count, testList.Count);
			Assert("UNDGPackageTypeList Type", testList is UntranslatableCodeDescriptionPairList);
			AssertEquals("UNDGPackageTypeList Descriptions", "\u94a2\u5236\u4e0d\u53ef\u62c6\u88c5\u6876\u9876\u5706\u6876", testList.GetDescriptionFromCode("1A1"));
		}
	}
}

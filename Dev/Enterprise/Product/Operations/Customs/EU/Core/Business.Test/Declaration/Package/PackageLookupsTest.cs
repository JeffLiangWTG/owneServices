using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class PackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBulkAndBreakBulkPackingUnitTypesList()
		{
			SetUpRefData();
			Factory.Save();

			CombineAssertions(() =>
			{
				var package = Factory.New<Package>();
				var unitTypesList = package.Lookups.BulkAndBreakBulkPackingUnitTypesList;
				AssertEquals("Count", 4, unitTypesList.Count);
				AssertEquals("Contains VQ?", true, unitTypesList.ContainsCode("VQ"));
				AssertEquals("Contains VG?", true, unitTypesList.ContainsCode("VG"));
				AssertEquals("Contains NE?", true, unitTypesList.ContainsCode("NE"));
				AssertEquals("Contains NF?", true, unitTypesList.ContainsCode("NF"));
				AssertSame("Cached", unitTypesList, package.Lookups.BulkAndBreakBulkPackingUnitTypesList);
			});
		}

		public void TestBulkOnlyPackingUnitTypesList()
		{
			SetUpRefData();
			Factory.Save();

			CombineAssertions(() =>
			{
				var package = Factory.New<Package>();
				var unitTypesList = package.Lookups.BulkOnlyPackingUnitTypesList;
				AssertEquals("Count", 2, unitTypesList.Count);
				AssertEquals("Contains VQ?", true, unitTypesList.ContainsCode("VQ"));
				AssertEquals("Contains VG?", true, unitTypesList.ContainsCode("VG"));
				AssertSame("Cached", unitTypesList, package.Lookups.BulkOnlyPackingUnitTypesList);
			});
		}

		public void TestBreakBulkOnlyPackingUnitTypesList()
		{
			SetUpRefData();
			Factory.Save();

			CombineAssertions(() =>
			{
				var package = Factory.New<Package>();
				var unitTypesList = package.Lookups.BreakBulkOnlyPackingUnitTypesList;
				AssertEquals("Count", 2, unitTypesList.Count);
				AssertEquals("Contains NE?", true, unitTypesList.ContainsCode("NE"));
				AssertEquals("Contains NF?", true, unitTypesList.ContainsCode("NF"));
				AssertSame("Cached", unitTypesList, package.Lookups.BreakBulkOnlyPackingUnitTypesList);
			});
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		}
	}
}

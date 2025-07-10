using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStoragePackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerList()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var container1 = Factory.NewWithPrimaryKey<TemporaryStorageContainer>(Guid.NewGuid());
			var container2 = Factory.NewWithPrimaryKey<TemporaryStorageContainer>(Guid.NewGuid());
			storageHeader.Containers.Add(container1);
			storageHeader.Containers.Add(container2);
			var bill = storageHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var containers = pack.Lookups.Containers;
			AssertContainsExactElementsInAnyOrder("Containers in the Lookup should match ones in the header", new[] { container1.PK.ToString(), container2.PK.ToString() }, containers.ToArray().Select(s => s.PK.ToString()));
		}

		public void TestPackUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var une = helper.CreateNewOrGetExistingDataGrouping(Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "United Nations");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, une);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(
				Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAAA",
				"AAAAA",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBBB",
				"BBBBB",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Latvia,
				RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"CCCC",
				"CCCCC",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Germany,
				RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"DDDD",
				"DDDDD",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("AMA_RN_NKCountry must be LV", Core.Constants.CountryCodes.Latvia, storageHeader.AMA_RN_NKCountry);
			var bill = storageHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var collection = pack.Lookups.PackUQList;
			AssertEquals("List should be filled with UNE codes of type UNPKG", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder("List should be filled with UNE codes of type UNPKG", new string[] { "AAAA", "BBBB" }, collection.ToArray().Select(x => x.Code));
		}

		public void TestBulkOnlyPackingUnitTypesList()
		{
			SetUpPackageTypes();
			Factory.Save();

			var package = Factory.New<TemporaryStoragePack>();
			var unitTypesList = package.Lookups.BulkOnlyPackingUnitTypesList;
			unitTypesList.Sort();

			CombineAssertions(() =>
			{
				AssertEquals("BulkOnlyPackingUnitTypesList CodesAsString", "VG, VQ", unitTypesList.CodesAsString);
				AssertSame("Cached", unitTypesList, package.Lookups.BulkOnlyPackingUnitTypesList);
			});
		}

		public void TestBreakBulkOnlyPackingUnitTypesList()
		{
			SetUpPackageTypes();
			Factory.Save();

			var package = Factory.New<TemporaryStoragePack>();
			var unitTypesList = package.Lookups.BreakBulkOnlyPackingUnitTypesList;
			unitTypesList.Sort();

			CombineAssertions(() =>
			{
				AssertEquals("BreakBulkOnlyPackingUnitTypesList CodesAsString", "NE, NF", unitTypesList.CodesAsString);
				AssertSame("Cached", unitTypesList, package.Lookups.BreakBulkOnlyPackingUnitTypesList);
			});
		}

		void SetUpPackageTypes()
		{
			const string dataGrouping = Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations;
			const string codeType = RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "UN Package code List");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		}
	}
}

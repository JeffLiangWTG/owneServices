using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class TemporaryStorageSupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"AAAA",
				"ENS",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"BBBB",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"CCCC",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"DDDD",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("AMA_RN_NKCountry must be LV", Core.Constants.CountryCodes.Latvia, storageHeader.AMA_RN_NKCountry);
			var bill = storageHeader.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();
			AssertEquals(0, supportingDocument.Lookups.CodeList.Count);
			var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals("List should be filled with EUN or FR codes of type DC44T", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "AAAA", "BBBB", "CCCC" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestCodeList_ParentIsBillItem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"AAAA",
				"ENS",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"BBBB",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"CCCC",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(
				Core.Constants.CountryCodes.Germany,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6,
				"DDDD",
				"Transit",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("AMA_RN_NKCountry must be LV", Core.Constants.CountryCodes.Latvia, storageHeader.AMA_RN_NKCountry);
			var bill = storageHeader.Bills.AddNew();
			var billPackedItem = bill.PackedItems.AddNew();
			var itemSupportingDocument = billPackedItem.SupportingDocuments.AddNew();
			AssertEquals(0, itemSupportingDocument.Lookups.CodeList.Count);
			var collection = (ZZRefCusCodeListCombinedCollection)itemSupportingDocument.Lookups.CodeList;
			collection.Load();
			AssertEquals("List should be filled with EUN or FR codes of type DC44T", 3, collection.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "AAAA", "BBBB", "CCCC" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}
	}
}

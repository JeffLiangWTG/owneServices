using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	class TemporaryStorageAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList_ParentBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();

			var lookups = addInfo.Lookups;
			var list = lookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", new[] { AdditionalInfoSubTypeList.Codes.AdditionalInformation, AdditionalInfoSubTypeList.Codes.AdditionalReference, AdditionalInfoSubTypeList.Codes.TransportDocument }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}

		public void TestSubTypeList_ParentPackedItem()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var addInfo = packedItem.AdditionalInfos.AddNew();

			var lookups = addInfo.Lookups;
			var list = lookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", new[] { AdditionalInfoSubTypeList.Codes.AdditionalInformation, AdditionalInfoSubTypeList.Codes.AdditionalReference, AdditionalInfoSubTypeList.Codes.TransportDocument }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}
	}
}

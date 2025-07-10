using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(OwnerOfGoodsCollection))]
	sealed class OwnerOfGoodsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new OwnerOfGoodsCollection(Factory.New<CusEntryInstruction>());

		public void TestOwnersOfGoodsJobDocAddressCollectionMaxCount()
		{
			AssertEquals(98, GetCollectionToTest().MaxCount);
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (OwnerOfGoodsCollection)GetCollectionToTest();
			var place = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("E2_AddressType", DocAddressTypes.Codes.OwnerOfGoods, place.E2_AddressType);
				AssertEquals("E2_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, place.E2_ParentTableCode);
				AssertEquals("HasChanges", false, place.HasChanges);
			});
		}
	}
}

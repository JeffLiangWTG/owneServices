using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePackedItem))]
	public class TemporaryStoragePackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalInfos()
		{
			AssertType<TemporaryStorageAdditionalInfo>("Should return IE TemporaryStorageAdditionalInfo for AdditionalInfos.", packedItem.AdditionalInfos.AddNew());
		}

		public void TestValidationType()
		{
			AssertType<TemporaryStoragePackedItemValidation>(packedItem.Validation);
		}

		protected override BusinessObject GetNewBusinessObject() => packedItem;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => packedItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => packedItem;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
		}

		TemporaryStoragePackedItem packedItem;
	}
}

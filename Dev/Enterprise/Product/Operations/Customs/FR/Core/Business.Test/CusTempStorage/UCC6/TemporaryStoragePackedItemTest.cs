using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePackedItem))]
	sealed class TemporaryStoragePackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var packedItem = GetTemporaryStoragePackedItem();
			AssertEquals("Default ABL_GrossWeightUQ should be Kilograms.", Core.Constants.Weight.Kilograms, packedItem.API_GrossWeightUQ);
		}

		public void TestReadOnly_API_GrossWeightUQ()
		{
			var packedItem = GetTemporaryStoragePackedItem();
			AssertEquals("API_GrossWeightUQ should be read only.", true, packedItem.API_GrossWeightUQInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => GetTemporaryStoragePackedItem();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetTemporaryStoragePackedItem();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetTemporaryStoragePackedItem();

		TemporaryStoragePackedItem GetTemporaryStoragePackedItem()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			return bill.PackedItems.AddNew();
		}
	}
}

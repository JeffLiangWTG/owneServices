using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageBill))]
	sealed class TemporaryStorageBillTest : EU.Business.CusTempStorage.Testing.TemporaryStorageBillAbstractTest<TemporaryStorageBill, TemporaryStorageHeader>
	{
		public void TestPackedItems()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertType<TemporaryStoragePackedItem>("Should get TemporaryStoragePackedItem for PackedItems.", bill.PackedItems.AddNew());
		}

		public void TestGetPackedItemType()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertEquals("Should get TemporaryStoragePackedItem type.", typeof(TemporaryStoragePackedItem), bill.GetPackedItemType());
		}

		public void TestGetPackType()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertEquals("Should get TemporaryStoragePack type.", typeof(TemporaryStoragePack), bill.GetPackType());
		}

		public void TestSetDefaultValues()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertEquals("Default ABL_GrossWeightUQ should be Kilograms.", Core.Constants.Weight.Kilograms, bill.ABL_GrossWeightUQ);
		}

		public void TestReadOnly_ABL_GrossWeightUQ()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertEquals("ABL_GrossWeightUQ should be read only.", true, bill.ABL_GrossWeightUQInfo.ReadOnly);
		}

		public void TestTypeOfPackedItems()
		{
			var bill = (TemporaryStorageBill)GetNewBusinessObject();
			AssertType<EU.Business.CusTempStorage.TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>>("Should get TemporaryStoragePackedItemCollection for PackedItems.", bill.PackedItems);
		}
	}
}

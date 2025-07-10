using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePackedItemCollection<,>))]
	public class TemporaryStoragePackedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			return new TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>(bill);
		}

		public void TestAllowNew()
		{
			var cusSupportingInfoCollection = GetCollectionToTest();
			AssertEquals(99999, cusSupportingInfoCollection.MaxCount);
		}

		public void TestSequenceNumberCalculator()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItems = bill.PackedItems;
			AssertType<HugeSequenceNumberGenerator>(packedItems.SequenceNumberCalculator);
		}
	}
}

using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickOrderedInventoryWrapperCollection))]
	sealed class WarehousePickOrderedInventoryWrapperCollectionTest : GenericWrapperCollectionTest<WarehousePickOrderedInventoryWrapperCollection>
	{
		protected override WarehousePickOrderedInventoryWrapperCollection GetCollectionToTest()
		{
			return new WarehousePickOrderedInventoryWrapperCollection(Factory);
		}

		protected override WarehousePickOrderedInventoryWrapperCollection GetNewDocumentWrapperCollection()
		{
			WhsPick pick = Factory.New<WhsPick>();
			WhsPickOrderedInventoryCollection collection = new WhsPickOrderedInventoryCollection(Factory, pick);
			return new WarehousePickOrderedInventoryWrapperCollection(collection, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			WhsPickOrderedInventory orderedInventory = new WhsPickOrderedInventory(Factory);
			return new WarehousePickOrderedInventoryWrapper(orderedInventory, Factory);
		}
	}
}

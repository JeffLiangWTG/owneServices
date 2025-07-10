using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseInventoryWrapperCollection))]
	sealed class WarehouseInventoryWrapperCollectionTest : GenericWrapperCollectionTest<WarehouseInventoryWrapperCollection>
	{
		protected override WarehouseInventoryWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseInventoryWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseInventoryWrapper(Factory.NewWithValidTestData<WhsInventoryView>().InDocketLine, Factory);
		}
	}
}

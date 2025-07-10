using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickingSlipLineWrapperCollection))]
	sealed class WarehousePickingSlipLineWrapperCollectionTest : GenericWrapperCollectionTest<WarehousePickingSlipLineWrapperCollection>
	{
		protected override WarehousePickingSlipLineWrapperCollection GetCollectionToTest()
		{
			return new WarehousePickingSlipLineWrapperCollection(Factory);
		}
		protected override WarehousePickingSlipLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehousePickingSlipLineWrapperCollection(Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehousePickingSlipLineWrapper(Factory.NewWithValidTestData<WhsPickLine>(), Factory);
		}
	}
}

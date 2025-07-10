using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseJobEmptyWrapperCollection))]
	sealed class WarehouseJobEmptyWrapperCollectionTest : WarehouseJobGenericWrapperCollectionTest<WarehouseJobEmptyWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseJobEmptyWrapper(null, Factory);
		}

		protected override WarehouseJobEmptyWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseJobEmptyWrapperCollection(Factory);
		}
	}
}

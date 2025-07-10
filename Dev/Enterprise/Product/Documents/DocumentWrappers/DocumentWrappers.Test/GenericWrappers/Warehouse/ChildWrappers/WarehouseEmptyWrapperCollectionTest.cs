using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseEmptyWrapperCollection))]
	sealed class WarehouseEmptyWrapperCollectionTest : WarehouseGenericWrapperCollectionTest<WarehouseEmptyWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseEmptyJobLineWrapper(null, Factory);
		}

		protected override WarehouseEmptyWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseEmptyWrapperCollection(Factory);
		}
	}
}

using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickableDocketWrapperCollection))]
	sealed class WarehousePickableDocketWrapperCollectionTest : WarehouseJobGenericWrapperCollectionTest<WarehousePickableDocketWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseOrderWrapper(null, Factory);
		}

		protected override WarehousePickableDocketWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehousePickableDocketWrapperCollection(Factory);
		}
	}
}

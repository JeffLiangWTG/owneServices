using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseBOMStagingLocationPartWrapperCollection))]
	sealed class WarehouseBOMStagingAreaPartWrapperCollectionTest : WarehouseGenericWrapperCollectionTest<WarehouseBOMStagingLocationPartWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseBOMStagingLocationPartWrapper(null, Factory);
		}

		protected override WarehouseBOMStagingLocationPartWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseBOMStagingLocationPartWrapperCollection(Factory);
		}
	}
}

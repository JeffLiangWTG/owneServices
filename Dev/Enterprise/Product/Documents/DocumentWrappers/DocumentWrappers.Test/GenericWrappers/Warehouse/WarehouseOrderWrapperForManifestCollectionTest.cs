using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseOrderWrapperForManifestCollection))]
	sealed class WarehouseOrderWrapperForManifestCollectionTest : WarehouseJobGenericWrapperCollectionTest<WarehouseOrderWrapperForManifestCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new WarehouseOrderWrapperForManifest(null, Factory);
		}

		protected override WarehouseOrderWrapperForManifestCollection GetNewDocumentWrapperCollection()
		{
			return new WarehouseOrderWrapperForManifestCollection(Factory);
		}
	}
}

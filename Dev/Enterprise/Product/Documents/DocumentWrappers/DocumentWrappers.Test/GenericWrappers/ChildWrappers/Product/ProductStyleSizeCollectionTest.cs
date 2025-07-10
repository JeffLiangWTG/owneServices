using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductStyleSizeCollection))]
	sealed class ProductStyleSizeCollectionTest : GenericWrapperCollectionTest<ProductStyleSizeCollection>
	{
		#region Implementation

		protected override ProductStyleSizeCollection GetNewDocumentWrapperCollection()
		{
			return new ProductStyleSizeCollection(null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ProductStyleSizeWrapper(null, Factory);
		}

		#endregion
	}
}

using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperCollectionForLoadManifest))]
	internal class PackageWrapperCollectionForLoadManifestTest : PackageWrapperCollectionTest
	{
		#region Implementation

		protected override PackageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PackageWrapperCollectionForLoadManifest(null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PackageWrapperForLoadManifest(null, Factory);
		}

		#endregion
	}
}

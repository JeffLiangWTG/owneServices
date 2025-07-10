using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsPackageLabelCollection))]
	sealed class DocWhsPackageLabelCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocWhsPackageLabelCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsLabel label = new WhsLabel();
			return DocWhsPackageLabel.New(label, Factory);
		}

		protected override DocWhsPackageLabelCollection GetCollectionToTest()
		{
			return new DocWhsPackageLabelCollection(Factory);
		}
	}
}

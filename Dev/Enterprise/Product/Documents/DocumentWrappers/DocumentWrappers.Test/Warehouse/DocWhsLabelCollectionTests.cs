using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsLabelCollection))]
	sealed class DocWhsLabelCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocWhsLabelCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsLabel label = new WhsLabel();
			return DocWhsLabel.New(label, Factory);
		}

		protected override DocWhsLabelCollection GetCollectionToTest()
		{
			return new DocWhsLabelCollection(Factory);
		}
	}
}

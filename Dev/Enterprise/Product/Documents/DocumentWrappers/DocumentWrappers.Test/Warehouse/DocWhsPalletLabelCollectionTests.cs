using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsPalletLabelCollection))]
	sealed class DocWhsPalletLabelCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocWhsPalletLabelCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsLabel label = new WhsLabel();
			return DocWhsPalletLabel.New(label, Factory);
		}

		protected override DocWhsPalletLabelCollection GetCollectionToTest()
		{
			return new DocWhsPalletLabelCollection(Factory);
		}
	}
}

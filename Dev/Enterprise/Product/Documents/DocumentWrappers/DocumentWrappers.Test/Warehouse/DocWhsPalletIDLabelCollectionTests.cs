using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsPalletIDLabelCollection))]
	sealed class DocWhsPalletIDLabelCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocWhsPalletIDLabelCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsLabel label = new WhsLabel();
			return DocWhsPalletIDLabel.New(label, Factory);
		}

		protected override DocWhsPalletIDLabelCollection GetCollectionToTest()
		{
			return new DocWhsPalletIDLabelCollection(Factory);
		}
	}
}

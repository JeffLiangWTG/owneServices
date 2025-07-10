using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickOrderedInventoryCollection))]
	sealed class DocWhsPickOrderedInventoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsPickOrderedInventoryCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsPickOrderedInventory itemToPick = new WhsPickOrderedInventory(Factory);
			return DocWhsPickOrderedInventory.New(itemToPick, Factory);
		}

		protected override DocWhsPickOrderedInventoryCollection GetCollectionToTest()
		{
			return new DocWhsPickOrderedInventoryCollection(Factory);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InventorySelectionLineCollection))]
sealed class InventorySelectionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InventorySelectionLineCollection>
{
	public void TestCreateNonPersistentBusinessObject()
	{
		var inventorySelectionLineCollection = GetCollectionToTest();
		AssertType<InventorySelectionLine>("CreateNonPersistentBusinessObject Type", inventorySelectionLineCollection.AddNew());
	}

	protected override InventorySelectionLineCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		return new InventorySelectionLineCollection(inventorySelectionHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		return new InventorySelectionLine(inventorySelectionHeader);
	}
}

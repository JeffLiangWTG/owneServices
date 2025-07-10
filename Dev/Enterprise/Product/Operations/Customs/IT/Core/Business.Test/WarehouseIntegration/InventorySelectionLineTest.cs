using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(InventorySelectionLine))]
sealed class InventorySelectionLineTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var inventorySelectionHeader = new InventorySelectionHeader(declaration);
		return new InventorySelectionLine(inventorySelectionHeader);
	}
}

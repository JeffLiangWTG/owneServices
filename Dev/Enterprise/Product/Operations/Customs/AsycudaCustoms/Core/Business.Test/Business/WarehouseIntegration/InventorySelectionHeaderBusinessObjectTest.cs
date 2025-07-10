using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(InventorySelectionHeader))]
	class InventorySelectionHeaderBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsGroupByCartonSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			var inventorySelectionHeader = new InventorySelectionHeader(declaration);
			Assert(!inventorySelectionHeader.IsGroupByCartonSupported);
		}

		protected override BusinessObject GetNewBusinessObject() => new InventorySelectionHeader(Factory.New<JobDeclaration>());
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InventorySelectionLineCollection))]
	sealed class InventorySelectionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InventorySelectionLineCollection>
	{
		protected override InventorySelectionLineCollection GetCollectionToTest() => new InventorySelectionLineCollection(HeaderMock.Object);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new InventorySelectionLine(HeaderMock.Object);

		Mock<DeclarationInventorySelectionHeader> HeaderMock => headerMock ?? (headerMock = new Mock<DeclarationInventorySelectionHeader>(Declaration) { CallBase = true });
		Mock<DeclarationInventorySelectionHeader> headerMock;

		BaseJobDeclaration Declaration => declaration ?? (declaration = Factory.New<BaseJobDeclaration>());
		BaseJobDeclaration declaration;
	}
}

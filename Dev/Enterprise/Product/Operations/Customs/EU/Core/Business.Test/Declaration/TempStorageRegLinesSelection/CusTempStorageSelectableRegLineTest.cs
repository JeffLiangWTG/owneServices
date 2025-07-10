using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusTempStorageSelectableRegLine))]

	sealed class CusTempStorageSelectableRegLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var regLineMock = new Mock<Integration.Customs.EU.ICusTempStorageRegLine>();
			var itemQuantitiyCollectionMock = new Mock<IRegLineItemQuantityCollection<IRegLineItemQuantity>>();
			regLineMock.Setup(r => r.RegLineItemQuantities).Returns(itemQuantitiyCollectionMock.Object);
			return new CusTempStorageSelectableRegLine(regLineMock.Object);
		}
	}
}

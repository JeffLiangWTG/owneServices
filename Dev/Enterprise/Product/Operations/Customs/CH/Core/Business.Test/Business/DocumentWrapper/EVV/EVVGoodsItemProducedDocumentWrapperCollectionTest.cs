using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemProducedDocumentWrapperCollection))]
sealed class EVVGoodsItemProducedDocumentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemProducedDocumentWrapperCollection>
{
	protected override EVVGoodsItemProducedDocumentWrapperCollection GetCollectionToTest() => EVVGoodsItemProducedDocumentWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var producedDocumentMock = new Mock<IEvvGoodsItemProducedDocument>();
		return EVVGoodsItemProducedDocumentWrapper.New(producedDocumentMock.Object, Factory);
	}
}

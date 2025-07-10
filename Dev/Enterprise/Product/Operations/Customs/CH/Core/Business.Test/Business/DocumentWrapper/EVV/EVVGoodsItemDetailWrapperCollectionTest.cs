using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemDetailWrapperCollection))]
sealed class EVVGoodsItemDetailWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemDetailWrapperCollection>
{
	protected override EVVGoodsItemDetailWrapperCollection GetCollectionToTest() => EVVGoodsItemDetailWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var detailMock = new Mock<IEvvGoodsItemDetail>();
		return EVVGoodsItemDetailWrapper.New(detailMock.Object, Factory);
	}
}

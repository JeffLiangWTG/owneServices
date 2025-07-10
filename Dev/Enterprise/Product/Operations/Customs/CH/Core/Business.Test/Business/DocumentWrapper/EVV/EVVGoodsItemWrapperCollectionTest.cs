using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemWrapperCollection))]
sealed class EVVGoodsItemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemWrapperCollection>
{
	protected override EVVGoodsItemWrapperCollection GetCollectionToTest() => EVVGoodsItemWrapperCollection.New(null, Factory, SwissCustomsLanguageList.Codes.German);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var goodsItemMock = new Mock<IEvvGoodsItem>();
		return EVVGoodsItemWrapper.New(goodsItemMock.Object, Factory, SwissCustomsLanguageList.Codes.German);
	}
}

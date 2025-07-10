using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemPackagingWrapperCollection))]
sealed class EVVGoodsItemPackagingWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemPackagingWrapperCollection>
{
	protected override EVVGoodsItemPackagingWrapperCollection GetCollectionToTest() => EVVGoodsItemPackagingWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var packagingMock = new Mock<IEvvGoodsItemPackaging>();
		return EVVGoodsItemPackagingWrapper.New(packagingMock.Object, Factory);
	}
}

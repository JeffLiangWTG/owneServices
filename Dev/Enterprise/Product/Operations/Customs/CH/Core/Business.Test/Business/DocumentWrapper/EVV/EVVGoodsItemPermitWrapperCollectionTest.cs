using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemPermitWrapperCollection))]
sealed class EVVGoodsItemPermitWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemPermitWrapperCollection>
{
	protected override EVVGoodsItemPermitWrapperCollection GetCollectionToTest() => EVVGoodsItemPermitWrapperCollection.New(null, Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var permitMock = new Mock<IEvvGoodsItemPermit>();
		return EVVGoodsItemPermitWrapper.New(permitMock.Object, Factory);
	}
}

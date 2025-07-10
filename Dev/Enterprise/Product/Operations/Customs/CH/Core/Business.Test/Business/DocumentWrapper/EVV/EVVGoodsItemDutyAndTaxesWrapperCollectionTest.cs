using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVGoodsItemDutyAndTaxesWrapperCollection))]
sealed class EVVGoodsItemDutyAndTaxesWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVGoodsItemDutyAndTaxesWrapperCollection>
{
	protected override EVVGoodsItemDutyAndTaxesWrapperCollection GetCollectionToTest() => EVVGoodsItemDutyAndTaxesWrapperCollection.New(null, Factory, SwissCustomsLanguageList.Codes.German);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var dutyAmountMock = new Mock<IEvvGoodsItemDutyOrTax>();
		return EVVGoodsItemDutyOrTaxWrapper.New(dutyAmountMock.Object, Factory, SwissCustomsLanguageList.Codes.German);
	}
}

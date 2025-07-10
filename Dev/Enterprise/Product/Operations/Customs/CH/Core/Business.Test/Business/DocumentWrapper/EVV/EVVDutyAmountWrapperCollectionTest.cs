using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EVVDutyAmountWrapperCollection))]
class EVVDutyAmountWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EVVDutyAmountWrapperCollection>
{
	protected override EVVDutyAmountWrapperCollection GetCollectionToTest() => EVVDutyAmountWrapperCollection.New(null, Factory, SwissCustomsLanguageList.Codes.German);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var dutyAmountMock = new Mock<IEvvDutyAmount>();
		return EVVDutyAmountWrapper.New(dutyAmountMock.Object, Factory, SwissCustomsLanguageList.Codes.German);
	}
}

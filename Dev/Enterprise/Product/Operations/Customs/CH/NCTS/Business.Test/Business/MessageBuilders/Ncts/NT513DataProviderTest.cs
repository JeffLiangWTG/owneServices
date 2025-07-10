using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NT513DataProviderTest : BaseTransitDeclarationDataProviderTest<NT513DataProvider, INT513>
{
	protected override NT513DataProvider CreateDataProvider(NctsHeaderDepartureMessageSendingObject messageSendingObject) => new NT513DataProvider(messageSendingObject);

	public void TestJustification() => CombineAssertions(() =>
	{
		MessageSendingObject.ReasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others;
		AssertNotNull(DataProvider.Justification);
		AssertSame("cached", DataProvider.Justification, DataProvider.Justification);
	});
}

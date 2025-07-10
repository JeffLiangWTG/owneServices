using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT013DataProviderTest : BaseTransitDeclarationDataProviderTest<NT013DataProvider, INT013>
{
	protected override NT013DataProvider CreateDataProvider(NctsHeaderDepartureMessageSendingObject messageSendingObject) => new NT013DataProvider(messageSendingObject);

	public void TestJustification() => CombineAssertions(() =>
	{
		MessageSendingObject.ReasonCode = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others;
		AssertNotNull(DataProvider.Justification);
		AssertSame("cached", DataProvider.Justification, DataProvider.Justification);
	});
}

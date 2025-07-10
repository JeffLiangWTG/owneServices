using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NT515DataProviderTest : BaseTransitDeclarationDataProviderTest<NT515DataProvider, INT515>
{
	protected override NT515DataProvider CreateDataProvider(NctsHeaderDepartureMessageSendingObject messageSendingObject) => new NT515DataProvider(messageSendingObject);
}

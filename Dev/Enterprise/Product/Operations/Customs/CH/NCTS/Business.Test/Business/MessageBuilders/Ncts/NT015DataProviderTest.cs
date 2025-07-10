using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NT015DataProviderTest : BaseTransitDeclarationDataProviderTest<NT015DataProvider, INT015>
{
	protected override NT015DataProvider CreateDataProvider(NctsHeaderDepartureMessageSendingObject messageSendingObject) => new NT015DataProvider(messageSendingObject);
}

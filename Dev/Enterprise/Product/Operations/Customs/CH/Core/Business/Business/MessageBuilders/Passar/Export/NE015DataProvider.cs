using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class NE015DataProvider : BasePassarExportDeclarationDataProvider, INE015
{
	public NE015DataProvider(DeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}
}

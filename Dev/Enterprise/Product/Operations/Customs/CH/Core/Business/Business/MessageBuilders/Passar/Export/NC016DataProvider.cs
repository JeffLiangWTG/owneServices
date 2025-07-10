using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class NC016DataProvider : BasePassarDeclarationMessageDataProvider, INC016
{
	public NC016DataProvider(ExportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public string ExportOperationGDRN => entryHeader?.EntryNumber ?? string.Empty;

	public string TransitOperationMRN => string.Empty;

	public string TransportOperationJRN => string.Empty;
}

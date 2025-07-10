using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class NE014DataProvider : BasePassarDeclarationMessageDataProvider, INE014
{
	public NE014DataProvider(ExportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	ExportDeclarationMessageSendingObject SendingObject => (ExportDeclarationMessageSendingObject)base.sendingObject;

	public IExportOperation ExportOperation => exportOperation ??= ExportOperationDataProvider.New(entryHeader);
	IExportOperation exportOperation;

	public IJustification Justification => justification ??= JustificationDataProvider.New(SendingObject);
	IJustification justification;
}

using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public abstract class AESMessageHeaderProvider : MessageHeaderProvider, IAESMessageHeader
{
	protected AESMessageHeaderProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
		exportEntryMessageSendingAction = messageSendingAction;
	}
	readonly ExportEntryMessageSendingAction exportEntryMessageSendingAction;

	public IExportOperation ExportOperation => exportOperation ?? (exportOperation = new ExportOperationProvider(exportEntryMessageSendingAction));
	IExportOperation exportOperation;

	public string LanguageCode => declaration.JE_DeclarationLanguage;

	public override string MessageSender => BECustomsRegistry.Instance.SenderIDs.GetTargetSystemName(SendMessageTypes.Codes.AES);

	public override string MessageRecipient => BECustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(SendMessageTypes.Codes.AES);
}

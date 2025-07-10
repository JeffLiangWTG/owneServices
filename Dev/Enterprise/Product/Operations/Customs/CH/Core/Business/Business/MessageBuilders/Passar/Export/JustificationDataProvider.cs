using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class JustificationDataProvider : IJustification
{
	public static JustificationDataProvider New(ExportDeclarationMessageSendingObject sendingObject) => IsNullOrNoReason(sendingObject) ? null : new JustificationDataProvider(sendingObject);

	static bool IsNullOrNoReason(ExportDeclarationMessageSendingObject sendingObject) => sendingObject == null || sendingObject.VOCReason.IsEmpty;

	JustificationDataProvider(ExportDeclarationMessageSendingObject sendingObject)
	{
		this.sendingObject = sendingObject;
	}
	readonly ExportDeclarationMessageSendingObject sendingObject;

	public string Code => sendingObject.VOCReason;

	public string Text => sendingObject.ReasonText.ReturnNullIfEmpty();
}

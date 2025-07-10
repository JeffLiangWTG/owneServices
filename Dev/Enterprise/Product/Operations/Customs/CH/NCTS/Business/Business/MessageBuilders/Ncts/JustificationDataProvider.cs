using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class JustificationDataProvider : IJustification
{
	public static JustificationDataProvider New(NctsHeaderDepartureMessageSendingObject sendingObject) => IsNullOrEmpty(sendingObject) ? null : new JustificationDataProvider(sendingObject);

	static bool IsNullOrEmpty(NctsHeaderDepartureMessageSendingObject sendingObject) => sendingObject == null || sendingObject.ReasonCode.IsEmpty;

	JustificationDataProvider(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		this.sendingObject = sendingObject;
	}
	readonly NctsHeaderDepartureMessageSendingObject sendingObject;

	public string Code => sendingObject.ReasonCode;

	public string Text => sendingObject.ReasonText;
}

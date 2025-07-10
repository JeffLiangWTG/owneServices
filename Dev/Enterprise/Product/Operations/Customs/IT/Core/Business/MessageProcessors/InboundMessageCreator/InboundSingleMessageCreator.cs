using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.Business;

sealed class InboundSingleMessageCreator
{
	public EDIMessage CreateMessageForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var newMessage = interchange.ContainedMessages.AddNew();
		newMessage.EM_IsTestMessage = true;
		newMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
		newMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		newMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		newMessage.EM_MessageText = interchange.EI_BodyText;
		newMessage.EM_MessageType = interchange.EI_InterchangeType;
		return newMessage;
	}
}

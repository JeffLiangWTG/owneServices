using System.Linq;
 using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Messaging.Business;
 using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public static class ParentFinderHelper
	{
		public static ZString GetMessageStatus(ZString eventReference)
		{
			ZString result;
			switch (eventReference)
			{
				case CustomsMessageStatusList.Codes.MessageAcknowledged:
				case CustomsMessageStatusList.Codes.MessageDeliveredToCustoms:
					result = EDIMessageStatusList.Codes.Acknowledged;
					break;
				case CustomsMessageStatusList.Codes.MessageNotAcknowledged:
				case CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms:
				case CustomsMessageStatusList.Codes.MessageRejected:
					result = EDIMessageStatusList.Codes.Rejected;
					break;
				default:
					result = ZString.Empty;
					break;
			}
			return result;
		}

		public static void UpdateMatchingOutgoingMessageStatus(IFRMessagesOwner owner, ZString outgoingInterchangeNumber, ZString messageStatus)
		{
			if (!outgoingInterchangeNumber.IsEmpty && !messageStatus.IsEmpty)
			{
				var outgoingMessage = owner.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.IsTransmitMessage && x.EM_InterchangeNumber == outgoingInterchangeNumber);
				if (outgoingMessage != null && outgoingMessage.EM_Status != messageStatus)
				{
					outgoingMessage.EM_Status = messageStatus;
				}
			}
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public interface IMessageSendingObject
	{
		BusinessObjectFactory Factory { get; }

		ZString MessageType { get; set; }

		ZString GetMessageOwner();

		ZString GetMessageTypeForEDIMessage();

		ZString GetMessageText();

		ZGuid GetGlbExternalPasswordPK();

		ZString GetApplicationReference();

		BusinessObject MessageAttachee { get; }
	}

	public static class IMessageSendingObjectExtension
	{
		public static EDIMessage CreateCustomsMessage(this IMessageSendingObject sendingObject)
		{
			var message = sendingObject.Factory.New<BREDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = sendingObject.GetMessageTypeForEDIMessage();
			message.EM_MessageSubType = sendingObject.MessageType;
			message.EM_MessageOwner = sendingObject.GetMessageOwner();
			message.EM_MessageText = sendingObject.GetMessageText();
			message.EM_GP = sendingObject.GetGlbExternalPasswordPK();
			message.EM_ApplicationReference = sendingObject.GetApplicationReference();
			message.EM_LinkedObject = sendingObject.MessageAttachee;

			return message;
		}
	}
}

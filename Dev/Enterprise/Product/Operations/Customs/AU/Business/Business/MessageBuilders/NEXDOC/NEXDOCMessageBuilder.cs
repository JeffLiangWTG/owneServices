using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class NEXDOCMessageBuilder
	{
		protected NEXDOCMessageBuilder(INEXDOCMessageParent parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
			Factory = parent.Factory;
		}

		protected readonly INEXDOCMessageParent Parent;
		protected readonly BusinessObjectFactory Factory;

		protected virtual XmlEDIMessage CreateNewMessage(string messageText)
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NEXDOCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageType = Constants.MessageType.NEXDOCS;
			message.EM_LinkedObject = Parent as BusinessObject;
			message.EM_MessageText = messageText;
			message.EM_ApplicationReference = Parent.RexNumber;

			return message;
		}

		protected IdentificationType CreateNewIdentification()
		{
			return new IdentificationType
			{
				rexNumber = Parent.RexNumber
			};
		}
	}
}

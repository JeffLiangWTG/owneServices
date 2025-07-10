using CargoWise.Customs.IE.MessageContracts.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.ExitControl.Business.AES;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ExitControlMessageSender : IE.Business.MessageSender
	{
		public ExitControlMessageSender(IExitControlMessageSendingAction sendingObject)
			: base(sendingObject)
		{
			SendingObject = sendingObject;
		}

		protected IExitControlMessageSendingAction SendingObject { get; }

		protected override IE.Business.OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<IE.Business.AESOutboundEDIMessage>();

		protected override IXmlMessageBuilder CreateMessageBuilder(IE.Business.OutboundEDIMessage relatingMessage)
		{
			IXmlMessageBuilder result = null;
			switch (relatingMessage.EM_MessageType)
			{
				case AESOutgoingMessageTypeList.Codes.ArrivalAtExit:
					result = new IE507MessageBuilder(new IE507MessageProvider(SendingObject.MessagingObject));
					break;
				case AESOutgoingMessageTypeList.Codes.ExitNotification:
					result = new IE590MessageBuilder(new IE590MessageProvider(SendingObject.MessagingObject));
					break;
				case AESOutgoingMessageTypeList.Codes.InformationOnNonExitedExport:
					result = new IE583MessageBuilder(new IE583MessageProvider(SendingObject.MessagingObject));
					break;
				default:
					break;
			}
			return result;
		}
	}
}

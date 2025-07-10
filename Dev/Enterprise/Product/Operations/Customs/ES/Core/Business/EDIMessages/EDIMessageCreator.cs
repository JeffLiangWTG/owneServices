using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.EDIMessages
{
	public class EDIMessageCreator
	{
		public EDIMessageCreator(IMessageBuilderBase messageBuilder, BusinessObjectFactory factory)
		{
			this.messageBuilder = Argument.NotNull(messageBuilder, nameof(messageBuilder));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly IMessageBuilderBase messageBuilder;
		readonly BusinessObjectFactory factory;

		public ESEDIMessage CreateMessage()
		{
			var messageToSend = factory.New<ESEDIMessage>();
			messageToSend.EM_MessageType = messageBuilder.MessageType;
			messageToSend.EM_MessageSubType = messageBuilder.MessageSubType;
			messageToSend.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageToSend.EM_MessageText = messageBuilder.GetSignedMessageText();
			messageToSend.EM_Status = EDIMessage.Status.Queued;
			messageToSend.EM_IsTestMessage = messageBuilder.Provider.IsTest;
			messageToSend.EM_ApplicationReference = messageBuilder.Provider.CertificateName;
			messageToSend.BusinessObjectReference = messageBuilder.Provider.BusinessObjectReference;
			messageToSend.EM_GP = messageBuilder.Provider.CertificatePK;

			return messageToSend;
		}
	}
}

using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IE.Business
{
	public abstract class MessageSender
	{
		protected MessageSender(IMessageSendingAction sendingAction)
		{
			SendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
		}

		public OutboundEDIMessage Send()
		{
			var messageAttachee = SendingAction.MessageAttachee;
			var messageType = SendingAction.MessageType;

			var newMessage = CreateOutboundEDIMessage(messageAttachee.Factory);
			var branch = messageAttachee.Branch;
			newMessage.EM_GB = branch.PK;
			newMessage.EM_MessageType = messageType;

			PreSend();

			var messageBuilder = CreateMessageBuilder(newMessage);
			if (messageBuilder == null)
			{
				newMessage.Delete();
				return null;
			}

			var xmlMessage = messageBuilder.GenerateXmlMessage();
			newMessage.EM_MessageText = SendingAction.MessageCreated(xmlMessage.GetSerializedString());
			newMessage.EM_Status = EDIMessage.Status.Queued;
			newMessage.EM_GP = branch.Company.GetCredentialPK();

			messageAttachee.LogicalStatus = LogicalStatusList.Codes.Sent;
			SendingAction.AddMessage(newMessage);

			LogEvents();

			return newMessage;
		}

		protected abstract IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage);

		protected abstract OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory);

		protected IMessageSendingAction SendingAction { get; }

		protected virtual void PreSend()
		{
		}

		protected virtual void LogEvents()
		{
		}
	}
}

using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.EU.H7.Business
{
	public abstract class MessageSender
	{
		public MessageSender(IH7MessageSendingObject sendingObject)
		{
			SendingObject = sendingObject;
		}

		protected IH7MessageSendingObject SendingObject { get; }

		public EDIMessage Send()
		{
			var bill = SendingObject.Bill;
			var factory = bill.Factory;
			var newMessage = CreateOutboundEDIMessage(factory);
			SetupBranchSpecificInfo(newMessage, bill.Header.Branch);

			var messageBuilder = CreateMessageBuilder(newMessage);
			if (messageBuilder != null)
			{
				PopulateMessageDetails(factory, messageBuilder, newMessage);
				SetBillMessageStatus(bill);
			}
			bill.Messages.Add(newMessage);

			return newMessage;
		}

		protected virtual void PopulateMessageDetails(BusinessObjectFactory factory, IXmlMessageBuilder messageBuilder, EDIMessage message)
		{
			var xmlMessage = messageBuilder.GenerateXmlMessage();
			message.EM_MessageText = SendingObject.MessageCreated(xmlMessage.GetSerializedString());
			message.EM_Status = EDIMessage.Status.Queued;
		}

		protected virtual void SetBillMessageStatus(AsycudaBill bill)
		{
		}

		protected virtual void SetupBranchSpecificInfo(EDIMessage message, GlbBranch branch)
		{
			message.EM_MessageType = SendingObject.Action.Left(message.EM_MessageTypeInfo.MaxLength);
			message.EM_GB = branch.PK;
		}

		protected abstract IXmlMessageBuilder CreateMessageBuilder(EDIMessage message);

		protected abstract EDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory);
	}
}

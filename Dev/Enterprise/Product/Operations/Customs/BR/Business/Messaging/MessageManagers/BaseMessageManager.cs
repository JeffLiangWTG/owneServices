using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseMessageManager : SingleMessageManager
	{
		public BaseMessageManager(IMessageSendingObject messageSender)
		{
			this.messageSender = messageSender;
		}
		protected IMessageSendingObject messageSender;

		public override BusinessObject BusinessObject => messageSender as BusinessObject;

		protected abstract string OriginalMessageType { get; }

		protected abstract string AmendmentMessageType { get; }

		protected abstract string WithdrawalMessageType { get; }

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessagesCore(bizo as IMessageSendingObject, OriginalMessageType);
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return GenerateMessagesCore(bizo as IMessageSendingObject, AmendmentMessageType);
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessagesCore(bizo as IMessageSendingObject, WithdrawalMessageType);
		}

		protected override EDIMessage[] GenerateMessagesForAmendmentDetection(BusinessObject bizo)
		{
			return new[] { CreateCustomsMessage(bizo as IMessageSendingObject, OriginalMessageType) };
		}

		public EDIMessage[] GenerateMessages()
		{
			return GenerateMessagesCore(messageSender);
		}

		protected EDIMessage[] GenerateMessagesCore(IMessageSendingObject sendingObject, string forceMessageType = null)
		{
			EDIMessage[] messages = null;

			if (sendingObject != null)
			{
				messages = GenerateCustomsMessage(sendingObject, forceMessageType).ToArray();
			}
			AfterGenerateMessage(messages);
			return messages ?? Array.Empty<EDIMessage>();
		}

		protected virtual IEnumerable<EDIMessage> GenerateCustomsMessage(IMessageSendingObject sendingObject, string forceMessageType = null)
		{
			yield return CreateCustomsMessage(sendingObject, forceMessageType ?? sendingObject.MessageType);
		}

		protected virtual void AfterGenerateMessage(IEnumerable<EDIMessage> messages) { }

		public virtual void RollbackOnSavingFailed() { }

		protected EDIMessage CreateCustomsMessage(string forceMessageType = null) => CreateCustomsMessage(messageSender, forceMessageType);

		protected EDIMessage CreateCustomsMessage(IMessageSendingObject sendingObject, string forceMessageType = null)
		{
			EDIMessage message = null;

			if (sendingObject != null)
			{
				var oldMessageType = sendingObject.MessageType;

				if (!string.IsNullOrEmpty(forceMessageType))
				{
					sendingObject.MessageType = forceMessageType;
				}

				try
				{
					message = sendingObject.CreateCustomsMessage();
				}
				finally
				{
					sendingObject.MessageType = oldMessageType;
				}
			}

			return message;
		}
	}
}

using System;
using System.Configuration;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Integration;

namespace CargoWise.eHub.Gateway
{
	public abstract class InboxMessageHandler : DatabaseMessageHandler
	{
		const string throttleErrorMessage = "The server has refused new messages because it is still processing your previously sent messages. You do not need to take action, the service task will attempt to transfer them on the next run. Please ignore the following message: Too Many Requests not a valid ediEnterprise licence code.";
		static readonly int UnprocessedMessageCountLimit = GetUnprocessedMessageCountLimit();

		public virtual IInboxAccessor NewInboxAccessor
		{
			get { return DataAccessFactories.NewInboxAccessorInstance(); }
		}

		public virtual IExceptionsAccessor NewErrorAccessor
		{
			get { return DataAccessFactories.NewExceptionsAccessorInstance(); }
		}

		protected void InsertMessageToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, bool orderedDelivery = false)
		{
			Execute(() => NewInboxAccessor.InsertToInbox(senderID, envelopeTrackingID, message, orderedDelivery));
		}

		protected void InsertMessageToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPk, bool orderedDelivery = false)
		{
			Execute(() => NewInboxAccessor.InsertToInbox(senderID, envelopeTrackingID, inboxPk, MessageStatus.Received, message, orderedDelivery));
		}

		protected void EnqueueMessage(IMessageQueuer queuer, string senderID, Guid inboxPk, eHubGatewayMessage message)
		{
			DoEnqueue(() => NewInboxAccessor.EnqueueMessage(queuer, senderID, inboxPk, message));
		}

		protected void EnqueueMessage(IMessageQueuerLite queuer, string senderID, Guid inboxPk, eHubGatewayMessage message)
		{
			DoEnqueue(() => NewInboxAccessor.EnqueueMessage(queuer, senderID, inboxPk, message));
		}

		protected void InsertErrorAndUpdateInboxMessageStatus(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK)
		{
			Execute(() => NewErrorAccessor.SubmitErrorAndUpdateStatus(errorPK, source, errorType, description, inboxPK, outboxPK, Guid.Empty, Guid.Empty, null));
		}

		public override void CheckIfTooManyUnprocessedMessages(string senderId, string recipientId)
		{
			var accessor = DataAccessFactories.NewInboxAccessorInstance();
			var unprocessedMessageCount = accessor.GetUnprocessedMessageCount(senderId, recipientId);
			if (unprocessedMessageCount > UnprocessedMessageCountLimit)
			{
				if (Logger.IsInfoEnabled) Logger.InfoFormat("[Throttle declined] There are {0} messages unprocessed sent by Client {1} [IP: {2}] to {3} which exceeds the limit.", unprocessedMessageCount, senderId, ServiceHelper.GetClientIPAddress(), recipientId);
				throw new SystemThrottleException(throttleErrorMessage);
			}
		}
		static int GetUnprocessedMessageCountLimit()
		{
			if (!int.TryParse(ConfigurationManager.AppSettings["UnprocessedMessageCountLimitPerClient"], out var limit))
			{
				limit = 100;
			}
			return limit;
		}
	}
}
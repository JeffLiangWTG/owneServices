using System;
using System.Collections.Concurrent;
using CargoWise.eHub.Common;
using Common.Logging;

namespace CargoWise.eHub.Gateway
{
	public class BillingMessageWithBlacklistingHandler : BillingMessageHandler
	{
		static readonly ILog log = LogManager.GetLogger(typeof(BillingMessageWithBlacklistingHandler));
		protected static readonly ConcurrentDictionary<string, Exception> blacklistedBillingClients = new ConcurrentDictionary<string, Exception>();

		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			if (log.IsDebugEnabled) log.Debug("Processing billing message with blacklisting...");

			if (string.IsNullOrEmpty(senderID))
			{
				if (log.IsDebugEnabled) log.DebugFormat("Processing billing message with blacklisting... SKIPPING. Missing sender ID");
			}
			else
			{
				try
				{
					Handle(senderID, message);
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled) log.Error("Processing billing message with blacklisting... ERROR", ex);
					if (log.IsTraceEnabled) log.Trace(MessageToString(message));
					throw;
				}

				if (log.IsDebugEnabled) log.Debug("Processing billing message with blacklisting... SUCCESS");
			}
		}

		protected override void Handle(string senderID, eHubGatewayMessage message)
		{
			Exception previousException;

			blacklistedBillingClients.TryGetValue(senderID, out previousException);

			if (previousException != null)
			{
				if (log.IsDebugEnabled) log.DebugFormat("Processing billing message with blacklisting... Throwing cached Exception for blacklisted billing client  '{0}'", senderID);
				throw previousException;
			}
			else
			{
				try
				{
					base.Handle(senderID, message);
				}
				catch (BillingTransactionValidationException ex)
				{
					blacklistedBillingClients.TryAdd(senderID, ex);
					if (log.IsDebugEnabled) log.DebugFormat("Processing billing message with blacklisting... Blacklisting billing client  '{0}'", senderID);
					throw;
				}
			}
		}

        internal override void ApplyTransforms(BillingTransaction transaction, string schema)
        {
            BillingMessageHandler.PopulateCategory(transaction);
        }
	}
}

using System.Configuration;
using CargoWise.Billing.API;
using CargoWise.eHub.Common;
using Common.Logging;
using Confluent.Kafka;

namespace CargoWise.eHub.Gateway
{
	public abstract class UsageMessageHandler<T> : KafkaMessageHandler<T> where T : class
	{
		static string BillingUsageKafkaTopic = ConfigurationManager.AppSettings["BillingUsageKafkaTopic"];
		protected override string MessageType => "usage";
		static readonly ILog log = LogManager.GetLogger("UsageMessageHandler");
		protected override ILog Log => log;
		protected override void Handle(string senderID, eHubGatewayMessage message)
		{
			var transaction = DeserializeMessageContent(message);
			SendUsageInfo(message.MessageTrackingID.ToString(), CreateUsageTransaction(transaction));
		}

		public void SendUsageInfo(string trackingId, UsageTransaction transaction)
		{
			try
			{
				if (Log.IsDebugEnabled)
					Log.Debug("Processing usage message...");

				KafkaClient.SendUsageInfoToELK(BillingUsageKafkaTopic, trackingId, transaction, HandleKafkaDeliveryReport);
			}
			catch (ProduceException<string, ELKTransaction> e) when(e.InnerException is ValidationException validationEx)
			{
				throw new BillingTransactionValidationException(validationEx.Errors, validationEx);
			}
		}

		public abstract UsageTransaction CreateUsageTransaction(T transaction);
	}
}

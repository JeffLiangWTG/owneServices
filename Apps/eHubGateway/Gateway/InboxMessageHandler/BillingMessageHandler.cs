using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using CargoWise.eHub.Common;
using CargoWise.Billing.API;
using Common.Logging;
using Confluent.Kafka;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;

namespace CargoWise.eHub.Gateway
{
	public class BillingMessageHandler : KafkaMessageHandler<BillingTransaction>
	{
		protected override string MessageType => "billing";
		static readonly ILog log = LogManager.GetLogger(typeof(BillingMessageHandler));
		static readonly ILog clientMismatchLog = LogManager.GetLogger("ClientMismatch");
		protected override ILog Log => log;

		protected override void Handle(string senderID, eHubGatewayMessage message)
		{
			var transaction = DeserializeMessageContent(message);
			if (clientMismatchLog.IsWarnEnabled && IsCW1Sender(senderID, message) && (!senderID.StartsWith(transaction.ClientID.Substring(0, 3), StringComparison.OrdinalIgnoreCase) || !senderID.EndsWith(transaction.ClientID.Substring(6, 3), StringComparison.OrdinalIgnoreCase)))
			{
				clientMismatchLog.WarnFormat("System ID '{0}' with IP {1} is not match with Auth ID '{2}'. Billing Transaction: {3}", transaction.ClientID, ServiceHelper.GetClientIPAddress(), senderID, transaction.ToString());
			}
			SendBillingInfo(message.MessageTrackingID.ToString(), transaction);
		}

		internal virtual void SendBillingInfo(string trackingId, BillingTransaction transaction)
		{
			try
			{
				var billingTransaction = new APIBillingTransaction
				{
					BillableCount = transaction.BillableCount,
					Branch = transaction.Branch,
					Category = transaction.Category,
					ClientID = transaction.ClientID,
					ClientNumber = transaction.ClientNumber,
					ClientStaffCode = transaction.ClientStaffCode,
					PriceItemCode = transaction.PriceItemCode,
					ReportingSource = transaction.ReportingSource,
					ServiceOccuredUTC = transaction.ServiceOccuredUTC,
					Reference1 = transaction.Reference1,
					Reference2 = transaction.Reference2,
					Reference3 = transaction.Reference3,
					Reference4 = transaction.Reference4,
					Reference5 = transaction.Reference5,
					Version = transaction.Version,
					MessageTrackingID = transaction.MessageTrackingID,
					AdditionalRefs = transaction.AdditionalRefs
				};

				if (Log.IsDebugEnabled) Log.Debug("Processing billing message...");
				if (SendBillingToKafka)
				{
					KafkaClient.SendBillingInfoToKafka(BillingKafkaTopic, trackingId, billingTransaction, HandleKafkaDeliveryReport);
				}
				else
				{
					SendBillingInfoToService(billingTransaction);
					if (Log.IsDebugEnabled)
						Log.Debug("Processing billing message... SUCCESS");
				}
			}
			catch (ValidationException e)
			{
				throw new BillingTransactionValidationException(e.Errors, e);
			}
		}

		void SendBillingInfoToService(APIBillingTransaction transaction)
		{
			using (var client = CreateBillingServiceClient())
			{
				client.AddTransaction(transaction);
			}
		}

		internal virtual bool IsCW1Sender(string senderID, eHubGatewayMessage message)
        {
            var result = true;
            if (!string.IsNullOrEmpty(senderID) && senderID.Length != 9)
            {
                result = false;
            }
            else if (senderID != message.ClientID && senderID.Length == 9)
            {
                var licenceType = "";
                result = TryLoadEnterpriseLicenceType(message.ClientID, out licenceType);
            }
            return result;
        }

		protected static void PopulateCategory(BillingTransaction transaction)
		{
			if (transaction.PriceItemCode.StartsWith("IC", true, CultureInfo.InvariantCulture) ||
					transaction.PriceItemCode.StartsWith("IU", true, CultureInfo.InvariantCulture) ||
					(transaction.PriceItemCode == "EAO" && transaction.Reference2 == null))
			{
				transaction.Category = "EAD";
			}
			else if (
					transaction.PriceItemCode.StartsWith("CC", true, CultureInfo.InvariantCulture) ||
					transaction.PriceItemCode.StartsWith("CU", true, CultureInfo.InvariantCulture))
			{
				transaction.Category = "ICN";
			}
			else if (StlPriceItemCodes.Contains(transaction.PriceItemCode))
			{
				transaction.Category = "STL";
			}
			else
			{
				transaction.Category = "UNK";
			}
		}

		internal static bool SendBillingToKafka = bool.TryParse(ConfigurationManager.AppSettings["SendBillingToKafka"], out var sendBillingToKafka) && sendBillingToKafka;
		internal static string BillingKafkaTopic = ConfigurationManager.AppSettings["BillingKafkaTopic"];
		internal static ProducerConfig BillingKafkaConfig = ServiceHelper.GetKafkaProducerConfig();

		static readonly HashSet<string> StlPriceItemCodes = new HashSet<string> { "ACA", "ACM", "ACN", "ACO", "ACS", "ACX", "ADP", "AHK", "AI3", "ANZ", "BKA", "BKG", "BKU", "BOL", "CB2", "CDM", "CDT", "CFC", "CFN", "CFP", "CFT", "CME", "CMR", "COO", "CTE", "CTI", "CTT", "CTW", "DRW", "EAD", "EAO", "ECE", "ECM", "EFC", "FGB", "FTZ", "GFC", "GFT", "GTW", "HKC", "IFB", "IFG", "INB", "INT", "JPC", "K45", "LCH", "LDG", "LTC", "LTK", "LTL", "LTM", "LVS", "MSC", "OPM", "ORM", "PTC", "PTT", "RFP", "SCE", "SCI", "SHP", "SLG", "SPK", "SRE", "SRI", "TNP", "UNB", "URC", "USP", "USR", "W3T", "W4P", "WAD", "WBM", "WIN", "WOD", "WOL", "WTD", "WTH", "WTP", "WTR" };
	}
}

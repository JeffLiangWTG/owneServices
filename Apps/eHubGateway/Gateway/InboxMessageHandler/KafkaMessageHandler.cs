using Confluent.Kafka;
using System.Collections.Generic;
using System;
using System.Configuration;
using Common.Logging;
using CargoWise.Billing.Kafka.API;
using CargoWise.eHub.Common;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.eHub.Common.Extensions;
using CargoWise.Billing.API;
using CargoWise.Billing.Client;

namespace CargoWise.eHub.Gateway
{
	public abstract class KafkaMessageHandler<T> : KafkaMessageHandler where T : class
	{
		internal virtual void ApplyTransforms(T transaction, string schema)
		{
		}

		protected virtual T DeserializeMessageContent(eHubGatewayMessage message)
		{
			using (var stream = message.MessageStream.DecodeAndDecompress())
			{
				ValidateXml(stream);

				stream.Position = 0;
				using (var reader = new NamespaceIgnorantXmlTextReader(stream))
				{
					var serializer = new XmlSerializer(typeof(T));
					var transaction = (T)serializer.Deserialize(reader);
					ApplyTransforms(transaction, message.SchemaName);
					return transaction;
				}
			}
		}
	}

	public abstract class KafkaMessageHandler : MessageHandler
	{
		static readonly ILog log = LogManager.GetLogger(typeof(KafkaMessageHandler));
		public KeyValuePair<Guid, Exception> MessageException { get; protected set; }
		internal virtual IBillingKafkaClient KafkaClient { get; set; }
		protected abstract string MessageType { get; }
		protected XmlSchemaSet MessageSchemas = new XmlSchemaSet();
		protected virtual ILog Log => log;

		protected void HandleKafkaDeliveryReport<TKey, TValue>(DeliveryReport<TKey, TValue> report)
		{
			if (report.Error.Code != ErrorCode.NoError)
			{
				HandleFailureReport(report);
			}
			else
			{
				HandleSuccessReport(report);
			}
		}

		protected void HandleSuccessReport<TKey, TValue>(DeliveryReport<TKey, TValue> report)
		{
			if (Log.IsDebugEnabled)
				Log.Debug($"Sent {MessageType} to Kafka {report.TopicPartitionOffset}");
			if (Log.IsTraceEnabled)
				Log.Trace($"Message's Details: '{report.Value}'");
		}

		protected virtual void HandleFailureReport<TKey, TValue>(DeliveryReport<TKey, TValue> report)
		{
			var errorMessage = $"Sending {MessageType} transaction to Kafka failed. Topic: {report.Topic}. Reason: {report.Error.Reason}";
			var kafkaEx = new KafkaException(report.Error);
			eHubStreamedService.IssueManger.Value.ReportToIssueManager(errorMessage, kafkaEx, Log, ConfigurationManager.AppSettings, false);

			try
			{
				if (report.Value is Billing.API.BillingTransaction || report.Value is Billing.API.UsageTransaction)
				{
					if (Log.IsInfoEnabled)
					{
						Log.Info("Fallback to direct service");
					}

					SendTransactionToService(report.Value);

					if (Log.IsDebugEnabled)
					{
						Log.Debug($"Processing {MessageType} message... SUCCESS");
					}
				}
				else
				{
					MessageException = new KeyValuePair<Guid, Exception>(new Guid(report.Key.ToString()), new KafkaException(report.Error));
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Sending transaction to billing service failed (fallback). {ex.GetType().FullName}: {ex.Message}");
				var billingServiceEx = ex is ValidationException validationException
					? new BillingTransactionValidationException(validationException.Errors, ex)
					: ex;
				MessageException = new KeyValuePair<Guid, Exception>(new Guid(report.Key.ToString()),
					new AggregateException($"Errors occurred when sending transactions to both kafka topic '{report.Topic}' and service", kafkaEx, billingServiceEx));
			}
			finally
			{
				if (Log.IsTraceEnabled)
					Log.Trace($"Message's Details: '{report.Value}'");
			}
		}

		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			if (string.IsNullOrEmpty(senderID))
			{
				if (Log.IsDebugEnabled)
					Log.DebugFormat($"Processing {MessageType} message... SKIPPING. Missing sender ID");
			}
			else
			{
				try
				{
					Handle(senderID, message);
				}
				catch (Exception ex)
				{
					if (Log.IsErrorEnabled)
						Log.Error($"Processing {MessageType} message... ERROR", ex);
					if (Log.IsTraceEnabled)
						Log.Trace(MessageToString(message));
					throw;
				}
			}
		}

		void SendTransactionToService(object transaction)
		{
			using (var client = CreateBillingServiceClient())
			{
				if (transaction is Billing.API.BillingTransaction billingTransaction)
				{
					client.AddTransaction(billingTransaction);
				}

				if (transaction is Billing.API.UsageTransaction usageTransaction)
				{
					client.AddUsageTransaction(usageTransaction);
				}
			}
		}

		protected internal virtual IBillingServiceClient CreateBillingServiceClient()
		{
			return new BillingServiceClient();
		}

		protected abstract void Handle(string senderID, eHubGatewayMessage message);

		protected void AddSchemas(string SchemaDocumentFileName, string targetNamespace)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SchemaDocumentFileName))
			using (var reader = new StreamReader(stream))
			{
				MessageSchemas.Add(targetNamespace, XmlReader.Create(reader));
			}
		}

		protected void ValidateXml(Stream stream)
		{
			var settings = new XmlReaderSettings
			{
				Schemas = MessageSchemas,
				ValidationType = ValidationType.Schema,
			};
			var errors = new List<string>();
			settings.ValidationEventHandler += (sender, args) =>
			{
				if (args.Severity == XmlSeverityType.Error)
				{
					errors.Add(args.Message);
				}
			};

			using (var reader = XmlReader.Create(stream, settings))
			{
				try
				{
					while (reader.Read())
					{
						// Validation is performed while reading xml
					}
				}
				catch (XmlException e)
				{
					throw new BillingTransactionValidationException(new[] { e.Message }, e);
				}
				if (errors.Count > 0)
				{
					throw new BillingTransactionValidationException(errors);
				}
			}
		}
	}
}

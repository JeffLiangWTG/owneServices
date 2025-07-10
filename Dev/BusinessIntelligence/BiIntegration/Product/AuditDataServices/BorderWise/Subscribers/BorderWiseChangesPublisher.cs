using System;
using System.Threading.Tasks;
using BorderWise.Sync;
using CargoWise.Common;
using Confluent.Kafka;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class BorderWiseChangesPublisher : IBorderWiseChangesPublisher
	{
		public void Publish(string key, string message)
		{
			try
			{
				try
				{
					var result = PublishAsync(key, message).Result
						?? throw new BorderWiseChangesPublishingException(GetExceptionErrorMessage("Did not receive message acknowledgement from Kafka server.", message));
				}
				catch (AggregateException aggregateException) when (aggregateException.InnerException is ProduceException<string, string> produceException)
				{
					var errorMessage = FormattableString.Invariant(
						$"Message was not published with error code {produceException.Error.Code.ToString()} and reason '{produceException.Error.Reason}' returned from Kafka server.");
					throw new BorderWiseChangesPublishingException(GetExceptionErrorMessage(errorMessage, message),
						produceException);
				}
			}
			catch (BorderWiseChangesPublishingException)
			{
				throw;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new BorderWiseChangesPublishingException(GetExceptionErrorMessage(ex.Message, message), ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Task<DeliveryResult<string, string>> PublishAsync(string key, string message)
		{
			return PublishAsync(SyncTopic, key, message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual Task<DeliveryResult<string, string>> PublishAsync(string topic, string key, string messageText)
		{
			return Publisher.ProduceAsync(topic, new Message<string, string> { Key = key, Value = messageText });
		}

		IProducer<string, string> Publisher
		{
			get
			{
				if (publisher == null)
				{
					var config = new ProducerConfig
					{
						BootstrapServers = BorderWiseSyncConfig.SyncBootstrapServers,
						SslCaLocation = @".\Subscribers\Certificate\letsencryptintermediate.pem",
						SecurityProtocol = SecurityProtocol.Ssl
					};

					if (!string.IsNullOrWhiteSpace(BorderWiseSyncConfig.SyncUserName) && !string.IsNullOrWhiteSpace(BorderWiseSyncConfig.SyncPassword))
					{
						config.SecurityProtocol = SecurityProtocol.SaslSsl;
						config.SaslMechanism = SaslMechanism.Plain;
						config.SaslUsername = BorderWiseSyncConfig.SyncUserName;
						config.SaslPassword = BorderWiseSyncConfig.SyncPassword;
					}

					publisher = new ProducerBuilder<string, string>(config).Build();
				}
				return publisher;
			}
		}
		IProducer<string, string> publisher;

		string SyncTopic => syncTopic ?? (syncTopic = BorderWiseSyncConfig.SyncTopicOutbound);
		string syncTopic;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (publisher != null)
				{
					var publisherToDispose = publisher;
					publisher = null;
					publisherToDispose.Dispose();
				}
			}
		}

		string GetExceptionErrorMessage(string message, string data) => FormattableString.Invariant($"Error: {message} Data: {data}");
	}

	[Serializable]
	public class BorderWiseChangesPublishingException : Exception
	{
		public BorderWiseChangesPublishingException(string message) : base(message) { }

		public BorderWiseChangesPublishingException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		protected BorderWiseChangesPublishingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}

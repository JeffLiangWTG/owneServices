using System;
using System.Threading;
using Confluent.Kafka;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.DataScience.Business.Extensions
{
	static class KafkaExtensions
	{
		public static void ProduceWithQHandling<TKey, TValue>(
			this IProducer<TKey, TValue> producer,
			ILogger logger,
			string topic,
			Message<TKey, TValue> message,
			Action<DeliveryReport<TKey, TValue>> deliveryHandler = null,
			int waitLimitMs = 30_000,
			int initTimeoutMs = 500,
			int retryCount = 0,
			Action<int> sleepAction = null)
		{
			try
			{
				producer.Produce(topic, message, deliveryHandler);
			}
			catch (ProduceException<TKey, TValue> ex) when (
				ex.Error.IsLocalError
				&& ex.Error.Code == ErrorCode.Local_QueueFull
				&& initTimeoutMs * (1 << retryCount) is var waitMs
				&& waitMs <= waitLimitMs)
			{
				logger.Error($"Confluent.Kafka producer: Local queue full: Retries {retryCount}; consider altering producer config; retrying...");
				(sleepAction ?? Thread.Sleep)(waitMs);
				producer.ProduceWithQHandling(logger, topic, message, deliveryHandler, waitLimitMs, initTimeoutMs, ++retryCount, sleepAction);
			}
		}
	}
}

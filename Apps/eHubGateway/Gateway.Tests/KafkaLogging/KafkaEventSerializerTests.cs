using System;
using System.Text;
using Confluent.Kafka;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.Tests.KafkaLogging
{
	public class KafkaEventSerializerTests
	{
		private KafkaEventSerializer kafkaEventSerializer;

		[SetUp]
		public void SetUp()
		{
			kafkaEventSerializer = new KafkaEventSerializer();
		}

		[Test]
		public void KafkaEventSerializer_ShouldReturnEmptyArray_WhenEventIsNull()
		{
			var result = kafkaEventSerializer.Serialize(null, SerializationContext.Empty);
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Length, Is.EqualTo(0));
		}

		[Test]
		public void KafkaEventSerializer_ShouldReturnValidBytes_WhenEventIsSerialized()
		{
			var logEvent = new LogEvent
			{
				Timestamp = new DateTime(2023, 12, 12, 06, 32, 00, DateTimeKind.Utc),
				MachineName = "SYDCO-MACHINE-1",
				ProcessId = 4752,
				ApplicationName = "CargoWise.eHub.Gateway",
				LoggerName = "eHubStreamedService",
				Level = "INFO",
				Message = "This is a logging message"
			};

			var result = kafkaEventSerializer.Serialize(logEvent, SerializationContext.Empty);

			var expected = Encoding.UTF8.GetBytes(@"{""@timestamp"":""2023-12-12T06:32:00+00:00"",""MachineName"":""SYDCO-MACHINE-1"",""ProcessId"":4752,""ApplicationName"":""CargoWise.eHub.Gateway"",""LoggerName"":""eHubStreamedService"",""Level"":""INFO"",""Message"":""This is a logging message"",""ExceptionID"":null}");

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.EqualTo(expected));
		}

	}
}

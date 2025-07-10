using System.Collections.Generic;
using Confluent.Kafka;
using Enterprise.Client.EDI.DataScience.Business.Extensions;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using LoggerForTest = Enterprise.EConversation.Testing.LoggerForTest;

namespace Enterprise.Client.EDI.DataScience.Business.Test
{
	class KafkaExtensionsTest : TestCase
	{
		Mock<IProducer<string, string>> ProducerMock { get; set; }
		LoggerForTest Logger { get; set; }
		Mock<Message<string, string>> Message { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			ProducerMock = new Mock<IProducer<string, string>>();
			Logger = new LoggerForTest();
			Message = new Mock<Message<string, string>>();
		}

		public void TestUsualCase()
		{
			// Arrange / Act
			ProducerMock.Object.ProduceWithQHandling(Logger, "test-topic", Message.Object, sleepAction: _ => Fail());

			// Assert
			ProducerMock.Verify(p => p.Produce("test-topic", Message.Object, null), Times.Once());
			Assert(true);
		}

		public void TestRetries()
		{
			// Retry on failure with exponential backoff until max delay is excedded, then give up.
			// Note that in the event of a poison payload, or a sleepAction argument that crashes the service task,
			// we potentially keep retrying and crashing every time the service task runs,
			// blocking any further data from being processed.
			// However, in this case we are trusting the AuditSubscriber framework to raise an issue in such a case.
			// The sleepAction code should come from us, and should not be malicious; the payload comes out of
			// the AuditSubscriber and we are not parsing it, so a poison payload should be impossible.

			// Arrange
			ProducerMock.Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null))
				.Callback(() => throw new ProduceException<string, string>(new Error(ErrorCode.Local_QueueFull), null));
			var sleepCalls = new List<int>();

			// Act / Assert
			AssertExceptionThrown<ProduceException<string, string>>(() =>
				ProducerMock.Object.ProduceWithQHandling(Logger, "test-topic", Message.Object, sleepAction: sleepCalls.Add));

			// Assert
			ProducerMock.Verify(p => p.Produce("test-topic", Message.Object, null), Times.Exactly(7));
			AssertSequencesEqual(new[] { 500, 1_000, 2_000, 4_000, 8_000, 16_000 }, sleepCalls);
			foreach (var log in Logger.Logs)
			{
				AssertEquals(LogType.Error, log.type);
				AssertContains("Local queue full", log.message);
			}
		}
	}
}

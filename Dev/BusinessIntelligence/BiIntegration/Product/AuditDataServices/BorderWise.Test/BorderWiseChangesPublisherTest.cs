using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Confluent.Kafka;
using Enterprise.AuditDataServices.BorderWise.Subscribers;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class BorderWiseChangesPublisherTest : TestCaseWithFactory
	{
		public void TestPublishMessage()
		{
			using (TestHelper.EnableSyncForTest(out var bwSyncConfig))
			{
				bwSyncConfig.SyncTopicOutbound = "TEST";

				var bwChangesPublisher = new BorderWiseChangesPublisherTestOverride();
				bwChangesPublisher.Publish("ABC", "XYZ");

				AssertEquals(1, bwChangesPublisher.PublishedMessages.Count);
				AssertEquals("TEST", bwChangesPublisher.PublishedMessages[0].Key);
				AssertEquals("ABC", bwChangesPublisher.PublishedMessages[0].Value.Key);
				AssertEquals("XYZ", bwChangesPublisher.PublishedMessages[0].Value.Value);
			}
		}

		public void TestPublishMessageError()
		{
			var bwChangesPublisher = new BorderWiseChangesPublisherTestOverride();
			AssertExceptionThrown<BorderWiseChangesPublishingException>("Should throw exception",
					$"Error: Message was not published with error code {ErrorCode.Unknown} and reason 'Because' returned from Kafka server. Data: <ERROR>",
					() => bwChangesPublisher.Publish("ABC", "<ERROR>"));
		}

		public void TestPublishMessageException()
		{
			var bwChangesPublisher = new BorderWiseChangesPublisherTestOverride();
			try
			{
				bwChangesPublisher.Publish("ABC", "<EXCEPTION>");
				Fail("Should have thrown exception");
			}
			catch (BorderWiseChangesPublishingException ex)
			{
				AssertNotNull(ex.InnerException);
				AssertEquals(typeof(AggregateException), ex.InnerException.GetType());
				var aggregatedEx = (AggregateException)ex.InnerException;
				AssertEquals(1, aggregatedEx.InnerExceptions.Count);
				AssertEquals(typeof(ApplicationException), aggregatedEx.InnerExceptions[0].GetType());
				AssertEquals("Exception requested", aggregatedEx.InnerExceptions[0].Message);
#if NET48
				AssertEquals("Error: One or more errors occurred. Data: <EXCEPTION>", ex.Message);
#else
				AssertEquals("Error: One or more errors occurred. (Exception requested) Data: <EXCEPTION>", ex.Message);
#endif

			}
		}

		public void TestPublishMessageNoAcknowledgement()
		{
			var bwChangesPublisher = new BorderWiseChangesPublisherTestOverride();
			AssertExceptionThrown<BorderWiseChangesPublishingException>("Should throw exception",
				"Error: Did not receive message acknowledgement from Kafka server. Data: <NO_RESULT>",
				() => bwChangesPublisher.Publish("ABC", "<NO_RESULT>"));
		}

		#region Implementation

		class BorderWiseChangesPublisherTestOverride : BorderWiseChangesPublisher
		{
			protected override Task<DeliveryResult<string, string>> PublishAsync(string topic, string key, string messageText)
			{
				return Task.Run(() =>
				{
					if (messageText.Contains("<EXCEPTION>"))
					{
						throw new ApplicationException("Exception requested");
					}

					if (messageText.Contains("<ERROR>"))
					{
						throw new ProduceException<string, string>(
							new Error(ErrorCode.Unknown, "Because"),
							new DeliveryResult<string, string>
							{
								Message = new Message<string, string>
								{
									Key = key,
									Value = messageText,
								},
								Topic = topic,
							});
					}

					if (messageText.Contains("<NO_RESULT>"))
					{
						return null;
					}

					var result = new DeliveryResult<string, string>
					{
						Topic = topic,
						Message = new Message<string, string>
						{
							Key = key,
							Value = messageText,
						},
						Timestamp = new Timestamp(),
					};
					PublishedMessages.Add(new KeyValuePair<string, DeliveryResult<string, string>>(topic, result));

					return result;
				});
			}

			public List<KeyValuePair<string, DeliveryResult<string, string>>> PublishedMessages { get; } = new List<KeyValuePair<string, DeliveryResult<string, string>>>();
		}

		#endregion
	}
}

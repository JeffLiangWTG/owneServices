using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	class BaseMessageProcessorRetryTest : TestCaseWithFactory
	{
		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestRetryOfHeldMessages()
		{
			var queuedMessage = EDIMessageTestFactory.New(Factory);
			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			queuedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			queuedMessage.EM_ApplicationCode = "DAN";
			Factory.Save();
			var baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.Fail);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Processed", queuedMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Failed, queuedMessage.EM_Status);
			AssertEquals(ZDateTime.Empty, queuedMessage.EM_HeldUntilDate);

			queuedMessage.EM_MessageText = "Waiting again";
			Factory.Save();
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Not picked up as not QUEued or retryable", "Waiting again", queuedMessage.EM_MessageText);

			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.MarkForRetry);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("QUEd message was processed, failed but marked for retry", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(ExpectedMessageQueuedStatus, queuedMessage.EM_Status);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), queuedMessage.EM_HeldUntilDate);

			queuedMessage.EM_MessageText = "Waiting again";
			Factory.Save();
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Not picked up, not due yet", "Waiting again", queuedMessage.EM_MessageText);
			AssertEquals(ExpectedMessageQueuedStatus, queuedMessage.EM_Status);

			queuedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 3, 59, 0); // past
			queuedMessage.EM_MessageText = "Waiting again";
			Factory.Save();
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Now overdue and marked for retry, gets processed", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(ExpectedMessageQueuedStatus, queuedMessage.EM_Status);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 1, 0), queuedMessage.EM_HeldUntilDate);  //future

			queuedMessage.EM_MessageText = "Waiting again";
			queuedMessage.EM_HeldUntilDate = new ZDateTime(1986, 3, 12, 3, 59, 0);  //past
			Factory.Save();
			baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.Success);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Due to be retried, completed successfully", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Received, queuedMessage.EM_Status);

			queuedMessage.EM_MessageText = "Waiting again";
			queuedMessage.EM_HeldUntilDate = ZDateTime.Empty;
			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.ForgotToSetStatusAndDate);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Picked up successfully", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Error, queuedMessage.EM_Status);  // set when we see that the message has not been updated
			AssertEquals(ZDateTime.Empty, queuedMessage.EM_HeldUntilDate);
			AssertContains("BaseMessageProcessor-BadState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			queuedMessage.EM_MessageText = "Waiting again";
			queuedMessage.EM_HeldUntilDate = ZDateTime.Empty;
			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.ForgotToSetStatusButDateSetToPast);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Picked up successfully", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Error, queuedMessage.EM_Status);
			AssertEquals(ZDateTime.BrettsBirthday, queuedMessage.EM_HeldUntilDate);
			AssertContains("BaseMessageProcessor-BadState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			queuedMessage.EM_MessageText = "Waiting again";
			queuedMessage.EM_HeldUntilDate = ZDateTime.Empty;
			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			baseProcessor = GetMessageProcessorWithRetryForTest(RetryResults.ForgotToSetStatusButDateSetToPast);
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals("Picked up successfully", "Processed", queuedMessage.EM_MessageText);
			AssertEquals(EDIMessage.Status.Error, queuedMessage.EM_Status);
			AssertEquals(ZDateTime.BrettsBirthday, queuedMessage.EM_HeldUntilDate);
			AssertContains("Message is still held but processor did not adjust the date", "BaseMessageProcessor-BadState", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		protected virtual string ExpectedMessageQueuedStatus => EDIMessage.Status.Queued;

		#region Implementation

		protected virtual IMessageProcessorForTest GetMessageProcessorWithRetryForTest(RetryResults desiredResult)
		{
			return new MessageProcessorWithRetryForTest(desiredResult);
		}

		protected class ApplicationTypeMessageProcessorWithRetryForTest : ApplicationTypeMessageProcessor
		{
			public ApplicationTypeMessageProcessorWithRetryForTest(LoggingInformation logger, RetryResults desiredResult)
				: base(logger)
			{
				this.desiredResult = desiredResult;
			}
			readonly RetryResults desiredResult;

			protected override string ApplicationCodeCore => "DAN";
			protected override string MessageFriendlyNameCore => "This member is pointless";

			protected override void ProcessMessageCore(EDIMessage ediMessage)
			{
				ediMessage.EM_MessageText = "Processed";
				switch (desiredResult)
				{
					case RetryResults.Fail:
						ediMessage.EM_Status = EDIMessage.Status.Failed;
						break;

					case RetryResults.MarkForRetry:
						ediMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
						break;

					case RetryResults.Success:
						ediMessage.EM_Status = EDIMessage.Status.Received;
						break;

					case RetryResults.ForgotToSetStatusButDateSetToPast:
						ediMessage.EM_HeldUntilDate = ZDateTime.BrettsBirthday;
						break;

					case RetryResults.ForgotToSetStatusAndDate:
						break;
				}
			}
		}

		class MessageProcessorWithRetryForTest : MessageProcessorForTest
		{
			public MessageProcessorWithRetryForTest(RetryResults desiredResult)
			{
				this.desiredResult = desiredResult;
			}
			readonly RetryResults desiredResult;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>()
				{
					new ApplicationTypeMessageProcessorWithRetryForTest(Logger, desiredResult)
				};
			}
		}

		protected enum RetryResults
		{
			Fail,
			MarkForRetry,
			Success,
			ForgotToSetStatusAndDate,
			ForgotToSetStatusButDateSetToPast
		}

		#endregion
	}
}

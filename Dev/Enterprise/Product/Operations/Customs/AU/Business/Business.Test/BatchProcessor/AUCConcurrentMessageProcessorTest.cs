using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCConcurrentMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestMessageSetToFailedIfExceptionWhilstProcessing()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			ExceptionThrowingBatchAUCConcurrentMessageProcessor processor = new ExceptionThrowingBatchAUCConcurrentMessageProcessor();
			processor.ExecuteBatch();
			AssertEquals("LastDevError", typeof(DivideByZeroException), ErrorReporter.LastExceptionReported.GetType());
			ErrorReporter.Clear();
			message.Reload();
			AssertEquals("Status", EDIMessage.Status.Failed, message.EM_Status);
		}

		[ExpectNoExceptions]
		public void TestDontProcessMessageOwnedByADifferentCompany()
		{
			var query = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_GB = Factory.LoadTop1<GlbBranch>(query).PK;
			Factory.Save();

			ExceptionThrowingBatchAUCConcurrentMessageProcessor processor = new ExceptionThrowingBatchAUCConcurrentMessageProcessor();
			processor.ExecuteBatch();
		}

		public void TestNextMessageProcessedIfMessageBreaksFactory()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();

			BadRecordInsertingAUCConcurrentMessageProcessor processor = new();
			processor.MessagePKsToBarfOn.Add(message1.PK);
			processor.ExecuteBatch();
#if NETFRAMEWORK
			var expectedMessage = $@"sender cannot be null or empty.{System.Environment.NewLine}Parameter name: sender";
#else
			var expectedMessage = $@"sender cannot be null or empty. (Parameter 'sender')";
#endif
			AssertEquals("LastDevError", expectedMessage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
			message1.Reload();
			AssertEquals("Status", EDIMessage.Status.Failed, message1.EM_Status);
			message2.Reload();
			AssertEquals("Status", EDIMessage.Status.Received, message2.EM_Status);
			processor.ExecuteBatch();
		}

		public void TestGetMessageProcessors()
		{
			var messageProcessors = new AUCConcurrentMessageProcessorForTest().GetMessageProcessorsExposed();

			Assert("CMRConcurrentMessageProcessor", messageProcessors.Any(x => x is CMRConcurrentMessageProcessor));
			AssertEquals("Processor Count", 1, messageProcessors.Count);
		}

		sealed class AUCConcurrentMessageProcessorForTest : AUCConcurrentMessageProcessor
		{
			public List<ApplicationTypeMessageProcessor> GetMessageProcessorsExposed() => GetMessageProcessors();
		}

		sealed class ExceptionThrowingBatchAUCConcurrentMessageProcessor : AUCConcurrentMessageProcessor
		{
			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null) => false;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>() { new ExceptionThrowingMessageProcessor() };
			}

			sealed class ExceptionThrowingMessageProcessor : ApplicationTypeMessageProcessor
			{
				public ExceptionThrowingMessageProcessor()
					: base(new LoggingInformation())
				{
				}

				protected override string ApplicationCodeCore => "CMR";

				protected override string MessageFriendlyNameCore => "";

				protected override void ProcessMessageCore(EDIMessage message)
				{
					int x = 0;
					int i = x / x;
				}
			}
		}

		sealed class BadRecordInsertingAUCConcurrentMessageProcessor : AUCConcurrentMessageProcessor
		{
			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null) => false;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>() { new BadRecordInsertingMessageProcessor(this) };
			}

			sealed class BadRecordInsertingMessageProcessor : ApplicationTypeMessageProcessor
			{
				protected override string ApplicationCodeCore => "CMR";

				protected override string MessageFriendlyNameCore => "test";

				public BadRecordInsertingMessageProcessor(BadRecordInsertingAUCConcurrentMessageProcessor parent)
					: base(new LoggingInformation())
				{
					this.parent = parent;
				}

				readonly BadRecordInsertingAUCConcurrentMessageProcessor parent;

				protected override void ProcessMessageCore(EDIMessage message)
				{
					if (parent.MessagePKsToBarfOn.Contains(message.PK))
					{
						//insert dbo.ediinterchange without populating the essential fields EI_To and EI_From
						EDIInterchange interchange1 = message.Factory.New<EDIInterchange>();
					}
					message.EM_Status = EDIMessage.Status.Received;
				}
			}

			public ArrayList MessagePKsToBarfOn = new ArrayList();
		}
	}
}

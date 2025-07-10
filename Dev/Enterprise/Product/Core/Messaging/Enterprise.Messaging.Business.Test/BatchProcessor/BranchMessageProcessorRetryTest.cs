using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using Moq;
using Moq.Protected;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class BranchMessageProcessorRetryTest : BaseMessageProcessorRetryTest
	{
		public void TestFailedMessageStatusIsSaved()
		{
			var queuedMessage = EDIMessageTestFactory.New(Factory);
			queuedMessage.EM_Status = EDIMessage.Status.Queued;
			queuedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			queuedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandMAFeBACCa;
			queuedMessage.EM_ApplicationReference = "REF1";
			queuedMessage.EM_MessageType = "TST";
			queuedMessage.EM_MessageSubType = "T23";
			queuedMessage.EM_MessageNum = "NUM323423";
			Factory.Save();
			var logger = new LoggingInformation();
			var applicationTypeMessageProcessorMock = new Mock<ApplicationTypeMessageProcessor>(logger);
			applicationTypeMessageProcessorMock.CallBase = true;
			applicationTypeMessageProcessorMock
				.Protected()
				.Setup<string>("ApplicationCodeCore", ItExpr.IsAny<string>())
				.Returns(EDIMessage.ApplicationCodes.NewZealandMAFeBACCa);

			var processorMock = new Mock<BranchMessageProcessor>(logger);
			processorMock.CallBase = true;
			processorMock.Setup(m => m.GetApplicationTypeProcessorCore(It.IsAny<EDIMessage>()))
				.Returns((ApplicationTypeMessageProcessor)null);
			processorMock.Protected()
						 .Setup<List<ApplicationTypeMessageProcessor>>("GetMessageProcessors")
						 .Returns(new List<ApplicationTypeMessageProcessor> { applicationTypeMessageProcessorMock.Object });
			var baseProcessor = processorMock.Object;
			baseProcessor.ExecuteBatch();
			queuedMessage.Reload();
			AssertEquals(EDIMessage.Status.Failed, queuedMessage.EM_Status);
			AssertEquals(ZDateTime.Empty, queuedMessage.EM_HeldUntilDate);
			AssertMultilineASCIIEquals("Logs", @"	Failed to locate a processor for message (Application Code: NZM, Application Reference: REF1, Message Type: TST, Message Sub Type: T23)", string.Join(System.Environment.NewLine, logger.DebugLogStrings.Cast<string>()));
		}

		protected override string ExpectedMessageQueuedStatus => EDIMessage.Status.PreProcessedOK;

		protected override IMessageProcessorForTest GetMessageProcessorWithRetryForTest(RetryResults desiredResult)
		{
			return new BranchMessageProcessorWithRetryForTest(desiredResult);
		}

		class ApplicationTypeMessageProcessorForPreprocessingWithRetryForTest : ApplicationTypeMessageProcessorWithRetryForTest
		{
			public ApplicationTypeMessageProcessorForPreprocessingWithRetryForTest(LoggingInformation logger, RetryResults desiredResult)
				: base(logger, desiredResult)
			{
			}

			protected override bool RequiresPreProcessingCore => true;
		}

		class BranchMessageProcessorWithRetryForTest : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorWithRetryForTest(RetryResults desiredResult)
			{
				this.desiredResult = desiredResult;
			}
			readonly RetryResults desiredResult;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>()
				{
					new ApplicationTypeMessageProcessorForPreprocessingWithRetryForTest(Logger, desiredResult)
				};
			}
		}
	}
}

using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(AECUniversalCustomsMessageProcessor))]
sealed class AECUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<AECUniversalCustomsMessageProcessor>
{
	public void TestShouldMessageBeProcessedInASeparateFactory()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		AssertEquals(false, processor.ShouldMessageBeProcessedInASeparateFactory(Factory.New<EDIMessage>()));
	}

	public void TestGetLinkedBusinessObjectMetaData_Known()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		var message = Factory.New<EDIMessage>();
		var logger = new LoggingInformation();
		var mockMessageProcessorFactory = new Mock<IMessageProcessorFactory>();
		var mockMessageProcessor = new Mock<IMessageProcessor<IInboundMessageDataProvider>>();
		var expectedResult = ProcessingResult.New(new LinkedBusinessObjectMetaData(JobDeclaration.Schema.TableName, ZGuid.BrettsGuid, ZGuid.BrettsGuid, "JobNumber"));
		mockMessageProcessor
			.Setup(x => x.GetLinkedBusinessObjectMetaData(message, logger))
			.Returns(expectedResult);
		mockMessageProcessorFactory.Setup(x => x.GetMessageProcessor(message)).Returns(mockMessageProcessor.Object);

		MessageProcessorFactory.Instance.Value = mockMessageProcessorFactory.Object;
		var actualResult = processor.GetLinkedBusinessObjectMetaData(message, logger);
		AssertEquals(expectedResult, actualResult);
		MessageProcessorFactory.Instance.ResetValue();
	}

	public void TestGetLinkedBusinessObjectMetaData_UnknownMessageProcessor()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		var message = Factory.New<EDIMessage>();
		var logger = new LoggingInformation();
		var mockMessageProcessorFactory = new Mock<IMessageProcessorFactory>();
		mockMessageProcessorFactory.Setup(x => x.GetMessageProcessor(message)).Returns((IMessageProcessor<IInboundMessageDataProvider>)null);

		MessageProcessorFactory.Instance.Value = mockMessageProcessorFactory.Object;
		var expectedResult = ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, AECUniversalCustomsMessageProcessor.GetUnknownMessageTypeError(message));
		var actualResult = processor.GetLinkedBusinessObjectMetaData(message, logger);
		AssertEquals(expectedResult, actualResult);
		MessageProcessorFactory.Instance.ResetValue();
	}

	public void TestGetBranch()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		var message = Factory.New<EDIMessage>();
		var logger = new LoggingInformation();
		var linkedBusinessObjectBranchPk = ZGuid.BrettsGuid;
		var mockMessageProcessorFactory = new Mock<IMessageProcessorFactory>();
		var mockMessageProcessor = new Mock<IMessageProcessor<IInboundMessageDataProvider>>();
		var expectedResult = ProcessingResult.New(ZGuid.BrettsGuid);
		mockMessageProcessor
			.Setup(x => x.GetBranch(message, logger, linkedBusinessObjectBranchPk))
			.Returns(expectedResult);
		mockMessageProcessorFactory.Setup(x => x.GetMessageProcessor(message)).Returns(mockMessageProcessor.Object);

		MessageProcessorFactory.Instance.Value = mockMessageProcessorFactory.Object;
		var actualResult = processor.GetBranch(message, logger, linkedBusinessObjectBranchPk);
		AssertEquals(expectedResult, actualResult);
		MessageProcessorFactory.Instance.ResetValue();
	}

	public void TestProcessMessage_Known()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		var message = Factory.New<EDIMessage>();
		var calls = 0;
		var mockMessageProcessorFactory = new Mock<IMessageProcessorFactory>();
		var loggingInformation = new LoggingInformation();
		var mockMessageProcessor = new Mock<IMessageProcessor<IInboundMessageDataProvider>>();
		mockMessageProcessor.Setup(x => x.ProcessMessage(message, loggingInformation)).Callback(() => calls++);
		mockMessageProcessorFactory.Setup(x => x.GetMessageProcessor(message)).Returns(mockMessageProcessor.Object);

		MessageProcessorFactory.Instance.Value = mockMessageProcessorFactory.Object;
		processor.ProcessMessage(message, loggingInformation, null);
		AssertEquals(1, calls);
		MessageProcessorFactory.Instance.ResetValue();
	}

	public void TestProcessMessage_Unknown()
	{
		var processor = new AECUniversalCustomsMessageProcessor();
		var message = Factory.New<EDIMessage>();
		var mockMessageProcessorFactory = new Mock<IMessageProcessorFactory>();
		var loggingInformation = new LoggingInformation();
		mockMessageProcessorFactory.Setup(x => x.GetMessageProcessor(message)).Returns((IMessageProcessor<IInboundMessageDataProvider>)null);

		MessageProcessorFactory.Instance.Value = mockMessageProcessorFactory.Object;
		AssertNoExceptionThrown(() =>
		{
			processor.ProcessMessage(message, loggingInformation, null);
		});
		mockMessageProcessorFactory.Verify(x => x.GetMessageProcessor(message), Times.Once);
		MessageProcessorFactory.Instance.ResetValue();
	}

	protected override string ApplicationCode => EDIMessage.ApplicationCodes.UnitedArabEmirates;
}

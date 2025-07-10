using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderXTradeSignatureErrorResponseMessageProcessorTest : TemporaryStorageHeaderResponseMessageProcessorAbstractTest<XTradeSignatureErrorResponseMessageProcessor>
{
	public void TestProcessMessage_MessageTypeNEW_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, "XSE");

		sentMessage.EM_MessageType = "NEW";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1, "XSE");
			AssertEquals(nameof(header.AMA_MessageStatus), "FAL", header.AMA_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeCAN_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, "XSE");

		sentMessage.EM_MessageType = "CAN";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1, "XSE");
			AssertEquals(nameof(header.AMA_MessageStatus), "FAL", header.AMA_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeAMD_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, "XSE");

		sentMessage.EM_MessageType = "AMD";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1, "XSE");
			AssertEquals(nameof(header.AMA_MessageStatus), "FAL", header.AMA_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeNotNEW_CAN_AMD_DoesNotSetStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, "XSE");

		sentMessage.EM_MessageType = "ITM";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1, "XSE");
			AssertEquals(nameof(header.AMA_MessageStatus), "", header.AMA_MessageStatus);
		});
	}

	protected override XTradeSignatureErrorResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new XTradeSignatureErrorResponseMessageProcessor(logger);

	const string ReasonContainsMessageTransmissionFailureKey = "Enterprise.Customs.IT.TemporaryStorage.Business.Testing.TemporaryStorageHeader.MessageProcessors.TestFiles.XTradeSginatureErrorFile_ReasonContainsMessageTransmissionFailure.xml";
}

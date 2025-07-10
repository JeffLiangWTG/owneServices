using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsXTradeSignatureErrorResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<XTradeSignatureErrorResponseMessageProcessor>
{
	public void TestProcessMessage_MessageTypeNEW_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, originalMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "XSE");

		var movementHeader = nctsHeader.MovementHeader;
		originalMessage.EM_MessageType = "NEW";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "XSE");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "FAL", movementHeader.BM_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeCAN_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, originalMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "XSE");

		var movementHeader = nctsHeader.MovementHeader;
		originalMessage.EM_MessageType = "CAN";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "XSE");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "FAL", movementHeader.BM_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeAMD_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, originalMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "XSE");

		var movementHeader = nctsHeader.MovementHeader;
		originalMessage.EM_MessageType = "AMD";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "XSE");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "FAL", movementHeader.BM_MessageStatus);
		});
	}

	public void TestProcessMessage_MessageTypeNotNEW_CAN_AMD_DoesNotSetStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, originalMessage, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "XSE");

		var movementHeader = nctsHeader.MovementHeader;
		originalMessage.EM_MessageType = "ITM";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "XSE");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "", movementHeader.BM_MessageStatus);
		});
	}

	protected override XTradeSignatureErrorResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new XTradeSignatureErrorResponseMessageProcessor(logger);

	const string ReasonContainsMessageTransmissionFailureKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.XTradeSginatureErrorFile_ReasonContainsMessageTransmissionFailure.xml";
}

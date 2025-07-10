using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsXTradeErrorResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<XTradeErrorResponseMessageProcessor>
{
	public void TestProcessMessage_WhenEntryIsUnlockedAndAwaitingResponseAndTransmissionHasFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "ERR");

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_MessageStatus = "SNT";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var unlockForEditLog = nctsHeader.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);
		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "ERR");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "FAL", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(nctsHeader.IsLocked), expected: false, nctsHeader.IsLocked);
			AssertNull("Unlock log", unlockForEditLog);
		});
	}

	public void TestProcessMessage_WhenEntryIsLockedAndAwaitingResponseAndTransmissionHasFailed_Locked()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: errorMessageContent, responseMessageType: "ERR");
		nctsHeader.LockFile(string.Empty);

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_MessageStatus = "SNT";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var unlockForEditLog = nctsHeader.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);
		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "ERR");
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "FAL", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(nctsHeader.IsLocked), expected: false, nctsHeader.IsLocked);
			AssertNotNull("Unlock log", unlockForEditLog);
			AssertEquals("Unlock log ReferenceFreeText", "Response message error failed for transmission", unlockForEditLog.ReferenceFreeText);
		});
	}

	protected override XTradeErrorResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new XTradeErrorResponseMessageProcessor(logger);

	const string ReasonContainsMessageTransmissionFailureKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_ReasonContainsMessageTransmissionFailure.xml";
}

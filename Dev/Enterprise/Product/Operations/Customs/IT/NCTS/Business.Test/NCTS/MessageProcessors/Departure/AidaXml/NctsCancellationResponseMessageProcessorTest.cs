using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsCancellationResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<ResponseMessageProcessor>
{
	public void TestProcess_WhenCancellationMessageResponseIsRejected()
	{
		var messageContent = ManifestResourceHelper.ReadManifestResourceContent(RejectedCancellationMessageResponseKey);
		var (header, sentMessage, receivedMessage) = PrepareTestData(messageText: messageContent, sentMessageType: "CAN");
		var movementHeader = header.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);

			AssertEquals("BM_MessageStatus", "ERR", movementHeader.BM_MessageStatus);
			AssertEquals("BM_MessageStatus", "", movementHeader.BM_CustomsStatus);
		});
	}

	public void TestProcess_WhenCancellationMessageReponseIsAcceptedBySystem()
	{
		var messageContent = ManifestResourceHelper.ReadManifestResourceContent(CancellationAcceptedBySystemMessageResponseKey);
		var (header, sentMessage, receivedMessage) = PrepareTestData(messageText: messageContent, sentMessageType: "CAN");
		var movementHeader = header.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);

			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", "ACS", movementHeader.BM_CustomsStatus);
		});
	}

	public void TestProcess_WhenCancellationMessageResponseIsConfirmedByCustoms()
	{
		var messageContent = ManifestResourceHelper.ReadManifestResourceContent(CancellationConfirmedCustomsMessageResponseKey);
		var (header, sentMessage, receivedMessage) = PrepareTestData(messageText: messageContent, responseMessageType: "RES", sentMessageType: "CAN");
		var movementHeader = header.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);

			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", "CAN", movementHeader.BM_CustomsStatus);
		});
	}

	protected override ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new ResponseMessageProcessor(logger);

	const string RejectedCancellationMessageResponseKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.CancellationRejectedMessageResponse.xml";
	const string CancellationAcceptedBySystemMessageResponseKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.CancellationAcceptedBySystemMessageResponse.xml";
	const string CancellationConfirmedCustomsMessageResponseKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.CancellationConfirmedByCustomsMessageResponse.xml";
}

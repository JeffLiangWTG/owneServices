using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsAcknowledgementResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<AcknowledgementResponseMessageProcessor>
{
	public void TestProcessPositiveAcknowledgement_WhenMessageAcquired()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveAckText, responseMessageType: "ACK");
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_MessageStatus = "SNT";
		AssertEquals("PRE-CONDITION 1", "0001", sentMessage.EM_MessageNum);
		AssertEquals("PRE-CONDITION 2", string.Empty, receivedMessage.EM_MessageNum);
		AssertEquals("PRE-CONDITION 3", "SNT", movementHeader.BM_MessageStatus);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "ACK");
			AssertEquals("BM_MessageStatus", "ACC", movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", "ACK", movementHeader.BM_CustomsStatus);
		});
	}

	public void TestProcessNegativeAcknowledgement_WhenInvalidXMLMessage()
	{
		var invalidAckText = ManifestResourceHelper.ReadManifestResourceContent(InvalidAcknowledgementMessageAcquiredFileKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: invalidAckText, responseMessageType: "ACK");
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_MessageStatus = "SNT";

		AssertEquals("PRE-CONDITION 1", "0001", sentMessage.EM_MessageNum);
		AssertEquals("PRE-CONDITION 2", string.Empty, receivedMessage.EM_MessageNum);
		AssertEquals("PRE-CONDITION 3", "SNT", movementHeader.BM_MessageStatus);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "ACK");
			AssertEquals("BM_MessageStatus", "ERR", movementHeader.BM_MessageStatus);
		});
	}

	public void TestProcessNegativeAcknowledgement_WhenNctsHeaderHasGuaranteeTransactions()
	{
		var invalidAckText = ManifestResourceHelper.ReadManifestResourceContent(InvalidAcknowledgementMessageAcquiredFileKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: invalidAckText, responseMessageType: "ACK");
		nctsHeader.MovementHeader.BM_MessageStatus = "SNT";
		nctsHeader.BH_JobReference = "A0001";

		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, sentMessage.EM_MessageNum, nctsHeader.BH_JobReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		for (var i = 0; i < transactions.Length; i++)
		{
			transactions[i] = Factory.Load<BaseCusGuaranteeLineTransaction>(transactions[i].PK);
		}

		CombineAssertions("Guarantee Transactions", () =>
		{
			AssertNull("First PND with matching AppId and Reference is deleted", transactions[0]);
			AssertNull("Second PND with matching AppId and Reference is deleted", transactions[1]);
			AssertNotNull("CON with matching AppId and Reference isn't deleted", transactions[2]);
			AssertNotNull("PND with matching Reference and different AppId isn't deleted", transactions[3]);
			AssertNotNull("PND with matching AppId and different Reference isn't deleted", transactions[4]);
		});
	}

	protected override AcknowledgementResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new AcknowledgementResponseMessageProcessor(logger);

	const string InvalidAcknowledgementMessageAcquiredFileKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsAcknowledgementInvalidXMLMessage.xml";
}

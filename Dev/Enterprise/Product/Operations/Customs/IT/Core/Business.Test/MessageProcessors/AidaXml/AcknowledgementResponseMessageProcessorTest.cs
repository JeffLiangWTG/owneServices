using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AcknowledgementResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<AcknowledgementResponseMessageProcessor>
{
	public void TestEM_MessageNum()
	{
		var positiveAcknowledgement20 = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(messageText: positiveAcknowledgement20, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		AssertEquals("PRE-CONDITION 1", "0001", sentMessage.EM_MessageNum);
		AssertEquals("PRE-CONDITION 2", string.Empty, receivedMessage.EM_MessageNum);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("EM_MessageNum in sent and received message is the same", sentMessage.EM_MessageNum, receivedMessage.EM_MessageNum);
	}

	public void TestProcessPositiveAcknowledgement()
	{
		var positiveAcknowledgement20 = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: positiveAcknowledgement20, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfAcknowledgement(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
	}

	public void TestProcessNegativeAcknowledgement()
	{
		var negativeAcknowledgement3 = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("3", "L'Autorità di certificazione non è ritenuta sicura");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: negativeAcknowledgement3, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfAcknowledgement(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ERO", entryHeader.CH_Status);
	}

	public void TestFaiureToLocateMatchingSentInterchange()
	{
		var genericAcknowledgement = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: genericAcknowledgement, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var wrongSessionGuid = ZGuid.NewZGuid();
		var interchange = receivedMessage.Interchange;
		interchange.EI_SessionGUID = wrongSessionGuid;
		entryHeader.CH_Status = "";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfAcknowledgement(entryHeader, 0);
		AssertEquals(nameof(entryHeader.CH_Status), "", entryHeader.CH_Status);

		AssertFailMessage(receivedMessage, interchange, logText: $"Unable to locate the related sent interchange with Session ID = {wrongSessionGuid}.", assertionMessage: "When Interchange Session Guid is wrong");
	}

	public void TestMissingOrCorruptedDataInterchange()
	{
		var corruptedAcknowledgement = "<Corrupted!!@##";
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: corruptedAcknowledgement, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfAcknowledgement(entryHeader, 0);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);

		AssertFailMessage(receivedMessage, receivedMessage.Interchange, logText: "Unable to parse the response message [ACK].", assertionMessage: "When Interchange Session Guid is wrong");
	}

	public void TestProcessAcknowledgementWhenEntryIsNotAwaitingOriginal()
	{
		var positiveAcknowledgement20 = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(messageText: positiveAcknowledgement20, messageType: "ACK");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		CombineAssertions("On message processing when the entry is awaiting (message is linked and status changed)", () =>
		{
			AssertNumberOfAcknowledgement(entryHeader, 1);
			AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
		});

		var negativeAcknowledgement = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("197", "Elaborazione KO: senza esito");
		var duplicatedReceivedInterchange = Factory.NewWithValidTestData<ITEDIInterchange>();
		duplicatedReceivedInterchange.EI_SessionGUID = sentMessage.Interchange.EI_SessionGUID;
		var duplicatedReceivedMessage = Factory.NewWithValidTestData<EDIMessage>();
		duplicatedReceivedInterchange.ContainedMessages.Add(duplicatedReceivedMessage);
		duplicatedReceivedMessage.EM_MessageType = "ACK";
		duplicatedReceivedMessage.EM_MessageText = negativeAcknowledgement;

		processor.ProcessMessage(duplicatedReceivedMessage);
		CombineAssertions("On message processing when the entry is not awaiting (message is linked but status not changed)", () =>
		{
			AssertNumberOfAcknowledgement(entryHeader, 2);
			AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
		});
	}

	public void TestProcessAcknowledgementWhenEntryIsNotDepositedDoesNotCreateUniqueTransactionIDRequest()
	{
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(messageText: AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema"), messageType: "ACK");
		sentMessage.EM_MessageSubType = "H1";
		sentMessage.EM_MessageType = "NEW";
		sentMessage.IsTransmitMessage = true;
		sentMessage.EM_Status = "SNT";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		entryHeader.CH_EntryStatus = "ACO";

		var originalReceivedInterchange = Factory.New<EDIInterchange>();
		originalReceivedInterchange.IsTransmitInterchange = false;
		originalReceivedInterchange.EI_InterchangeType = "ACK";
		originalReceivedInterchange.EI_ApplicationCode = "ITH";
		originalReceivedInterchange.EI_SessionGUID = sentMessage.Interchange.EI_SessionGUID;
		originalReceivedInterchange.EI_BodyText = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("40", "Test");

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		entryHeader.Messages.Reload(true);
		AssertNumberOfUniqueTransactionIdentifierMessages(entryHeader, expectedNumberOfMessages: 0);
	}

	public void TestProcessAcknowledgementWhenEntryIsDepositedButRelatedInterchangeIsNotFoundDoesNotCreateUniqueTransactionIDRequest()
	{
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(messageText: AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema"), messageType: "ACK");
		sentMessage.EM_MessageSubType = "H1";
		sentMessage.EM_MessageType = "NEW";
		sentMessage.IsTransmitMessage = true;
		sentMessage.EM_Status = "SNT";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		entryHeader.CH_EntryStatus = "DEP";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		entryHeader.Messages.Reload(true);
		AssertNumberOfUniqueTransactionIdentifierMessages(entryHeader, expectedNumberOfMessages: 0);
	}

	public void TestProcessAcknowledgementWhenEntryIsDepositedCreatesUniqueTransactionIDRequest()
	{
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(messageText: AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("20", "Acquisito a sistema"), messageType: "ACK");
		sentMessage.EM_MessageSubType = "H1";
		sentMessage.EM_MessageType = "NEW";
		sentMessage.IsTransmitMessage = true;
		sentMessage.EM_Status = "SNT";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		entryHeader.CH_EntryStatus = "DEP";

		var originalReceivedInterchange = Factory.New<EDIInterchange>();
		originalReceivedInterchange.IsTransmitInterchange = false;
		originalReceivedInterchange.EI_InterchangeType = "ACK";
		originalReceivedInterchange.EI_ApplicationCode = "ITH";
		originalReceivedInterchange.EI_SessionGUID = sentMessage.Interchange.EI_SessionGUID;
		originalReceivedInterchange.EI_BodyText = AcknowledgementResponseMessageProcessorTestHelper.BuildAcknowledgement("40", "Test");

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		entryHeader.Messages.Reload(true);
		AssertNumberOfUniqueTransactionIdentifierMessages(entryHeader, expectedNumberOfMessages: 1);
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "ACK" };

	void AssertNumberOfAcknowledgement(CusEntryHeader entryHeader, int expectedNumberOfMessages)
	{
		AssertEquals("Number of ACK messages", expectedNumberOfMessages, GetNumberOfMessages(entryHeader, "ACK"));
	}

	void AssertNumberOfUniqueTransactionIdentifierMessages(CusEntryHeader entryHeader, int expectedNumberOfMessages)
	{
		AssertEquals("Number of IUT messages", expectedNumberOfMessages, GetNumberOfMessages(entryHeader, "IUT"));
	}

	int GetNumberOfMessages(CusEntryHeader entryHeader, string messageType)
		=> entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == messageType);

	protected override AcknowledgementResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
	{
		return new AcknowledgementResponseMessageProcessor(logger);
	}
}

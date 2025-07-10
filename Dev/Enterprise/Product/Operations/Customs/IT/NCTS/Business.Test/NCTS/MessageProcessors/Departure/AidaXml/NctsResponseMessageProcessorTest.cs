using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<ResponseMessageProcessor>
{
	public void TestProcessNegativeResponse_WhenUnlocked()
	{
		var negativeResponseText = ManifestResourceHelper.ReadManifestResourceContent(NegativeResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: negativeResponseText, "RES");
		var movementHeader = nctsHeader.MovementHeader;

		AssertEquals("[PRE-CONDITION]", SentMessageApplicationReference, sentMessage.EM_ApplicationReference);
		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var unlockForEditLog = nctsHeader.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);
		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(nctsHeader.IsLocked), expected: false, nctsHeader.IsLocked);
			AssertNull("Unlock log", unlockForEditLog);
		});
	}

	public void TestProcessNegativeResponse_WhenLocked()
	{
		var negativeResponseText = ManifestResourceHelper.ReadManifestResourceContent(NegativeResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: negativeResponseText, "RES");
		nctsHeader.LockFile(string.Empty);
		var movementHeader = nctsHeader.MovementHeader;

		AssertEquals("[PRE-CONDITION]", SentMessageApplicationReference, sentMessage.EM_ApplicationReference);
		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var unlockForEditLog = nctsHeader.Logs.MostRecentLogByEventTime(Events.UnlockForEdit);
		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ERR", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(nctsHeader.IsLocked), expected: false, nctsHeader.IsLocked);
			AssertNotNull("Unlock log", unlockForEditLog);
			AssertEquals("Unlock log ReferenceFreeText", "Response message error", unlockForEditLog.ReferenceFreeText);
		});
	}

	public void TestProcessPositiveResponse_UpdateMovementReferenceNumberOnly()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, "RES");
		var movementHeader = nctsHeader.MovementHeader;
		AssertEquals("[PRE-CONDITION]", SentMessageApplicationReference, sentMessage.EM_ApplicationReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var regularEntryNumber = GetCusEntryNumber(nctsHeader, "REG");
		var clearanceEntryNumber = GetCusEntryNumber(nctsHeader, "CLR");

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "22ITQTG4TD591055R8", nctsHeader.MovementReferenceNumber);

			AssertNotNull("EntryHeader -> CusEntryNumber", regularEntryNumber);
			AssertNull("EntryHeader -> RegistrationInfo", clearanceEntryNumber);
		});

		AssertEntryNumber(regularEntryNumber, "REG", "4 T-D591055", "CUS", "279100", new ZDate(2023, 12, 28));
	}

	[TestDate(2023, 09, 05)]
	public void TestProcessPositiveResponse_UpdateClearance()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, "RES");
		var movementHeader = nctsHeader.MovementHeader;
		AssertEquals("[PRE-CONDITION]", SentMessageApplicationReference, sentMessage.EM_ApplicationReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var regularEntryNumber = GetCusEntryNumber(nctsHeader, "REG");
		var clearanceEntryNumber = GetCusEntryNumber(nctsHeader, "CLR");

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);

			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "REL", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "23ITQ0B08AA33266J0", nctsHeader.MovementReferenceNumber);

			AssertNotNull("EntryHeader -> CusEntryNumber", regularEntryNumber);
			AssertNotNull("EntryHeader -> RegistrationInfo", clearanceEntryNumber);
		});
		AssertEntryNumber(regularEntryNumber, "REG", "8 -AA33266", "CUS", "279100", new ZDate(2023, 12, 28));
		AssertEntryNumber(clearanceEntryNumber, "CLR", "IN5Z6S", "CUS", "", new ZDate(2023, 12, 28));
	}

	public void TestProcessPositiveResponse_NotReleased()
	{
		var positiveNotReleasedResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveNotReleasedResponseManifestResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveNotReleasedResponseText, "RES");
		var movementHeader = nctsHeader.MovementHeader;
		AssertEquals("[PRE-CONDITION]", SentMessageApplicationReference, sentMessage.EM_ApplicationReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "CO3", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "23ITQXT08AA16430J6", nctsHeader.MovementReferenceNumber);
		});
	}

	[TestDate(2023, 09, 05)]
	public void TestProcessMultipleResponsesInRandomOrder()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);

		var (nctsHeader, sentMessage, positiveMessage2) = PrepareTestData(messageText: positiveResponseText, "RES");
		var movementHeader = nctsHeader.MovementHeader;
		var sentSessionGuid = sentMessage.Interchange.EI_SessionGUID;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(positiveMessage2);
		Factory.Save();

		var regularEntryNumber = GetCusEntryNumber(nctsHeader, "REG");
		AssertNotNull("EntryHeader -> CusEntryNumber", regularEntryNumber);
		AssertEntryNumber(regularEntryNumber, "REG", "4 T-D591055", "CUS", "279100", new ZDate(2023, 12, 28));

		CombineAssertions("Positive Message", () =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);

			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "22ITQTG4TD591055R8", nctsHeader.MovementReferenceNumber);
		});

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", sentSessionGuid);
		var ackProcessor = new AcknowledgementResponseMessageProcessor(new LoggingInformationForTesting());
		ackProcessor.ProcessMessage(ackMessage);
		Factory.Save();

		CombineAssertions("Ack Message", () =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, "ACK");

			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "MRN", movementHeader.BM_CustomsStatus);
		});

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", sentSessionGuid);
		processor.ProcessMessage(clearanceMessage);
		Factory.Save();

		CombineAssertions("Clearance Message", () =>
		{
			AssertNumberOfResponseMessages(movementHeader, 2);

			AssertEquals(nameof(movementHeader.BM_MessageStatus), "SNT", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "", movementHeader.BM_CustomsStatus);
		});

		var clearanceEntryNumber = GetCusEntryNumber(nctsHeader, "CLR");
		AssertNotNull("EntryHeader -> RegistrationInfo", clearanceEntryNumber);
		AssertEntryNumber(clearanceEntryNumber, "CLR", "IN5Z6S", "CUS", "", new ZDate(2023, 12, 28));
	}

	public void TestProcessResponseMessageOriginatedFromIutRequest()
	{
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var (nctsHeader, sentMessage, positiveResponseMessage) = PrepareTestData(positiveResponseText, "RES");
		sentMessage.EM_MessageText = "<IUT>20231228D17021414095</IUT>";
		sentMessage.EM_MessageType = "IUT";
		sentMessage.EM_ApplicationReference = "IUT";

		var movementHeader = nctsHeader.MovementHeader;
		var newSentMessage = movementHeader.Messages.AddNew();
		var newSentInterchange = Factory.New<EDIInterchange>();
		newSentInterchange.ContainedMessages.Add(newSentMessage);
		newSentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");
		newSentMessage.EM_MessageType = "NEW";
		newSentMessage.IsTransmitMessage = true;
		newSentMessage.EM_Status = "SNT";
		newSentMessage.EM_ApplicationReference = "TRA";

		var ackReceivedMessage = movementHeader.Messages.AddNew();
		var ackReceivedInterchange = Factory.New<EDIInterchange>();
		ackReceivedInterchange.ContainedMessages.Add(ackReceivedMessage);
		ackReceivedInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");
		ackReceivedMessage.EM_MessageType = "ACK";
		ackReceivedMessage.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(positiveResponseMessage);

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "MRN", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(nctsHeader.MovementReferenceNumber), "22ITQTG4TD591055R8", nctsHeader.MovementReferenceNumber);
		});
		var registrationEntryNumber = GetCusEntryNumber(nctsHeader, "REG");
		AssertNotNull("NctsHeader -> RegistrationInfo", registrationEntryNumber);
		AssertEntryNumber(registrationEntryNumber, "REG", "4 T-D591055", "CUS", "279100", new ZDate(2023, 12, 28));
	}

	public void TestProcessResponseMessageOriginatedFromIutRequestFailsIfNotAbleToFindTheOriginalMessage()
	{
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);

		var (_, sentMessage, positiveMessage) = PrepareTestData(positiveResponseText, "RES");
		sentMessage.EM_MessageText = "<IUT>1234567890</IUT>";
		sentMessage.EM_MessageType = "IUT";
		sentMessage.EM_ApplicationReference = "IUT";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(positiveMessage);
		AssertMessageProcessingFailure(positiveMessage, positiveMessage.Interchange, "Unable to retrieve the original sent message for the IUT request '1234567890'");
	}

	public void TestDuplicateInterchangeForEntry()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var positiveBodyText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var positiveClearanceBodyText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var messageNumber = 1;
		var processor = GetMessageProcessor();

		var (receivedInterchange, receivedMessage) = AddReceivedMessage(nctsHeader, positiveBodyText, ref messageNumber);
		processor.ProcessMessage(receivedMessage);

		CombineAssertions("Interchange message processed", () =>
		{
			AssertEquals("Message is linked to Entry", nctsHeader.MovementHeader.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "RCV", receivedMessage.EM_Status);
			AssertEquals("Interchange status", "RCV", receivedInterchange.EI_Status);
		});

		var (receivedDupInterchange, receivedDupMessage) = AddReceivedMessage(nctsHeader, positiveBodyText, ref messageNumber);
		processor.ProcessMessage(receivedDupMessage);

		CombineAssertions("Duplicate interchange message discarded", () =>
		{
			AssertEquals("Message isn't linked to Entry", ZGuid.Empty, receivedDupMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "FAL", receivedDupMessage.EM_Status);
			AssertEquals("Interchange status", "FAL", receivedDupInterchange.EI_Status);
			AssertLoggerContainsLogText("Message discarded because duplicated in the declaration");
		});

		var (receivedDiffInterchange, receivedDiffMessage) = AddReceivedMessage(nctsHeader, positiveClearanceBodyText, ref messageNumber);
		processor.ProcessMessage(receivedDiffMessage);

		CombineAssertions("Different interchange message processed", () =>
		{
			AssertEquals("Message is linked to Entry", nctsHeader.MovementHeader.PK, receivedDiffMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "RCV", receivedDiffMessage.EM_Status);
			AssertEquals("Interchange status", "RCV", receivedDiffInterchange.EI_Status);
		});
	}

	public void TestProcessNegativeResponse_WhenNctsHeaderHasGuaranteeTransactions()
	{
		var negativeResponseText = ManifestResourceHelper.ReadManifestResourceContent(NegativeResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: negativeResponseText, responseMessageType: "ACK");
		nctsHeader.MovementHeader.BM_MessageStatus = "SNT";
		nctsHeader.BH_JobReference = "A0001";

		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, sentMessage.EM_MessageNum, nctsHeader.BH_JobReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		ReloadTransactions(transactions);

		CombineAssertions("Guarantee Transactions", () =>
		{
			AssertNull("First PND with matching AppId and Reference is deleted", transactions[0]);
			AssertNull("Second PND with matching AppId and Reference is deleted", transactions[1]);
			AssertNotNull("CON with matching AppId and Reference isn't deleted", transactions[2]);
			AssertNotNull("PND with matching Reference and different AppId isn't deleted", transactions[3]);
			AssertNotNull("PND with matching AppId and different Reference isn't deleted", transactions[4]);
		});
	}

	public void TestProcessPositiveResponse_WhenNctsHeaderHasGuaranteeTransactions()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);

		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, responseMessageType: "ACK");
		nctsHeader.MovementHeader.BM_MessageStatus = "SNT";
		nctsHeader.BH_JobReference = "A0001";

		var transactions = CusGuaranteeLineTestHelper.GetGuaranteeLineTransactionTestCases(Factory, sentMessage.EM_MessageNum, nctsHeader.BH_JobReference);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		ReloadTransactions(transactions);

		const string expectedPreviousComment = "NCTS departure test";
		const string expectedUpdatedComment = "NCTS departure 22ITQTG4TD591055R8";

		AssertTransaction("First PND transaction with matching AppId and Reference is updated", transactions[0], PermitTransactionStatusList.Codes.Confirmed, expectedUpdatedComment);
		AssertTransaction("Second PND transaction with matching AppId and Reference is updated", transactions[1], PermitTransactionStatusList.Codes.Confirmed, expectedUpdatedComment);
		AssertTransaction("CON transaction with matching AppId and Reference isn't updated", transactions[2], PermitTransactionStatusList.Codes.Confirmed, expectedPreviousComment);
		AssertTransaction("PND transaction with matching Reference and different AppId isn't updated", transactions[3], PermitTransactionStatusList.Codes.Pending, expectedPreviousComment);
		AssertTransaction("PND transaction with matching AppId and different Reference isn't updated", transactions[4], PermitTransactionStatusList.Codes.Pending, expectedPreviousComment);

		void AssertTransaction(string message, BaseCusGuaranteeLineTransaction transaction, string expectedStatus, string expectedComment)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("CPL_TransactionStatus", expectedStatus, transaction.CPL_TransactionStatus);
				AssertEquals("CPL_Comment", expectedComment, transaction.CPL_Comment);
			});
		}
	}

	protected override ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new ResponseMessageProcessor(logger);

	(EDIInterchange, EDIMessage) AddReceivedMessage(NctsHeader nctsHeader, string bodyText, ref int messageNumber)
	{
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.NumberStrategy = new FixedMessageNumberStrategy(messageNumber++);

		var sentMessage = sentInterchange.ContainedMessages.AddNew();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_ApplicationReference = "IMP";
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		sentMessage.EM_MessageType = "NEW";

		nctsHeader.MovementHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.New<EDIInterchange>();
		receivedInterchange.NumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
		receivedInterchange.EI_ApplicationCode = "ITH";
		receivedInterchange.EI_ReceiveTransmit = "RCV";
		receivedInterchange.EI_BodyText = bodyText;

		var receivedMessage = receivedInterchange.ContainedMessages.AddNew();
		receivedMessage.EM_ApplicationCode = receivedInterchange.EI_ApplicationCode;
		receivedMessage.EM_ReceiveTransmit = receivedInterchange.EI_ReceiveTransmit;
		receivedMessage.EM_MessageText = receivedInterchange.EI_BodyText;
		receivedMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		receivedMessage.EM_MessageType = "RES";

		Factory.Save();

		return (receivedInterchange, receivedMessage);
	}

	void ReloadTransactions(BaseCusGuaranteeLineTransaction[] transactions)
	{
		for (var i = 0; i < transactions.Length; i++)
		{
			transactions[i] = Factory.Load<BaseCusGuaranteeLineTransaction>(transactions[i].PK);
		}
	}

	const string NegativeResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponseNegative.xml";
	const string PositiveNotReleasedResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRNNotReleased.xml";
}

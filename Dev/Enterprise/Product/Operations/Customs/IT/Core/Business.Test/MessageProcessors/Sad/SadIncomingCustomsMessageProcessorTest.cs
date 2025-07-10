using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SadIncomingCustomsMessageProcessorTest<TMessageProcessor> : IncomingCustomsMessageProcessorTest<TMessageProcessor, ISadCustomsLinkedObjectAdapter>
	where TMessageProcessor : IncomingCustomsMessageProcessor<ISadCustomsLinkedObjectAdapter>
{
	public void TestReceivedMessageFailingProcessWhenContentIsNotValid()
	{
		(_, var receivedEdiMessage, var receivedEdiInterchange) = SetupDeclarationAndGetDataForTest("222285", NotValidContent, "name", ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(receivedEdiMessage);

		AssertFailMessage(receivedEdiMessage, receivedEdiInterchange, logText: "Unable to parse the message content.");
	}

	public void TestReceivedMessageFailingWhenUnmatchedFilename()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(_, var receivedEdiMessage, var receivedEdiInterchange) = SetupDeclarationAndGetDataForTest("222285", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);

		var invalidReceivedFileName = GetInvalidResponseFileName();

		receivedEdiInterchange.EI_HeaderText = GetEDIInterchangeText(invalidReceivedFileName, eHubTrackingIDFromSentInterchange);
		processor.ProcessMessage(receivedEdiMessage);

		AssertFailMessage(receivedEdiMessage, receivedEdiInterchange, logText: $"Received File Name: {invalidReceivedFileName} does not match with Sent IDOC File Name: {ValidIdocFileName}");

		string GetInvalidResponseFileName()
		{
			var filenameDate = new ZDate(ZDate.Today.Year, month: ZInt.Parse(ValidIdocFileName.Substring(4, 2)), day: ZInt.Parse(ValidResponseFileName.Substring(6, 2)));
			return ValidIdocFileName.Remove(4, 4).Insert(4, filenameDate.AddDays(1).ToString("MMdd"));
		}
	}

	protected (CusEntryHeader, ITEDIMessage, EDIInterchange) SetupDeclarationAndGetDataForTest(string messageNumber, string customsReceivedMessageText, string receivedCustomsFileName, string eHubTrackingIDFromSentInterchange, string sentIdocFileName)
	{
		(var entry, var message, var interchange, _) = SetupDeclarationAndGetDataWithJobForTest(messageNumber, customsReceivedMessageText, receivedCustomsFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName);
		return (entry, message, interchange);
	}

	protected (CusEntryHeader, ITEDIMessage, EDIInterchange, JobDeclaration) SetupDeclarationAndGetDataWithJobForTest(string messageNumber, string customsReceivedMessageText, string receivedCustomsFileName, string eHubTrackingIDFromSentInterchange, string sentIdocFileName, string sentIdocEdiInterchageText = null)
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_ApplicationCode = "BLT";
		jobDeclaration.JE_MessageType = MessageType;
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var sentIdocEdiMessage = entryHeader.Messages.AddNew();
		sentIdocEdiMessage.EM_MessageText = FormattableString.Invariant($@"HERE THERE IS THE INTERCHANGE HEADER
T{MessageSubType}           {messageNumber}00012345	A	0123456	01	02072019	HERE SOME OTHER DATA");
		sentIdocEdiMessage.EM_Status = EDIInterchange.Status.Sent;
		sentIdocEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentIdocEdiMessage.EM_MessageSubType = MessageSubType;
		sentIdocEdiMessage.EM_MessageType = IdocMessageType;
		sentIdocEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber);

		var sentEdiInterchange = Factory.New<ITEDIInterchange>();
		sentEdiInterchange.EI_InterchangeNum = "1";
		sentEdiInterchange.EI_InterchangeType = IdocMessageType;
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentEdiInterchange.EI_SessionGUID = new ZGuid(eHubTrackingIDFromSentInterchange);
		sentEdiInterchange.EI_HeaderText = GetSentHeaderText(sentIdocFileName);
		sentEdiInterchange.EI_BodyText = sentIdocEdiInterchageText;
		sentEdiInterchange.ContainedMessages.Add(sentIdocEdiMessage);

		(var receivedEdiInterchange, var receivedEdiMessage) = GetReceivedInterchangeAndMessage(customsReceivedMessageText, receivedCustomsFileName, eHubTrackingIDFromSentInterchange, "2");
		Factory.Save();

		return (entryHeader, receivedEdiMessage, receivedEdiInterchange, jobDeclaration);
	}

	protected (EDIInterchange receivedEdiInterchange, ITEDIMessage receivedEdiMessage) GetReceivedInterchangeAndMessage(string receivedMessageText, string receivedCustomsFileName, string eHubTrackingIDFromSentInterchange, string interchangeNum)
	{
		var receivedEdiMessage = GetReceivedEdiMessage();
		receivedEdiMessage.EM_MessageText = receivedMessageText;
		var receivedEdiInterchange = GetReceivedEdiInterchange(interchangeNum, receivedEdiMessage, ZGuid.NewZGuid().ToString());
		receivedEdiInterchange.EI_BodyText = receivedMessageText;
		receivedEdiInterchange.EI_HeaderText = GetEDIInterchangeText(receivedCustomsFileName, eHubTrackingIDFromSentInterchange);

		return (receivedEdiInterchange, receivedEdiMessage);
	}
	protected EDIInterchange GetReceivedEdiInterchange(ZString interchangeNum, ITEDIMessage receivedEdiMessage, string sessionGuid)
	{
		var receivedEdiInterchange = Factory.New<ITEDIInterchange>();
		receivedEdiInterchange.EI_InterchangeNum = interchangeNum;
		receivedEdiInterchange.EI_SessionGUID = new ZGuid(sessionGuid);
		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);
		receivedEdiInterchange.EI_InterchangeType = ReceivedInterchangeType;
		return receivedEdiInterchange;
	}

	protected ITEDIMessage GetReceivedEdiMessage()
	{
		var receivedEdiMessage = Factory.New<ITEDIMessage>();
		receivedEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		receivedEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		return receivedEdiMessage;
	}

	protected ZString GetEDIInterchangeText(string fileName, string eHubTrackingIDFromSentInterchange)
	{
		return FormattableString.Invariant($@"<ITMessage>	
	<MessageType>{ReceivedInterchangeType}</MessageType>
	<FileName>{fileName}</FileName>
	<eHubTrackingIDFromSentInterchange>{new ZGuid(eHubTrackingIDFromSentInterchange)}</eHubTrackingIDFromSentInterchange>
</ITMessage>");
	}

	protected ZString GetSentHeaderText(ZString idocFileName) => SadIncomingCustomsMessageProcessorTestHelper.GetSentHeaderText(idocFileName);

	protected void AssertEDIMessage(string message, EDIMessage ediMessage, string messageStatus, string messageNum, string messageText, string expectedMessageSubType = null)
	{
		CombineAssertions(message, () =>
		{
			AssertEquals("EM_ApplicationCode", "ITM", ediMessage.EM_ApplicationCode);
			AssertEquals("EM_Status", messageStatus, ediMessage.EM_Status);
			AssertEquals("EM_MessageType", ReceivedInterchangeType, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", expectedMessageSubType ?? MessageSubType, ediMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", messageNum, ediMessage.EM_MessageNum);
			AssertEquals("EM_MessageText", messageText, ediMessage.EM_MessageText);
		});
	}

	protected void AssertProcessingResult(CusEntryHeader entryHeader, ITEDIMessage receivedEdiMessage, EDIInterchange receivedEdiInterchange, ZString expectedEntryMessageStatus, ZString expectedEntryHeaderStatus, ZString expectedClonedMessageText, ZString expectedMessageNum, int expectedEntryMessagesCount = 2, string expectedMessageSubType = null)
	{
		AssertEquals("Message status", "RCV", receivedEdiMessage.EM_Status);
		CombineAssertions("EntryHeader", () =>
		{
			AssertEquals("Interchange status", "RCV", receivedEdiInterchange.EI_Status);
			AssertEquals("Entry Message Status (CH_Status) ", expectedEntryMessageStatus, entryHeader.CH_Status);
			AssertEquals("Entry status (CH_EntryStatus)", expectedEntryHeaderStatus, entryHeader.CH_EntryStatus);
			var numberOfMessagesExcludingSWR = entryHeader.Messages.Find(x => x.EM_MessageType != MessageProcessorConstants.InterchangeTypes.SingleWindowRequest).Count();
			AssertEquals("Messages count for header", expectedEntryMessagesCount, numberOfMessagesExcludingSWR);
		});
		var ediMessage = entryHeader.Messages.Find(x => x.EM_Status == "RCV").OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
		AssertNotNull("IRISP message linked to entry header", ediMessage);
		CombineAssertions("Irisp EDI Message", () =>
		{
			AssertEquals("EM_ApplicationCode", "ITM", ediMessage.EM_ApplicationCode);
			AssertEquals("EM_Status", "RCV", ediMessage.EM_Status);
			AssertEquals("EM_MessageType", ReceivedInterchangeType, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", expectedMessageSubType ?? MessageSubType, ediMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", expectedMessageNum, ediMessage.EM_MessageNum);
			AssertEquals("EM_MessageText", expectedClonedMessageText, ediMessage.EM_MessageText);
			AssertEquals("EM_EI", receivedEdiInterchange.PK, ediMessage.EM_EI);
		});
		AssertEDIMessage("Irisp EDI Message", ediMessage, messageStatus: "RCV", expectedMessageNum, expectedClonedMessageText, expectedMessageSubType);
	}

	protected ZString GetRandomMessageNumber() => new Random().Next(0, 999999).ToString();
	protected abstract ZString ValidResponseContentMessageText { get; }
	protected abstract ZString ValidResponseFileName { get; }
	protected abstract ZString ValidIdocFileName { get; }
	protected abstract ZString IdocMessageType { get; }
	protected abstract ZString ReceivedInterchangeType { get; }
	protected abstract ZString MessageSubType { get; }
	protected abstract ZString MessageType { get; }

	const string NotValidContent = "I AM NOT A VALID CONTENT";
}

public static class SadIncomingCustomsMessageProcessorTestHelper
{
	public static ZString GetSentHeaderText(ZString idocFileName)
	{
		return FormattableString.Invariant($@"
<ITMessage>	
	<MessageType>{idocFileName.SubstringSafe(9, 1)}</MessageType>
	<Header>845A            {idocFileName}            279100    13149600150     003 00006</Header>
</ITMessage>");
	}
}

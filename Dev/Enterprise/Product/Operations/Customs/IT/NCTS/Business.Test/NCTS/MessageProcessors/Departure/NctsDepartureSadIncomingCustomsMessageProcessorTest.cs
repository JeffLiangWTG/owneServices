using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsDepartureSadIncomingCustomsMessageProcessorTest<TMessageProcessor> : NctsDepartureIncomingCustomsMessageProcessorTest<TMessageProcessor, ISadCustomsLinkedObjectAdapter>
   where TMessageProcessor : IncomingCustomsMessageProcessor<ISadCustomsLinkedObjectAdapter>
{
	public void TestReceivedMessageFailingProcessWhenContentIsNotValid()
	{
		(_, var receivedEdiMessage, var receivedEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest("222285", NotValidContent, "name", ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(receivedEdiMessage);

		AssertFailMessage(receivedEdiMessage, receivedEdiInterchange, logText: "Unable to parse the message content.");
	}

	public void TestReceivedMessageFailingWhenUnmatchedFilename()
	{
		var eHubTrackingIDFromSentInterchange = ZGuid.NewZGuid().ToString();
		(_, var receivedEdiMessage, var receivedEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest("222285", ValidResponseContentMessageText, ValidResponseFileName, eHubTrackingIDFromSentInterchange, ValidIdocFileName);

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

	public void TestReceivedMessageFailingWhenLinkedToNonDepartureJob()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;

		(_, var receivedEdiMessage, var receivedEdiInterchange) = SetupNctsHeaderAndGetDataForTest(nctsHeader, "222285", ValidResponseContentMessageText, ValidResponseFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		processor.ProcessMessage(receivedEdiMessage);

		AssertFailMessage(receivedEdiMessage, receivedEdiInterchange, logText: "Unsupported linked business object found.");
	}

	protected (NctsHeader, ITEDIMessage, EDIInterchange) SetupDepartureNctsHeaderAndGetDataForTest(string messageNumber, string customsReceivedMessageText, string receivedCustomsFileName, string eHubTrackingIDFromSentInterchange, string sentIdocFileName, string sentIdocEdiInterchageText = null)
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		return SetupNctsHeaderAndGetDataForTest(nctsHeader, messageNumber, customsReceivedMessageText, receivedCustomsFileName, eHubTrackingIDFromSentInterchange, sentIdocFileName, sentIdocEdiInterchageText);
	}

	protected (NctsHeader, ITEDIMessage, EDIInterchange) SetupNctsHeaderAndGetDataForTest(NctsHeader nctsHeader, string messageNumber, string customsReceivedMessageText, string receivedCustomsFileName, string eHubTrackingIDFromSentInterchange, string sentIdocFileName, string sentIdocEdiInterchageText = null)
	{
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationSent;

		var sentIdocEdiMessage = nctsHeader.Messages.AddNew();
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
		return (nctsHeader, receivedEdiMessage, receivedEdiInterchange);
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
	EDIInterchange GetReceivedEdiInterchange(ZString interchangeNum, ITEDIMessage receivedEdiMessage, string sessionGuid)
	{
		var receivedEdiInterchange = Factory.New<ITEDIInterchange>();
		receivedEdiInterchange.EI_InterchangeNum = interchangeNum;
		receivedEdiInterchange.EI_SessionGUID = new ZGuid(sessionGuid);
		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);
		receivedEdiInterchange.EI_InterchangeType = ReceivedInterchangeType;
		return receivedEdiInterchange;
	}

	ITEDIMessage GetReceivedEdiMessage()
	{
		var receivedEdiMessage = Factory.New<ITEDIMessage>();
		receivedEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		receivedEdiMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		return receivedEdiMessage;
	}

	ZString GetEDIInterchangeText(string fileName, string eHubTrackingIDFromSentInterchange)
	{
		return FormattableString.Invariant($@"<ITMessage>	
	<MessageType>{ReceivedInterchangeType}</MessageType>
	<FileName>{fileName}</FileName>
	<eHubTrackingIDFromSentInterchange>{new ZGuid(eHubTrackingIDFromSentInterchange)}</eHubTrackingIDFromSentInterchange>
</ITMessage>");
	}

	protected ZString GetSentHeaderText(ZString idocFileName) => SadIncomingCustomsMessageProcessorTestHelper.GetSentHeaderText(idocFileName);

	protected void AssertProcessingResult(NctsHeader nctsHeader, ITEDIMessage receivedEdiMessage, EDIInterchange receivedEdiInterchange, ZString expectedEntryMessageStatus, ZString expectedEntryHeaderStatus, ZString expectedClonedMessageText, ZString expectedMessageNum, int expectedEntryMessagesCount = 2, string expectedMessageSubType = null)
	{
		AssertEquals(nameof(receivedEdiMessage.EM_Status), "RCV", receivedEdiMessage.EM_Status);
		CombineAssertions(nameof(nctsHeader), () =>
		{
			AssertEquals(nameof(receivedEdiInterchange.EI_Status), "RCV", receivedEdiInterchange.EI_Status);
			AssertEquals(nameof(nctsHeader.BH_MessageStatus), expectedEntryMessageStatus, nctsHeader.BH_MessageStatus);
			AssertEquals(nameof(nctsHeader.MovementHeader.BM_CustomsStatus), expectedEntryHeaderStatus, nctsHeader.MovementHeader.BM_CustomsStatus);

			var numberOfMessagesExcludingSWR = nctsHeader.Messages.Find(x => x.EM_MessageType != MessageProcessorConstants.InterchangeTypes.SingleWindowRequest).Count();
			AssertEquals(nameof(numberOfMessagesExcludingSWR), expectedEntryMessagesCount, numberOfMessagesExcludingSWR);
		});

		var processedLinkedMessage = nctsHeader.Messages.Find(x => x.EM_Status == "RCV").OrderBy(x => x.EM_SystemCreateTimeUtc).LastOrDefault();
		AssertNotNull(nameof(processedLinkedMessage), processedLinkedMessage);
		CombineAssertions($"{nameof(processedLinkedMessage)} assertions", () =>
		{
			AssertEquals(nameof(processedLinkedMessage.EM_ApplicationCode), "ITM", processedLinkedMessage.EM_ApplicationCode);
			AssertEquals(nameof(processedLinkedMessage.EM_Status), "RCV", processedLinkedMessage.EM_Status);
			AssertEquals(nameof(processedLinkedMessage.EM_MessageType), ReceivedInterchangeType, processedLinkedMessage.EM_MessageType);
			AssertEquals(nameof(processedLinkedMessage.EM_MessageSubType), expectedMessageSubType ?? MessageSubType, processedLinkedMessage.EM_MessageSubType);
			AssertEquals(nameof(processedLinkedMessage.EM_MessageNum), expectedMessageNum, processedLinkedMessage.EM_MessageNum);
			AssertEquals(nameof(processedLinkedMessage.EM_MessageText), expectedClonedMessageText, processedLinkedMessage.EM_MessageText);
			AssertEquals(nameof(processedLinkedMessage.EM_EI), receivedEdiInterchange.PK, processedLinkedMessage.EM_EI);
		});
	}

	protected void AssertEntryNumber(CusEntryNumber entryNumber, ZString expectedEntryType, ZString expectedEntryNum, ZString expectedCategory, ZString expectedEntryLineReference, ZDateTime expectedIssueDate)
	{
		CombineAssertions($"Asserting entry number: '{entryNumber.CE_EntryType}'", () =>
		{
			AssertEquals(nameof(entryNumber.CE_EntryType), expectedEntryType, entryNumber.CE_EntryType);
			AssertEquals(nameof(entryNumber.CE_EntryNum), expectedEntryNum, entryNumber.CE_EntryNum);
			AssertEquals(nameof(entryNumber.CE_Category), expectedCategory, entryNumber.CE_Category);
			AssertEquals(nameof(entryNumber.CE_EntryLineReference), expectedEntryLineReference, entryNumber.CE_EntryLineReference);
			AssertEquals(nameof(entryNumber.CE_IssueDate), expectedIssueDate, entryNumber.CE_IssueDate);
			AssertEquals(nameof(entryNumber.CE_EntryIsSystemGenerated), ZBool.True, entryNumber.CE_EntryIsSystemGenerated);
		});
	}

	protected ZString GetRandomMessageNumber() => new Random().Next(0, 999999).ToString();
	protected abstract ZString ValidResponseContentMessageText { get; }
	protected abstract ZString ValidResponseFileName { get; }
	protected abstract ZString ValidIdocFileName { get; }
	protected abstract ZString IdocMessageType { get; }
	protected abstract ZString ReceivedInterchangeType { get; }
	protected abstract ZString MessageSubType { get; }

	const string NotValidContent = "I AM NOT A VALID CONTENT";
}

static class SadIncomingCustomsMessageProcessorTestHelper
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

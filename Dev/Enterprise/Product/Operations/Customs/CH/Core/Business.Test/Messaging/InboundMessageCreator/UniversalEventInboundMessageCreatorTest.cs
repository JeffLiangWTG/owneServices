using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

class UniversalEventInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsPassar;

	protected override string InterchangeType => MessageTypeCodeList.Codes.MSL;

	protected override (string From, string BodyText)[] FailingDeserializations => Array.Empty<(string From, string BodyText)>();

	protected override string UnsupportedSchemaVersion => string.Empty;

	protected override string GetExpectedMessageText(string parsingResult) => UniversalEventTestDataHelper.ParseBodyFromUniversalInterchange(parsingResult);

	protected override IInboundMessageCreator GetMessageCreator() => new UniversalEventInboundMessageCreator(Logger);

	public override void TestResponseDoNotContainContentType()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = @"Test String";

		MessageCreator.CreateMessagesForInterchange(interchange);
		AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
		AssertEquals("EDI Message Status should be: ", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages[0].EM_Status);
		AssertContains("EDI Message Log should contain: ", "error reading Universal Event response", interchange.ContainedMessages[0].Logs.MostRecentLog?.ReferenceFreeText);
		ErrorReporter.Instance.Clear();
	}

	public void TestAcceptance() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK"), MessageSubTypeCodeList.Codes.Acknowledged);

	public void TestRejection() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IRJ"), MessageSubTypeCodeList.Codes.Rejected);

	protected override string UnrecognizedResponseMessage => UniversalEventTestDataHelper.CreateUniversalInterchangeXml("");

	public void TestGenerateMessagesFromInterchange_XER()
	{
		var sessionGuid = Guid.NewGuid();
		var bodyText = UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IRJ");
		var trxInterchangeType = MessageTypeCodeList.Codes.MSG;

		var trxInterchange = Factory.New<EDIInterchange>();
		trxInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		trxInterchange.EI_SessionGUID = sessionGuid;
		trxInterchange.EI_InterchangeType = trxInterchangeType;

		var rcvInterchange = Factory.New<EDIInterchange>();
		rcvInterchange.EI_ApplicationCode = ApplicationCode;
		rcvInterchange.EI_BodyText = bodyText;
		rcvInterchange.EI_InterchangeType = MessageTypeCodeList.Codes.XER;
		rcvInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		rcvInterchange.EI_Status = EDIInterchange.Status.Queued;
		rcvInterchange.EI_SessionGUID = sessionGuid;

		MessageCreator.CreateMessagesForInterchange(rcvInterchange);

		AssertEquals($"InboundMessageCreatorTest for {ApplicationCode} should create 1 EDI Messages", 1, rcvInterchange.ContainedMessages.Count);

		var ediMessage = rcvInterchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault();

		CombineAssertions($"EDIMessage", () =>
		{
			AssertEquals("EM_ApplicationCode", rcvInterchange.EI_ApplicationCode, ediMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", trxInterchangeType, ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.Rejected, ediMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, ediMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
			AssertEquals("EM_MessageNum", "0001", ediMessage.EM_MessageNum);
			AssertEquals("EM_EI", rcvInterchange.PK, ediMessage.EM_EI);
			AssertEquals("EM_GB", rcvInterchange.EI_GB, ediMessage.EM_GB);
			AssertEquals("EM_MessageText", UniversalEventTestDataHelper.ParseBodyFromUniversalInterchange(bodyText), ediMessage.EM_MessageText);
		});
	}
}

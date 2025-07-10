using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CharteraOutputGetMessageInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput;

	protected override string InterchangeType => MessageTypeCodeList.Codes.MSG;

	protected override (string From, string BodyText)[] FailingDeserializations => System.Array.Empty<(string From, string BodyText)>();

	protected override string UnsupportedSchemaVersion => string.Empty;

	protected override string GetExpectedMessageText(string parsingResult) => UniversalEventTestDataHelper.ParseBodyFromUniversalInterchange(parsingResult);

	protected override IInboundMessageCreator GetMessageCreator() => new CharteraOutputGetMessageInboundMessageCreator(Logger);

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

	protected override string UnrecognizedResponseMessage => UniversalEventTestDataHelper.CreateUniversalInterchangeXml(string.Empty);

	public void TestRejection() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IRJ"), MessageSubTypeCodeList.Codes.Rejected);

	public void TestAcknowledgedNoDocument() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK"), MessageSubTypeCodeList.Codes.Undefined);

	public void TestAcknowledgedWithDocument() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK", responseMessage: TestingData.CharteraDocument), MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryResult);

	public void TestAcknowledgedWithSearchResult() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK", responseMessage: TestingData.CharteraSearchResult), MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchResult);

	public void TestAcknowledgedWithError() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK", responseMessage: TestingData.CharteraError), MessageSubTypeCodeList.Codes.CharteraOutputError);
}

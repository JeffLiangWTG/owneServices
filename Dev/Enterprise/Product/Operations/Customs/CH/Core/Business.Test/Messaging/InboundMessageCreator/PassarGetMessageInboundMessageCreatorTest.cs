using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarGetMessageInboundMessageCreator))]
sealed class PassarGetMessageInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsPassar;

	protected override string InterchangeType => MessageTypeCodeList.Codes.MSG;

	protected override (string From, string BodyText)[] FailingDeserializations => System.Array.Empty<(string From, string BodyText)>();

	protected override string UnsupportedSchemaVersion => string.Empty;

	protected override string GetExpectedMessageText(string parsingResult) => UniversalEventTestDataHelper.ParseBodyFromUniversalInterchange(parsingResult);

	protected override IInboundMessageCreator GetMessageCreator() => new PassarGetMessageInboundMessageCreator(Logger);

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

	public void TestUnknownAcceptanceResponse()
	{
		TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: "<test />"), MessageSubTypeCodeList.Codes.Undefined);
		AssertLog("Unknown Message Schema", logType: LogType.Warning);
	}

	public void TestAcceptance() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IAK"), MessageSubTypeCodeList.Codes.Undefined);

	public void TestRejection() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml("IRJ"), MessageSubTypeCodeList.Codes.Rejected);

	public void TestNC124DepartureActivationResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNC124()), MessageSubTypeCodeList.Codes.PassarActivationResponse);

	public void TestNC909TechnicalError() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNC909()), MessageSubTypeCodeList.Codes.PassarTechnicalError);

	public void TestNE004() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE004()), MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse);

	public void TestNE009() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE009()), MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected);

	public void TestNE021() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE021()), MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse);

	public void TestNE028() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE028()), MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse);

	public void TestNE029() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE029()), MessageSubTypeCodeList.Codes.PassarExportReleaseResponse);

	public void TestNE060() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE060()), MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification);

	public void TestNE083() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE083()), MessageSubTypeCodeList.Codes.PassarExportDeclarationIssuanceOfAssessmentDecision);

	public void TestNE096() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE096()), MessageSubTypeCodeList.Codes.PassarExportDecisionRectification);

	public void TestNE131ExportActivationResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNE131()), MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse);

	public void TestNT004DepartureAmendmentResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT004()), MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse);

	public void TestNT008ArrivalResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT008()), MessageSubTypeCodeList.Codes.PassarArrivalResponse);

	public void TestNT009DepartureWithdrawalResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT009()), MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse);

	public void TestNT019TransitDiscrepancies() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT019()), MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies);

	public void TestNT021() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT021()), MessageSubTypeCodeList.Codes.PassarNctsPayloadRequestGoodsDeclarationResponse);

	public void TestNT025ArrivalIndication() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT025()), MessageSubTypeCodeList.Codes.PassarArrivalIndication);

	public void TestNT028DepartureResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT028()), MessageSubTypeCodeList.Codes.PassarDepartureResponse);

	public void TestNT029DepartureResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT029()), MessageSubTypeCodeList.Codes.PassarTransitReleased);

	public void TestNT035RecoveryNotification() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT035()), MessageSubTypeCodeList.Codes.PassarRecoveryNotification);

	public void TestNT043V4InventoryRequest() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT043V4()), MessageSubTypeCodeList.Codes.PassarInventoryRequest);

	public void TestNT043V5InventoryRequest() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT043V5()), MessageSubTypeCodeList.Codes.PassarInventoryRequest);

	public void TestNT045TransitClosed() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT045()), MessageSubTypeCodeList.Codes.PassarTransitClosed);

	public void TestNT055DepartureCustomsStatusUpdates() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT055()), MessageSubTypeCodeList.Codes.PassarInvalidGuarantee);

	public void TestNT057UnloadingRemarksResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT057()), MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse);

	public void TestNT060DepartureCustomsStatusUpdates() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT060()), MessageSubTypeCodeList.Codes.PassarIntentionToControl);

	public void TestNT061ControlDecisionNotification() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT061()), MessageSubTypeCodeList.Codes.PassarControlDecisionNotification);

	public void TestNT140EnquiryOfNotArrivedTransit() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT140()), MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit);

	public void TestNT146NonArrivedTransitMovementInformation() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT146()), MessageSubTypeCodeList.Codes.PassarNonArrivedTransitMovementInformation);

	public void TestNT182EventDuringTheJourney() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT182()), MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney);

	public void TestNT504NationalTransitDeclarationAmendmentResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT504()), MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse);

	public void TestNT528NationalTransitDeclarationResponse() => TestGenerateMessagesFromInterchange(UniversalEventTestDataHelper.CreateUniversalInterchangeXml(responseMessage: TestingData.GetNT528()), MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse);
}

using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT009ResponseMessageProcessor))]
sealed class NT009ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	[TestDate(2021, 9, 29, 3, 50, 45)]
	public void TestProcessMessageReceived() => AssertProcessMessage(DecisionStateList.Codes.Received, CHLogicalStatusList.Codes.Accepted, string.Empty, string.Empty, Events.DeclarationCancellationQueued);

	[TestDate(2021, 9, 29, 3, 50, 45)]
	public void TestProcessMessageAccepted() => AssertProcessMessage(DecisionStateList.Codes.Accepted, CHLogicalStatusList.Codes.Accepted, NCTS5DepartureCustomsStatusList.Codes.Cancelled, string.Empty, Events.CustomsEntryStatus);

	[TestDate(2021, 9, 29, 3, 50, 45)]
	public void TestProcessMessageRejected() => AssertProcessMessage(DecisionStateList.Codes.Rejected, CHLogicalStatusList.Codes.Invalid, string.Empty, DeparturePhaseList.Codes.Declaration, Events.DeclarationCancellationRejected);

	void AssertProcessMessage(string decision, string expectedBHMessageStatus, string expectedCustomsStatus, string expectedPhase, Event expectedEvent)
	{
		const string mrn = "21CH16360164625756";
		const string mrnVersion1 = "1";
		const string mrnVersion2 = "2";

		var sessionGuid = ZGuid.NewZGuid();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(TestedMovementType);
		nctsHeader.MovementReferenceNumberSetter($"{mrn}.{mrnVersion1}");
		Factory.Save();

		CHEDIMessage receivedMessage;

		AssertProcessMessage(
			(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
			(correlationIdentifier) => TestingData.GetNT009(correlationId: correlationIdentifier, mrn: mrn, mrnVersion: mrnVersion1, decision: decision, initiatedByCustoms: "true"),
			(ediMessage) =>
			{
				AssertEquals("MRN Version same", $"{mrn}.{mrnVersion1}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus not updated", ZString.Empty, nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
				AssertEquals("EffectiveMessageStatus", expectedBHMessageStatus, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", expectedCustomsStatus, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("BM_Phase", expectedPhase, nctsHeader.MovementHeader.BM_Phase);
				if (expectedEvent == Events.CustomsEntryStatus)
				{
					AssertNotNull($"{expectedEvent.Code} Event added", nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(expectedEvent));
				}
				else
				{
					AssertNotNull($"{expectedEvent.Code} Event added", nctsHeader.Logs.MostRecentLogByEventTime(expectedEvent));
				}
			},
			assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		AssertProcessMessage(
			(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
			(correlationIdentifier) => TestingData.GetNT009(correlationId: correlationIdentifier, mrn: mrn, mrnVersion: mrnVersion2, decision: decision, initiatedByCustoms: "true"),
			(ediMessage) =>
			{
				AssertEquals("MRN Version updated", $"{mrn}.{mrnVersion2}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus new", "NEW", nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
			},
			assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override string ExpectedFriendlyName => "NT009 - Departure Withdrawal Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT009ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT009();

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => TestingData.GetNT009(mrn: mrn, mrnVersion: mrnVersion, initiatedByCustoms: "true");
}

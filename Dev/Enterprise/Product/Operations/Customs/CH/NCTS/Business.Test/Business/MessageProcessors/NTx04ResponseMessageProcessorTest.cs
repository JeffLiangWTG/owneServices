using System;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class NTx04ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	protected override string ExpectedFriendlyName => "NT004/NT504 - Departure Amendment Response Message Processor";

	protected override string[] MessageFilterSubTypes => new[] { MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse };

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NTx04ResponseMessageProcessor(Logger);

	protected abstract string GetMessage(bool initiatedByCustoms, string mrn, string mrnVersion, string correlationId = null, string decision = null, ZDateTime? decisionDateAndTime = null, ZDate? activationDeadline = null);

	protected override string GetResponseMessage() => GetMessage(false, mrn: MRN, mrnVersion: NewMRNVersion);

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => GetMessage(true, mrn: mrn, mrnVersion: mrnVersion);

	public void TestProcessMessageAccepted()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(corrlationIdentifier).nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: false, correlationId: corrlationIdentifier, decision: "ACCEPTED", mrn: MRN, mrnVersion: NewMRNVersion, decisionDateAndTime: DecisionDateAndTimeUtc, activationDeadline: ActivationDeadline),
			(ediMessage) => AssertAcceptance(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	public void TestProcessMessageAccepted_initiatedByCustoms()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{OldMRNVersion}", prepareNctsHeader: h => h.MovementHeader.BM_CustomsStatus = "XYZ").nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: true, decision: "ACCEPTED", mrn: MRN, mrnVersion: NewMRNVersion, decisionDateAndTime: DecisionDateAndTimeUtc, activationDeadline: ActivationDeadline),
			(ediMessage) => AssertAcceptance(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	void AssertAcceptance(NctsHeader nctsHeader)
	{
		AssertEquals("BM_Phase", NCTS5DeparturePhaseList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
		AssertEquals("BM_EntryDate", DecisionDateAndTime, nctsHeader.MovementHeader.BM_EntryDate);
		AssertEquals("CE_EntryNum", $"{MRN}.{NewMRNVersion}", nctsHeader.MovementReferenceEntryNumber.CE_EntryNum);
		AssertEquals("CE_ExpiryDate", ActivationDeadline, nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
		AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
		EventsTestHelper.AssertEventAdded(nctsHeader.MovementHeader, Events.MessageStatusChange, expectedReference: CHLogicalStatusList.Codes.Accepted);
	}

	public void TestProcessMessageReceived()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(corrlationIdentifier, mrn: $"{MRN}.{OldMRNVersion}").nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: false, correlationId: corrlationIdentifier, decision: "RECEIVED", mrn: MRN, mrnVersion: NewMRNVersion),
			(ediMessage) => AssertReceived(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	public void TestProcessMessageReceived_initatedByCustoms()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{OldMRNVersion}").nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: true, decision: "RECEIVED", mrn: MRN, mrnVersion: NewMRNVersion),
			(ediMessage) => AssertReceived(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	void AssertReceived(NctsHeader nctsHeader)
	{
		AssertEquals("MRN Version updated", $"{MRN}.{NewMRNVersion}", nctsHeader.MovementReferenceNumber);
		AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
		AssertEquals("BM_Phase", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
		AssertEquals("BM_EntryDate", ZDateTime.Empty, nctsHeader.MovementHeader.BM_EntryDate);
		AssertEquals("CE_ExpiryDate not updated", ZDateTime.Empty, nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
		EventsTestHelper.AssertEventAdded(nctsHeader, Events.DeclarationAmendmentQueued);
	}

	public void TestProcessMessageRejected()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(corrlationIdentifier, mrn: $"{MRN}.{OldMRNVersion}").nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: false, correlationId: corrlationIdentifier, decision: "REJECTED", mrn: MRN, mrnVersion: OldMRNVersion),
			(ediMessage) => AssertRejected(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	public void TestProcessMessageRejected_initatedByCustoms()
	{
		NctsDepartureMovementHeader nctsMovementHeader = null;

		AssertProcessMessage(
			(corrlationIdentifier) => nctsMovementHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{OldMRNVersion}").nctsHeader.MovementHeader,
			(corrlationIdentifier) => GetMessage(initiatedByCustoms: true, decision: "REJECTED", mrn: MRN, mrnVersion: OldMRNVersion),
			(ediMessage) => AssertRejected(nctsMovementHeader.Header),
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	void AssertRejected(NctsHeader nctsHeader)
	{
		AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
		AssertEquals("MRN Version not updated", $"{MRN}.{OldMRNVersion}", nctsHeader.MovementReferenceNumber);
		AssertEquals("BM_Phase not updated", ZString.Empty, nctsHeader.MovementHeader.BM_Phase);
		AssertEquals("BM_EntryDate", ZDateTime.Empty, nctsHeader.MovementHeader.BM_EntryDate);
		AssertEquals("CE_ExpiryDate not updated", ZDateTime.Empty, nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
		EventsTestHelper.AssertEventAdded(nctsHeader, Events.DeclarationAmendmentRejected);
	}

	const string MRN = "24DE000000000123J0";
	const string OldMRNVersion = "2";
	const string NewMRNVersion = "3";
	ZDateTime DecisionDateAndTimeUtc => new DateTime(2023, 2, 1, 12, 33, 16);
	ZDateTime DecisionDateAndTime => new ZDateTime(2023, 2, 1, 13, 33, 16);
	ZDate ActivationDeadline => new ZDate(2023, 3, 2);
}

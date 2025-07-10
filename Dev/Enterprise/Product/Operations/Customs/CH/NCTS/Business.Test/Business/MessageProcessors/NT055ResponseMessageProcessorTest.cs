using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT055ResponseMessageProcessor))]
sealed class NT055ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT055 - Invalid Guarantee Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarInvalidGuarantee;

	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT055ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT055();

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => TestingData.GetNT055(mrn: mrn, mrnVersion: mrnVersion);

	public void TestProcessMessage()
	{
		const string MRN = "00AA12345678900000";
		const string MRNVersion1 = "1";
		const string MRNVersion2 = "2";

		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT055(mrn: MRN, mrnVersion: MRNVersion2),
			(ediMessage) =>
			{
				AssertEquals("MRN Version updated", $"{MRN}.{MRNVersion2}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus updated", "NEW", nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, nctsHeader.MovementHeader.BM_CustomsStatus);
				Helper.AssertEvent("Event", nctsHeader.MovementHeader.Logs, Events.CustomsEntryStatus, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT055(mrn: MRN, mrnVersion: MRNVersion1),
			(ediMessage2) =>
			{
				AssertEquals("MRN Version same", $"{MRN}.{MRNVersion1}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus not updated", ZString.Empty, nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
			}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	public void TestProcessMessage_afterNT060()
	{
		const string MRN = "00AA12345678900000";
		const string MRNVersion = "1";

		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, mrn: $"{MRN}.{MRNVersion}").nctsHeader;
				nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT055(mrn: MRN, mrnVersion: MRNVersion),
			(ediMessage) =>
			{
				AssertEquals("BM_CustomsStatus set to CO4", NCTS5DepartureCustomsStatusList.Codes.CHControlForInvalidGuarantee, nctsHeader.MovementHeader.BM_CustomsStatus);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK
		);
	}
}

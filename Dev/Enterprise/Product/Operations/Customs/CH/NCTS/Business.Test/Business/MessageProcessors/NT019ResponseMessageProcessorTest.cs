using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT019ResponseMessageProcessor))]
sealed class NT019ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	protected override string ExpectedFriendlyName => "NT019 - Discrepancies at Destination Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT019ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT019();

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => TestingData.GetNT019(mrn: mrn, mrnVersion: mrnVersion);

	public void TestProcessMessage()
	{
		const string MRN = "00AA12345678900000";
		const string MRNVersion1 = "1";
		const string MRNVersion2 = "2";

		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT019(mrn: MRN, mrnVersion: MRNVersion2),
			(ediMessage) =>
			{
				AssertEquals("MRN Version updated", $"{MRN}.{MRNVersion2}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus updated", "NEW", nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination, nctsHeader.MovementHeader.BM_CustomsStatus);
				Helper.AssertEvent("Event", nctsHeader.MovementHeader.Logs, Events.CustomsEntryStatus, NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT019(mrn: MRN, mrnVersion: MRNVersion1),
			(ediMessage2) =>
			{
				AssertEquals("MRN Version same", $"{MRN}.{MRNVersion1}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus not updated", ZString.Empty, nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
			}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}

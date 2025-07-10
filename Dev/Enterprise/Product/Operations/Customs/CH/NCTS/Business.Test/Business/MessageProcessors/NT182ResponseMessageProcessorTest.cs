using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public class NT182ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT182 - Transit forwarded incident notification to ed Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney;

	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT182ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT182();

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => TestingData.GetNT182(mrn: mrn, mrnVersion: mrnVersion);

	public void TestProcessMessage()
	{
		const string MRN = "21CH16360164625756";
		const string MRNVersion1 = "1";
		const string MRNVersion2 = "2";

		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT182(mrn: MRN, mrnVersion: MRNVersion2),
			(ediMessage) =>
			{
				AssertEquals("MRN Version updated", $"{MRN}.{MRNVersion2}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus updated", "NEW", nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, nctsHeader.MovementHeader.BM_CustomsStatus);
				Helper.AssertEvent("Event", nctsHeader.MovementHeader.Logs, Events.CustomsEntryStatus, NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT182(mrn: MRN, mrnVersion: MRNVersion1),
			(ediMessage2) =>
			{
				AssertEquals("MRN Version same", $"{MRN}.{MRNVersion1}", nctsHeader.MovementReferenceNumber);
				AssertEquals("MRN CE_EntryStatus not updated", ZString.Empty, nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus);
			}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class NTx28ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	[TestDate(2023, 2, 21, 12, 0, 0)]
	public void TestProcessMessageAccepted() => AssertProcessMessage(DecisionStateList.Codes.Accepted, CHLogicalStatusList.Codes.Accepted, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, new ZDateTime(2022, 11, 03, 15, 46, 20).ToLocalBranchTime(), Events.CustomsEntryStatus, true);

	[TestDate(2023, 2, 21, 12, 0, 0)]
	public void TestProcessMessageRejected() => AssertProcessMessage(DecisionStateList.Codes.Rejected, CHLogicalStatusList.Codes.Invalid, NctsTransitStatusList.Codes.Unknown, ZDateTime.Empty, Events.DeclarationRejected, false);

	[TestDate(2023, 2, 21, 12, 0, 0)]
	public void TestProcessMessageReceived() => AssertProcessMessage(DecisionStateList.Codes.Received, CHLogicalStatusList.Codes.Invalid, NctsTransitStatusList.Codes.Unknown, ZDateTime.Empty, Events.DeclarationRejected, false);

	protected override string ExpectedFriendlyName => "NT028/NT528 - Departure Response Message Processor";

	protected override string TestedMovementType => NctsMovementType.Codes.Departure;

	protected override string[] MessageFilterSubTypes => new[] { MessageSubTypeCodeList.Codes.PassarDepartureResponse, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse };

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NTx28ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => GetResponseMessage(CorrelationIdentifier, Decision);

	protected abstract string GetResponseMessage(string correlationIdentifier, string decision);

	void AssertProcessMessage(string decision, string expectedHeaderStatus, string expectedMovementHeaderStatus, ZDateTime expectedEntryDate, Event expectedEvent, bool shouldUpdateMRN)
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier).nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => GetResponseMessage(correlationIdentifier, decision),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", expectedHeaderStatus, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", expectedMovementHeaderStatus, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("BM_EntryDate", expectedEntryDate, nctsHeader.MovementHeader.BM_EntryDate);
				if (expectedEvent == Events.CustomsEntryStatus)
				{
					AssertNotNull($"Event '{expectedEvent.CodeAndDescription}' added", nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(expectedEvent));
				}
				else
				{
					AssertNotNull($"Event '{expectedEvent.CodeAndDescription}' added", nctsHeader.Logs.MostRecentLogByEventTime(expectedEvent));
				}

				if (shouldUpdateMRN)
				{
					AssertEquals("CE_EntryNum", "22CHVL2525YYN7IZJ7.1", nctsHeader.MovementReferenceNumber);
					AssertEquals("CE_ExpiryDate", new ZDateTime(2022, 12, 03), nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
					Assert("Logged MRN update", Logger.ContainsLogEntry($"Movement Reference Number changed from '' to '22CHVL2525YYN7IZJ7.1' by the EDI Message '{ediMessage.EM_MessageNum}'."));
				}
				else
				{
					AssertEquals("CE_EntryNum", "", nctsHeader.MovementReferenceNumber);
					AssertEquals("CE_ExpiryDate", ZDateTime.Empty, nctsHeader.MovementReferenceEntryNumber.CE_ExpiryDate);
					AssertEquals("Logged MRN update", 0, Logger.Logs.Count());
				}
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	const string CorrelationIdentifier = "0091444b-8599-4333-b1f6-9a3b105775d9";
	const string Decision = DecisionStateList.Codes.Accepted;
}

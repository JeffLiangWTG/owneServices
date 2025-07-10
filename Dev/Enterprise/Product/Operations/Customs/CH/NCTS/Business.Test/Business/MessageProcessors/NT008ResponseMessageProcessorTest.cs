using System;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT008ResponseMessageProcessor))]
[TestDate(2023, 2, 1)]
[TestUtcOffset(10, 0, 0)]
sealed class NT008ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Arrival;

	public void TestProcessMessageAccepted()
	{
		NctsHeader nctsHeader = null;
		GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var decisionDateAndTimeUtc = new DateTime(2023, 2, 1, 12, 33, 16);
		var decisionDateAndTime = new ZDateTime(2023, 2, 1, 13, 33, 16);

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT008(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Accepted, arrivalReferenceNumber: "ARN123", decisionDateAndTime: decisionDateAndTimeUtc),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.CHActive, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("CE_EntryNum", "ARN123", nctsHeader.ArrivalReferenceEntryNumber.CE_EntryNum);
				AssertEquals("CE_IssueDate", decisionDateAndTime, nctsHeader.ArrivalReferenceEntryNumber.CE_IssueDate);
				AssertEquals("BM_EntryDate", decisionDateAndTime, nctsHeader.ArrivalMovementHeader.BM_EntryDate);
				var headerEvent = nctsHeader.ArrivalMovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertNotNull("CES Event added", headerEvent);
				AssertEquals("SL_Reference", NCTS5DepartureCustomsStatusList.Codes.CHActive, headerEvent.SL_Reference);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);
	}

	public void TestProcessMessageAccepted_afterNT061_CLR()
	{
		TestProcessMessageAccepted_afterNT061(NCTS5ArrivalCustomsStatusList.Codes.CHClear);
	}

	public void TestProcessMessageAccepted_afterNT061_CD4()
	{
		TestProcessMessageAccepted_afterNT061(NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);
	}

	void TestProcessMessageAccepted_afterNT061(string initialCustomStatus)
	{
		NctsHeader nctsHeader = null;
		GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var decisionDateAndTimeUtc = new DateTime(2023, 2, 1, 12, 33, 16);
		var decisionDateAndTime = new ZDateTime(2023, 2, 1, 13, 33, 16);

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = initialCustomStatus;
				return nctsHeader;
			},
			(correlationIdentifier) => TestingData.GetNT008(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Accepted, arrivalReferenceNumber: "ARN123", decisionDateAndTime: decisionDateAndTimeUtc),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("CE_EntryNum", "ARN123", nctsHeader.ArrivalReferenceEntryNumber.CE_EntryNum);
				AssertEquals("CE_IssueDate", decisionDateAndTime, nctsHeader.ArrivalReferenceEntryNumber.CE_IssueDate);
				AssertEquals("BM_EntryDate", decisionDateAndTime, nctsHeader.ArrivalMovementHeader.BM_EntryDate);

				AssertEquals("BM_CustomsStatus not changed", initialCustomStatus, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				EventsTestHelper.AssertEventAdded(nctsHeader.ArrivalMovementHeader, Events.CustomsEntryStatus, withReference: initialCustomStatus, message: $"Last CES Event by setting {initialCustomStatus} (processing NT061)");
				EventsTestHelper.AssertEventNotAdded(nctsHeader.ArrivalMovementHeader, Events.CustomsEntryStatus, withReference: NCTS5ArrivalCustomsStatusList.Codes.CHActive, message: "No CES Event with Reference ACT");
			},
		expectedMessageStatus: EDIMessage.Status.ProcessedOK
	);
	}

	public void TestProcessMessageReceived()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT008(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Received),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("BM_EntryDate", ZDateTime.Empty, nctsHeader.ArrivalMovementHeader.BM_EntryDate);
				AssertNotNull("DCP Event added", nctsHeader.Logs.MostRecentLogByEventTime(Events.DeclarationQueued));
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);
	}

	public void TestProcessMessageRejected()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT008(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Rejected),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_CustomsStatus", ZString.Empty, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("BM_EntryDate", ZDateTime.Empty, nctsHeader.ArrivalMovementHeader.BM_EntryDate);
				AssertNotNull("DCR Event added", nctsHeader.Logs.MostRecentLogByEventTime(Events.DeclarationRejected));
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);
	}

	protected override string ExpectedFriendlyName => "NT008 - Arrival Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarArrivalResponse;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT008ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT008();
}

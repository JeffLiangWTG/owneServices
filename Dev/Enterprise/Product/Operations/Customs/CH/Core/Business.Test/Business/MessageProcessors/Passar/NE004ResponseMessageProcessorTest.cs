using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

class NE004ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string ExpectedFriendlyName => "NE004 - Passar Export Declaration Amendment Response";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse;

	public void TestProcessMessage_Accepted() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Accepted, expectedCH_Status: CHLogicalStatusList.Codes.Accepted, expectedCH_PhaseStatus: PassarDeclarationPhaseList.Codes.Declaration, expectedMRNUpdate: true);

	public void TestProcessMessage_Rejected() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Rejected, expectedCH_Status: CHLogicalStatusList.Codes.Invalid, expectedDAREvent: true);

	public void TestProcessMessage_Received() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Received, expectedCH_Status: CHLogicalStatusList.Codes.Invalid);

	void TestProcessMessage(string decision, string expectedCH_Status = null, string expectedCH_PhaseStatus = null, bool expectedMRNUpdate = false, bool expectedDAREvent = false)
	{
		const string gdrn = "24CH202405270001N4";
		var decisionDateAndTimeUTC = new ZDateTime(2023, 06, 01, 12, 42, 00);
		var decisionDateAndTimeCET = new ZDateTime(2023, 06, 01, 14, 42, 00);
		var activationDeadline = new ZDate(2023, 07, 01);

		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNE004(correlationId: correlationIdentifier, decision: decision, gdrn: gdrn, gdrnVersion: "1", activationDeadline: activationDeadline, decisionDateAndTime: decisionDateAndTimeUTC),
			(ediMessage) =>
			{
				AssertEquals("CH_Status", expectedCH_Status ?? string.Empty, entryHeader.CH_Status);
				AssertEquals("CH_PhaseStatus", expectedCH_PhaseStatus ?? string.Empty, entryHeader.CH_PhaseStatus);
				if (expectedMRNUpdate)
				{
					AssertEquals("CE_EntryNum", $"{gdrn}.1", entryHeader.MovementReferenceNumber);
					AssertEquals("CE_IssueDate", decisionDateAndTimeCET, entryHeader.MovementReferenceNumberIssueDate);
					AssertEquals("CE_ExpiryDate", activationDeadline, entryHeader.MovementReferenceNumberExpiryDate);
				}
				else
				{
					AssertEquals("CE_EntryNum", ZString.Empty, entryHeader.MovementReferenceNumber);
					AssertEquals("CE_IssueDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberIssueDate);
					AssertEquals("CE_ExpiryDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberExpiryDate);
				}
				EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange);
				if (expectedDAREvent)
				{
					EventsTestHelper.AssertEventAdded(entryHeader, Events.DeclarationAmendmentRejected);
				}
				else
				{
					EventsTestHelper.AssertEventNotAdded(entryHeader, Events.DeclarationAmendmentRejected);
				}
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	public void TestLinkByGDRN()
	{
		const string gdrn = "24CH202405270001N4";

		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter($"{gdrn}.1");
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNE004(decision: PassarMessagingConstants.DecisionCodes.Accepted, initiatedByCustoms: true, gdrn: gdrn, gdrnVersion: "2"),
			(ediMessage) =>
			{
				AssertEquals("CE_EntryNum", $"{gdrn}.2", entryHeader.MovementReferenceNumber);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override string GetResponseMessage() => TestingData.GetNE004();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE004ResponseMessageProcessor(Logger);
}

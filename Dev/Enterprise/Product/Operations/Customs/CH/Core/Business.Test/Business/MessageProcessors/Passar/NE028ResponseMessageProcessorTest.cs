using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class NE028ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string ExpectedFriendlyName => "NE028 - Passar Export Declaration Response";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse;

	public void TestProcessMessage_Accepted() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, Common.Shared.MessageStatusList.Codes.ClearReplace, Events.CustomsEntryStatus, true);

	public void TestProcessMessage_Refjected() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationRejected, false);

	void TestProcessMessage(string decision, string expectedEntryHeaderStatus, string expectedEntryHeaderEntryStatus, Event expectedEvent, bool shouldUpdateEntryNum)
	{
		var issueDateTimeUTC = new ZDateTime(2023, 06, 01, 12, 42, 00);
		var issueDateTimeCET = new ZDateTime(2023, 06, 01, 14, 42, 00);
		var expiryDate = new ZDate(2023, 07, 01);

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
			(correlationIdentifier) => TestingData.GetNE028(correlationIdentifier, expiryDate, issueDateTimeUTC, decision),
			(ediMessage) =>
			{
				AssertEquals("MessageStatus", expectedEntryHeaderStatus, entryHeader.CH_Status);

				if (!expectedEntryHeaderEntryStatus.IsNullOrEmpty())
				{
					AssertEquals("CH_EntryStatus", expectedEntryHeaderEntryStatus, entryHeader.CH_EntryStatus);
				}
				if (expectedEvent != null)
				{
					EventsTestHelper.AssertEventAdded(entryHeader, expectedEvent);
				}
				if (shouldUpdateEntryNum)
				{
					AssertEquals("CE_EntryNum", "23CHODUG284YTOACN7.1", entryHeader.MovementReferenceNumber);
					AssertEquals("CE_IssueDate", issueDateTimeCET, entryHeader.MovementReferenceNumberIssueDate);
					AssertEquals("CE_ExpiryDate", expiryDate, entryHeader.MovementReferenceNumberExpiryDate);
					Assert("Logged MRN update", Logger.ContainsLogEntry($"Movement Reference Number changed from '' to '23CHODUG284YTOACN7.1' by the EDI Message '{ediMessage.EM_MessageNum}'."));
				}
				else
				{
					AssertEquals("CE_EntryNum", "", entryHeader.MovementReferenceNumber);
					AssertEquals("CE_IssueDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberIssueDate);
					AssertEquals("CE_ExpiryDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberExpiryDate);
					AssertEquals("Logged MRN update", 0, Logger.Logs.Count());
				}
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override string GetResponseMessage() => TestingData.GetNE028();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE028ResponseMessageProcessorForTesting(Logger);

	class NE028ResponseMessageProcessorForTesting : NE028ResponseMessageProcessor
	{
		public NE028ResponseMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}
	}
}

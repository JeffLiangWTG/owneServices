using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class PassarDecisionGetMessageInboundMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	public void TestProcessMessage_FindLinkedObject_NotInitiatedByCustoms()
	{
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
			(correlationIdentifier) => GetResponseMessage_FindLinkedObject_NotInitiatedByCustoms(correlationIdentifier),
			(ediMessage) =>
			{
				AssertEquals("Found by CorrelationIdentifier - EM_LinkTable", ediMessage.EM_LinkTable, entryHeader.TableName);
				AssertEquals("Found by CorrelationIdentifier - EM_LinkUniqueID", ediMessage.EM_LinkUniqueID, entryHeader.PK);
				AssertEquals("Found by CorrelationIdentifier - EM_LinkedObject", ediMessage.EM_LinkedObject, entryHeader);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK, assertEDIMessageLinked: false);
	}

	public void TestProcessMessage_FindLinkedObject_InitiatedByCustoms()
	{
		var gdrnNumber = "23CH12EXTZMNGLFXN3";
		var gdrnVersion = "1";

		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter($"{gdrnNumber}.{gdrnVersion}");
				return entryHeader;
			},
			(correlationIdentifier) => GetResponseMessage_FindLinkedObject_InitiatedByCustoms(gdrnNumber, gdrnVersion),
			(ediMessage) =>
			{
				AssertEquals("Found by GDRN - EM_LinkTable", ediMessage.EM_LinkTable, entryHeader.TableName);
				AssertEquals("Found by GDRN - EM_LinkUniqueID", ediMessage.EM_LinkUniqueID, entryHeader.PK);
				AssertEquals("Found by GDRN - EM_LinkedObject", ediMessage.EM_LinkedObject, entryHeader);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK, assertEDIMessageLinked: false);
	}

	public void TestProcessMessage(string decision, string expectedEntryHeaderStatus, string expectedEntryHeaderEntryStatus, Event expectedEvent, bool shouldUpdateEntryNum)
	{
		var issueDateTimeUTC = new ZDateTime(2023, 06, 01, 12, 42, 00);
		var issueDateTimeCET = new ZDateTime(2023, 06, 01, 14, 42, 00);
		var gdrn = "23CH12EXTZMNGLFXN3";
		var gdrnVersion = "1";

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
			(correlationIdentifier) => GetResponseMessage_ProcessMessage(correlationIdentifier, gdrn, gdrnVersion, issueDateTimeUTC, decision),
			(ediMessage) =>
			{
				AssertEquals("MessageStatus", expectedEntryHeaderStatus, entryHeader.CH_Status);
				if (!expectedEntryHeaderEntryStatus.IsEmpty())
				{
					AssertEquals("CH_EntryStatus", expectedEntryHeaderEntryStatus, entryHeader.CH_EntryStatus);
				}
				if (expectedEvent != null)
				{
					EventsTestHelper.AssertEventAdded(entryHeader, expectedEvent);
				}
				if (shouldUpdateEntryNum)
				{
					AssertEquals("CE_EntryNum", $"{gdrn}.{gdrnVersion}", entryHeader.MovementReferenceNumber);
					AssertEquals("CE_IssueDate", issueDateTimeCET, entryHeader.MovementReferenceNumberIssueDate);
					AssertEquals("CE_ExpiryDate", ZDateTime.Empty, entryHeader.MovementReferenceNumberExpiryDate);
					Assert("Logged MRN update", Logger.ContainsLogEntry($"Movement Reference Number changed from '' to '{gdrn}.{gdrnVersion}' by the EDI Message '{ediMessage.EM_MessageNum}'."));
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

	protected abstract string GetResponseMessage_FindLinkedObject_NotInitiatedByCustoms(string correlationIdentifier);

	protected abstract string GetResponseMessage_FindLinkedObject_InitiatedByCustoms(string gdrnNumber, string gdrnVersion);

	protected abstract string GetResponseMessage_ProcessMessage(string correlationIdentifier, string gdrn, string gdrnVersion, ZDateTime issueDateTimeUTC, string decision);
}

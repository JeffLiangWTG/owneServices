using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE509MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC509C;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Statement</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Non-acceptance information</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>TestStatementDescriptionValue</i></td></tr>");
			sb.Append("<tr><td><b>Invalidation Date:</b></td><td><i>" + StatusEffectiveDateTimeZoneDependent + "</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	ZDateTime StatusEffectiveDateTimeZoneDependent
	{
		get
		{
			ZDateTime statusEffective;
			ZDateTime.TryParseExact("20200723143432Z", out statusEffective, "yyyyMMddHHmmssZ");
			return statusEffective;
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC509C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><SequenceNumeric>1</SequenceNumeric><StatementCode>1</StatementCode><StatementDescription>TestStatementDescriptionValue</StatementDescription><StatementTypeCode>BAL</StatementTypeCode></AdditionalInformation><Status><EffectiveDateTime formatCode='304'>20200723143432Z</EffectiveDateTime></Status></Response></MetaData>";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE509MessageProcessor(logger);

	public new void TestProcessMessage()
	{
		AssertProcessMessage("", "", "", NLConstants.Status.Cancelled, NLConstants.EntryStatus.ExportCancellation, "");
		AssertProcessMessage("", "", CustomsEntryPhaseStatusList.Codes._514, StatusNew.Accepted, EntryStatusNew.Cancelled, CustomsEntryPhaseStatusList.Codes._515);
		AssertProcessMessage("", "", CustomsEntryPhaseStatusList.Codes._515, StatusNew.Accepted, EntryStatusNew.Cancelled, CustomsEntryPhaseStatusList.Codes._515);
		AssertProcessMessage("", "", CustomsEntryPhaseStatusList.Codes._583, StatusNew.Accepted, EntryStatusNew.Cancelled, CustomsEntryPhaseStatusList.Codes._515);
	}

	void AssertProcessMessage(string status, string entryStatus, string phaseStatus, string expectedStatus, string expectedEntryStatus, string expectedPhaseStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		entryHeader.CH_Status = status;
		entryHeader.CH_EntryStatus = entryStatus;
		entryHeader.CH_PhaseStatus = phaseStatus;
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2021, 10, 02);

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions($"Status: {status}, EntryStatus: {entryStatus}, PhaseStatus: {phaseStatus}", () =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}
}

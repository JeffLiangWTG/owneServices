using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class RRDMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", NLConstants.StatusNew.ReminderReceived, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", NLConstants.EntryStatusNew.RequestForInformation, entryHeader.CH_EntryStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	public void TestProcessMessage_PhaseStatus513()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", ZString.Empty, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", ZString.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", CustomsEntryPhaseStatusList.Codes._513, entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	public void TestProcessMessage_PhaseStatusSUP()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", ZString.Empty, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", ZString.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", CustomsEntryPhaseStatusList.Codes.SUP, entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCRRDA;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Reminder</i></td></tr>");
			sb.Append("<tr><td><b>Expiry Date:</b></td><td><i>20200612</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>RFI awaiting CRE reply to Customs</i></td></tr>");
			sb.Append("<tr><td><b>Customs Remark:</b></td><td><i>If no CRE is sent before Expiry the declaration may be canceled by Customs</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CCRRDA</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceRRD</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><Declaration><ExpirationDateTime>20200612</ExpirationDateTime></Declaration></Response></MetaData>";

	protected override string BGMReference => "TestReferenceRRD";

	protected override DMSResponseMessageProcessor MessageProcessor => new RRDMessageProcessor(logger);
}

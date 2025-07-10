using System.Text;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE556MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	public void TestProcessMessage_PhaseStatus515()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._515, ZString.Empty, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatus514_SNT()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._514, StatusNew.SentToCustoms, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatus514_ACC()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._514, StatusNew.Accepted, NLConstants.StatusNew.Accepted, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatus511_SNT()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._511, StatusNew.SentToCustoms, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatusREG_ACC()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes.REG, StatusNew.Accepted, NLConstants.StatusNew.Cancelled, EntryStatusNew.NoRelease);
	}

	public void TestProcessMessage_PhaseStatus513_SNT()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._513, StatusNew.SentToCustoms, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatus513_ACC()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._513, StatusNew.Accepted, NLConstants.StatusNew.Accepted, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatus583()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes._583, StatusNew.Accepted, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatusCRE()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes.CRE, StatusNew.SentToCustoms, NLConstants.StatusNew.Invalid, ZString.Empty);
	}

	public void TestProcessMessage_PhaseStatusSUP()
	{
		AssertProcessMessage(CustomsEntryPhaseStatusList.Codes.SUP, StatusNew.SentToCustoms, StatusNew.Invalid, ZString.Empty);
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC556C;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><H1>");
			sb.Append("Error Information");
			sb.Append("</H1><BR/>");
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>22NL13215444</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReference556</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Status details</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>STATEMENTDESC</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR><H1>");
			sb.Append("Errors reported by customs");
			sb.Append("</H1>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>ERRVAL</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>ERROR DESCRIPTION</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>REJECTIONVAL</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>LOCATION1</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>LOCATION2</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>ERRVAL2</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>ERROR DESCRIPTION2</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>REJECTIONVAL2</i></td></tr>");
			sb.Append("</table>");

			return sb.ToString();
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC556C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference556</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><StatementTypeCode>AHN</StatementTypeCode></AdditionalInformation><Control><AdditionalInformation><StatementDescription>STATEMENTDESC</StatementDescription></AdditionalInformation></Control><BusinessRejectionTypeCode>513</BusinessRejectionTypeCode><Declaration><RejectionDateTime formatCode =\"304\">20220112</RejectionDateTime><ID>22NL13215444</ID></Declaration><Error><Description>ERROR DESCRIPTION</Description><ValidationCode>ERRVAL</ValidationCode><OriginalAttributeValue>REJECTIONVAL</OriginalAttributeValue><Pointer><Location>LOCATION1</Location></Pointer><Pointer><Location>LOCATION2</Location></Pointer></Error><Error><Description>ERROR DESCRIPTION2</Description><ValidationCode>ERRVAL2</ValidationCode><OriginalAttributeValue>REJECTIONVAL2</OriginalAttributeValue></Error></Response></MetaData>";

	protected override string BGMReference => "TestReference556";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE456And556MessageProcessor(logger);

	public new void TestProcessMessage()
	{
		AssertProcessMessage("", "", NLConstants.Status.Rejection, NLConstants.EntryStatus.ExportRejection_513);
	}

	void AssertProcessMessage(ZString initialPhaseStatus, ZString initialStatus, ZString expectedStatus, ZString expectedEntryStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_PhaseStatus = initialPhaseStatus;
		entryHeader.CH_Status = initialStatus;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}
}

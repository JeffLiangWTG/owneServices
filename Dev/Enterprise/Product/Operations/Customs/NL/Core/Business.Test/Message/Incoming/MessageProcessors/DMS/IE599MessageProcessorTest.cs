using System.Text;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE599MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override DMSResponseMessageProcessor MessageProcessor => new IE599MessageProcessor(logger);

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Exit information</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Exit confirmed</i></td></tr>");
			sb.Append("<tr><td><b>Exit Date:</b></td><td><i>20240820</i></td></tr>");
			sb.Append("<tr><td><b>Control Results:</b></td><td><i>666</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC599C;

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd'\r\n    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'\r\n    xmlns='urn:wco:datamodel:WCO:DMS.Response:1'>\r\n    <WCOTypeCode>CC599C</WCOTypeCode>\r\n    <CommunicationMetaData>\r\n        <ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID>\r\n        <CommunicationsAgreementID>325656</CommunicationsAgreementID>\r\n        <Recipient>\r\n            <ID>00000001</ID>\r\n        </Recipient>\r\n        <Sender>\r\n            <ID>00000002</ID>\r\n        </Sender>\r\n    </CommunicationMetaData>\r\n    <Response>\r\n        <Control>\r\n            <TypeCode>40</TypeCode>\r\n            <InspectionStartDateTime formatCode='102'>20220620</InspectionStartDateTime>\r\n            <AdditionalInformation>\r\n                <StatementDescription>STATEMENTDESC</StatementDescription>\r\n            </AdditionalInformation>\r\n\t\t\t<ControlResult>\r\n\t\t\t\t<ID>666</ID>\r\n\t\t\t\t<Description>sb</Description>\r\n\t\t\t\t<ExitDateTime formatCode='102'>20240820</ExitDateTime>\r\n\t\t\t</ControlResult>\r\n        </Control>\r\n        <Status>\r\n            <EffectiveDateTime formatCode='304'>20220620145912Z</EffectiveDateTime>\r\n        </Status>\r\n        <RequestedDocument>\r\n            <Description>DOCDESC1</Description>\r\n            <TypeCode>TRA</TypeCode>\r\n        </RequestedDocument>\r\n        <Declaration>\r\n            <ID>22NL13215444</ID>\r\n        </Declaration>\r\n    </Response>\r\n</MetaData>";

	public void TestProcessMessage_EffectiveDate()
	{
		var testMessageText2 = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd'\r\n    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'\r\n    xmlns='urn:wco:datamodel:WCO:DMS.Response:1'>\r\n    <WCOTypeCode>CC599C</WCOTypeCode>\r\n    <CommunicationMetaData>\r\n        <ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID>\r\n        <CommunicationsAgreementID>325656</CommunicationsAgreementID>\r\n        <Recipient>\r\n            <ID>00000001</ID>\r\n        </Recipient>\r\n        <Sender>\r\n            <ID>00000002</ID>\r\n        </Sender>\r\n    </CommunicationMetaData>\r\n    <Response>\r\n        <Control>\r\n            <TypeCode>40</TypeCode>\r\n            <InspectionStartDateTime formatCode='102'>20220620</InspectionStartDateTime>\r\n            <AdditionalInformation>\r\n                <StatementDescription>STATEMENTDESC</StatementDescription>\r\n            </AdditionalInformation>\r\n\t\t\t<ControlResult>\r\n\t\t\t\t<ID>666</ID>\r\n\t\t\t\t<Description>sb</Description>\r\n\t\t\t\t<EffectiveDateTime formatCode='102'>20240820</EffectiveDateTime>\r\n\t\t\t</ControlResult>\r\n        </Control>\r\n        <Status>\r\n            <EffectiveDateTime formatCode='304'>20220620145912Z</EffectiveDateTime>\r\n        </Status>\r\n        <RequestedDocument>\r\n            <Description>DOCDESC1</Description>\r\n            <TypeCode>TRA</TypeCode>\r\n        </RequestedDocument>\r\n        <Declaration>\r\n            <ID>22NL13215444</ID>\r\n        </Declaration>\r\n    </Response>\r\n</MetaData>";
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;

		var testMessage = CreateNewTestMessage();
		testMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;
		testMessage.EM_MessageText = testMessageText2;

		Factory.Save();

		var sb = new StringBuilder();
		sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
		sb.Append("<tr><td><b>Event Type:</b></td><td><i>Exit information</i></td></tr>");
		sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Exit stopped</i></td></tr>");
		sb.Append("<tr><td><b>Exit Date:</b></td><td></td></tr>");
		sb.Append("<tr><td><b>Control Results:</b></td><td><i>666</i></td></tr>");
		sb.Append("</table></font>");

		MessageProcessor.ProcessMessage(testMessage);
		AssertEquals("EDI Message - Message Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, testMessage.EM_Status);
		AssertEquals("EDI Message - Message Interpretation", sb.ToString(), testMessage.EM_MessageInterpretation);
	}

	public void TestProcessMessage_PhaseStatus515()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", NLConstants.EntryStatusNew.GoodsExitedEU, entryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", NLConstants.StatusNew.Accepted, entryHeader.CH_Status);
		});
	}

	public void TestProcessMessage_PhaseStatus583()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._583;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", NLConstants.EntryStatusNew.GoodsExitedEU, entryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", NLConstants.StatusNew.Accepted, entryHeader.CH_Status);
			AssertEquals("CH_PhaseStatus", CustomsEntryPhaseStatusList.Codes._515, entryHeader.CH_PhaseStatus);
		});
	}

	protected override void AssertMessage(NLEDIMessage testMessage)
	{
		AssertEquals("Test AddInfo Field", "20240820", entryHeader.CH_ExitDate.ToString("yyyyMMdd"));
	}
}

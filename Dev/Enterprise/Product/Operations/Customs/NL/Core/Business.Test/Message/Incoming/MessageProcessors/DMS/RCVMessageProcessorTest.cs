using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class RCVMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCRCVA;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'>" +
			"<tr><td><b>Event Type:</b></td><td><i>Receive Message</i></td></tr>" +
			"<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>" +
			"<tr><td><b>Statement Description Type:</b></td><td><i>Receive Message</i></td></tr>" +
			"<tr><td><b>Customs Remark:</b></td><td><i>Receive Message</i></td></tr>" +
			"</table></font>";

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CCRCVA</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><PreparationDateTime>202201251458Z</PreparationDateTime><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Declaration><FunctionalReferenceID>TestReferenceABC</FunctionalReferenceID><ID>22NL123456789</ID><DeclarationOffice><ID>NL1234</ID></DeclarationOffice><Agent><ID>NL123456789B01</ID><FunctionCode>AG</FunctionCode><Contact><Name>Frans Klaassen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>++3164253699</ID><TypeCode>2</TypeCode></Communication><Communication><SequenceNumeric>2</SequenceNumeric><ID>frans.klaassen@agent.nl</ID><TypeCode>2</TypeCode></Communication></Contact></Agent><Declarant><Name>Declarant name</Name><ID>NL987654321B02</ID><Address><CityName>Amsterdam</CityName><Country>NL</Country><Line>Havenweg 36</Line><PostcodeID>1234AB</PostcodeID><Contact><Name>Jef Franssen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+3164853249</ID><TypeCode>2</TypeCode></Communication><Communication><SequenceNumeric>2</SequenceNumeric><ID>jef.franssen@declarant.nl</ID><TypeCode>1</TypeCode></Communication></Contact></Address></Declarant></Declaration></Response></MetaData>";

	protected override DMSResponseMessageProcessor MessageProcessor => new RCVMessageProcessor(logger);

	public new void TestProcessMessage()
	{
		AssertProcessMessage(JobMessageTypeList.Codes.Import, null, NLConstants.EntryStatus.AdvanceDeclarationSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.AdvanceDeclarationReceived, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Import, null, NLConstants.EntryStatus.InformationSentToCustoms, null, NLConstants.Status.Received, NLConstants.EntryStatus.InformationReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Import, null, NLConstants.EntryStatus.SupplementSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.SupplementReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Import, null, NLConstants.EntryStatus.InvalidationRequestSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.InvalidationRequestReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Import, null, NLConstants.EntryStatus.AmendmentRequestSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.AmendmentRequestReceivedByCustoms, null);

		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.AdvanceDeclarationSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.AdvanceDeclarationReceived, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.InformationSentToCustoms, null, NLConstants.Status.Received, NLConstants.EntryStatus.InformationReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.SupplementSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.SupplementReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.InvalidationRequestSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.InvalidationRequestReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.AmendmentRequestSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.AmendmentRequestReceivedByCustoms, null);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, NLConstants.EntryStatus.ExitInformationDetailsSent, null, NLConstants.Status.Received, NLConstants.EntryStatus.ExitInformationDetailsReceived, null);

		AssertProcessMessage(JobMessageTypeList.Codes.Export, NLConstants.StatusNew.SentToCustoms, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes.CRE, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._515);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, null, CustomsEntryPhaseStatusList.Codes._513, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._513);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, null, CustomsEntryPhaseStatusList.Codes._514, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._514);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, null, null, CustomsEntryPhaseStatusList.Codes._583, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._515);
		AssertProcessMessage(JobMessageTypeList.Codes.Export, NLConstants.StatusNew.SentToCustoms, null, CustomsEntryPhaseStatusList.Codes.SUP, NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._515);
	}

	void AssertProcessMessage(string declarationType, string status, string entryStatus, string phaseStatus, string expectedStatus, string expectedEntryStatus, string expectedPhaseStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = declarationType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = entryStatus;
		entryHeader.CH_BGMReference = "TestReferenceABC";

		if (!status.IsEmpty())
		{
			entryHeader.CH_Status = status;
		}

		if (!phaseStatus.IsEmpty())
		{
			entryHeader.CH_PhaseStatus = phaseStatus;
		}

		var testMessage = CreateNewTestMessage();
		testMessage.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;
		testMessage.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(testMessage);
		CombineAssertions($"{declarationType} - {status} - {entryStatus} - {phaseStatus}:", () =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, testMessage.EM_Status);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);
			AssertEquals("EntryHeader - Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("EntryHeader - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			if (!(phaseStatus.IsEmpty() && expectedPhaseStatus.IsEmpty()))
			{
				AssertEquals("EntryHeader - Phase Status", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
			}
		});
	}
}

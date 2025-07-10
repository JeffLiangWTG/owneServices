using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE456MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC456A;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><H1>Error Information</H1><BR/><table style='margin-left: 10pt'><tr><td><b>MRN:</b></td><td><i>22NL13215444</i></td></tr><tr><td><b>Functional Reference ID:</b></td><td><i>TestReference456</i></td></tr><tr><td><b>Statement Type:</b></td><td><i>Status details</i></td></tr><tr><td><b>Statement Description:</b></td><td></td></tr></table><BR><H1>Errors reported by customs</H1><BR/><table style='margin-left: 10pt'><tr><td><b>Error Validation:</b></td><td></td></tr><tr><td><b>Error Description:</b></td><td></td></tr><tr><td><b>Value that is rejected:</b></td><td><i>AttValue</i></td></tr></table>";

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC456A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference456</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><StatementTypeCode>AHN</StatementTypeCode></AdditionalInformation><BusinessRejectionTypeCode>413</BusinessRejectionTypeCode><Error><Sequence>1</Sequence><OriginalAttributeValue>AttValue</OriginalAttributeValue></Error><Declaration><RejectionDateTime formatCode=\"304\">20220112143432Z</RejectionDateTime><ID>22NL13215444</ID></Declaration></Response></MetaData>";

	protected override string BGMReference => "TestReference456";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE456And556MessageProcessor(logger);

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
			AssertEquals("Entry Header - Message Status", NLConstants.Status.Rejection, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", NLConstants.EntryStatus.Rejection_413, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Issue date", new ZDateTime(new DateTime(2022, 01, 12, 14, 34, 32, DateTimeKind.Utc).ToLocalTime().Date), mrnEntryNumber.CE_IssueDate);
			AssertEquals("Entry Header - Mrn", "22NL13215444", mrnEntryNumber.CE_EntryNum);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}
}

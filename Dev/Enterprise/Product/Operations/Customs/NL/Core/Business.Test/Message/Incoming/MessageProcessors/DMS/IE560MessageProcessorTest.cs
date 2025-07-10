using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE560MessageProcessorTest : IE460MessageProcessorTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC560C;

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC560C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><StatementTypeCode>BLF</StatementTypeCode></AdditionalInformation><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC</StatementDescription></AdditionalInformation></Control><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC2</StatementDescription></AdditionalInformation></Control><Status><EffectiveDateTime formatCode=\"304\">20220216172832Z</EffectiveDateTime></Status><RequestedDocument><Description>DOCDESC1</Description><TypeCode>TRA</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC2</Description><TypeCode>REF</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC3</Description><TypeCode>XXX</TypeCode></RequestedDocument><Declaration><FunctionalReferenceID>TestReference460</FunctionalReferenceID><ID>MRN460</ID></Declaration></Response></MetaData>";

	protected override string ExpectedEntryHeaderStatus => StatusNew.Accepted;

	protected override string ExpectedEntryHeaderEntryStatus => EntryStatusNew.PhysicalInspection;
}

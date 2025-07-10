using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CancelDeclarationEDIMessage))]
sealed class CancelDeclarationEDIMessageTest : EnterpriseBusinessObjectTestCase
{
	public void TestSetDefaultValues()
	{
		var message = Factory.New<CancelDeclarationEDIMessage>();
		AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
	}

	public void TestEM_MessageInterpretation()
	{
		var message = Factory.New<CancelDeclarationEDIMessage>();
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = ExportSendMessageTypes.Codes.CAN;
		message.EM_MessageText = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.AdditionalMessage:1 DMS.AdditionalMessage_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.AdditionalMessage:1'><WCOTypeCode>CC414A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Declaration><FunctionalReferenceID>1-B000001</FunctionalReferenceID><ID>22NL123456MRN</ID><IssueDateTime>20220120145234Z</IssueDateTime><DeclarationOffice><ID>NL01234</ID></DeclarationOffice><AdditionalInformation><SequenceNumeric>1</SequenceNumeric><StatementDescription>Reason for invalidation</StatementDescription><StatementTypeCode>CUS</StatementTypeCode></AdditionalInformation><Agent><ID>NL123456789B01</ID><FunctionCode>1</FunctionCode><Contact><Name>Frans Klaasen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+3168523697</ID><TypeCode>2</TypeCode></Communication></Contact></Agent><Declarant><Name>Declarant name</Name><ID>NL987654321B02</ID><Address><CityName>Enschede</CityName><CountryCode>NL</CountryCode><Line>Declaratiestraat 5</Line><PostcodeID>1234AB</PostcodeID></Address><Contact><Name>Jef Franssen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+316987456321</ID><TypeCode>2</TypeCode></Communication></Contact></Declarant></Declaration></MetaData>";

		AssertEquals("<font size='2' face='Courier New'><table style='margin-left:10pt'>" +
			"<tr><td><b>Invalidation reason:</b></td><td><i>Reason for invalidation</i></td></tr>" +
			"</table></font>", message.GetOutgoingMessageInterpretation());
	}

	public void TestInvalidationReason()
	{
		var message = Factory.New<CancelDeclarationEDIMessage>();
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = ExportSendMessageTypes.Codes.CAN;
		message.EM_MessageText = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.AdditionalMessage:1 DMS.AdditionalMessage_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.AdditionalMessage:1'><WCOTypeCode>CC414A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Declaration><FunctionalReferenceID>1-B000001</FunctionalReferenceID><ID>22NL123456MRN</ID><IssueDateTime>20220120145234Z</IssueDateTime><DeclarationOffice><ID>NL01234</ID></DeclarationOffice><AdditionalInformation><SequenceNumeric>1</SequenceNumeric><StatementDescription>Reason for invalidation</StatementDescription><StatementTypeCode>CUS</StatementTypeCode></AdditionalInformation><Agent><ID>NL123456789B01</ID><FunctionCode>1</FunctionCode><Contact><Name>Frans Klaasen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+3168523697</ID><TypeCode>2</TypeCode></Communication></Contact></Agent><Declarant><Name>Declarant name</Name><ID>NL987654321B02</ID><Address><CityName>Enschede</CityName><CountryCode>NL</CountryCode><Line>Declaratiestraat 5</Line><PostcodeID>1234AB</PostcodeID></Address><Contact><Name>Jef Franssen</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+316987456321</ID><TypeCode>2</TypeCode></Communication></Contact></Declarant></Declaration></MetaData>";

		AssertEquals("Invalidation reason", "Reason for invalidation", message.InvalidationReason);
	}
}

using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class RFIMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCRFIA;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'><tr><td><b>MRN:</b></td><td><i>MRN123</i></td></tr><tr><td><b>Functional Reference ID:</b></td><td><i>TestReferenceABC</i></td></tr><tr><td style='padding-left:1em;'><b>Control Date:</b></td><td><i>20220207</i></td></tr><tr><td style='padding-left:2em;'><b>Control Type:</b></td><td><i>10: Documents Control</i></td></tr><tr><td style='padding-left:2em;'><b>Applies to Entry Line:</b></td><td><i>7</i></td></tr><tr><td style='padding-left:2em;'><b>Control Statement:</b></td><td><i>Documents not complete</i></td></tr><tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td></td></tr><tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[7]/PreviousDocument</i></td></tr><tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide all available documents</i></td></tr><tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td><i>TEST</i></td></tr><tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/Consignment/Consignee</i></td></tr><tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide phone number and e-mail address(es) of Consignee</i></td></tr><tr><td><b></b></td><td><i>&nbsp;</i></td></tr><tr><td style='padding-left:1em;'><b>Control Date:</b></td><td><i>20220208</i></td></tr><tr><td style='padding-left:2em;'><b>Control Type:</b></td><td><i>50: Other</i></td></tr><tr><td style='padding-left:2em;'><b>Applies to Entry Line:</b></td><td><i>6</i></td></tr><tr><td style='padding-left:2em;'><b>Control Statement:</b></td><td><i>Statement description</i></td></tr><tr><td style='padding-left:2em;'><b>Corrected Value:</b></td><td></td></tr><tr><td style='padding-left:3em;'><b>Name Path:</b></td><td><i>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[6]/TransportContractDocument</i></td></tr><tr><td style='padding-left:3em;'><b>Control Statement:</b></td><td><i>Provide all available documents</i></td></tr><tr><td><b></b></td><td><i>&nbsp;</i></td></tr></table></font>";

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CCRFIA</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><ControlResult><GoodsItemNumeric>7</GoodsItemNumeric><Control><InspectionStartDateTime>20220207</InspectionStartDateTime><TypeCode>10</TypeCode><AdditionalInformation><StatementDescription>Documents not complete</StatementDescription></AdditionalInformation><ControlDetails><CorrectedAttributeValue/><AdditionalInformation><StatementType>D10</StatementType><StatementDescription>Provide all available documents</StatementDescription></AdditionalInformation><Pointer><Location>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[7]/PreviousDocument</Location></Pointer></ControlDetails><ControlDetails><CorrectedAttributeValue>TEST</CorrectedAttributeValue><AdditionalInformation><StatementType>B20</StatementType><StatementDescription>Provide phone number and e-mail address(es) of Consignee</StatementDescription></AdditionalInformation><Pointer><Location>Declaration/GoodsShipment/Consignment/Consignee</Location></Pointer></ControlDetails></Control></ControlResult><ControlResult><GoodsItemNumeric>6</GoodsItemNumeric><Control><InspectionStartDateTime>20220208</InspectionStartDateTime><TypeCode>50</TypeCode><AdditionalInformation><StatementDescription>Statement description</StatementDescription></AdditionalInformation><ControlDetails><CorrectedAttributeValue/><AdditionalInformation><StatementType>D10</StatementType><StatementDescription>Provide all available documents</StatementDescription></AdditionalInformation><Pointer><Location>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[6]/TransportContractDocument</Location></Pointer></ControlDetails></Control></ControlResult><Declaration><FunctionalReferenceID>TestReferenceABC</FunctionalReferenceID><ID>MRN123</ID><VersionID>2</VersionID><DeclarationOffice><ID>NLRTM0001</ID></DeclarationOffice><Agent><ID>AGENT-ID</ID><FunctionCode>AGENT-FUNCTION</FunctionCode><Contact><Name>AGENT CONTACT</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+31652369874</ID><TypeCode>2</TypeCode></Communication><Communication><SequenceNumeric>2</SequenceNumeric><ID>mail@agent.nl</ID><TypeCode>1</TypeCode></Communication></Contact></Agent><Declarant><Name>DECLARANT NAME</Name><ID>DECLARANT-ID</ID><Address><CityName>DECLARANT CITY</CityName><CountryCode>DECLARANT COUNTRY</CountryCode><Line>DECLARANT LINE</Line><PostcodeID>DECLARANT POSTCODE</PostcodeID></Address><Contact><Name>DECLARANT CONTACT</Name><Communication><SequenceNumeric>1</SequenceNumeric><ID>+31641287963</ID><TypeCode>2</TypeCode></Communication><Communication><SequenceNumeric>2</SequenceNumeric><ID>mail@declarant.nl</ID><TypeCode>1</TypeCode></Communication></Contact></Declarant></Declaration></Response></MetaData>";

	protected override DMSResponseMessageProcessor MessageProcessor => new RFIMessageProcessor(logger);

	public new void TestProcessMessage()
	{
		var testMessage = MessageTestHelper.SetupRFIMessage(Factory, BGMReference, TestMessageText);
		MessageProcessor.ProcessMessage(testMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, testMessage.EM_Status);
			AssertXMLEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);
			AssertEquals("Entry Header - Entry Status", NLConstants.EntryStatusNew.RequestForInformation, ((CusEntryHeader)testMessage.EM_LinkedObject).CH_EntryStatus);
			AssertEquals("Entry Header - Status", NLConstants.StatusNew.Accepted, ((CusEntryHeader)testMessage.EM_LinkedObject).CH_Status);
		});
	}

	public void TestProcessMessage_PhaseStatus513()
	{
		var testMessage = MessageTestHelper.SetupRFIMessage(Factory, BGMReference, TestMessageText, CustomsEntryPhaseStatusList.Codes._513);
		MessageProcessor.ProcessMessage(testMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, testMessage.EM_Status);
			AssertXMLEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);
			AssertEquals("Entry Header - Entry Status", ZString.Empty, ((CusEntryHeader)testMessage.EM_LinkedObject).CH_EntryStatus);
			AssertEquals("Entry Header - Status", ZString.Empty, ((CusEntryHeader)testMessage.EM_LinkedObject).CH_Status);
		});
	}
}

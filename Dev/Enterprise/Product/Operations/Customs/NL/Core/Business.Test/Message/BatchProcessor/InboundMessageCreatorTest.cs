using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class InboundMessageCreatorTest : TestCaseWithFactory
{
	public void TestCreateMessagesForInterchange()
	{
		var expectedXMl = @"<MetaData xsi:schemaLocation=""urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DMS.Response:1""><WCOTypeCode>CC428A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference428</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Declaration><AcceptanceDateTime formatCode=""102"">20220112</AcceptanceDateTime><FunctionalReferenceID>TestReference428</FunctionalReferenceID><ID>22NL13215444</ID></Declaration></Response></MetaData>";
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange.EI_BodyText = expectedXMl;
		ProcessInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
			AssertEquals("EM_MessageText", expectedXMl, createdMessage.EM_MessageText);
			AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.DMS, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "428", createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum was not set", ZString.Empty, createdMessage.EM_MessageNum);
		});
	}

	public void TestCreateNctsMessagesForInterchange()
	{
		var expectedXMl = @"<?xml version=""1.0"" encoding=""UTF-8""?>
			<CC917C xmlns=""http://ncts.dgtaxud.ec"" xmlns:vc=""http://www.w3.org/2007/XMLSchema-versioning"" PhaseID=""NCTS5.1"">
			<messageSender>DVA.NL</messageSender> 
			<messageRecipient>NL100005354.00</messageRecipient> 
			<preparationDateAndTime>2024-11-29T13:08:40</preparationDateAndTime> 
			<messageIdentification>NL202411291308400862882913</messageIdentification> 
			<messageType>CC917C</messageType> 
			<correlationIdentifier>00000000000132</correlationIdentifier> 
			<XMLError> 
			<errorLineNumber>117</errorLineNumber> 
			<errorColumnNumber>14</errorColumnNumber> 
			<errorCode>52</errorCode> 
			<errorText>Element &apos;Consignment&apos; is not valid for content model: &apos;(messageSender,messageRecipient,preparationDateAndTime,messageIdentification,messageType,correlationIdentifier?,TransitOperation,Authorisation*,CustomsOfficeOfDeparture,CustomsOfficeOfDestinationDeclared,CustomsOfficeOfTransitDeclared*,CustomsOfficeOfExitForTransitDeclared*,HolderOfTheTransitPr</errorText> 
			</XMLError> 
			</CC917C>";
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange.EI_InterchangeType = NLEDIMessageTypes.Codes.NCT;
		interchange.EI_BodyText = expectedXMl;
		ProcessInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
			AssertEquals("EM_MessageText", expectedXMl, createdMessage.EM_MessageText);
			AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.NCT, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "917", createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum was not set", ZString.Empty, createdMessage.EM_MessageNum);
		});
	}

	public void TestCreateMessagesForInterchange_ExitControl()
	{
		var expectedXMl =
			"<MD_01:MetaData xmlns:MD_01=\"SW:MD:01\"><MD_01:CommunicationMetaData><MD_01:ApplicationReferenceID>Int1</MD_01:ApplicationReferenceID><MD_01:PreparationDateTime><ds:Date_304 formatCode=\"304\" xmlns:ds=\"SW:DS:01\">20200824143432Z</ds:Date_304></MD_01:PreparationDateTime><MD_01:Recipient><MD_01:ID>NL100004064</MD_01:ID></MD_01:Recipient><MD_01:Sender><MD_01:ID>ECS.NL</MD_01:ID></MD_01:Sender></MD_01:CommunicationMetaData><MD_01:MetaData><MD_01:AgencyAssignedCustomizationCode>RES01M</MD_01:AgencyAssignedCustomizationCode><MD_01:CommunicationMetaData><MD_01:ApplicationReferenceID>Msg1</MD_01:ApplicationReferenceID></MD_01:CommunicationMetaData></MD_01:MetaData><MD_01:MetaData><MD_01:AgencyAssignedCustomizationCode>PDM01M</MD_01:AgencyAssignedCustomizationCode><MD_01:CommunicationMetaData><MD_01:ApplicationReferenceID>Msg2</MD_01:ApplicationReferenceID></MD_01:CommunicationMetaData></MD_01:MetaData></MD_01:MetaData><RES_01:Response xmlns:RES_01=\"SW2B:RES:01\"><RES_01:FunctionalReferenceID>NLCUSPDM00000000001</RES_01:FunctionalReferenceID><RES_01:IssueDateTime><ds:DateTimeString formatCode=\"304\" xmlns:ds=\"SW:DS:01\">20200824143432Z</ds:DateTimeString></RES_01:IssueDateTime><RES_01:TypeCode>RES</RES_01:TypeCode><RES_01:Declaration><RES_01:FunctionalReferenceID>FunctionalReferenceID NOD</RES_01:FunctionalReferenceID></RES_01:Declaration></RES_01:Response><PDM_01:Response xmlns:PDM_01=\"SW2B:PDM:01\"><PDM_01:Function>55</PDM_01:Function><PDM_01:TypeCode>PDM</PDM_01:TypeCode><PDM_01:Declaration><PDM_01:BorderTransportMeans><PDM_01:StayID>Unique StayID</PDM_01:StayID><PDM_01:Itinerary><PDM_01:SequenceNumeric>1</PDM_01:SequenceNumeric><PDM_01:DepartureDateTime><ds:Date_304 formatCode=\"304\" xmlns:ds=\"SW:DS:01\">20200901160000Z</ds:Date_304></PDM_01:DepartureDateTime></PDM_01:Itinerary><PDM_01:Itinerary><PDM_01:SequenceNumeric>2</PDM_01:SequenceNumeric><PDM_01:DepartureDateTime><ds:Date_304 formatCode=\"304\" xmlns:ds=\"SW:DS:01\">20200901180000Z</ds:Date_304></PDM_01:DepartureDateTime></PDM_01:Itinerary></PDM_01:BorderTransportMeans></PDM_01:Declaration></PDM_01:Response>";
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange.EI_InterchangeType = NLEDIMessageTypes.Codes.EXT;
		interchange.EI_BodyText = "<SW2B xmlns=\"urn:wco:datamodel:SW:SW2B:01\" xmlns:MD_01=\"SW:MD:01\" xmlns:ACT_01=\"SW2B:ACT:01\" xmlns:AIN_01=\"SW2B:AIN:01\" xmlns:CNE_01=\"SW2B:CNE:01\" xmlns:CNI_01=\"SW2B:CNI:01\" xmlns:DEC_01=\"SW2B:DEC:01\" xmlns:DRN_01=\"SW2B:DRN:01\" xmlns:ESD_01=\"SW2B:ESD:01\" xmlns:NCB_01=\"SW2B:NCB:01\" xmlns:NTS_01=\"SW2B:NTS:01\" xmlns:PDM_01=\"SW2B:PDM:01\" xmlns:RCP_01=\"SW2B:RCP:01\" xmlns:RES_01=\"SW2B:RES:01\" xmlns:RNE_01=\"SW2B:RNE:01\" xmlns:RNI_01=\"SW2B:RNI:01\" xmlns:SDW_01=\"SW2B:SDW:01\" xmlns:ds=\"SW:DS:01\">"
								 + expectedXMl + "</SW2B>";
		ProcessInterchange(interchange);

		AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);

		var createdMessage = interchange.ContainedMessages[0];
		AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
		AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
		AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
		AssertEquals("EM_MessageText", expectedXMl.Replace("\r\n", "").Replace("\t", ""), createdMessage.EM_MessageText);
		AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.EXT, createdMessage.EM_MessageType);
		AssertEquals("EM_MessageSubType", "PDM", createdMessage.EM_MessageSubType);
		AssertEquals("EM_MessageNum was not set", ZString.Empty, createdMessage.EM_MessageNum);
	}

	public void TestCreateMessageForInterchange_Control()
	{
		var expectedXMl = @"<?xml version=""1.0"" encoding=""UTF-8""?><XML_Control xmlns=""urn:wco:datamodel:XML_Control:1"" xmlns:clm63055=""urn:un:unece:uncefact:codelist:standard:UNECE:AgencyIdentificationCode:D12B"" xmlns:ds=""XML_Control:DS:1""><DocumentMetaData><WCODataModelVersionCode>3.50</WCODataModelVersionCode><ResponsibleCountryCode>NL</ResponsibleCountryCode><ResponsibleAgencyName>DOUANE</ResponsibleAgencyName><AgencyAssignedCustomizationVersionCode>1.1</AgencyAssignedCustomizationVersionCode><CommunicationMetaData><ApplicationReferenceID>DMS</ApplicationReferenceID><PreparationDateTime formatCode=""304"">20250317140426Z</PreparationDateTime></CommunicationMetaData></DocumentMetaData><Response><FunctionalReferenceID>25913546827B0100000003</FunctionalReferenceID><Error><Description>Error message: Element 'CommunicationsAgreementID' is not valid for content model: '(ApplicationReferenceID,CommunicationsAgreementID?,PreparationDateTime,Recipient,Sender)' </Description><Pointer><Location>Line-number: 13  ### Column-number: 27</Location></Pointer></Error><Error><Description>Error message: Datatype error: Type:InvalidDatatypeValueException, Message:Value '1118 AB 1' does not match regular expression facet '[A-Z]{2}' .""</Description><Pointer><Location>Line-number: 56 ### Column-number: 49</Location></Pointer></Error></Response></XML_Control>";
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange.EI_BodyText = expectedXMl;
		ProcessInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("EM_EI", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
			AssertEquals("EM_MessageText", expectedXMl, createdMessage.EM_MessageText);
			AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.DMS, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "CTL", createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum was not set", ZString.Empty, createdMessage.EM_MessageNum);
		});
	}

	public void TestCreateMessagesForInterchange_InvalidXMLFormat()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = "InvalidXML";
		interchange.EI_InterchangeNum = "0001";
		ProcessInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals("ContainedMessages.Count", 0, interchange.ContainedMessages.Count);
			AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Error, interchange.EI_Status);
			AssertNotNull("Invalid", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, "The Message XML is not a valid XML, the Message creation failed"));
			AssertNotNull("No message", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, "No message has been created for interchange 0001"));
		});
	}

	public void TestCreateMessagesForInterchange_NoMessageSubTypeFound()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeNum = "0001";
		interchange.EI_BodyText = @"<s0:NLCustomsBusinessResponse xmlns:s0='http://cargowise.com/ehub/products/NLCustoms'><ResponseHeader><ConversationID>f34f537f-9311-4859-893c-6a783f6917a2</ConversationID></ResponseHeader><s0:ResponseBody>
									<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>XXXXXX</WCOTypeCode></MetaData>
									</s0:ResponseBody></s0:NLCustomsBusinessResponse>";
		ProcessInterchange(interchange);

		AssertNotNull(interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport, "Interchange 0001: Can not determine Message Sub Type for the Received Interchange"));
	}

	public void TestCreateMessagesForInterchange_ThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => ProcessInterchange(null));
	}

	public void TestInterchangesProcessedToCorrectBranches()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "ZC1";
		company.GC_Name = "TEST COMP";
		company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		Factory.Save();

		var messageText = "<s0:NLCustomsBusinessResponse xmlns:s0='http://cargowise.com/ehub/products/NLCustoms'><s0:ResponseHeader><s0:ConversationID>f34f537f-9311-4859-893c-6a783f6917a2</s0:ConversationID></s0:ResponseHeader>"
						+ "<s0:ResponseBody><MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC428A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference428</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Declaration><AcceptanceDateTime formatCode =\"102\">20220112</AcceptanceDateTime><FunctionalReferenceID>TestReference428</FunctionalReferenceID><ID>22NL13215444</ID></Declaration></Response></MetaData>"
						+ "</s0:ResponseBody></s0:NLCustomsBusinessResponse>";
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_From = "NLCustoms";
		interchange1.EI_To = "TEST";
		interchange1.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange1.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange1.EI_InterchangeNum = "00000000000000000031";
		interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange1.EI_Status = EDIInterchange.Status.Queued;
		interchange1.EI_IsActive = true;
		interchange1.EI_BodyText = messageText;
		interchange1.EI_GB = GlbBranch.CurrentBranch.PK;
		Factory.Save();

		var interchangeProc = new NLInboundInterchangeProcessor(GetNewLoggerForTesting());
		interchangeProc.ExecuteBatch();

		var newBranch2 = company.Branches.AddNew();
		newBranch2.GB_Code = "BZ2";
		newBranch2.GB_BranchName = "B2 NAME";
		newBranch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
		Factory.Save();

		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_From = "NLCustoms";
		interchange2.EI_To = "TEST";
		interchange2.EI_ApplicationCode = ApplicationCodes.NLCustoms;
		interchange2.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange2.EI_InterchangeNum = "00000000000000000032";
		interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange2.EI_Status = EDIInterchange.Status.Queued;
		interchange2.EI_IsActive = true;
		interchange2.EI_BodyText = messageText;
		interchange2.EI_GB = newBranch2.PK;
		Factory.Save();

		interchangeProc.ExecuteBatch();

		interchange1.Reload();
		interchange2.Reload();

		CombineAssertions("Interchange1 Pre-reqs", () =>
		{
			AssertEquals("Interchange1 Received", EDIInterchange.Status.Received, interchange1.EI_Status);
			AssertEquals("Interchange1 has 1 message", 1, interchange1.ContainedMessages.Count);
		});

		CombineAssertions("Message1 details", () =>
		{
			var msg1 = interchange1.ContainedMessages[0];
			AssertEquals("Msg1", GlbBranch.CurrentBranch.PK, msg1.EM_GB);
		});

		CombineAssertions("Interchange2 Pre-reqs", () =>
		{
			AssertEquals("Interchange2 Received", EDIInterchange.Status.Received, interchange2.EI_Status);
			AssertEquals("Interchange2 has 1 message", 1, interchange2.ContainedMessages.Count);
		});

		CombineAssertions("Message2 details", () =>
		{
			var msg2 = interchange2.ContainedMessages[0];
			AssertEquals("Msg2", newBranch2.PK, msg2.EM_GB);
		});
	}

	public void TestRunTaskWithSomeMatchingInterchanges()
	{
		var expectedxml1 = @"<MetaData xsi:schemaLocation=""urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DMS.Response:1""><WCOTypeCode>CC428A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReference428</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Declaration><AcceptanceDateTime formatCode=""102"">20220112</AcceptanceDateTime><FunctionalReferenceID>TestReference428</FunctionalReferenceID><ID>22NL13215444</ID></Declaration></Response></MetaData>";
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_BodyText = expectedxml1;
		interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange1.EI_Status = EDIInterchange.Status.Queued;
		interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
		interchange1.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange1.EI_InterchangeNum = "0001";
		interchange1.EI_From = "TEST";
		interchange1.EI_To = "TEST";

		var expectedxml2 = @"<MetaData xsi:schemaLocation=""urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DMS.Response:1""><WCOTypeCode>CC429A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>7243213</CommunicationsAgreementID><PreparationDateTime>20220214151920Z</PreparationDateTime><Recipient><ID>NL123456789</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Control><SequenceNumeric>1</SequenceNumeric><ControlResult><ID>A2</ID><Description>Geacht conform te zijn</Description></ControlResult></Control><Status><EffectiveDateTime>20220214151920Z</EffectiveDateTime><NameCode>4</NameCode><ReleaseDateTime>20220214</ReleaseDateTime></Status></Response></MetaData>";
		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_BodyText = expectedxml2;
		interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange2.EI_Status = EDIInterchange.Status.Queued;
		interchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;
		interchange2.EI_InterchangeType = NLEDIMessageTypes.Codes.DMS;
		interchange2.EI_InterchangeNum = "0002";
		interchange2.EI_From = "TEST";
		interchange2.EI_To = "TEST";

		var unmatchingInterchange = Factory.New<EDIInterchange>();
		unmatchingInterchange.EI_Status = EDIInterchange.Status.Cancelled;
		unmatchingInterchange.EI_From = "TEST";
		unmatchingInterchange.EI_To = "TEST";

		Factory.Save();

		// Run service task
		var log = GetNewLoggerForTesting();
		var interchangeProc = new NLInboundInterchangeProcessor(log);
		interchangeProc.ExecuteBatch();
		List<ISimpleLog> logs = (List<ISimpleLog>)log.Logs;

		interchange1.Reload();
		interchange2.Reload();
		unmatchingInterchange.Reload();

		CombineAssertions("Status and Message/Log Count", () =>
		{
			AssertEquals("Interchange1 Status", EDIInterchange.Status.Received, interchange1.EI_Status);
			AssertEquals("Interchange2 Status", EDIInterchange.Status.Received, interchange2.EI_Status);
			AssertEquals("Unmatched Interchange Status", EDIInterchange.Status.Cancelled, unmatchingInterchange.EI_Status);

			AssertEquals("Interchange1 Message Count", 1, interchange1.ContainedMessages.Count);
			AssertEquals("Interchange2 Message Count", 1, interchange2.ContainedMessages.Count);
			AssertEquals("Unmatching Interchange Message Count", 0, unmatchingInterchange.ContainedMessages.Count);

			AssertEquals("Log Count", 2, logs.Count);
		});

		CombineAssertions("Logs", () =>
		{
			AssertEquals($"1st Log [{logs[0]}] contains expected message?", true, logs[0].Message.EndsWith("Interchange '0001' has been processed successfully."));
			AssertEquals($"2nd Log [{logs[1]}] contains expected message?", true, logs[1].Message.EndsWith("Interchange '0002' has been processed successfully."));
		});

		CombineAssertions("Interchange1 Message", () =>
		{
			var createdMessage11 = interchange1.ContainedMessages[0];
			AssertEquals("EM_EI", interchange1.PK, createdMessage11.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.NLCustoms, createdMessage11.EM_ApplicationCode);
			AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.DMS, createdMessage11.EM_MessageType);
			AssertEquals("EM_MessageSubType", "428", createdMessage11.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "", createdMessage11.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", false, createdMessage11.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedxml1, createdMessage11.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage11.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage11.EM_Status);
		});

		CombineAssertions("Interchange2 Message", () =>
		{
			var createdMessage21 = interchange2.ContainedMessages[0];
			AssertEquals("EM_EI", interchange2.PK, createdMessage21.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.NLCustoms, createdMessage21.EM_ApplicationCode);
			AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.DMS, createdMessage21.EM_MessageType);
			AssertEquals("EM_MessageSubType", "429", createdMessage21.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "", createdMessage21.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", false, createdMessage21.EM_IsTestMessage);
			AssertEquals("EM_MessageText", expectedxml2, createdMessage21.EM_MessageText);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, createdMessage21.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage21.EM_Status);
		});
	}

	void ProcessInterchange(EDIInterchange interchange)
	{
		IInboundMessageCreator messageCreator = new InboundMessageCreator();
		messageCreator.CreateMessagesForInterchange(interchange);
	}

	static LoggingInformation GetNewLoggerForTesting() => new LoggingInformationForTesting();
}

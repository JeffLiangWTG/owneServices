using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CDSResponse.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.CDS.ServiceTasks.Testing
{
	[TestedType(typeof(CDSMessageRetrieverServiceTask))]
	class CDSMessageRetrieverServiceTaskTests : ServiceTaskTestCase<CDSMessageRetrieverServiceTask>
	{
		public void TestParseOneRealExample()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "B124";
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			entry2.LRN = "8GB123456789001-S0001000";

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_DeclarationReference = "B125";
			var entry3 = dec3.CustomsEntryHeaders.AddNew();
			entry3.LRN = "8GB123456789002-S0001000";
			Factory.Save();

			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = @"<GBCustomsBusinessResponse>
    <ResponseHeader>
        <ConversationID>5a013903-c8c6-404f-8683-d8e84d06f134</ConversationID>
    </ResponseHeader>
	<ResponseBody>
    <MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
        <Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
            <FunctionCode>01</FunctionCode>
            <FunctionalReferenceID>d5710483848740849ce7415470c2886a</FunctionalReferenceID>
            <IssueDateTime>
                <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
            </IssueDateTime>
            <Declaration>
                <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
                <ID>15GB000060100C85A1</ID>
            </Declaration>
        </Response>
        <Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
            <FunctionCode>02</FunctionCode>
            <FunctionalReferenceID>d5710483848740849ce8415470c2886b</FunctionalReferenceID>
            <IssueDateTime>
                <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
            </IssueDateTime>
            <Declaration>
                <FunctionalReferenceID>8GB123456789001-S0001000</FunctionalReferenceID>
                <ID>15GB000060100C85A2</ID>
            </Declaration>
        </Response>
        <Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
            <FunctionCode>03</FunctionCode>
            <FunctionalReferenceID>d5710483848740849ce9415470c2886c</FunctionalReferenceID>
            <IssueDateTime>
                <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
            </IssueDateTime>
            <Declaration>
                <FunctionalReferenceID>8GB123456789002-S0001000</FunctionalReferenceID>
                <ID>15GB000060100C85A4</ID>
            </Declaration>
        </Response>
    </MetaData>
	</ResponseBody>
</GBCustomsBusinessResponse>";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.Response);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			CombineAssertions(() =>
			{
				AssertEquals(3, messages.Length);

				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, "d5710483848740849ce7", entry.PK, "5a013903c8c6404f8683d8e84d06f134");
				AssertMessage(messages[1], EDIMessageStatusList.Codes.ProcessedOK, "d5710483848740849ce8", entry2.PK, "5a013903c8c6404f8683d8e84d06f134");
				AssertMessage(messages[2], EDIMessage.Status.ProcessedOK, "d5710483848740849ce9", entry3.PK, "5a013903c8c6404f8683d8e84d06f134");
			});
		}

		public void TestCspInventoryPreCheckDmsLikeMessageWithoutConversationId()
		{
			var inventoryPreCheck = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:eHubTrackingId>7900c66b-dc82-4e19-af48-9042bdad3393</s0:eHubTrackingId>
    <s0:CSPEntryTrackingID>Anything</s0:CSPEntryTrackingID>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <MetaData xmlns:ns2=""urn:wco:datamodel:WCO:RES-DMS:2"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"" xmlns:ns4=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:ns3=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:ns5=""urn:wco:datamodel:WCO:DEC-DMS:2"">
      <WCODataModelVersionCode>3.6</WCODataModelVersionCode>
      <WCOTypeName>RES</WCOTypeName>
      <ResponsibleAgencyName>CSP</ResponsibleAgencyName>
      <ns2:Response>
        <ns2:FunctionCode>03</ns2:FunctionCode>
        <ns2:FunctionalReferenceID>JNE-1607604176788</ns2:FunctionalReferenceID>
        <ns2:IssueDateTime>
          <ns3:DateTimeString formatCode=""304"">20200519165557Z</ns3:DateTimeString>
        </ns2:IssueDateTime>
        <ns2:AdditionalInformation>
          <ns2:StatementCode>MCP60288</ns2:StatementCode>
          <ns2:StatementDescription>60288 - Vessel has not yet arrived</ns2:StatementDescription>
          <ns2:Pointer>
            <ns2:SequenceNumeric>1</ns2:SequenceNumeric>
            <ns2:DocumentSectionCode>07B</ns2:DocumentSectionCode>
          </ns2:Pointer>
          <ns2:Pointer>
            <ns2:SequenceNumeric>1</ns2:SequenceNumeric>
            <ns2:DocumentSectionCode>53A</ns2:DocumentSectionCode>
          </ns2:Pointer>
        </ns2:AdditionalInformation>
        <ns2:Error>
          <ns2:ValidationCode>CDS20001</ns2:ValidationCode>
          <ns2:Pointer>
            <ns2:DocumentSectionCode>42A</ns2:DocumentSectionCode>
            <ns2:TagID>D026</ns2:TagID>
          </ns2:Pointer>
        </ns2:Error>
        <ns2:Declaration>
          <ns2:FunctionalReferenceID>HYEDUKMIK0000000002435</ns2:FunctionalReferenceID>
          <ns2:RejectionDateTime>
            <ns3:DateTimeString formatCode=""304"">20200519165557Z</ns3:DateTimeString>
          </ns2:RejectionDateTime>
          <ns2:VersionID>1</ns2:VersionID>
        </ns2:Declaration>
      </ns2:Response>
    </MetaData>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";

			var lrnFromAboveMessage = "HYEDUKMIK0000000002435";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = lrnFromAboveMessage;

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_From = "Any";
			inboundInterchange.EI_To = "AnyOther";
			inboundInterchange.EI_BodyText = inventoryPreCheck;
			inboundInterchange.EI_ReceiveTransmit = "RCV";
			inboundInterchange.EI_Status = "QUE";

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var entryReloaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			var interchangeReloaded = new BusinessObjectFactory().Load<EDIInterchange>(inboundInterchange.PK);
			AssertEquals(1, entry.Messages.Count);
			var message = entry.Messages[0];
			AssertEquals("JNE-1607604176788", message.EM_MessageNum);
			AssertEquals("PRS", message.EM_Status);
			AssertEquals("RCV", interchangeReloaded.EI_Status);
			AssertContains("60288 - Vessel has not yet arrived", message.EM_MessageText);
			AssertEquals(message.EM_EI, inboundInterchange.PK);
			AssertContains("Response from CSP", message.EM_MessageInterpretation);
		}

		public void TestTransportAcknowledgement()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "8GB123456789000-S0001000";
			Factory.Save();

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_From = "CCSUK";
			inboundInterchange.EI_To = "WISETECHGLOBAL";
			inboundInterchange.EI_BodyText =
@"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<SynchronousResponse>
			<status>202</status>
				<code>ACCEPTED</code>
				<ResponseHeaders>
					<x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
				</ResponseHeaders>
		</SynchronousResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";
			inboundInterchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			var inboundMessage = (CDSSynchronousResponseEDIMessage)inboundInterchange.ContainedMessages.AddNew(typeof(CDSSynchronousResponseEDIMessage));
			inboundMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			inboundMessage.EM_MessageText = $@"<SynchronousResponse>
               <status>202</status>
               <code>ACCEPTED</code>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</SynchronousResponse>
<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { inboundInterchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.SynchronousResponse);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = new BusinessObjectFactory().Load<EDIMessage>(query);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);

				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, "", entry.PK, "77685C3D14D53E25E0540003BA9676AB");
			});
		}

		public void TestInventoryResponse()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "8GB123456789000-S0001000";
			Factory.Save();

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_From = "CCSUK";
			inboundInterchange.EI_To = "WISETECHGLOBAL";
			inboundInterchange.EI_BodyText =
@"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingControlResponse>
            <messageCode>EAC</messageCode>
            <actionCode>3</actionCode>
            <ucr>
                <ucr>9GB896458895023-B00031258</ucr>
                <ucrType>D</ucrType>
            </ucr>
            <error>
                <errorCode>15</errorCode>
            </error>
        </inventoryLinkingControlResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";
			inboundInterchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			var inboundMessage = (CDSInventoryLinkingControlResponseEDIMessage)inboundInterchange.ContainedMessages.AddNew(typeof(CDSInventoryLinkingControlResponseEDIMessage));
			inboundMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			inboundMessage.EM_MessageText = $@"<inventoryLinkingControlResponse>
            <messageCode>EAC</messageCode>
            <actionCode>3</actionCode>
            <ucr>
                <ucr>9GB896458895023-B00031258</ucr>
                <ucrType>D</ucrType>
            </ucr>
            <error>
                <errorCode>15</errorCode>
            </error>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</inventoryLinkingControlResponse>
<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";

			entry.Messages.Add(inboundMessage);
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { inboundInterchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = new BusinessObjectFactory().Load<EDIMessage>(query);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);

				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, string.Format(CultureInfo.InvariantCulture, "{0}B", outgoingMessage.EM_MessageNum), entry.PK, "77685C3D14D53E25E0540003BA9676AB");
			});
		}

		public void TestParseRealExampleForInventoryLinkingControlResponseSuccess()
		{
			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Control Response from CDS:</H3><p><strong>UCR: </strong>9GB896458895023-B00031258<br><strong>Action Code: </strong>1 - Acknowledged and processed<br><strong>Error Code: </strong>15 - Declaration is cancelled/terminated</p>";
			TestParseRealExampleForInventoryLinkingControlResponse("1", EDIMessageStatusList.Codes.Acknowledged, expectedInterpretation);
		}

		public void TestParseRealExampleForInventoryLinkingControlResponseRejection()
		{
			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Inventory Linking Control Response from CDS:</H3><p><strong>UCR: </strong>9GB896458895023-B00031258<br><strong>Action Code: </strong>3 - Rejected<br><strong>Error Code: </strong>15 - Declaration is cancelled/terminated</p>";
			TestParseRealExampleForInventoryLinkingControlResponse("3", EDIMessageStatusList.Codes.Rejected, expectedInterpretation);
		}

		public void TestParseRealExampleForInventoryLinkingMovementResponse()
		{
			var message = @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingMovementResponse>
  <messageCode>EAA</messageCode>
  <crc>CRC</crc>
  <goodsArrivalDateTime>2018-08-08T08:08:08</goodsArrivalDateTime>
  <goodsLocation>LOC</goodsLocation>
  <shedOPID>SHED</shedOPID>
  <movementReference>123</movementReference>
  <submitRole>ROLE</submitRole>
  <ucrBlock>
    <ucr>UCR</ucr>
    <ucrType>D</ucrType>
  </ucrBlock>
  <goodsItem>
    <commodityCode>123</commodityCode>
    <totalPackages>1</totalPackages>
    <totalNetMass>1.1</totalNetMass>
  </goodsItem>
    <goodsItem>
    <commodityCode>456</commodityCode>
    <totalPackages>2</totalPackages>
    <totalNetMass>2.2</totalNetMass>
  </goodsItem><entryStatus>
    <ics>ICS</ics>
    <roe>6</roe>
    <soe>3</soe>
  </entryStatus>
</inventoryLinkingMovementResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";

			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>A response to a movement request message of type (EAL, EAA, EDL)</H3><p><strong>Message Code: </strong>EAA<br><strong>CRC: </strong>CRC<br><strong>Goods Arrival Date Time: </strong>2018-08-08T08:08:08<br><strong>Goods Location: </strong>LOC<br><strong>Shed Operator: </strong>SHED<br><strong>Movement Reference Number: </strong>123<br><strong>Submit Role: </strong>ROLE<br><strong>UCR: </strong>UCR<br><strong>UCR Type: </strong>D<br><strong>Entry Status ICS: </strong>ICS<br><strong>Entry Status ROE: </strong>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6<br><strong>Entry Status SOE: </strong>3 - Non-Blocking Documentary Control (A documentary check has been requested, but the goods do not need to be held.). CHIEF Equivalent: 3</p><p><strong>Goods Items</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Commodity Code</strong></td><td><strong>Total Packages</strong></td><td><strong>Total Net Mass</strong></td></tr><tr><td>123</td><td>1</td><td>1.1</td></tr><tr><td>456</td><td>2</td><td>2.2</td></tr></table></p>";
			TestRealWorldExample(message, CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse, EDIMessageStatusList.Codes.Acknowledged, expectedInterpretation);
		}

		public void TestParseRealExampleForInventoryLinkingMovementTotalsResponse()
		{
			var message = @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingMovementTotalsResponse>
  <messageCode>EMR</messageCode>
  <crc>CRC</crc>
  <goodsLocation>LOC</goodsLocation>
  <goodsArrivalDateTime>2018-08-08T08:08:08</goodsArrivalDateTime>
  <shedOPID>shed</shedOPID>
  <movementReference>123</movementReference>
</inventoryLinkingMovementTotalsResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";

			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>An EMR / ERS sent by the Inventory Linking Component</H3><p><strong>Message Code: </strong>EMR<br><strong>CRC: </strong>CRC<br><strong>Goods Location: </strong>LOC<br><strong>Goods Arrival Date Time: </strong>2018-08-08T08:08:08<br><strong>Shed Operator: </strong>shed<br><strong>Movement Reference Number: </strong>123</p>";
			TestRealWorldExample(message, CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse, EDIMessageStatusList.Codes.Acknowledged, expectedInterpretation);
		}

		public void TestParseRealExampleForInventoryLinkingQueryResponse()
		{
			var message = @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingQueryResponse>
  <queriedDUCR>
    <ucr>123</ucr>
    <movement>
      <messageCode>EAA</messageCode>
      <movementReference/>
    </movement>
    <entryStatus>
      <ics>ICS</ics>
      <roe>6</roe>
      <soe>3</soe>
    </entryStatus>
  </queriedDUCR>
  <childDUCR>
    <UCR>UCR</UCR>
    <declarationID>dec123</declarationID>
  </childDUCR>
</inventoryLinkingQueryResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";

			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>A response to an inventory linking query</H3><H4>The response includes the queried MUCR: 1/ children (the whole subtree with all MUCRs and DUCRs with declarations); 2/ all movements of the queried MUCR; 3/ all parents with their movements</H4><p><strong>Queried UCR Type: </strong>D<br><strong>Entry Status ICS: </strong>ICS<br><strong>Entry Status ROE: </strong>6 - No control required (The declaration has been risked and no control has been required.). CHIEF Equivalent: 6<br><strong>Entry Status SOE: </strong>3 - Declaration Clearance</p><H4>Movement: 1</H4><p><strong>Message Code: </strong>EAA</p><ul><p><strong>Child DUCRs</strong>:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>DUCR</strong></td><td><strong>Declaration ID</strong></td><td><strong>Parent MUCR</strong></td><td><strong>Entry Status ICS</strong></td><td><strong>Entry Status ROE</strong></td><td><strong>Entry Status SOE</strong></td></tr><tr><td>UCR</td><td>dec123</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr></table></p></ul>";
			TestRealWorldExample(message, CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse, EDIMessageStatusList.Codes.Acknowledged, expectedInterpretation);
		}

		void TestParseRealExampleForInventoryLinkingControlResponse(ZString actionCode, ZString expectedOutgoingMessageStatus, ZString expectedMessageInterpretation)
		{
			var message = string.Format(CultureInfo.InvariantCulture, @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingControlResponse>
            <messageCode>EAC</messageCode>
            <actionCode>{0}</actionCode>
            <ucr>
                <ucr>9GB896458895023-B00031258</ucr>
                <ucrType>D</ucrType>
            </ucr>
            <error>
                <errorCode>15</errorCode>
            </error>
        </inventoryLinkingControlResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>", actionCode);

			TestRealWorldExample(message, CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse, expectedOutgoingMessageStatus, expectedMessageInterpretation);
		}

		void TestRealWorldExample(ZString message, ZString code, ZString expectedOutgoingMessageStatus, ZString expectedMessageInterpretation)
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = message;
			interchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, code);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			var originalMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);
				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, string.Format(CultureInfo.InvariantCulture, "{0}B", outgoingMessage.EM_MessageNum), entry.PK, "77685C3D14D53E25E0540003BA9676AB");
				AssertEquals(expectedOutgoingMessageStatus, originalMessage.EM_Status);
				AssertEquals(expectedMessageInterpretation, messages[0].EM_MessageInterpretation);
			});
		}

		public void TestParseRealWorldExampleForConsol()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_RL_NKLoadPort = "GBLHR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "AUSYD";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_JS = shipment.PK;
			dec.JE_DeclarationReference = "B123";

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			consol.Messages.Add(outgoingMessage);
			Factory.Save();

			var message = @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingControlResponse>
            <messageCode>CST</messageCode>
            <actionCode>1</actionCode>
            <ucr>
                <ucr>9GB896458895023-B00031258</ucr>
                <ucrType>D</ucrType>
            </ucr>
        </inventoryLinkingControlResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";
			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = message;
			interchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			var originalMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);

			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);
				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, string.Format(CultureInfo.InvariantCulture, "{0}B", outgoingMessage.EM_MessageNum), consol.PK, "77685C3D14D53E25E0540003BA9676AB", ForwardingConsol.Schema.TableName);
				AssertEquals(EDIMessageStatusList.Codes.Acknowledged, originalMessage.EM_Status);
				Assert(consolWrapper?.MawbExportHelper?.ME_ChiefConsolIsClosed ?? false);
			});
		}

		public void TestMUCRClearedWhenOutboundMessageIsDIS()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_MasterUCR = "8GB123456789000-S0001000";
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "8GB123456789000-S0001000";

			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ApplicationReference = "77685C3D14D53E25E0540003BA9676AB";
			outgoingMessage.EM_MessageText = @"<inventoryLinkingConsolidationRequest>
  <messageCode>CST</messageCode>
  <masterUCR/>
  <ucrBlock>
	<ucr>8GB123456789000-S0001000</ucr>
	   <ucrType>D</ucrType>
	 </ucrBlock>
   </inventoryLinkingConsolidationRequest>";
			outgoingMessage.EM_MessageSubType = "DIS";
			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			var message = @"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<ConversationID>77685C3D14D53E25E0540003BA9676AB</ConversationID>
	</ResponseHeader>
	<ResponseBody>
		<inventoryLinkingControlResponse>
            <messageCode>CST</messageCode>
            <actionCode>1</actionCode>
            <ucr>
                <ucr>9GB896458895023-B00031258</ucr>
                <ucrType>D</ucrType>
            </ucr>
        </inventoryLinkingControlResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";
			var interchange = Factory.New<CDSInterchange>();
			interchange.EI_InterchangeNum = "1";
			interchange.EI_BodyText = message;
			interchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			Factory.Save();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { interchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = Factory.Load<EDIMessage>(query);

			var originalMessage = new BusinessObjectFactory().Load<CDSEDIMessage>(outgoingMessage.PK);

			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);

			CombineAssertions(() =>
			{
				AssertEquals(1, messages.Length);
				AssertMessage(messages[0], EDIMessageStatusList.Codes.ProcessedOK, string.Format(CultureInfo.InvariantCulture, "{0}B", outgoingMessage.EM_MessageNum), entry.PK, "77685C3D14D53E25E0540003BA9676AB");
				AssertEquals(EDIMessageStatusList.Codes.Acknowledged, originalMessage.EM_Status);
				Assert(declaration.JE_MasterUCR.IsEmpty);
			});
		}

		static void AssertMessage(EDIMessage message, ZString messageStatus, ZString expectedMessageNum, ZGuid expectedLinkedID, ZString expectedApplicationReference, string expectedLinkTable = CusEntryHeaderSchema.Constants.TableName)
		{
			AssertEquals("Message.EM_Status", messageStatus, message.EM_Status);
			AssertEquals("Message.EM_MessageNum", expectedMessageNum, message.EM_MessageNum);
			AssertEquals("Message.EM_LinkTable", expectedLinkTable, message.EM_LinkTable);
			AssertEquals("Message.EM_LinkUniqueID", expectedLinkedID, message.EM_LinkUniqueID);
			AssertEquals("Conversation ID Stored", expectedApplicationReference, message.EM_ApplicationReference);
		}

		public void TestRealDISQueryWithMultipleDeclarationResponsesInterpretation()
		{
			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "WTG";
			outgoingInterchange.EI_To = "GBCDS";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			Factory.Save();

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			inboundInterchange.EI_Status = EDIInterchange.Status.Queued;
			inboundInterchange.EI_From = "GBCDS";
			inboundInterchange.EI_To = "WTG";
			inboundInterchange.EI_BodyText = $@"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<eHubTrackingId>{outgoingInterchange.PK}</eHubTrackingId>
	</ResponseHeader>
	<ResponseBody>
		<p:DeclarationStatusResponse 
		xsi:schemaLocation=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/declarationInformationRetrieval/status/v2"">
			<p:DeclarationStatusDetails>
				<p:Declaration>
					<p:AcceptanceDateTime>
						<p1:DateTimeString formatCode=""304"">20200528145050Z</p1:DateTimeString>
					</p:AcceptanceDateTime>
					<p:ID>20GB5WCZQ922CFGVR9</p:ID>
					<p:VersionID>1</p:VersionID>
					<p:ReceivedDateTime>
						<p:DateTimeString formatCode=""304"">20200528145050Z</p:DateTimeString>
					</p:ReceivedDateTime>
					<p:ROE>H</p:ROE>
					<p:ICS>14</p:ICS>
				</p:Declaration>
				<p2:Declaration>
					<p2:FunctionCode>9</p2:FunctionCode>
					<p2:TypeCode>IMD</p2:TypeCode>
					<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
					<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
					<p2:Submitter>
						<p2:ID>GB8172025690</p2:ID>
					</p2:Submitter>
					<p2:GoodsShipment>
						<p2:PreviousDocument>
							<p2:ID>0GB896458895023-B00031398</p2:ID>
							<p2:TypeCode>DCR</p2:TypeCode>
						</p2:PreviousDocument>
						<p2:UCR>
							<p2:TraderAssignedReferenceID>0GB896458895023-B00031398</p2:TraderAssignedReferenceID>
						</p2:UCR>
					</p2:GoodsShipment>
				</p2:Declaration>
			</p:DeclarationStatusDetails>
			<p:DeclarationStatusDetails>
				<p:Declaration>
					<p:ID>20GB5XKJE313MFGVR1</p:ID>
					<p:VersionID>1</p:VersionID>
					<p:ReceivedDateTime>
						<p:DateTimeString formatCode=""304"">20200529110951Z</p:DateTimeString>
					</p:ReceivedDateTime>
					<p:ROE>H</p:ROE>
					<p:ICS>5</p:ICS>
				</p:Declaration>
				<p2:Declaration>
					<p2:FunctionCode>9</p2:FunctionCode>
					<p2:TypeCode>IMD</p2:TypeCode>
					<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
					<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
					<p2:Submitter>
						<p2:ID>GB8172025690</p2:ID>
					</p2:Submitter>
					<p2:GoodsShipment>
						<p2:PreviousDocument>
							<p2:ID>0GB896458895023-B00031398</p2:ID>
							<p2:TypeCode>DCR</p2:TypeCode>
						</p2:PreviousDocument>
						<p2:UCR>
							<p2:TraderAssignedReferenceID>0GB896458895023-B00031398/1</p2:TraderAssignedReferenceID>
						</p2:UCR>
					</p2:GoodsShipment>
				</p2:Declaration>
			</p:DeclarationStatusDetails>
			<p:DeclarationStatusDetails>
				<p:Declaration>
					<p:AcceptanceDateTime>
						<p1:DateTimeString formatCode=""304"">20200529110947Z</p1:DateTimeString>
					</p:AcceptanceDateTime>
					<p:ID>20GB5XKJAT03OFGVR7</p:ID>
					<p:VersionID>1</p:VersionID>
					<p:ReceivedDateTime>
						<p:DateTimeString formatCode=""304"">20200529110947Z</p:DateTimeString>
					</p:ReceivedDateTime>
					<p:ROE>H</p:ROE>
					<p:ICS>14</p:ICS>
				</p:Declaration>
				<p2:Declaration>
					<p2:FunctionCode>9</p2:FunctionCode>
					<p2:TypeCode>IMD</p2:TypeCode>
					<p2:GoodsItemQuantity>1</p2:GoodsItemQuantity>
					<p2:TotalPackageQuantity>55.0</p2:TotalPackageQuantity>
					<p2:Submitter>
						<p2:ID>GB8172025690</p2:ID>
					</p2:Submitter>
					<p2:GoodsShipment>
						<p2:PreviousDocument>
							<p2:ID>0GB896458895023-B00031398</p2:ID>
							<p2:TypeCode>DCR</p2:TypeCode>
						</p2:PreviousDocument>
						<p2:UCR>
							<p2:TraderAssignedReferenceID>0GB896458895023-B00031398/1</p2:TraderAssignedReferenceID>
						</p2:UCR>
					</p2:GoodsShipment>
				</p2:Declaration>
			</p:DeclarationStatusDetails>
		</p:DeclarationStatusResponse>
	</ResponseBody>
</GBCustomsBusinessResponse>";

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { inboundInterchange.PK });
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.QueryResponse);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;
			var messages = new BusinessObjectFactory().Load<EDIMessage>(query);
			var cleanEHubTrackingId = new ZString($"{outgoingInterchange.PK}").KeepAlphanumericCharacters();

			AssertEquals(1, messages.Length);
			AssertContains("<p><strong>Acceptance Date Time: </strong>28-May-20 14:50:50<br><strong>ID: </strong>20GB5WCZQ922CFGVR9<br><strong>Version ID: </strong>1<br><strong>Received Date Time: </strong>28-May-20 14:50:50<br><strong>ROE: </strong>H<br><strong>ICS: </strong>14 - Declaration Risked (Notification that the declaration has now been risked and will contain a specific ROE code.). CHIEF Equivalent: N/A<br><strong>Function Code: </strong>9<br><strong>Type Code: </strong>IMD<br><strong>Goods Item Quantity: </strong>1<br><strong>Total Package Quantity: </strong>55.0<br><strong>Submitter: </strong>GB8172025690<br><strong>UCR: </strong>0GB896458895023-B00031398</p>", messages[0].EM_MessageInterpretation);
			AssertContains("<p><strong>ID: </strong>20GB5XKJE313MFGVR1<br><strong>Version ID: </strong>1<br><strong>Received Date Time: </strong>29-May-20 11:09:51<br><strong>ROE: </strong>H<br><strong>ICS: </strong>5 - Declaration Rejected (If the declaration is rejected, then a Declaration Status Notification will be received with this code.). CHIEF Equivalent: N/A<br><strong>Function Code: </strong>9<br><strong>Type Code: </strong>IMD<br><strong>Goods Item Quantity: </strong>1<br><strong>Total Package Quantity: </strong>55.0<br><strong>Submitter: </strong>GB8172025690<br><strong>UCR: </strong>0GB896458895023-B00031398/1</p>", messages[0].EM_MessageInterpretation);
			AssertContains("<p><strong>Acceptance Date Time: </strong>29-May-20 11:09:47<br><strong>ID: </strong>20GB5XKJAT03OFGVR7<br><strong>Version ID: </strong>1<br><strong>Received Date Time: </strong>29-May-20 11:09:47<br><strong>ROE: </strong>H<br><strong>ICS: </strong>14 - Declaration Risked (Notification that the declaration has now been risked and will contain a specific ROE code.). CHIEF Equivalent: N/A<br><strong>Function Code: </strong>9<br><strong>Type Code: </strong>IMD<br><strong>Goods Item Quantity: </strong>1<br><strong>Total Package Quantity: </strong>55.0<br><strong>Submitter: </strong>GB8172025690<br><strong>UCR: </strong>0GB896458895023-B00031398/1</p>", messages[0].EM_MessageInterpretation);
		}

		public void TestDocumentUploadConfirmation()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_Code = "ABC";
			user.GS_LoginName = "ABC";
			user.GS_EmailAddress = "abc@abc.com";
			user.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));

			var declaration = Factory.New<JobDeclaration>();
			declaration.Logs.AddNew(AutoEvents.DocumentSent, GetFormattedEventReference("622bc0f8-ef14-48dd-b1ca-0dbdeea96c24"));
			declaration.JE_DeclarationReference = "B00001000";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_SessionGUID = new ZGuid("f127bdc2-b97b-4305-aaba-8026dc82949a");

			var outgoingMessage = Factory.New<CDSEDIMessage>();
			declaration.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = "ABC";

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			inboundInterchange.EI_Status = EDIInterchange.Status.Queued;
			inboundInterchange.EI_From = "eHub";
			inboundInterchange.EI_To = "WTG";
			inboundInterchange.EI_BodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
    <s0:eHubTrackingId>f127bdc2-b97b-4305-aaba-8026dc82949a</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <Root xmlns=""hmrc:fileupload"">
      <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
      <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
      <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
      <Outcome>SUCCESS</Outcome>
      <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
    </Root>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_EI, inboundInterchange.PK);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation);
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;

			var messages = new BusinessObjectFactory().Load<EDIMessage>(query);
			AssertEquals(1, messages.Length);
			AssertEquals(EDIMessage.Status.ProcessedOK, messages[0].EM_Status);
			AssertEquals(new ZString("f127bdc2-b97b-4305-aaba-8026dc82949a").KeepAlphanumericCharacters(), messages[0].EM_ApplicationReference);

			var targetDeclaration = (JobDeclaration)messages[0].EM_LinkedObject;
			AssertEquals(declaration.PK, targetDeclaration.PK);

			var newLog = targetDeclaration.Logs.Find(l => l.Event.SE_Code == AutoEvents.DocumentDeliveredCode).FirstOrDefault();
			AssertNotNull(newLog);
			AssertEquals("Delivered successfully to CDS: CDS Entry Document - 2GB427168118378-B60004374.pdf|Job Number: B00001000|Details: Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.", newLog.SL_Reference);
		}

		ZString GetFormattedEventReference(string fileReference)
		{
			var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContentID, fileReference }
			};
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"UK Customs CDS messages inbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsDeclarationServices),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"UK Customs CDS interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GbCustomsDeclarationServices),
				};
			}
		}

		#region Notification Email Tests

		public void TestEmailNotificationToPostmaster()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup(setupPostmaster: false);
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "PostMaster@Gallifrey.com", 0, false);
				SetupEntryAndPostmastersGroup(setupEntry: false);
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "PostMaster@Gallifrey.com", 1, true);
			}
		}

		public void TestEmailNotificationToCustomsGroup()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Selina.Kyle@CustomsGroup.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem, "Selina.Kyle@CustomsGroup.com", "CUS");
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Selina.Kyle@CustomsGroup.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSGroup()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Harley.Quinn@DefaultCDS.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDS, "Harley.Quinn@DefaultCDS.com", "CDS");
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Harley.Quinn@DefaultCDS.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSPositiveRepliesGroup()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Oswald.Cobblepot@Positives.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDSPositiveReplies, "Oswald.Cobblepot@Positives.com", "POS");
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Oswald.Cobblepot@Positives.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSPositiveReplies_RCV()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Edward.Nygma@gcpd.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDSRCV, "Edward.Nygma@gcpd.com", "RCV");
				ProcessCDSMessageAndAssertEmail(entry, "02-RCV", "Edward.Nygma@gcpd.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSPositiveReplies_ACC()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "01-ACC", "james.gordon@gcpd.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDSACC, "james.gordon@gcpd.com", "ACC");
				ProcessCDSMessageAndAssertEmail(entry, "01-ACC", "james.gordon@gcpd.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSRejections()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "03-REJ", "Edward.Nygma@gcpd.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDSRejections, "Edward.Nygma@gcpd.com", "REJ");
				ProcessCDSMessageAndAssertEmail(entry, "03-REJ", "Edward.Nygma@gcpd.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdates()
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, "04-INC", "Edward.Nygma@gcpd.com", 1, false);
				SetupNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCDSUnsolicitedUpdates, "Edward.Nygma@gcpd.com", "UNS");
				ProcessCDSMessageAndAssertEmail(entry, "04-INC", "Edward.Nygma@gcpd.com", 2, true);
			}
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesINC()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSINC, "04-INC");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesCTL()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSCTL, "05-CTL");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesDOC()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSDOC, "06-DOC");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesRES()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSRES, "07-RES");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesROG()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSROG, "08-ROG");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesCLE()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSCLE, "09-CLE");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesINV()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSINV, "10-INV");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesREQ()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSREQ, "11-REQ");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesTAX()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSTAX, "13-TAX");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesCPI()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSCPI, "14-CPI");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesCPR()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSCPR, "15-CPR");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesEOG()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSEOG, "16-EOG");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesEXT()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSEXT, "17-EXT");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesGER()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSGER, "18-GER");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesALV()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSALV, "50-ALV");
		}

		public void TestEmailNotificationToCDSUnsolicitedUpdatesQRY()
		{
			RunUnsolicitedUpdatesForCode(GBCustomsDataRegistry.Instance.NotificationCDSQRY, "51-QRY");
		}

		public void RunUnsolicitedUpdatesForCode(IRegistryItem regItem, ZString code)
		{
			var branch = GetBranchGuid();
			using (DisposableEnvironment.ForBranch(branch))
			{
				ZString responseBody = code;
				ZString responseCode = responseBody.SubstringSafe(3);
				var entry = SetupEntryAndPostmastersGroup();
				ProcessCDSMessageAndAssertEmail(entry, responseBody, "Edward.Nygma@gcpd.com", 1, false);
				SetupNotificationGroup(regItem, "Edward.Nygma@gcpd.com", responseCode);
				ProcessCDSMessageAndAssertEmail(entry, responseBody, "Edward.Nygma@gcpd.com", 2, true);
			}
		}

		Guid GetBranchGuid()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			company.GC_Code = "KSE";
			var testBranch = company.Branches.AddNew();
			testBranch.GB_Code = "AFC";
			testBranch.GB_RL_NKHomePort = "GBLON";
			testBranch.GB_BranchName = "Test Branch AFC";
			Factory.Save();
			return testBranch.PK.ToGuid();
		}

		CusEntryHeader SetupEntryAndPostmastersGroup(bool setupEntry = true, bool setupPostmaster = true)
		{
			CusEntryHeader entry = null;
			if (setupEntry)
			{
				ResponseFunctionTests.SetUpZZRefData(Factory);
				var dec = Factory.New<JobDeclaration>();
				dec.JE_DeclarationReference = "B123";
				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				entry = dec.CustomsEntryHeaders.AddNew();
				entry.LRN = "8GB123456789000-S0001000";
			}
			if (setupPostmaster)
			{
				var postmasters = Factory.Load<GlbGroup>(Groups.PostMastersGroupPK);
				var postMaster = postmasters.Staff.AddNew();
				postMaster.GS_EmailAddress = "PostMaster@Gallifrey.com";
			}
			Factory.Save();
			return entry;
		}

		void SetupNotificationGroup(IRegistryItem notificationRegoItem, ZString emailAddress, ZString code)
		{
			if (notificationRegoItem != null)
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = code;
				group.GG_Desc = "DCGroup" + code;
				var staff = group.Staff.AddNew();
				staff.GS_Code = code;
				staff.GS_LoginName = "DCUser" + code;
				staff.GS_EmailAddress = emailAddress;
				Factory.Save();
				notificationRegoItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
				Factory.Save();
			}
		}

		void ProcessCDSMessageAndAssertEmail(CusEntryHeader entry, ZString emailBodyCode, ZString emailAddress, int expectedEmailCount, bool assertionResult)
		{
			var functionCode = emailBodyCode.Left(2);
			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			#region ediMessage.EM_MessageText = response from CDS
			ediMessage.EM_MessageText = $@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>{functionCode}</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212+01</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			#endregion EM_MessageText
			entry.Messages.Add(ediMessage);
			Factory.Save();

			var responseFunc = CDSResponse.ResponseFunction.New(functionCode);

			InitialiseAndRunTaskSchedule(new CDSMessageRetrieverServiceTask());

			AssertEquals("Pre-req: Email count expected" + Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count + " doesn't match expected count " + expectedEmailCount, true, expectedEmailCount == Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			CombineAssertions(() =>
			{
				if (Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count > 0)
				{
					var sentEmail = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[expectedEmailCount - 1];
					AssertContains("Email subject expected for " + emailBodyCode, "A CDS response has been received for job B123", sentEmail.Subject);
					AssertContains("Email body expected for " + emailBodyCode, $"Response from CDS: {responseFunc.GetDescription(entry)}</H3><p><strong>Function Code: </strong>" + emailBodyCode, sentEmail.Body);
					AssertEquals("Email address expected for " + emailBodyCode, assertionResult, emailAddress == sentEmail.Recipients[0].Email);
				}
			});
		}

		#endregion Notification Email Tests
	}
}

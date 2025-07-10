using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSInventoryLinkingQueryResponseMessageProcessorTests : CDSInventoryLinkingResponseMessageProcessorTests
	{
		public void TestMessageFriendlyName()
		{
			var processor = new CDSInventoryLinkingQueryResponseMessageProcessor(new LoggingInformation());
			AssertEquals("CDS Inventory Linking Query Response Message", processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			var processor = new CDSInventoryLinkingQueryResponseMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse, messageTypesToInclude[0]);
		}

		public void TestFindingInventoryLinkingQueryJob()
		{
			LoggingInformation logger = new LoggingInformation();
			var message = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
	<s0:ResponseHeader>
		<s0:ConversationID>d03f84e3b5894aaaa1f8cafd744add8e</s0:ConversationID>
		<s0:eHubTrackingId>{0}</s0:eHubTrackingId>
	</s0:ResponseHeader>
	<s0:ResponseBody>
		<s0:inventoryLinkingQueryResponse>
		  <s0:queriedUCR>
		    <s0:ucr>123</s0:ucr>
		    <s0:ucrType>D</s0:ucrType>
		  </s0:queriedUCR>
		  <s0:shut>true</s0:shut>
		  <s0:children>
		    <s0:declaration>
		      <s0:UCR>UCR</s0:UCR>
		      <s0:declarationID>dec123</s0:declarationID>
		    </s0:declaration>
		  </s0:children>
		  <s0:movement>
		    <s0:messageCode>EAA</s0:messageCode>
		    <s0:shedOPID/>
		    <s0:movementReference/>
		    <s0:entryStatus>
		      <s0:ics>ICS</s0:ics>
		      <s0:roe>6</s0:roe>
		      <s0:soe>3</s0:soe>
		    </s0:entryStatus>
		  </s0:movement>
		</s0:inventoryLinkingQueryResponse>
	</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingQueryResponseMessageProcessor(logger), logger);
		}

		public void TestFindingInventoryLinkingQueryJobFromMessageWithEnvelopeBody()
		{
			var logger = new LoggingInformation();
			var message = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>d03f84e3b5894aaaa1f8cafd744add8e</s0:ConversationID>
    <s0:eHubTrackingId>{0}</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <S:Envelope xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/03/addressing"" xmlns:ebi=""http://www.myvan.descartes.com/ebi/2004/r1"" xmlns:pnt=""http://www.myvan.descartes.com/pnt/2018/r1"" xmlns:S=""http://www.w3.org/2003/05/soap-envelope"">
      <S:Header>
        <wsa:From>
          <wsa:Address>urn:zz:PENTANT_CDS</wsa:Address>
        </wsa:From>
        <wsa:To>urn:duns:211430898</wsa:To>
        <wsa:MessageID>d03f84e3b5894aaaa1f8cafd744add8e</wsa:MessageID>
        <wsa:Action>urn:myvan:INVEQRES</wsa:Action>
        <ebi:CorrelationId>d03f84e3b5894aaaa1f8cafd744add8e</ebi:CorrelationId>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
          <pnt:X-Correlation-ID>5853b804-25a6-4f8c-af15-8af1a955fbfa</pnt:X-Correlation-ID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
  		<ns2:inventoryLinkingQueryResponse xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1"" xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"">
		  <ns2:queriedUCR>
		    <ns2:ucr>123</ns2:ucr>
		    <ns2:ucrType>D</ns2:ucrType>
		  </ns2:queriedUCR>
		  <ns2:shut>true</ns2:shut>
		  <ns2:children>
		    <ns2:declaration>
		      <ns2:UCR>UCR</ns2:UCR>
		      <ns2:declarationID>dec123</ns2:declarationID>
		    </ns2:declaration>
		  </ns2:children>
		  <ns2:movement>
		    <ns2:messageCode>EAA</ns2:messageCode>
		    <ns2:shedOPID/>
		    <ns2:movementReference/>
		    <ns2:entryStatus>
		      <ns2:ics>ICS</ns2:ics>
		      <ns2:roe>6</ns2:roe>
		      <ns2:soe>3</ns2:soe>
		    </ns2:entryStatus>
		  </ns2:movement>
		</ns2:inventoryLinkingQueryResponse>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingQueryResponseMessageProcessor(logger), logger);
		}

		public void TestCloseForConsolJobs()
		{
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

			var outgoingMessage = Factory.New<CDSInventoryLinkingQueryRequestEDIMessage>();
			outgoingMessage.EM_LinkUniqueID = consol.PK;
			outgoingMessage.EM_LinkTable = "JobConsol";
			outgoingMessage.EM_ApplicationReference = "XYZ1234";

			var messageText = @"<ns2:inventoryLinkingQueryResponse xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"" xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1""> 
    <ns2:queriedMUCR>
        <ns2:UCR>A:24682009062</ns2:UCR>
        <ns2:entryStatus>
            <ns2:roe>6</ns2:roe>
            <ns2:soe>3</ns2:soe>
        </ns2:entryStatus>
        <ns2:shut>{SHUT}</ns2:shut>
    </ns2:queriedMUCR>
</ns2:inventoryLinkingQueryResponse>";
			var message = Factory.New<CDSInventoryLinkingQueryResponseEDIMessage>();
			message.EM_LinkUniqueID = consol.PK;
			message.EM_LinkTable = "JobConsol";
			message.EM_ApplicationReference = "XYZ1234";
			message.EM_MessageText = messageText.Replace("{SHUT}", "false");

			var logger = new LoggingInformation();
			var processor = new CDSInventoryLinkingQueryResponseMessageProcessor(logger);
			processor.ProcessMessage(message);

			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			AssertEquals("ME_ChiefConsolIsClosed, queriedMUCR.shut = false", expected: false, consolWrapper.MawbExportHelper.ME_ChiefConsolIsClosed);

			message.EM_MessageText = messageText.Replace("{SHUT}", "true");
			processor.ProcessMessage(message);

			AssertEquals("ME_ChiefConsolIsClosed, queriedMUCR.shut = true", expected: true, consolWrapper.MawbExportHelper.ME_ChiefConsolIsClosed);
		}
	}
}

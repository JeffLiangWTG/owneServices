using Enterprise.BatchProcessor;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSInventoryLinkingMovementResponseMessageProcessorTests : CDSInventoryLinkingResponseMessageProcessorTests
	{
		public void TestMessageFriendlyName()
		{
			var processor = new CDSInventoryLinkingMovementResponseMessageProcessor(new LoggingInformation());
			AssertEquals("CDS Inventory Linking Movement Response Message", processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			var processor = new CDSInventoryLinkingMovementResponseMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementResponse, messageTypesToInclude[0]);
		}

		public void TestFindingInventoryLinkingMovementJob()
		{
			LoggingInformation logger = new LoggingInformation();
			var message = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
	<s0:ResponseHeader>
		<s0:ConversationID>d03f84e3b5894aaaa1f8cafd744add8e</s0:ConversationID>
		<s0:eHubTrackingId>{0}</s0:eHubTrackingId>
	</s0:ResponseHeader>
	<s0:ResponseBody>
		<s0:inventoryLinkingMovementResponse>
		  <s0:messageCode>EAA</s0:messageCode>
		  <s0:crc>CRC</s0:crc>
		  <s0:goodsArrivalDateTime>2018-08-08T08:08:08</s0:goodsArrivalDateTime>
		  <s0:goodsLocation>LOC</s0:goodsLocation>
		  <s0:shedOPID>SHED</s0:shedOPID>
		  <s0:movementReference>123</s0:movementReference>
		  <s0:submitRole>ROLE</s0:submitRole>
		  <s0:ucrBlock>
		    <s0:ucr>UCR</s0:ucr>
		    <s0:ucrType>D</s0:ucrType>
		  </s0:ucrBlock>
		  <s0:goodsItem>
		    <s0:commodityCode>123</s0:commodityCode>
		    <s0:totalPackages>1</s0:totalPackages>
		    <s0:totalNetMass>1.1</s0:totalNetMass>
		  </s0:goodsItem>
		    <s0:goodsItem>
		    <s0:commodityCode>456</s0:commodityCode>
		    <s0:totalPackages>2</s0:totalPackages>
		    <s0:totalNetMass>2.2</s0:totalNetMass>
		  </s0:goodsItem><s0:entryStatus>
		    <s0:ics>ICS</s0:ics>
		    <s0:roe>ROE</s0:roe>
		    <s0:soe>SOE</s0:soe>
		  </s0:entryStatus>
		</s0:inventoryLinkingMovementResponse>
	</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingMovementResponseMessageProcessor(logger), logger);
		}

		public void TestFindingInventoryLinkingMovementJobFromMessageWithEnvelopeBody()
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
        <wsa:Action>urn:myvan:INVEMRES</wsa:Action>
        <ebi:CorrelationId>d03f84e3b5894aaaa1f8cafd744add8e</ebi:CorrelationId>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
          <pnt:X-Correlation-ID>5853b804-25a6-4f8c-af15-8af1a955fbfa</pnt:X-Correlation-ID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
		<ns2:inventoryLinkingMovementResponse xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1"" xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"">
		  <ns2:messageCode>EAA</ns2:messageCode>
		  <ns2:crc>CRC</ns2:crc>
		  <ns2:goodsArrivalDateTime>2018-08-08T08:08:08</ns2:goodsArrivalDateTime>
		  <ns2:goodsLocation>LOC</ns2:goodsLocation>
		  <ns2:shedOPID>SHED</ns2:shedOPID>
		  <ns2:movementReference>123</ns2:movementReference>
		  <ns2:submitRole>ROLE</ns2:submitRole>
		  <ns2:ucrBlock>
		    <ns2:ucr>UCR</ns2:ucr>
		    <ns2:ucrType>D</ns2:ucrType>
		  </ns2:ucrBlock>
		  <ns2:goodsItem>
		    <ns2:commodityCode>123</ns2:commodityCode>
		    <ns2:totalPackages>1</ns2:totalPackages>
		    <ns2:totalNetMass>1.1</ns2:totalNetMass>
		  </ns2:goodsItem>
		    <ns2:goodsItem>
		    <ns2:commodityCode>456</ns2:commodityCode>
		    <ns2:totalPackages>2</ns2:totalPackages>
		    <ns2:totalNetMass>2.2</ns2:totalNetMass>
		  </ns2:goodsItem><ns2:entryStatus>
		    <ns2:ics>ICS</ns2:ics>
		    <ns2:roe>ROE</ns2:roe>
		    <ns2:soe>SOE</ns2:soe>
		  </ns2:entryStatus>
		</ns2:inventoryLinkingMovementResponse>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingMovementResponseMessageProcessor(logger), logger);
		}
	}
}

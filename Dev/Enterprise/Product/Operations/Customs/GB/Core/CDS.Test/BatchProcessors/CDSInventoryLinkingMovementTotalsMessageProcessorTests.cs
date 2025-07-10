using Enterprise.BatchProcessor;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSInventoryLinkingMovementTotalsMessageProcessorTests : CDSInventoryLinkingResponseMessageProcessorTests
	{
		public void TestMessageFriendlyName()
		{
			var processor = new CDSInventoryLinkingMovementTotalsMessageProcessor(new LoggingInformation());
			AssertEquals("CDS Inventory Linking Movement Totals Message", processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			var processor = new CDSInventoryLinkingMovementTotalsMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse, messageTypesToInclude[0]);
		}

		public void TestFindingInventoryLinkingMovementTotalsJob()
		{
			LoggingInformation logger = new LoggingInformation();
			var message = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
	<s0:ResponseHeader>
		<s0:ConversationID>d03f84e3b5894aaaa1f8cafd744add8e</s0:ConversationID>
		<s0:eHubTrackingId>{0}</s0:eHubTrackingId>
	</s0:ResponseHeader>
	<s0:ResponseBody>
		<s0:inventoryLinkingMovementTotalsResponse>
		  <s0:messageCode>EMR</s0:messageCode>
		  <s0:crc>CRC</s0:crc>
		  <s0:goodsLocation>LOC</s0:goodsLocation>
		  <s0:goodsArrivalDateTime>2018-08-08T08:08:08</s0:goodsArrivalDateTime>
		  <s0:shedOPID>shed</s0:shedOPID>
		  <s0:movementReference>123</s0:movementReference>
		</s0:inventoryLinkingMovementTotalsResponse>
	</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingMovementResponseMessageProcessor(logger), logger);
		}

		public void TestFindingInventoryLinkingMovementTotalsJobFromMessageWithEnvelopeBody()
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
        <wsa:Action>urn:myvan:INVEMTR</wsa:Action>
        <ebi:CorrelationId>d03f84e3b5894aaaa1f8cafd744add8e</ebi:CorrelationId>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
          <pnt:X-Correlation-ID>5853b804-25a6-4f8c-af15-8af1a955fbfa</pnt:X-Correlation-ID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
		<ns2:inventoryLinkingMovementTotalsResponse xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1"" xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"">
		  <ns2:messageCode>EMR</ns2:messageCode>
		  <ns2:crc>CRC</ns2:crc>
		  <ns2:goodsLocation>LOC</ns2:goodsLocation>
		  <ns2:goodsArrivalDateTime>2018-08-08T08:08:08</ns2:goodsArrivalDateTime>
		  <ns2:shedOPID>shed</ns2:shedOPID>
		  <ns2:movementReference>123</ns2:movementReference>
		</ns2:inventoryLinkingMovementTotalsResponse>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingMovementResponseMessageProcessor(logger), logger);
		}
	}
}

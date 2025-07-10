using Enterprise.BatchProcessor;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSInventoryLinkingControlResponseMessageProcessorTests : CDSInventoryLinkingResponseMessageProcessorTests
	{
		public void TestMessageFriendlyName()
		{
			var processor = new CDSInventoryLinkingControlResponseMessageProcessor(new LoggingInformation());
			AssertEquals("CDS Inventory Linking Control Response Message", processor.MessageFriendlyName);
		}

		public void TestMessageTypesToInclude()
		{
			var processor = new CDSInventoryLinkingControlResponseMessageProcessor(new LoggingInformation());
			var messageTypesToInclude = processor.MessageTypesToInclude;
			AssertEquals(1, messageTypesToInclude.Count);
			AssertEquals(CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse, messageTypesToInclude[0]);
		}

		public void TestFindingInventoryLinkingControlJob()
		{
			LoggingInformation logger = new LoggingInformation();
			var message = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
	<s0:ResponseHeader>
		<s0:ConversationID>d03f84e3b5894aaaa1f8cafd744add8e</s0:ConversationID>
		<s0:eHubTrackingId>{0}</s0:eHubTrackingId>
	</s0:ResponseHeader>
	<s0:ResponseBody>
		<s0:inventoryLinkingControlResponse>
            <s0:messageCode>EAC</s0:messageCode>
            <s0:actionCode>LCR</s0:actionCode>
            <s0:ucr>
                <s0:ucr>9GB896458895023-B00031258</s0:ucr>
                <s0:ucrType>D</s0:ucrType>
            </s0:ucr>
            <s0:error>
                <s0:errorCode>15</s0:errorCode>
            </s0:error>
        </s0:inventoryLinkingControlResponse>
	</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingControlResponseMessageProcessor(logger), logger);
		}

		public void TestFindingInventoryLinkingControlJobFromMessageWithEnvelopeBody()
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
			  <wsa:Address>urn:zz:PENTANT_TDR</wsa:Address>
			</wsa:From>
			<wsa:To>urn:zz:211430898</wsa:To>
			<wsa:MessageID>d03f84e3b5894aaaa1f8cafd744add8e</wsa:MessageID>
			<wsa:Action>urn:myvan:INVECTL</wsa:Action>
			<ebi:CorrelationId>d03f84e3b5894aaaa1f8cafd744add8e</ebi:CorrelationId>
			<pnt:CSPInformation>
			  <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
			  <pnt:X-Correlation-ID>5853b804-25a6-4f8c-af15-8af1a955fbfa</pnt:X-Correlation-ID>
			</pnt:CSPInformation>
		  </S:Header>
		  <S:Body>
			<ns2:inventoryLinkingControlResponse xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1"" xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"">
			  <ns2:messageCode>EAC</ns2:messageCode>
			  <ns2:actionCode>LCR</ns2:actionCode>
			  <ns2:ucr>
				<ns2:ucr>9GB896458895023-B00031258</ns2:ucr>
				<ns2:ucrType>D</ns2:ucrType>
			  </ns2:ucr>
			  <ns2:error>
				<ns2:errorCode>15</ns2:errorCode>
			  </ns2:error>
			</ns2:inventoryLinkingControlResponse>
		  </S:Body>
		</S:Envelope>
	</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			TestFindingJob(message, new CDSInventoryLinkingControlResponseMessageProcessor(logger), logger);
		}
	}
}

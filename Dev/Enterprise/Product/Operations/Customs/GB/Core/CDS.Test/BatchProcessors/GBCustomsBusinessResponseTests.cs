using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class GBCustomsBusinessResponseTests : TestCase
	{
		public void TestAllPropertiesOnGBCustomsBusinessResponse()
		{
			var res = new GBCustomsBusinessResponse(new ZGuid("7E517C9A-210E-45D4-B874-7168970781AD"), "<SynchronousResponse></SynchronousResponse>");
			AssertEquals(@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>7e517c9a210e45d4b8747168970781ad</ConversationID>
</ResponseHeader>
<ResponseBody><SynchronousResponse></SynchronousResponse></ResponseBody>
</GBCustomsBusinessResponse>", res.Xml);
			AssertEquals("7e517c9a210e45d4b8747168970781ad", res.ConversationId);
			AssertNotNull(res.SynchronousResponse);
			AssertNull(res.MetaData);
			Assert(!res.Responses.Any());

			res = new GBCustomsBusinessResponse(@"<GBCustomsBusinessResponse>
    <ResponseHeader Provider='DANIEL'>
        <ConversationID>5a013903-c8c6-404f-8683-d8e84d06f134</ConversationID>
        <eHubTrackingId>B7E59A1E-13C0-4C87-82C4-72D75119E7C1</eHubTrackingId>
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
</GBCustomsBusinessResponse>");
			AssertEquals("5a013903-c8c6-404f-8683-d8e84d06f134", res.ConversationId);
			AssertEquals("B7E59A1E-13C0-4C87-82C4-72D75119E7C1", res.EHubTrackingId);
			AssertEquals("DANIEL", res.Provider);
			AssertNull(res.SynchronousResponse);
			AssertNotNull(res.MetaData);
			AssertEquals(3, res.Responses.Count());

			res = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""GVMS"">
    <NotificationBoxId>4979BB8FB13A44209F6A64452397B657</NotificationBoxId>
    <MessageId>5B8DAF2FCBED4F168E1D47D15274CA42</MessageId>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""JSON"" Encoding=""base64"">
    VEVTVA==
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>");
			AssertEquals("5B8DAF2FCBED4F168E1D47D15274CA42", res.MessageId);
			AssertEquals("4979BB8FB13A44209F6A64452397B657", res.NotificationBoxId);
			AssertEquals("TEST", res.ResponseBodyJson);

			res = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""anything"">
  </s0:ResponseHeader>
  <s0:ResponseBody Encoding=""none"">{something: '<IamXml />'}</s0:ResponseBody>
</s0:GBCustomsBusinessResponse>");
			AssertEquals("{something: '<IamXml />'}", res.ResponseBodyJson);

			res = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""anything"">
    <RequestID>/customs/transits/movements/departures/16763</RequestID>
    <ServiceReference>16763</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody Encoding=""none"">
     &lt;CC016A/&gt;
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>");
			AssertEquals("<CC016A/>", res.ResponseBodyXml);
			AssertEquals("16763", res.ServiceReference);
			AssertEquals("/customs/transits/movements/departures/16763", res.RequestID);
			AssertEquals("16A", res.GetMessageSubType(null));

			res = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""anything"">
    <RequestID>/customs/transits/movements/departures/16763</RequestID>
    <ServiceReference>16763</ServiceReference>
  </s0:ResponseHeader>
  <s0:ResponseBody Encoding=""none"">
     &lt;CC016A&gt;
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>");
			AssertEquals("<CC016A>", res.ResponseBodyXml);
			AssertEquals("16763", res.ServiceReference);
			AssertEquals("/customs/transits/movements/departures/16763", res.RequestID);
			var logger = new LoggingInformation();
			var result = res.GetMessageSubType(logger);
			AssertEquals("", result);
			AssertContains("Failed to parse the MessageSubType : ", GetUserLogStrings(logger.UserLogStrings));

			res = new GBCustomsBusinessResponse(
@"<GBCustomsBusinessResponse>
	<ResponseHeader>
		<CorrelationID>87491122139921</CorrelationID>
		<eHubTrackingId>E9AC3350-FABD-4C8E-AB64-0DFB5973C459</eHubTrackingId>
	</ResponseHeader>
	<ResponseBody>
	</ResponseBody>
</GBCustomsBusinessResponse>");
			AssertEquals("87491122139921", res.CorrelationId);

			res = new GBCustomsBusinessResponse(@"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
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
</s0:GBCustomsBusinessResponse>");
			AssertEquals("622bc0f8-ef14-48dd-b1ca-0dbdeea96c24", res.ConversationId);
			var expectedDocumentUploadConfirmationResponse = new XmlDocument();
			expectedDocumentUploadConfirmationResponse.LoadXml(@"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>SUCCESS</Outcome>
  <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
</Root>");
			AssertEquals(expectedDocumentUploadConfirmationResponse.OuterXml, res.DocumentUploadConfirmationRoot);
		}

		protected string GetUserLogStrings(StringCollection userLogStrings)
		{
			return new ZStringBuilder(userLogStrings.OfType<string>()).ToStringWithNewLineBetweenAppends().Trim();
		}

		public void TestMetaDataFromMultipleXPaths()
		{
			var xml = @"<GBCustomsBusinessResponse>
    <ResponseHeader>
        <ConversationID>5a013903-c8c6-404f-8683-d8e84d06f134</ConversationID>
        <eHubTrackingId>B7E59A1E-13C0-4C87-82C4-72D75119E7C1</eHubTrackingId>
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
    </MetaData>
	</ResponseBody>
</GBCustomsBusinessResponse>";

			var res = new GBCustomsBusinessResponse(xml);
			AssertNotNull("GBCustomsBusinessResponse/ResponseBody/MetaData", res.MetaData);

			xml = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>ed82f8c9-fe62-4c29-9e8a-3bc320e3794c</s0:ConversationID>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <S:Envelope xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/03/addressing"" xmlns:ebi=""http://www.myvan.descartes.com/ebi/2004/r1"" xmlns:pnt=""http://www.myvan.descartes.com/pnt/2018/r1"" xmlns:S=""http://www.w3.org/2003/05/soap-envelope"">
      <S:Header>
        <wsa:From>
          <wsa:Address>urn:zz:PENTANT_CDS</wsa:Address>
        </wsa:From>
        <wsa:To>urn:zz:211430898</wsa:To>
        <wsa:MessageID>ed82f8c9-fe62-4c29-9e8a-3bc320e3794c</wsa:MessageID>
        <wsa:Action>urn:myvan:DMSREJ</wsa:Action>
        <ebi:CorrelationId>ed82f8c9-fe62-4c29-9e8a-3bc320e3794c</ebi:CorrelationId>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
          <pnt:X-Correlation-ID>8f36a7f6-3c2d-41b2-beac-c655922678ed</pnt:X-Correlation-ID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
        <_2:MetaData xmlns:_2=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
          <_2:WCODataModelVersionCode>3.6</_2:WCODataModelVersionCode>
          <_2:WCOTypeName>RES</_2:WCOTypeName>
          <_2:ResponsibleCountryCode />
          <_2:ResponsibleAgencyName />
          <_2:AgencyAssignedCustomizationCode />
          <_2:AgencyAssignedCustomizationVersionCode />
          <_2_1:Response xmlns:_2_1=""urn:wco:datamodel:WCO:RES-DMS:2"">
            <_2_1:FunctionCode>03</_2_1:FunctionCode>
            <_2_1:FunctionalReferenceID>93fa308e1e4249ec80cb1d98d2e36181</_2_1:FunctionalReferenceID>
            <_2_1:IssueDateTime>
              <_2_2:DateTimeString formatCode=""304"" xmlns:_2_2=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20211102120238Z</_2_2:DateTimeString>
            </_2_1:IssueDateTime>
            <_2_1:Error>
              <_2_1:ValidationCode>CDS10020</_2_1:ValidationCode>
              <_2_1:Pointer>
                <_2_1:DocumentSectionCode>42A</_2_1:DocumentSectionCode>
              </_2_1:Pointer>
              <_2_1:Pointer>
                <_2_1:DocumentSectionCode>67A</_2_1:DocumentSectionCode>
              </_2_1:Pointer>
              <_2_1:Pointer>
                <_2_1:SequenceNumeric>1</_2_1:SequenceNumeric>
                <_2_1:DocumentSectionCode>68A</_2_1:DocumentSectionCode>
              </_2_1:Pointer>
              <_2_1:Pointer>
                <_2_1:SequenceNumeric>1</_2_1:SequenceNumeric>
                <_2_1:DocumentSectionCode>70A</_2_1:DocumentSectionCode>
                <_2_1:TagID>166</_2_1:TagID>
              </_2_1:Pointer>
            </_2_1:Error>
            <_2_1:Declaration>
              <_2_1:FunctionalReferenceID>HYEDUKMIK0000000003898</_2_1:FunctionalReferenceID>
              <_2_1:ID>21GBC4J43GK49FYNR7</_2_1:ID>
              <_2_1:RejectionDateTime>
                <_2_2:DateTimeString formatCode=""304"" xmlns:_2_2=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20211102120238Z</_2_2:DateTimeString>
              </_2_1:RejectionDateTime>
              <_2_1:VersionID>1</_2_1:VersionID>
            </_2_1:Declaration>
          </_2_1:Response>
        </_2:MetaData>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>
";
			res = new GBCustomsBusinessResponse(xml);
			AssertNotNull("GBCustomsBusinessResponse/ResponseBody/Envelope/Body/MetaData", res.MetaData);
		}

		public void TestInventoryMessageFromPentantResponse()
		{
			var xml = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>6b91d532-25ab-47f0-8edd-5cbbf5974928</s0:ConversationID>
    <s0:eHubTrackingId>9315eb69-e841-40af-bcb2-6094c36de393</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <S:Envelope xmlns:S=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ebi=""http://www.myvan.descartes.com/ebi/2004/r1"" xmlns:pnt=""http://www.myvan.descartes.com/pnt/2018/r1"" xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/03/addressing"">
      <S:Header>
        <wsa:From>
          <wsa:Address>urn:zz:PENTANT_INV</wsa:Address>
        </wsa:From>
        <wsa:To>urn:duns:218162397</wsa:To>
        <wsa:Action>urn:myvan:INVRES</wsa:Action>
        <ebi:Sequence>
          <ebi:MessageNumber>qFrRJ9Y7NUeW0Jse98Xa7g==</ebi:MessageNumber>
        </ebi:Sequence>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTNDM</pnt:X-Badge-Identifier>
          <pnt:SoftwareVendor>WTG</pnt:SoftwareVendor>
          <pnt:TraderID>GB181516022000</pnt:TraderID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
        <pnt:InventoryMessage>BEGINMESSAGE~4837~PNTNDMDOVSND~PNTNDMDOVRCV~SHORTSEA~C~I~27/09/2022~15:20~DOG~FSA~CLS00326M~34DYL313~0~ZFSA85511113446~ENDMESSAGE</pnt:InventoryMessage>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			var res = new GBCustomsBusinessResponse(xml);
			AssertEquals("BEGINMESSAGE~4837~PNTNDMDOVSND~PNTNDMDOVRCV~SHORTSEA~C~I~27/09/2022~15:20~DOG~FSA~CLS00326M~34DYL313~0~ZFSA85511113446~ENDMESSAGE", res.InventoryMessage);
		}

		public void TestEnvelopeBodyInventoryLinkingControlResponseFromPentantResponse()
		{
			var xml = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>8e4fb769-98b2-4cfc-bc23-8e7dfb11c224</s0:ConversationID>
    <s0:eHubTrackingId>35cc172d-f956-4a68-8bbc-82c1e0606830</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <S:Envelope xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/03/addressing"" xmlns:ebi=""http://www.myvan.descartes.com/ebi/2004/r1"" xmlns:pnt=""http://www.myvan.descartes.com/pnt/2018/r1"" xmlns:S=""http://www.w3.org/2003/05/soap-envelope"">
      <S:Header>
        <wsa:From>
          <wsa:Address>urn:zz:PENTANT_TDR</wsa:Address>
        </wsa:From>
        <wsa:To>urn:zz:211430898</wsa:To>
        <wsa:MessageID>8e4fb769-98b2-4cfc-bc23-8e7dfb11c224</wsa:MessageID>
        <wsa:Action>urn:myvan:INVECTL</wsa:Action>
        <ebi:CorrelationId>8e4fb769-98b2-4cfc-bc23-8e7dfb11c224</ebi:CorrelationId>
        <pnt:CSPInformation>
          <pnt:X-Badge-Identifier>PNTWTG</pnt:X-Badge-Identifier>
          <pnt:X-Correlation-ID>5853b804-25a6-4f8c-af15-8af1a955fbfa</pnt:X-Correlation-ID>
        </pnt:CSPInformation>
      </S:Header>
      <S:Body>
        <ns2:inventoryLinkingControlResponse xmlns:ns2=""http://gov.uk/customs/inventoryLinking/v1"" xmlns=""http://gov.uk/customs/inventoryLinking/gatewayHeader/v1"">
          <ns2:messageCode>EAC</ns2:messageCode>
          <ns2:actionCode>3</ns2:actionCode>
          <ns2:ucr>
            <ns2:ucr>3GB427168118378-B60006340</ns2:ucr>
            <ns2:ucrType>D</ns2:ucrType>
          </ns2:ucr>
          <ns2:error>
            <ns2:errorCode>11</ns2:errorCode>
          </ns2:error>
        </ns2:inventoryLinkingControlResponse>
      </S:Body>
    </S:Envelope>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";

			var res = new GBCustomsBusinessResponse(xml);
			AssertNotNull(res.InventoryLinkingControlResponse);
			AssertEquals("8e4fb769-98b2-4cfc-bc23-8e7dfb11c224", res.ConversationId);
			AssertEquals("35cc172d-f956-4a68-8bbc-82c1e0606830", res.EHubTrackingId);
			AssertEquals("3", res.InventoryLinkingControlResponse.ActionCode);
			AssertEquals("EAC", res.InventoryLinkingControlResponse.MessageCode);
			AssertEquals("3GB427168118378-B60006340", res.InventoryLinkingControlResponse.UCR);
			AssertEquals("D", res.InventoryLinkingControlResponse.UCRType);
			AssertEquals(1, res.InventoryLinkingControlResponse.ErrorCodes.Length);
			AssertEquals("11", res.InventoryLinkingControlResponse.ErrorCodes[0]);
		}
	}
}

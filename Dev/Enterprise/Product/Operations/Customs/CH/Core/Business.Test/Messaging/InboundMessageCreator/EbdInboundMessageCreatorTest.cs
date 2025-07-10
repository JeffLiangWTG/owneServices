using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class EbdInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override IInboundMessageCreator GetMessageCreator() => new EbdInboundMessageCreator(Logger);

	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsEdec;

	protected override string InterchangeType => MessageTypeCodeList.Codes.EBD;

	protected override string GetExpectedMessageText(string parsingResult) => MIMETypeXTParser.ParseSoapEnvelope(parsingResult);

	protected override (string From, string BodyText)[] FailingDeserializations => new[]
	{
			(MessagingConstants.CustomsDestinationCodes.CustomsEbdSoap, FailingDeserialization),
		};

	protected override string UnsupportedSchemaVersion => TestingData.InputEdbUnsupportedSchema;

	public void TestGenerateMessageFromInterchangeAcceptance() => TestGenerateMessagesFromInterchange(TestingData.InputEdbResponseAcceptance, MessageSubTypeCodeList.Codes.Accepted);

	public void TestGenerateMessageFromInterchangeRejection() => TestGenerateMessagesFromInterchange(TestingData.InputEdbResponseRejection, MessageSubTypeCodeList.Codes.CustomsRejected);

	const string FailingDeserialization = @"HTTP/1.1 200 OK
Content-Type: multipart/related; boundary=""---- = _Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d""

------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
Content-Type: application/xop+xml; characterset=utf8; type=""text/xml""

<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
   <SOAP-ENV:Header/>
   <SOAP-ENV:Body>
      <ns2:ebdDocumentImportResponse schemaVersion = ""0.2"" xsi:schemaLocation=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1 https://www.ezv.admin.ch/dam/ezv/de/dokumente/e-dec/E-Begleitdokument/Webservice/eBD_DocumentImportResponse_xsd.xsd.download.xsd/ebdDocumentImportResponse_v_0_2.xsd"" xmlns:ns2=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportResponse/v1"" xmlns:ns3=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportRequest/v1"" xmlns:xmime=""http://www.w3.org/2005/05/xmlmime"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
         <wrongTag/>
      </ns2:ebdDocumentImportResponse>
   </SOAP-ENV:Body>
</SOAP-ENV:Envelope>
------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d--
";
}

using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing
{
	sealed class EdecBordereauInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
	{
		protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsEdec;

		protected override string InterchangeType => MessageTypeCodeList.Codes.BOR;

		protected override (string From, string BodyText)[] FailingDeserializations => new[]
		{
			(MessagingConstants.CustomsDestinationCodes.CustomsBordereauSoap, FailingDeserialization),
		};

		protected override string UnsupportedSchemaVersion => TestingData.EdecBordereauUnsupportedSchema;

		protected override string GetExpectedMessageText(string parsingResult) => MIMETypeXTParser.ParseSoapEnvelope(parsingResult);

		protected override IInboundMessageCreator GetMessageCreator() => new EdecBordereauInboundMessageCreator(Logger);

		public void TestGenerateMessageFromInterchangeBordereau() => TestGenerateMessagesFromInterchange(TestingData.EdecBordereauTypeBordereau, MessageSubTypeCodeList.Codes.BordereauResponse);

		public void TestGenerateMessageFromInterchangeBordereauList() => TestGenerateMessagesFromInterchange(TestingData.EdecBordereauTypeBordereauList, MessageSubTypeCodeList.Codes.BordereauList);

		public void TestGenerateMessageFromInterchangeBordereauRequestRejectionRuleError() => TestGenerateMessagesFromInterchange(TestingData.EdecBordereauTypeBordereauRequestRejectionRuleError, MessageSubTypeCodeList.Codes.RuleError);

		public void TestGenerateMessageFromInterchangeBordereauRequestRejectionXMLSchemaError() => TestGenerateMessagesFromInterchange(TestingData.EdecBordereauTypeBordereauRequestRejectionXMLSchemaError, MessageSubTypeCodeList.Codes.XmlSchemaError);

		const string FailingDeserialization = @"
HTTP/1.1 200 OK
Content-Type: multipart/related; boundary=""----=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d""

------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
Content-Type: application/xop+xml; characterset=utf8; type=""text/xml""

<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <bordereauRequestResponse schemaVersion="" 0.1""
      xsi:schemaLocation="" http:// www.e-dec.ch/ xml/ schema/ edecBordereauResponse/ v1 http:// www.ezv.admin.ch/ pdf_linker.php?doc=edecBordereauResponse_v_0_1""
			xmlns:ns1=""http://www.e-dec.ch/xml/schema/edecBordereauResponse/v1""
			xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<wrongTag>
    </bordereauRequestResponse>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>

------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d--
";
	}
}

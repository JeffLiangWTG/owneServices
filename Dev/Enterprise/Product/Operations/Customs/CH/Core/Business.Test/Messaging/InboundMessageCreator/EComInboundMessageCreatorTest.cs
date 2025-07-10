using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EComInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	public void TestGenerateMessageFromInterchangeAcceptance() => TestGenerateMessagesFromInterchange(TestingData.InputEComResponseAcceptanceSOAP, MessageSubTypeCodeList.Codes.Accepted);

	public void TestGenerateMessageFromInterchangeRejection() => TestGenerateMessagesFromInterchange(TestingData.InputEComResponseRejectionSOAP, MessageSubTypeCodeList.Codes.RuleError);

	public void TestGenerateMessageFromInterchangeSchemaError() => TestGenerateMessagesFromInterchange(TestingData.InputEComResponseXMLSchemaErrorsSOAP, MessageSubTypeCodeList.Codes.XmlSchemaError);

	public void TestGenerateMessageFromInterchangeCustomsComplaintRequestECM() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEComEmail, MessageTypeCodeList.Codes.ECM, TestingData.InputEComCustomsCompliantRequestMail, 1);

	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsEdec;

	protected override string InterchangeType => MessageTypeCodeList.Codes.ECM;

	protected override (string From, string BodyText)[] FailingDeserializations => new[]
	{
			(MessagingConstants.CustomsDestinationCodes.CustomsEComSoap, FailingDeserialization),
		};

	protected override IInboundMessageCreator GetMessageCreator() => new EComInboundMessageCreator(Logger);

	protected override string GetExpectedMessageText(string parsingResult) => MIMETypeXTParser.ParseSoapEnvelope(parsingResult);

	protected override string UnsupportedSchemaVersion => @"HTTP/1.1 200 OK
Content-Type: multipart/related; boundary=""----=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0""

------=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0
Content-Type: text/xml; charset=UTF-8
Content-Description: e-dec_edecComplaintResponse_20220830_143630_22CHEI000043145845_CHE326684996

<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <edecComplaintResponse schemaVersion=""0.1"" xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1 http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintResponse_v_1_0"">
      <requestorTraderIdentificationNumber>CHE326684996</requestorTraderIdentificationNumber>
      <requestorCorrelationID>cb-1661862989733</requestorCorrelationID>
      <edecComplaintAcceptance>
        <customsDeclarationNumber>22CHEI000043145845</customsDeclarationNumber>
        <acceptanceDate>2022-08-30</acceptanceDate>
        <acceptanceTime>14:36:30</acceptanceTime>
      </edecComplaintAcceptance>
    </edecComplaintResponse>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>

------=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0--
";

	const string FailingDeserialization = @"HTTP/1.1 200 OK
Content-Type: multipart/related; boundary=""----=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0""

------=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0
Content-Type: text/xml; charset=UTF-8
Content-Description: e-dec_edecComplaintResponse_20220830_143630_22CHEI000043145845_CHE326684996

<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Body>
    <edecComplaintResponse schemaVersion=""1.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecComplaintResponse/v1 http://www.ezv.admin.ch/pdf_linker.php?doc=edecComplaintResponse_v_1_0"">
      <requestorTraderIdentificationNumber>CHE326684996</requestorTraderIdentificationNumber>
      <requestorCorrelationID>cb-1661862989733</requestorCorrelationID>
      <edecComplaintAcceptance>
        <wrongTag>
      </edecComplaintAcceptance>
    </edecComplaintResponse>
  </SOAP-ENV:Body>
</SOAP-ENV:Envelope>

------=_Part_f4e2ae5e831dd10309a64a94fbca96445442c11979eefef0--
";
}

using System.IO;
using System.Linq;
using System.Xml.Linq;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecInboundMessageCreatorTest : InboundMessageCreatorAbstractTest
{
	protected override IInboundMessageCreator GetMessageCreator() => new EdecInboundMessageCreator(Logger);

	protected override string ApplicationCode => EDIInterchange.ApplicationCodes.CHCustomsEdec;

	protected override string InterchangeType => MessageTypeCodeList.Codes.Import;

	protected override string GetExpectedMessageText(string parsingResult) => parsingResult;

	protected override (string From, string BodyText)[] FailingDeserializations => new[]
	{
			(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, FailingDeserializationSOAP),
			(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, FailingDeserializationMail),
		};

	public void TestGenerateMessagesFromInterchangeSOAPAcceptanceImport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, MessageTypeCodeList.Codes.Import, TestingData.InputEdecResponseAcceptanceResponseSOAP, 3);

	public void TestGenerateMessagesFromInterchangeSOAPAcceptanceExport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, MessageTypeCodeList.Codes.Export, TestingData.InputEdecResponseAcceptanceResponseSOAP, 3);

	public void TestGenerateMessagesFromInterchangeMailAcceptanceImport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Import, TestingData.InputEdecResponseAcceptanceResponseMail, 3);

	public void TestGenerateMessagesFromInterchangeMailAcceptanceExport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Export, TestingData.InputEdecResponseAcceptanceResponseMail, 3);

	public void TestGenerateMessagesFromInterchangeSOAPRejectionImport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, MessageTypeCodeList.Codes.Import, TestingData.InputEdecEdecResponseRejectionResponseSOAP, 1);

	public void TestGenerateMessagesFromInterchangeSOAPRejectionExport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap, MessageTypeCodeList.Codes.Export, TestingData.InputEdecEdecResponseRejectionResponseSOAP, 1);

	public void TestGenerateMessagesFromInterchangeMailRejectionImport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Import, TestingData.InputEdecEdecResponseRejectionResponseMail, 1);

	public void TestGenerateMessagesFromInterchangeMailRejectionExport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Export, TestingData.InputEdecEdecResponseRejectionResponseMail, 1);

	public void TestGenerateMessagesFromInterchangeMailStatusImport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Import, TestingData.InputEdecEdecResponseStatusResponseMail, 1);

	public void TestGenerateMessagesFromInterchangeMailStatusExport() => TestGenerateMessagesFromInterchange(MessagingConstants.CustomsDestinationCodes.CustomsEdecEmail, MessageTypeCodeList.Codes.Export, TestingData.InputEdecEdecResponseStatusResponseMail, 1);

	public void TestParseSOAPAcceptance()
	{
		var splittedResponses = MIMETypeXTParser.ParseTextHttp(TestingData.InputEdecResponseAcceptanceResponseSOAP);
		AssertEquals(3, splittedResponses.Count());
		CombineAssertions("XML with Envelope", () =>
		{
			var parsingResult = splittedResponses.ElementAt(0);
			AssertResponseParsingResult(parsingResult, "text/xml", "e-dec_Import_edecResponse_0100011210809998_21CHEI000042027074_1_CHE293274655_72", true, false);
			Assert("Starts With Soap Envelope", parsingResult.BodyText.TrimStart('\r', '\n').StartsWith("<?xml"));
			Assert("End With Soap Envelope", parsingResult.BodyText.TrimEnd('\r', '\n').EndsWith("</SOAP-ENV:Envelope>"));
		});

		CombineAssertions("PDF", () =>
		{
			var parsingResult = splittedResponses.ElementAt(1);
			AssertResponseParsingResult(parsingResult, "application/pdf", "e-dec_Import_BS_0100011210809998_21CHEI000042027074_1_CHE293274655_72", false, true);
			AssertEquals("Body Length", 3443, parsingResult.BodyData.Length);
		});
	}

	public void TestParseMailAcceptance()
	{
		using (TextReader reader = new StringReader(TestingData.InputEdecResponseAcceptanceResponseMail))
		{
			var splittedResponses = MIMETypeXTParser.ParseTextMail(reader);
			AssertEquals(3, splittedResponses.Count());
			CombineAssertions("Xml", () =>
			{
				var parsingResult = splittedResponses.ElementAt(0);
				AssertResponseParsingResult(parsingResult, "application/octet-stream", "e-dec_Import_edecResponse_0100211210001004_21CHEI000041234892_2_CHE375081047_9", true, false);
				AssertEquals("Body Length", 1965, parsingResult.BodyText.Length);
			});

			CombineAssertions("PDF", () =>
			{
				var parsingResult = splittedResponses.ElementAt(1);
				AssertResponseParsingResult(parsingResult, "application/pdf", "e-dec_Import_BS_0100211210001004_21CHEI000041234892_2_CHE375081047_9", false, true);
				AssertEquals("Body Length", 3518, parsingResult.BodyData.Length);
			});
		}
	}

	public void TestParseSOAPRejection()
	{
		var splittedResponses = MIMETypeXTParser.ParseTextHttp(TestingData.InputEdecEdecResponseRejectionResponseSOAP);
		AssertEquals(1, splittedResponses.Count());
		CombineAssertions("Xml with Envelope", () =>
		{
			var parsingResult = splittedResponses.ElementAt(0);
			AssertResponseParsingResult(parsingResult, "text/xml", "e-dec_Import_ruleErrors_0100011210809998_202110141136240925_0_CHE293274655_72", true, false);
			Assert("Starts With Soap Envelope", parsingResult.BodyText.TrimStart('\r', '\n').StartsWith("<?xml"));
			Assert("End With Soap Envelope", parsingResult.BodyText.TrimEnd('\r', '\n').EndsWith("</SOAP-ENV:Envelope>"));
		});
	}

	public void TestParseMailRejection()
	{
		using (TextReader reader = new StringReader(TestingData.InputEdecEdecResponseRejectionResponseMail))
		{
			var splittedResponses = MIMETypeXTParser.ParseTextMail(reader);
			AssertEquals(1, splittedResponses.Count());
			CombineAssertions("Xml", () =>
			{
				var parsingResult = splittedResponses.ElementAt(0);
				AssertResponseParsingResult(parsingResult, "application/octet-stream", "e-dec_Import_edecResponse_0100211210001060_21CHEI000042952464_2_CHE375081047_72", true, false);
				AssertEquals("Body Length", 930, parsingResult.BodyText.Length);
			});
		}
	}

	public void TestParseMailStatus()
	{
		using (TextReader reader = new StringReader(TestingData.InputEdecEdecResponseStatusResponseMail))
		{
			var splittedResponses = MIMETypeXTParser.ParseTextMail(reader);
			AssertEquals(1, splittedResponses.Count());
			CombineAssertions("Xml", () =>
			{
				var parsingResult = splittedResponses.ElementAt(0);
				AssertResponseParsingResult(parsingResult, "application/octet-stream", "e-dec_Import_edecResponse_0100211210001056_21CHEI000042952421_1_CHE375081047_72", true, false);
				AssertEquals("Body Length", 999, parsingResult.BodyText.Length);
			});
		}
	}

	public void TestGetMessageTextForPdf()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = TestingData.InputEdecResponseAcceptanceDocOnlyResponse;
		MessageCreator.CreateMessagesForInterchange(interchange);

		var actualXml = XDocument.Parse(interchange.ContainedMessages[0].EM_MessageText);
		var expectedXml = XDocument.Parse(TestingData.ExpectedEdecResponseAcceptanceDocOnlySOAP);
		AssertEquals(true, XNode.DeepEquals(actualXml, expectedXml));
	}

	protected override string UnsupportedSchemaVersion => @"HTTP/1.1 200 OK
Content-Type: multipart/related; boundary=""----=_Part_69ee2336e25e897ec229f30a8609a4140c7addbb8d58e4b1""; type=""text/xml""; start=""<parameters=-7ebab1aa:183d03624f7:-48f5_0@edec.ezv.admin.ch>""
Transfer-Encoding: chunked

------=_Part_69ee2336e25e897ec229f30a8609a4140c7addbb8d58e4b1
Content-Type: text/xml; charset=UTF-8
Content-Description: e-dec_Import_ruleErrors_0100011210809998_202110141136240925_0_CHE293274655_72

<?xml version=""1.0"" encoding=""UTF-8""?>
<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
	<SOAP-ENV:Body>
		<SOAP-ENV:Fault>
			<faultcode>SOAP-ENV:Client</faultcode>
			<faultstring>see in detail</faultstring>
			<detail>
				<goodsDeclarationsResponse schemaVersion=""1.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
					<goodsDeclarationRejection>
						<rejectionDate>2021-10-14</rejectionDate>
						<rejectionTime>11:36:24</rejectionTime>
						<errors>
							<ruleErrors>
								<traderDeclarationNumber>0100011210809998</traderDeclarationNumber>
								<traderReference>MM_TEST_2009_eBegleit</traderReference>
								<declarant>
									<traderIdentificationNumber>CHE293274655</traderIdentificationNumber>
									<declarantNumber>72</declarantNumber>
								</declarant>
								<error>
									<ruleName>R109c</ruleName>
									<checkType>Version Check</checkType>
									<reference>goodsDeclaration</reference>
								</error>
							</ruleErrors>
						</errors>
					</goodsDeclarationRejection>
				</goodsDeclarationsResponse>
			</detail>
		</SOAP-ENV:Fault>
	</SOAP-ENV:Body>
</SOAP-ENV:Envelope>

------=_Part_69ee2336e25e897ec229f30a8609a4140c7addbb8d58e4b1--
";

	const string FailingDeserializationSOAP = @"HTTP/1.1 200 OK
messageType: multipart/related
traceparent: 00-0e14abec452260279cd037862a6cfb3e-c53d05cb2e07e2fe-01
Content-Type: multipart/related; boundary=""----=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d""; type=""text/xml"";
Date: Mon, 17 Oct 2022 10:22:53 GMT

------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
Content-Type: text/xml;

<SOAP-ENV:Body><goodsDeclarationsResponse><wrongTag></wrongTag></goodsDeclarationsResponse></SOAP-ENV:Body>
------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d--
";

	const string FailingDeserializationMail = @"Return-Path: <customs_declaration_unsigned_a@edec.ezv.admin.ch>
Received: from mxgw02.mysisa.ch ([10.210.3.2])
	by mxgw01.mysisa.ch with ESMTPS
	(using TLSv1.2 with cipher ECDHE-RSA-AES256-GCM-SHA384 (256 bits))
	for declareit_as_test_V1@mysisa.ch;
	Tue, 21 Dec 2021 10:53:59 +0100
Received: from mail12.admin.ch (mail12.admin.ch [162.23.32.12])
	by mxgw02.mysisa.ch  with ESMTP id 1BL9rgTR027832-1BL9rgTT027832
	(version=TLSv1.2 cipher=ECDHE-RSA-AES256-GCM-SHA384 bits=256 verify=CAFAIL)
	for <declareit_as_test_V1@mysisa.ch>; Tue, 21 Dec 2021 10:53:42 +0100
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed; d=ezv.admin.ch; h=from
	:reply-to:to:message-id:subject:mime-version:content-type:date;
	 s=dkimkey1; bh=6FY8y5P+1oboqgbu1hlwnbb0JoVzhMRuM4gNK2/Omds=; b=
	m6gS5frvFoDU1c2a1NLFYeae6tK1266c8QODxajHVj7NiOWIEL5BoGKpzsb/7A0F
	YC2JGv5nYlkAo9DnHF+A6g3qSSrjzMp95bo/nf0WvUB6pQsxY0Cy6rNtHssGO3dh
	0d+FSCavCTnraYvFWHMrgV8JP0tCi8tyCEqZcv5rHx0=
From: customs_declaration_unsigned_a@edec.ezv.admin.ch
Reply-To: customs_declaration_response_a@edec.ezv.admin.ch
To: declareit_as_test_V1@mysisa.ch
Message-ID: <282437711.19654.1640080416966@L821000109807A.adr.admin.ch>
Subject: e-dec status message (approval / vmedecesb node 1, vmedecesb node
 2)
MIME-Version: 1.0
Content-Type: multipart/mixed;
	boundary=""----=_Part_19652_488779371.1640080393952""
Date: Tue, 21 Dec 2021 10:53:36 +0100 (CET)
X-TM-AS-GCONF: 00
X-MSH-Id: B4AB0421F43A4F67B56D0FBDB5273D66

------=_Part_19652_488779371.1640080393952
Content-Type: text/plain; charset=""ISO-8859-1""
Content-Transfer-Encoding: 7bit

e-dec status message
------=_Part_19652_488779371.1640080393952
Content-Type: application/octet-stream;
	name=e-dec_Import_edecResponse_0100211210001056_21CHEI000042952421_1_CHE375081047_72.XML
Content-Transfer-Encoding: base64
Content-Disposition: attachment;
	filename=e-dec_Import_edecResponse_0100211210001056_21CHEI000042952421_1_CHE375081047_72.XML

PFNPQVAtRU5WOkJvZHk+PGdvb2RzRGVjbGFyYXRpb25zUmVzcG9uc2U+PHdyb25nVGFnPjwvd3Jv
bmdUYWc+PC9nb29kc0RlY2xhcmF0aW9uc1Jlc3BvbnNlPjwvU09BUC1FTlY6Qm9keT4=
------=_Part_19652_488779371.1640080393952--


";
}

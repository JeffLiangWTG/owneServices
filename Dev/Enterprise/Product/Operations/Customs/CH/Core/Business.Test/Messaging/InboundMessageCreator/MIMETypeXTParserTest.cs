using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class MIMETypeXTParserTest : TestCase
{
	public void TestSoapEmptyMessage() => TestParseTextSoap(string.Empty, Array.Empty<ResponseParsingResult>());

	public void TestSOAPEmptyContent() => TestParseTextSoap(MissingMessage, Array.Empty<ResponseParsingResult>());

	public void TestMailEmptyContent() => TestParseTextMail(MissingMessage, Array.Empty<ResponseParsingResult>());

	public void TestSoapUnknownContentType() => TestParseTextSoap(UnknownMessageType, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "type", Description = null, BodyText = "This is a Message." },
	});

	public void TestSoapHeaderMissingNoLineBreak() => TestParseTextSoap(HeaderMissingWithoutLineBreak, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is implicitly typed plain ASCII text." },
	});

	public void TestSoapHeaderMissingContainingLineBreak() => TestParseTextSoap(HeaderMissingWithLineBreak, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is implicitly typed plain ASCII text.{System.Environment.NewLine}{System.Environment.NewLine}It does NOT end with a linebreak." },
	});

	public void TestSoapContentTypeTextPlain() => TestParseTextSoap(ContentTypeTextPlain, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is explicitly typed plain text.{System.Environment.NewLine}It DOES end with a linebreak.{System.Environment.NewLine}" },
	});

	public void TestSoapContentTypeTextXml() => TestParseTextSoap(ContentTypeTextXml, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "text/xml", Description = "MyFile", BodyText = $"<?xml version=\"1.0\"?>{System.Environment.NewLine}<MyData>Some Data</MyData>{System.Environment.NewLine}" },
	});

	public void TestSoapContentTypeApplicationXml() => TestParseTextSoap(ContentTypeApplicationXml, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/xop+xml", Description = string.Empty, BodyText = $"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">{System.Environment.NewLine}  <SOAP-ENV:Header/><SOAP-ENV:Body/>{System.Environment.NewLine}</SOAP-ENV:Envelope>" },
	});

	public void TestSoapContentTypeApplicationPdf() => TestParseTextSoap(ContentTypeApplicationPdfSoap, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/pdf", Description = "MyFile", BodyData = new byte[] { 68, 105, 101, 115, 32, 105, 115, 116, 32, 101, 105, 110, 32, 84, 101, 115, 116, 46 } },
	});

	public void TestSoapContentTypeApplicationJson() => TestParseTextSoap(ContentTypeApplicationJsonSoap, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/json", Description = null, BodyText = @"{""error_description"":""ClientAuthenticationFailed"",""error"":""invalid_client""}" },
	});

	public void TestSoapContentTypeApplicationJson_ContentTypeLowerCase() => TestParseTextSoap(ContentTypeApplicationJsonSoapLowerCase, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/json", Description = null, BodyText = @"{""error_description"":""ClientAuthenticationFailed"",""error"":""invalid_client""}" },
	});

	public void TestSoapContentTypeMultipartMixedSimple() => TestParseTextSoap(MultipartMixedSimpleSoap, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is implicitly typed plain ASCII text.{System.Environment.NewLine}{System.Environment.NewLine}It does NOT end with a linebreak." },
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is explicitly typed plain text.{System.Environment.NewLine}It DOES end with a linebreak.{System.Environment.NewLine}" },
	});

	public void TestSoapContentTypeMultipartRelatedComplex() => TestParseTextSoap(MultipartRelatedComplexSoap, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/xop+xml", Description = string.Empty, BodyText = $"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">{System.Environment.NewLine}  <SOAP-ENV:Header/><SOAP-ENV:Body/>{System.Environment.NewLine}</SOAP-ENV:Envelope>" },
			new ResponseParsingResult() { Type = "application/pdf", Description = "MyFile", BodyData = new byte[] { 68, 105, 101, 115, 32, 105, 115, 116, 32, 101, 105, 110, 32, 84, 101, 115, 116, 46 } },
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is implicitly typed plain ASCII text.{System.Environment.NewLine}{System.Environment.NewLine}It does NOT end with a linebreak." },
			new ResponseParsingResult() { Type = "text/plain", Description = null, BodyText = $"This is explicitly typed plain text.{System.Environment.NewLine}It DOES end with a linebreak.{System.Environment.NewLine}" },
	});

	public void TestMailContentTypeMultipartMixedSimple() => TestParseTextMail(MulitipartMixedSimpleMail, new ResponseParsingResult[]
	{
			new ResponseParsingResult() { Type = "application/octet-stream", Description = "My_edecResponse_File", BodyText = $"Dies ist ein Test." },
			new ResponseParsingResult() { Type = "application/pdf", Description = "MyFile", BodyData = new byte[] { 68, 105, 101, 115, 32, 105, 115, 116, 32, 101, 105, 110, 32, 84, 101, 115, 116, 46 } },
	});

	void TestParseTextSoap(string input, ResponseParsingResult[] expectedResultList) => TestParseText(input, expectedResultList, MIMETypeXTParser.ParseTextHttp);

	void TestParseTextMail(string input, ResponseParsingResult[] expectedResultList) => TestParseText(input, expectedResultList, MIMETypeXTParser.ParseTextMail);

	void TestParseText(string input, ResponseParsingResult[] expectedResultList, Func<string, IEnumerable<ResponseParsingResult>> parserFunction)
	{
		var resultList = parserFunction(input);

		AssertEquals("Element count", expectedResultList.Length, resultList.Count());

		for (int i = 0; i < expectedResultList.Length; i++)
		{
			var expectedResultItem = expectedResultList[i];
			var resultItem = resultList.ElementAt(i);
			CombineAssertions($"Checking result at index {i}", () =>
			{
				AssertEquals(nameof(expectedResultItem.Type), expectedResultItem.Type, resultItem.Type);
				AssertEquals(nameof(expectedResultItem.Description), expectedResultItem.Description, resultItem.Description);
				if (!resultItem.IsPdf)
				{
					AssertEquals(nameof(expectedResultItem.BodyText), expectedResultItem.BodyText, resultItem.BodyText);
				}
				else
				{
					AssertEquals(nameof(expectedResultItem.BodyData), expectedResultItem.BodyData, resultItem.BodyData);
				}
			});
		}
	}

	public void TestParseSoapEnvelope()
	{
		AssertExceptionThrown<XmlException>("Empty Text", () => MIMETypeXTParser.ParseSoapEnvelope(string.Empty));
		AssertNull("Non-Envelope", MIMETypeXTParser.ParseSoapEnvelope(@"<response>text</response>"));
		AssertEquals("Envelope > Body", "<response>text</response>",
			MIMETypeXTParser.ParseSoapEnvelope(@"
<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
	<SOAP-ENV:Body>
		<response>text</response>
	</SOAP-ENV:Body>
</SOAP-ENV:Envelope>"));

		AssertEquals("Envelope > Body > Fault > detail", "<response>text</response>",
			MIMETypeXTParser.ParseSoapEnvelope(@"
<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
	<SOAP-ENV:Body>
		<SOAP-ENV:Fault>
			<faultcode>SOAP-ENV:Client</faultcode>
			<faultstring>see in detail</faultstring>
			<detail>
				<response>text</response>
			</detail>
		</SOAP-ENV:Fault>
	</SOAP-ENV:Body>
</SOAP-ENV:Envelope>"));
	}

	public void TestCutOffFirstSave_NullString() => AssertEquals("a --This is a test!-- a", MIMETypeXTParser.CutOffFirstSafe("a --This is a test!-- a", null));

	public void TestCutOffFirstSave_EmptyString() => AssertEquals("a --This is a test!-- a", MIMETypeXTParser.CutOffFirstSafe("a --This is a test!-- a", string.Empty));

	public void TestCutOffFirstSave_Front() => AssertEquals("This is a test!", MIMETypeXTParser.CutOffFirstSafe("a --This is a test!", "--"));

	public void TestCutOffFirstSave_Back() => AssertEquals("This is a test!", MIMETypeXTParser.CutOffFirstSafe("This is a test!-- a", "--"));

	public void TestCutOffFirstSave_FrontAndBack() => AssertEquals("This is -- a test!", MIMETypeXTParser.CutOffFirstSafe("a --This is -- a test!-- a", "--"));

	const string HeaderMissingWithoutLineBreak = @"This is implicitly typed plain ASCII text.";

	const string HeaderMissingWithLineBreak = @"This is implicitly typed plain ASCII text.

It does NOT end with a linebreak.";

	const string ContentTypeTextPlain = @"Content-Type: text/plain;

This is explicitly typed plain text.
It DOES end with a linebreak.
";

	const string ContentTypeTextXml = @"Content-Description: MyFile.xml
Content-Type: text/xml; charset=UTF-8
Date: Wed, 28 Dec 2022 11:30:43 GMT

<?xml version=""1.0""?>
<MyData>Some Data</MyData>
";

	const string ContentTypeApplicationXml = @"Content-Type: application/xop+xml; characterset=utf8; type=""text/xml""

<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <SOAP-ENV:Header/><SOAP-ENV:Body/>
</SOAP-ENV:Envelope>";

	const string ContentTypeApplicationPdfSoap = @"Content-Type: application/pdf
Content-Transfer-Encoding: base64
Content-Description: MyFile

RGllcyBpc3QgZWluIFRlc3Qu";

	const string ContentTypeApplicationJsonSoap = @"HTTP/1.1 200 OK
Date: Fri, 25 Nov 2022 14:37:12 GMT
Content-Type: application/json
Content-Length: 2655
Connection: keep-alive

{""error_description"":""ClientAuthenticationFailed"",""error"":""invalid_client""}";

	const string ContentTypeApplicationJsonSoapLowerCase = @"HTTP/1.1 200 OK
Date: Fri, 25 Nov 2022 14:37:12 GMT
content-type: application/json
Content-Length: 2655
Connection: keep-alive

{""error_description"":""ClientAuthenticationFailed"",""error"":""invalid_client""}";

	const string ContentTypeApplicationPdfMail = @"Content-Type: application/pdf; 
	name=MyFile.pdf
Content-Transfer-Encoding: base64

RGllcyBpc3QgZWluIFRlc3Qu";

	const string ContentTypeApplicationOctetStream = @"Content-Type: application/octet-stream; 
	name=MyFile.XML
Content-Transfer-Encoding: base64
Content-Disposition: attachment; 
	filename=My_edecResponse_File.XML

RGllcyBpc3QgZWluIFRlc3Qu";

	const string UnknownMessageType = @"Return-Path: <customs_declaration_unsigned_a@edec.ezv.admin.ch>
Received: from mxgw02.mysisa.ch ([10.210.3.2])
	by mxgw01.mysisa.ch with ESMTPS
	(using TLSv1.2 with cipher ECDHE-RSA-AES256-GCM-SHA384 (256 bits))
	for declareit_as_test_V1@mysisa.ch;
	Tue, 21 Dec 2021 11:16:27 +0100
Received: from mail11.admin.ch (mail11.admin.ch [162.23.32.11])
	by mxgw02.mysisa.ch  with ESMTP id 1BLAGAlO027863-1BLAGAlQ027863
	(version=TLSv1.2 cipher=ECDHE-RSA-AES256-GCM-SHA384 bits=256 verify=CAFAIL)
	for <declareit_as_test_V1@mysisa.ch>; Tue, 21 Dec 2021 11:16:10 +0100
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed; d=ezv.admin.ch; h=from
	:reply-to:to:message-id:subject:mime-version:content-type:date;
	 s=dkimkey1; bh=VajxsemgM5VhuY3u0ku2QGFbbOlWeNq2riSACh3pQN4=; b=
	hNdJcXIJIRP7aqEo/Q0niVTwNf6wzMyYEEF3q2uuKvzJFUQrdUs13yjf0+92Vzho
	QUDRJ2XcoUP5H5Y5uBSVFSdvXiKYICE483nH3P7FlsTHjk43uNKHVGS05zQO/pBV
	psWD4JOFZ7vcfnaEYHOnU9wX8HZHh4lAz6PgBxe1Lns=
From: customs_declaration_unsigned_a@edec.ezv.admin.ch
Reply-To: customs_declaration_response_a@edec.ezv.admin.ch
To: declareit_as_test_V1@mysisa.ch
Message-ID: <137787712.19685.1640081748941@L821000109807A.adr.admin.ch>
Subject: Message rejected by e-dec because of customs decision (approval /
 vmedecesb node 1, vmedecesb node 2)
MIME-Version: 1.0
Content-Type: multipart/mixed; 
	boundary=""----=_Part_19684_2110862184.1640081748925""
Date: Tue, 21 Dec 2021 11:15:48 +0100 (CET)
X-TM-AS-GCONF: 00
X-MSH-Id: D6F40F39E82C4D0493AA225C5DAA2D29

------=_Part_19684_2110862184.1640081748925
Content-Type: type;

This is a Message.
------=_Part_19684_2110862184.1640081748925--";

	const string MissingMessage = @"Return-Path: <customs_declaration_unsigned_a@edec.ezv.admin.ch>
Received: from mxgw02.mysisa.ch ([10.210.3.2])
	by mxgw01.mysisa.ch with ESMTPS
	(using TLSv1.2 with cipher ECDHE-RSA-AES256-GCM-SHA384 (256 bits))
	for declareit_as_test_V1@mysisa.ch;
	Tue, 21 Dec 2021 11:16:27 +0100
Received: from mail11.admin.ch (mail11.admin.ch [162.23.32.11])
	by mxgw02.mysisa.ch  with ESMTP id 1BLAGAlO027863-1BLAGAlQ027863
	(version=TLSv1.2 cipher=ECDHE-RSA-AES256-GCM-SHA384 bits=256 verify=CAFAIL)
	for <declareit_as_test_V1@mysisa.ch>; Tue, 21 Dec 2021 11:16:10 +0100
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed; d=ezv.admin.ch; h=from
	:reply-to:to:message-id:subject:mime-version:content-type:date;
	 s=dkimkey1; bh=VajxsemgM5VhuY3u0ku2QGFbbOlWeNq2riSACh3pQN4=; b=
	hNdJcXIJIRP7aqEo/Q0niVTwNf6wzMyYEEF3q2uuKvzJFUQrdUs13yjf0+92Vzho
	QUDRJ2XcoUP5H5Y5uBSVFSdvXiKYICE483nH3P7FlsTHjk43uNKHVGS05zQO/pBV
	psWD4JOFZ7vcfnaEYHOnU9wX8HZHh4lAz6PgBxe1Lns=
From: customs_declaration_unsigned_a@edec.ezv.admin.ch
Reply-To: customs_declaration_response_a@edec.ezv.admin.ch
To: declareit_as_test_V1@mysisa.ch
Message-ID: <137787712.19685.1640081748941@L821000109807A.adr.admin.ch>
Subject: Message rejected by e-dec because of customs decision (approval /
 vmedecesb node 1, vmedecesb node 2)
MIME-Version: 1.0
Content-Type: multipart/mixed; 
	boundary=""----=_Part_19684_2110862184.1640081748925""
Date: Tue, 21 Dec 2021 11:15:48 +0100 (CET)
X-TM-AS-GCONF: 00
X-MSH-Id: D6F40F39E82C4D0493AA225C5DAA2D29

------=_Part_19684_2110862184.1640081748925
Content-Type: text/xml;

------=_Part_19684_2110862184.1640081748925--";

	string MultipartMixedSimpleSoap => $@"From: John Doe <john.doe@wisetechglobal.com> 
To:  Peter Smith <peter.smith@wisetechglobal.com> 
Subject: Sample message 
MIME-Version: 1.0 
Content-Type: multipart/mixed; boundary=""simple boundary"" 

This is the preamble.It is to be ignored.
--simple boundary
{HeaderMissingWithLineBreak}
--simple boundary
{ContentTypeTextPlain}
--simple boundary--
This is the epilogue. It is also to be ignored.";

	string MultipartRelatedComplexSoap => $@"HTTP/1.1 200 OK
messageType: multipart/related
traceparent: 00-0e14abec452260279cd037862a6cfb3e-c53d05cb2e07e2fe-01
Content-Type: multipart/related; boundary=""----=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d""; type=""text/xml"";
Date: Mon, 17 Oct 2022 10:22:53 GMT

------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
{ContentTypeApplicationXml}
------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
{ContentTypeApplicationPdfSoap}
------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d
{MultipartMixedSimpleSoap}
------=_Part_25dfb9b5894cf5c51e37f8333e1f9bdb77439611981e694d--
";

	string MulitipartMixedSimpleMail => $@"Return-Path: <customs_declaration_unsigned_a@edec.ezv.admin.ch>
Received: from mxgw02.mysisa.ch ([10.210.3.2])
	by mxgw01.mysisa.ch with ESMTPS
	(using TLSv1.2 with cipher ECDHE-RSA-AES256-GCM-SHA384 (256 bits))
	for declareit_as_test_V1@mysisa.ch;
	Tue, 21 Dec 2021 11:16:27 +0100
Received: from mail11.admin.ch (mail11.admin.ch [162.23.32.11])
	by mxgw02.mysisa.ch  with ESMTP id 1BLAGAlO027863-1BLAGAlQ027863
	(version=TLSv1.2 cipher=ECDHE-RSA-AES256-GCM-SHA384 bits=256 verify=CAFAIL)
	for <declareit_as_test_V1@mysisa.ch>; Tue, 21 Dec 2021 11:16:10 +0100
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed; d=ezv.admin.ch; h=from
	:reply-to:to:message-id:subject:mime-version:content-type:date;
	 s=dkimkey1; bh=VajxsemgM5VhuY3u0ku2QGFbbOlWeNq2riSACh3pQN4=; b=
	hNdJcXIJIRP7aqEo/Q0niVTwNf6wzMyYEEF3q2uuKvzJFUQrdUs13yjf0+92Vzho
	QUDRJ2XcoUP5H5Y5uBSVFSdvXiKYICE483nH3P7FlsTHjk43uNKHVGS05zQO/pBV
	psWD4JOFZ7vcfnaEYHOnU9wX8HZHh4lAz6PgBxe1Lns=
From: customs_declaration_unsigned_a@edec.ezv.admin.ch
Reply-To: customs_declaration_response_a@edec.ezv.admin.ch
To: declareit_as_test_V1@mysisa.ch
Message-ID: <137787712.19685.1640081748941@L821000109807A.adr.admin.ch>
Subject: Message rejected by e-dec because of customs decision (approval /
 vmedecesb node 1, vmedecesb node 2)
MIME-Version: 1.0
Content-Type: multipart/mixed; 
	boundary=""----=_Part_19684_2110862184.1640081748925""
Date: Tue, 21 Dec 2021 11:15:48 +0100 (CET)
X-TM-AS-GCONF: 00
X-MSH-Id: D6F40F39E82C4D0493AA225C5DAA2D29

------=_Part_19684_2110862184.1640081748925
{ContentTypeTextPlain}
------=_Part_19684_2110862184.1640081748925
{ContentTypeApplicationOctetStream}
------=_Part_19684_2110862184.1640081748925
{ContentTypeApplicationPdfMail}
------=_Part_19684_2110862184.1640081748925--


";
}

using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class EvvRuleErrorPrettyFormatterTest : TestCase
{
	#region Message Response

	string ResponseText => $@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
<SOAP-ENV:Header>
<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" SOAP-ENV:mustUnderstand=""1"">
<wsu:Timestamp xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" wsu:Id=""Timestamp-75243"">
<wsu:Created>2022-12-29T08:34:45.616Z</wsu:Created>
<wsu:Expires>2022-12-29T08:39:45.616Z</wsu:Expires>
</wsu:Timestamp>
<wsse:BinarySecurityToken xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"" ValueType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3"" wsu:Id=""CertId-6B481D7CCB0E04A21F1672302885617112864"">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</wsse:BinarySecurityToken>
<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""Signature-75244"">
<ds:SignedInfo>
<ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
<ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
<ds:Reference URI=""#Id-58929007"">
<ds:Transforms>
<ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
</ds:Transforms>
<ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
<ds:DigestValue>mbSoKLW67zxXMApnJ9nN+tosJCk=</ds:DigestValue>
</ds:Reference>
<ds:Reference URI=""#Timestamp-75243"">
<ds:Transforms>
<ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
</ds:Transforms>
<ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
<ds:DigestValue>WfOKXEyav31WUPMZucRanSIBX3E=</ds:DigestValue>
</ds:Reference>
</ds:SignedInfo>
<ds:SignatureValue>WMwDx2M5lJlCE7ZrVAaZ2kKRMz569Ih4dmr7JjmhrUCRL704e7kD7AMP3wiLIfQT/AW6gCm4Xywu 928XaGb7rDn3m+yRKGGNy2z8oRUGp+toGnrpy46AhiBIJESLUmaV8tAxiY1G+2JNlc47GvENLnk6 AHVnJqU51hLg0hlNCHmJYBMgNDTbJIbRQp/3rqqGiwt2c8goQX7ex0iicCbdEAn22xolt43cYhRV Q4GpywYF8dPMxA/SoJlAWnrwxpl2ipYklBTZIOkEMuY/14Mf4mb4fZBR6QvUYAG8B96yWmZ6BbYO Z2MyuDd5/Pv9Gb7W1m2nFCVbfwv2RIicxXd/UA==</ds:SignatureValue>
<ds:KeyInfo Id=""KeyId-6B481D7CCB0E04A21F1672302885617112865"">
<wsse:SecurityTokenReference xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" wsu:Id=""STRId-6B481D7CCB0E04A21F1672302885617112866"">
<wsse:Reference URI=""#CertId-6B481D7CCB0E04A21F1672302885617112864"" ValueType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3""/>
</wsse:SecurityTokenReference>
</ds:KeyInfo>
</ds:Signature>
</wsse:Security>
<wsa:To xmlns:wsa=""http://schemas.xmlsoap.org/ws/2004/08/addressing"" SOAP-ENV:mustUnderstand=""0"">http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:To>
</SOAP-ENV:Header>
<SOAP-ENV:Body xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" wsu:Id=""Id-58929007"">
<SOAP-ENV:Fault>
<faultcode>SOAP-ENV:Client</faultcode>
<faultstring>see in detail</faultstring>
<detail>
<receiptRequestResponse xmlns=""http://www.e-dec.ch/xml/schema/edecReceiptResponse/v3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" schemaVersion=""3.0"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecReceiptResponse/v3 http://www.ezv.admin.ch/pdf_linker.php?doc=edecReceiptResponse_v_3_0"">
<requestorTraderIdentificationNumber>CHE326684996</requestorTraderIdentificationNumber>
<receiptRequestRejection>
<rejectionDate>2022-12-29</rejectionDate>
<rejectionTime>09:34:45</rejectionTime>
<errors>
<ruleErrors>
<error>
<ruleName>v1</ruleName>
<checkType>Receipt Check</checkType>
<reference>goodsDeclaration</reference>
<descriptions>
	<description language=""de"">Die Zollanmeldung mit der gesuchten Nummer konnte nicht gefunden werden</description>
	<description language=""fr"">La declaration avec le no recherché n'a pas pu être retrouvé</description>
	<description language=""it"">La dichiarazione con il numero cercato non è stata trovata</description>
</descriptions>
</error>
</ruleErrors>
</errors>
</receiptRequestRejection>
</receiptRequestResponse>
</detail>
</SOAP-ENV:Fault>
</SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

	ZString FormattedText => $@"<h2>EVV Request has been rejected</h2>
<p>Rejection Date/Time: 2022-12-29 09:34:45</p>
<h3>Errors</h3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<thead><tr class=""tableheadings""><th>Code</th><th>Description</th></tr></thead>
<tr><td>v1</td><td>Die Zollanmeldung mit der gesuchten Nummer konnte nicht gefunden werden</td></tr>
</table>";

	ZString MessageIsEmpty => "<h2>Message is empty</h2>";

	#endregion

	public void TestGetFormattedText_SchemaErrorDetails()
	{
		var expectedMessageInterpretation = FormattedText;

		var interpretation = new EvvRuleErrorPrettyFormatter(null).GetFormattedText();
		AssertEquals($"When MessageDetail is empty, EM_MessageInterpretation should return '{MessageIsEmpty}'", MessageIsEmpty, interpretation);

		interpretation = new EvvRuleErrorPrettyFormatter((IEvvRuleErrorsResponseProvider)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}

	public void TestGetFormattedText_Language()
	{
		CombineAssertions(() =>
		{
			var interpretedMessage = new EvvRuleErrorPrettyFormatter((IEvvRuleErrorsResponseProvider)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("default language is german", interpretedMessage.Contains("Die Zollanmeldung mit"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR-CH";
			interpretedMessage = new EvvRuleErrorPrettyFormatter((IEvvRuleErrorsResponseProvider)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("French", interpretedMessage.Contains("La declaration avec"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-CH";
			interpretedMessage = new EvvRuleErrorPrettyFormatter((IEvvRuleErrorsResponseProvider)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("Italian", interpretedMessage.Contains("La dichiarazione con il numero"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE-CH";
			interpretedMessage = new EvvRuleErrorPrettyFormatter((IEvvRuleErrorsResponseProvider)MessageSchemaDecider.GetSpecificMessageAnalyzer(ResponseText).MessageDetail).GetFormattedText();
			Assert("German", interpretedMessage.Contains("Die Zollanmeldung mit"));
		});
	}
}

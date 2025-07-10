using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	class SignManagerTest : TestCaseWithFactory
	{
		public void TestSignDLTMessage_12840()
		{
			var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
			Sign(xmlDocument, "DLT_BeforeSigning.xml", "12840.pfx", "rk34879800!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12840(xmlDocument);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);

			var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
			nsmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
			AssertEquals("jfEkpjk8XRf/YQsBDBSrghJMh9xZj28poJf2aaxvQfY=", xmlDocument.SelectSingleNode("//*[local-name()='DigestValue']", nsmgr).InnerText);
			AssertEquals(@"MNjfmz+gVriUmwaJdTPYFnkQDZRvL+/2SkUgP9WCpOgTB1h9r1H2ti+/T8xafVedq4L6cslwQEHD
eo5pZca3ApYmeBzYEpQpf5oT3suKMjxbzwwN+m16UKigaMsQn5DVlSxKemBS5MJzsuKn3dwOUSj0
VQVgyImRnw6zmnwWqGQNIkDk/7SpnNHZe6X8Yjypi9RDnQBaGQlDlZSNAuEmZ/9rKTXjgJwx+V/i
yKolFDWulmWndheN5HDP63x9vSv5/v1IN/lkfIkrhWKwjYgeRTGpPDYYU6dS3XFhR0RRVrZUkXWt
v6fpXyHxRWoYU35TNwnGxZi27MML+Ovlv5uK0w==".Replace("\r\n", ""), xmlDocument.SelectSingleNode("//*[local-name()='SignatureValue']", nsmgr).InnerText);
		}

		public void TestSignDLTMessage_12410()
		{
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "DLT_BeforeSigning.xml", "12410.pfx", "readykorea1!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12410(xmlDocument);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);
		}

		public void TestSignDOCMessage_12840()
		{
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "DOC_BeforeSigning.xml", "12840.pfx", "rk34879800!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12840(xmlDocument);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);
		}

		public void TestSignDOCMessage_12410()
		{
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "DOC_BeforeSigning.xml", "12410.pfx", "readykorea1!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12410(xmlDocument);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);
		}

		public void TestSignPayload_12840()
		{
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "830_PayloadBeforeSigning.xml", "12840.pfx", "rk34879800!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12840(xmlDocument);
			AssertEquals("ds:Signature", xmlDocument.DocumentElement.LastChild.Name);
		}

		public void TestSignPayload_12410()
		{
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "830_PayloadBeforeSigning.xml", "12410.pfx", "readykorea1!");
			AssertCommonElements(xmlDocument);
			AssertCertificate12410(xmlDocument);
			AssertEquals("ds:Signature", xmlDocument.DocumentElement.LastChild.Name);
		}

		public void TestSignSoapHeaderWithPayload_12840()
		{
			var message = new TestDataSetupHelper(Factory).Create830EDIMessage();
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "830_SoapHeaderWithPayloadBeforeSigning.xml", "12840.pfx", "rk34879800!", message.GetEM_MessageDataReader());
			AssertCommonElements(xmlDocument);
			AssertCertificate12840(xmlDocument);
			AssertXMLContains(@"<ds:Reference URI=""cid:payload-1"">", xmlDocument.OuterXml);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);
		}

		public void TestSignSoapHeaderWithPayload_12410()
		{
			var message = new TestDataSetupHelper(Factory).Create830EDIMessage();
			var xmlDocument = new XmlDocument();
			Sign(xmlDocument, "830_SoapHeaderWithPayloadBeforeSigning.xml", "12410.pfx", "readykorea1!", message.GetEM_MessageDataReader());
			AssertCommonElements(xmlDocument);
			AssertCertificate12410(xmlDocument);
			AssertXMLContains(@"<ds:Reference URI=""cid:payload-1"">", xmlDocument.OuterXml);
			AssertEquals("eb:SyncReply", GetSignature(xmlDocument).PreviousSibling.Name);
		}

		void Sign(XmlDocument xmlDocument, string testFile, string certFile, string certPassword, Stream stream = null)
		{
			var inputMessageTest = new TestFileReader(typeof(SignManagerTest)).GetEmbeddedFileText(CertificateTestHelper.TestFilesPath, testFile);
			using (var textReader = new StringReader(inputMessageTest))
			{
				xmlDocument.Load(textReader);
				var password = new CertificateTestHelper().GetGlbExternalPassword(certFile, certPassword);
				SignManager.Sign(xmlDocument, password, stream);
			}
		}

		XmlNode GetSignature(XmlDocument xmlDocument)
		{
			var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
			nsmgr.AddNamespace("SOAP", @"http://schemas.xmlsoap.org/soap/envelope/");
			nsmgr.AddNamespace("ds", @"http://www.w3.org/2000/09/xmldsig#");
			return xmlDocument.SelectSingleNode("//SOAP:Envelope/SOAP:Header/ds:Signature", nsmgr);
		}

		void AssertCommonElements(XmlDocument xmlDocument)
		{
			AssertXMLContains(@"<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments"" />", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"" />", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:Reference URI="""">", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:Transform Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:Transform Algorithm=""http://www.w3.org/TR/1999/REC-xpath-19991116"">", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:XPath>not(ancestor-or-self::Signature)</ds:XPath>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:Exponent>AQAB</ds:Exponent>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />", xmlDocument.OuterXml);
		}

		void AssertCertificate12410(XmlDocument xmlDocument)
		{
			AssertXMLContains(@"<ds:X509SubjectName>CN=(주)레디코리아(READY KOREA)0020689201404182309126, OU=READY KOREA, OU=WOORI, OU=xUse4Esero, O=yessign, C=kr</ds:X509SubjectName>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509IssuerName>CN=yessignCA Class 2, OU=AccreditedCA, O=yessign, C=kr</ds:X509IssuerName>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509SerialNumber>770687891</ds:X509SerialNumber>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509Certificate>
MIIF6DCCBNCgAwIBAgIELe/DkzANBgkqhkiG9w0BAQsFADBSMQswCQYDVQQGEwJrcjEQMA4GA1UE
CgwHeWVzc2lnbjEVMBMGA1UECwwMQWNjcmVkaXRlZENBMRowGAYDVQQDDBF5ZXNzaWduQ0EgQ2xh
c3MgMjAeFw0yMjA0MjQxNTAwMDBaFw0yMzA0MjYxNDU5NTlaMIGcMQswCQYDVQQGEwJrcjEQMA4G
A1UECgwHeWVzc2lnbjETMBEGA1UECwwKeFVzZTRFc2VybzEOMAwGA1UECwwFV09PUkkxFDASBgNV
BAsMC1JFQURZIEtPUkVBMUAwPgYDVQQDDDco7KO8KeugiOuUlOy9lOumrOyVhChSRUFEWSBLT1JF
QSkwMDIwNjg5MjAxNDA0MTgyMzA5MTI2MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA
mcyLwFHu4hWxEo02r7DKwKCKQBUdY/ZIvD3DA6bwRMN1B4Uce+I3PdRXtKWtlBRHZbKtWYjEy44L
4Nk0ApJOJgg4RtwhM534CwJ/W2V6HBfdg2lSYXC/p5gtEvDpjNEQHjyQ+EcFaIcFgAb08QRQ30w1
JZyiMrymd0yNlqj9cvv7Z4QQrFdcyhqMVNa3zM3zIcJ+j8S73/CSO9cWCSs+P1vVpY0QTzLC1tPi
HmxbOngiKZGI7kElOwPHdpZ+GhcnsHmz8KMsgblx7bZR16u5Q7LsbCF/o4irWwTLpNHgpTQcmCGl
zUxd8RPxABuvPyMCf5fcqqthbQ87iDkAXkkq3wIDAQABo4ICeTCCAnUwgY8GA1UdIwSBhzCBhIAU
79xE0saNwA6jOMB8k8bDQb9Kj/ChaKRmMGQxCzAJBgNVBAYTAktSMQ0wCwYDVQQKDARLSVNBMS4w
LAYDVQQLDCVLb3JlYSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eSBDZW50cmFsMRYwFAYDVQQDDA1L
SVNBIFJvb3RDQSA0ggIQHDAdBgNVHQ4EFgQUZTtgg7swwzWomKMJN9e5d9ciM5gwDgYDVR0PAQH/
BAQDAgbAMIGOBgNVHSABAf8EgYMwgYAwfgYKKoMajJpFAQEGCDBwMEAGCCsGAQUFBwICMDQeMsd0
ACDHeMmdwRyylAAgrgjHNaywyBzG0MXQwRwAILwcrgnVXAAgx3jJncEcx4WyyLLkMCwGCCsGAQUF
BwIBFiBodHRwOi8vd3d3Lnllc3NpZ24ub3Iua3IvY3BzLmh0bTBzBgNVHREEbDBqoGgGCSqDGoya
RAoBAaBbMFkMFCjso7wp66CI65SU7L2U66as7JWEMEEwPwYKKoMajJpECgEBATAxMAsGCWCGSAFl
AwQCAaAiBCD5y2PXaYBvACuvcRAn1D7fX/F228Ij2ch/wOZrtynTwjByBgNVHR8EazBpMGegZaBj
hmFsZGFwOi8vZHMueWVzc2lnbi5vci5rcjozODkvb3U9ZHA1cDk1ODk0LG91PUFjY3JlZGl0ZWRD
QSxvPXllc3NpZ24sYz1rcj9jZXJ0aWZpY2F0ZVJldm9jYXRpb25MaXN0MDgGCCsGAQUFBwEBBCww
KjAoBggrBgEFBQcwAYYcaHR0cDovL29jc3AueWVzc2lnbi5vcmc6NDYxMjANBgkqhkiG9w0BAQsF
AAOCAQEAFLL/QRH7oHZlp5zrajjHkKWCNktafXYHWw7PKHpByM0YxpjeCPuvw0iSwdRIwR92jeKo
tLyLIldpKUCgpyO6r58l7ej9qZv5voxb8PxcqWGMQnBcuNWiaIJVEHuUv4nzyQEmAhtgXcsKzI09
vztR/JIrdjdX8Dxc5tplFWLXRKGkglGRy2UPVJ9NCBYC77kuGrDTA4FTQAcJC6W4Qtwvo41cBWX/
uViI/3q7qFokJh7Fk+tjN36PSeJmhjnQZBOOwhodd5Tbo27ueYN0THO4vtdI6Y7IeVZIS+e8GB/b
TGZDbLiE0v5LGVfgU7Zst3Uu4XkAF/ftgGoIUOymwwBKXw==</ds:X509Certificate>".Replace("\r\n", ""), xmlDocument.OuterXml);
		}

		void AssertCertificate12840(XmlDocument xmlDocument)
		{
			AssertXMLContains(@"<ds:X509SubjectName>CN=(주)레디코리아_0000111591, OU=KTNET, OU=AccreditedCA, O=TradeSign, C=KR</ds:X509SubjectName>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509IssuerName>CN=TradeSignCA3, OU=AccreditedCA, O=TradeSign, C=KR</ds:X509IssuerName>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509SerialNumber>1507875079</ds:X509SerialNumber>", xmlDocument.OuterXml);
			AssertXMLContains(@"<ds:X509Certificate>
MIIFqDCCBJCgAwIBAgIEWeBZBzANBgkqhkiG9w0BAQsFADBPMQswCQYDVQQGEwJLUjESMBAGA1UE
CgwJVHJhZGVTaWduMRUwEwYDVQQLDAxBY2NyZWRpdGVkQ0ExFTATBgNVBAMMDFRyYWRlU2lnbkNB
MzAeFw0yMjAzMTAwODMwNDVaFw0yMzAzMTIwNzMzMzZaMHIxCzAJBgNVBAYTAktSMRIwEAYDVQQK
DAlUcmFkZVNpZ24xFTATBgNVBAsMDEFjY3JlZGl0ZWRDQTEOMAwGA1UECwwFS1RORVQxKDAmBgNV
BAMMHyjso7wp66CI65SU7L2U66as7JWEXzAwMDAxMTE1OTEwggEiMA0GCSqGSIb3DQEBAQUAA4IB
DwAwggEKAoIBAQDK7pm+Ph7jJMszWQJlBHXYQnoKWr1XPC5EsNYCSEXWfVJVtTU7wvCSGMBXuEIc
MAWpBhlw8i85ZVzoxtsQIh1ANiBLO2ABIXReC3fV3CaDj26mZjxsN8cb031iojKJsnpecNYrQdje
dqVuqFe0UD0+9NLLS1rukUsXTSmrefkcVJ5ZNH2dwGCZuGgUeJ4GPkQUgopWsZcWXsYNjc0r0lpG
9PzVl0BLBBYlbN9jn2637COkzqRFEOj0q96UGbpcwIDM6w92aMRLrv8Q6DCu2Mhmh6RjQejRl7Oh
Q9M+MT2w83ULMnz1V9V5ZOTe7cfJ+psprkS2TaLA6FdOAMLcEPn7AgMBAAGjggJnMIICYzCBjwYD
VR0jBIGHMIGEgBS1ByNsV88+ro61MoGfkacg2+jr46FopGYwZDELMAkGA1UEBhMCS1IxDTALBgNV
BAoMBEtJU0ExLjAsBgNVBAsMJUtvcmVhIENlcnRpZmljYXRpb24gQXV0aG9yaXR5IENlbnRyYWwx
FjAUBgNVBAMMDUtJU0EgUm9vdENBIDSCAhAfMB0GA1UdDgQWBBSCQlGTAVCTow+zESp67N34jW09
zTAOBgNVHQ8BAf8EBAMCBsAwegYDVR0gAQH/BHAwbjBsBgkqgxqMmkwBAQkwXzAuBggrBgEFBQcC
AjAiHiDHdAAgx3jJncEcspQAIKz1s9nHeMmdwRwAIMeFssiy5DAtBggrBgEFBQcCARYhaHR0cDov
L3d3dy50cmFkZXNpZ24ubmV0L2Nwcy5odG1sMHMGA1UdEQRsMGqgaAYJKoMajJpECgEBoFswWQwU
KOyjvCnroIjrlJTsvZTrpqzslYQwQTA/BgoqgxqMmkQKAQEBMDEwCwYJYIZIAWUDBAIBoCIEIGiK
UM49Xy1u1OKTTvz6E5eGtbkwFW6eG/THfIUJMl39MGcGA1UdHwRgMF4wXKBaoFiGVmxkYXA6Ly9s
ZGFwLnRyYWRlc2lnbi5uZXQ6Mzg5L2NuPWNybDFkcDE1ODUsb3U9Y3JsZHAzLG91PUFjY3JlZGl0
ZWRDQSxvPVRyYWRlU2lnbixjPUtSMEYGCCsGAQUFBwEBBDowODA2BggrBgEFBQcwAYYqaHR0cDov
L29jc3AudHJhZGVzaWduLm5ldDoxODAwMC9PQ1NQU2VydmVyMA0GCSqGSIb3DQEBCwUAA4IBAQBY
Sd+63x6yNhxGKnnM/wdTwAO9ItKmaBva6pANWilfljwL/k++xZUaZmz9BgLn2RDuV68ea91jusWA
pqz1qa0A14Rka45kuc5Lo7kQ1249YY+pKli/y4c/FvENl1aQVwhPNFAt+SSNxGYes6fGHBjxBXVV
6FrU0vf8SyO6BYuFvIpABBPsvyD5WoHmR3IKf210wQ/q99R3WNCguBqmYOkB/oto3gCFT3bCinW7
mcRC+KY9aHKu0BRG3gUWvEyRZkSzsMNjVmjOb/BfE0nLCTJ+0jPPsUNDB6JF4UDvi7unObHwNb0L
M/1AnWDnX83LeidhcblgDRHZYtqTlYOhsP4X</ds:X509Certificate>".Replace("\r\n", ""), xmlDocument.OuterXml);
		}
	}
}

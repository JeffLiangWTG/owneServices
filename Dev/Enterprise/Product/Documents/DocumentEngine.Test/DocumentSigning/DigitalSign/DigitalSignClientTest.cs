using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class DigitalSignClientTest : TransactionedTestCase
	{
		public void TestWebException()
		{
			var client = new DigitalSignClientForTest();
			client.ThrowOnWeb = true;
			client.ThrowWebException = true;

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			var result = client.Sign(pdfContents);
			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode).ToString();
			var errorMessage = result.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();

			CombineAssertions(() =>
			{
				AssertEquals("WEB_EXCEPTION", errorCode);

				Assert("Should contain exception messages and stack trace: System.Net.WebException: Operation is not valid due to the current state of the object.",
					errorMessage.Contains(@"System.Net.WebException: Operation is not valid due to the current state of the object."));
			});
		}

		public void TestBadResponse()
		{
			var client = new DigitalSignClientForTest();

			client.WSResponse = new JObject();
			client.WSResponse.Add(DigitalSignConstants.Response.ErrorCode, DocumentSigningConstants.BadResponse);
			client.WSResponse.Add(DigitalSignConstants.Response.ErrorMessage, "Incorrect response received from the API. Please verify the endpoint url in the registry: source");
			client.EndpointUrl = "source";

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			var result = client.Sign(pdfContents);
			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode).ToString();
			var errorMessage = result.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();

			CombineAssertions(() =>
			{
				AssertEquals("BAD_RESPONSE", errorCode);

				Assert("Should mention checking the link in the registry. Error message: " + errorMessage,
					errorMessage.Contains("Incorrect response received from the API. Please verify the endpoint url in the registry: source"));
			});
		}

		public void TestNoUnhandled()
		{
			var client = new DigitalSignClientForTest();
			client.ThrowOnWeb = true;

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			var result = client.Sign(pdfContents);
			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode).ToString();
			AssertEquals("UNHANDLED_EXCEPTION", errorCode);
		}

		public void TestDoesNotThrow()
		{
			var client = new DigitalSignClientForTest();

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			client.Response = new JObject();
			var hashSig = Encoding.UTF8.GetBytes(new string('0', 5000));
			var signedDocsInfo = new JObject
			{
				{ DigitalSignConstants.Response.SignedDocumentID, "docID" },
				{ DigitalSignConstants.Response.SignedDocumentAlias, "1" },
				{ DigitalSignConstants.Response.SignedDocumentSignatureHash, Convert.ToBase64String(hashSig) }
			};
			var signedDocs = new JArray();
			signedDocs.Add(signedDocsInfo);
			client.Response.Add(DigitalSignConstants.Response.SignedDocuments, signedDocs);

			var result = client.Sign(pdfContents);
			AssertEquals(client.LastRequestType, 2);
			Assert(client.LastBodyContent.ContainsKey(DigitalSignConstants.Body.CertificateName));
			Assert(client.LastBodyContent.ContainsKey(DigitalSignConstants.Body.RequestDescription));
			Assert(client.LastBodyContent[DigitalSignConstants.Body.RequestDescription].ToString().Contains("Request_"));
			Assert(client.LastBodyContent.ContainsKey(DigitalSignConstants.Body.TotpID));
			Assert(client.LastBodyContent.ContainsKey(DigitalSignConstants.Body.TotpValue));
			Assert(client.LastBodyContent.ContainsKey(DigitalSignConstants.Body.DocumentsArray));

			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode);
			AssertNull(errorCode);
		}

		public void TestCertificateServerTimeOutCode()
		{
			var client = new DigitalSignClientForTest(emptyCertificateContent: true);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			var result = client.Sign(pdfContents);
			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode).ToString();
			var errorMessage = result.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();

			AssertEquals("GET_CERTIFICATE_SERVER_TIMEOUT", errorCode);
			AssertEquals("Server timeout while retrieve the certificate content (https://qscd-dev.digitalsign.pt/totp/getCertificateByTOTPID URL).", errorMessage);
		}

		public void TestInvalidToken()
		{
			var client = new DigitalSignClientForTest();

			client.WSResponse = new JObject();
			client.WSResponse.Add(DigitalSignConstants.Response.ErrorCode, "invalid_token");
			client.WSResponse.Add(DigitalSignConstants.Response.ErrorMessage, "Invalid access token");
			client.EndpointUrl = "source";

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", new byte[] { 1, 2, 3 });

			var result = client.Sign(pdfContents);
			var errorCode = result.GetValue(DigitalSignConstants.Response.ErrorCode).ToString();
			var errorMessage = result.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();

			CombineAssertions(() =>
			{
				AssertEquals("invalid_token", errorCode);
				AssertEquals("Invalid access token", errorMessage);
			});
		}

		public static byte[] CreateTestCertificate()
		{
			var keyPairGenerator = new RsaKeyPairGenerator();
			keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
			var keyPair = keyPairGenerator.GenerateKeyPair();

			var ord = new List<DerObjectIdentifier>();
			ord.Add(X509Name.C);
			ord.Add(X509Name.O);
			ord.Add(X509Name.L);
			ord.Add(X509Name.ST);
			ord.Add(X509Name.E);

			var values = new List<string>();
			values.Add("PT");
			values.Add("Digital Sign Test Certificate");
			values.Add("Lisboa");
			values.Add("Lisboa");
			values.Add("feedback@digitalsign.pt");

			var gen = new X509V1CertificateGenerator();
			gen.SetIssuerDN(new X509Name(ord, values));
			gen.SetSerialNumber(BigInteger.One);
			gen.SetNotBefore(ZDateTime.Now.AddSeconds(-50).ToDateTime());
			gen.SetNotAfter(ZDateTime.Now.AddSeconds(50).ToDateTime());
			gen.SetSubjectDN(new X509Name(ord, values));
			gen.SetPublicKey(keyPair.Public);
			var cert = gen.Generate(new Asn1SignatureFactory("SHA1withRSA", keyPair.Private, new SecureRandom()));
			var encodedCert = cert.GetEncoded();

			return encodedCert;
		}
	}
}

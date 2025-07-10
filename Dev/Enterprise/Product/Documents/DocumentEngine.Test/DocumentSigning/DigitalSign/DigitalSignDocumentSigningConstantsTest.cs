using NUnit.Framework;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class DigitalSignDocumentSigningConstantsTest : TestCase
	{
		public void TestUrlSuffixes()
		{
			AssertEquals(DigitalSignConstants.AssociatedCertificateUrlSuffix, "/totp/getCertificateByTOTPID");
			AssertEquals(DigitalSignConstants.RequestSignUrlSuffix, "/totp/sigCompleteTOTPPolling");
			AssertEquals(DigitalSignConstants.SignUrlSuffix, "/totp/sigFinalize");
			AssertEquals(DigitalSignConstants.SHA256_ID, "2.16.840.1.101.3.4.2.1");
		}

		public void TestRequestTypes()
		{
			AssertEquals(DigitalSignConstants.RequestType.GetAssociatedCertificate, 1);
			AssertEquals(DigitalSignConstants.RequestType.SigningRequest, 2);
			AssertEquals(DigitalSignConstants.RequestType.FinalizeSigning, 3);
		}

		public void TestBody()
		{
			AssertEquals(DigitalSignConstants.Body.DocumentAlias, "docAlias");
			AssertEquals(DigitalSignConstants.Body.HashAlgorithm, "hashAlg");
			AssertEquals(DigitalSignConstants.Body.DocumentHashBase64, "hashToSign_64");
			AssertEquals(DigitalSignConstants.Body.CertificateName, "certAlias");
			AssertEquals(DigitalSignConstants.Body.RequestID, "sigReqID");
			AssertEquals(DigitalSignConstants.Body.RequestDescription, "sigReqDescr");
			AssertEquals(DigitalSignConstants.Body.TotpID, "totpID");
			AssertEquals(DigitalSignConstants.Body.TotpValue, "totpValue");
			AssertEquals(DigitalSignConstants.Body.DocumentsArray, "docsToSign");
		}

		public void TestResponse()
		{
			AssertEquals(DigitalSignConstants.Response.CertificateName, "certAlias");
			AssertEquals(DigitalSignConstants.Response.CertificateContent, "cert_64");
			AssertEquals(DigitalSignConstants.Response.RequestID, "sigReqID");
			AssertEquals(DigitalSignConstants.Response.ErrorCode, "errorCode");
			AssertEquals(DigitalSignConstants.Response.ErrorMessage, "errorMessage");
			AssertEquals(DigitalSignConstants.Response.SignedDocuments, "signedDocsInfo");
			AssertEquals(DigitalSignConstants.Response.SignedDocumentAlias, "docAlias");
			AssertEquals(DigitalSignConstants.Response.SignedDocumentID, "docID");
			AssertEquals(DigitalSignConstants.Response.SignedDocumentSignatureHash, "hashSig");
		}
	}
}

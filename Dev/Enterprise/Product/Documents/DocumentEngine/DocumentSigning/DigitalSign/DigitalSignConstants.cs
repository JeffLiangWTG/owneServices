namespace Enterprise.DocumentEngine.DigitalSignature
{
	public static class DigitalSignConstants
	{
		public const string AssociatedCertificateUrlSuffix = "/totp/getCertificateByTOTPID";
		public const string RequestSignUrlSuffix = "/totp/sigCompleteTOTPPolling";
		public const string SignUrlSuffix = "/totp/sigFinalize";
		public const string SHA256_ID = "2.16.840.1.101.3.4.2.1";
		public const string DGS_InternalErrorPrefix = "DGS-";

		public static class RequestType
		{
			public const int GetAssociatedCertificate = 1;
			public const int SigningRequest = 2;
			public const int FinalizeSigning = 3;
		}

		public static class Body
		{
			public const string DocumentAlias = "docAlias";
			public const string HashAlgorithm = "hashAlg";
			public const string DocumentHashBase64 = "hashToSign_64";
			public const string CertificateName = "certAlias";
			public const string RequestID = "sigReqID";
			public const string RequestDescription = "sigReqDescr";
			public const string TotpID = "totpID";
			public const string TotpValue = "totpValue";
			public const string DocumentsArray = "docsToSign";
		}

		public static class Response
		{
			public const string CertificateName = "certAlias";
			public const string CertificateContent = "cert_64"; 
			public const string RequestID = "sigReqID";
			public const string ErrorCode = "errorCode";
			public const string ErrorMessage = "errorMessage";
			public const string SignedDocuments = "signedDocsInfo";
			public const string SignedDocumentAlias = "docAlias";
			public const string SignedDocumentID = "docID";
			public const string SignedDocumentSignatureHash = "hashSig";
		}
	}
}

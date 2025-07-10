using System.Collections.Generic;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public static class DocumentSigningConstants
	{
		public const string Unhandled = "UNHANDLED_EXCEPTION";
		public const string BadResponse = "BAD_RESPONSE";
		public const string WebException = "WEB_EXCEPTION";
		public const string EMD_InternalErrorPrefix = "RSDS-";
		public const string PlaceholderSizeErrorPrefix = "PLACEHOLDER_ERROR";

		public const string WebCertificateServerTimeOutCode = "GET_CERTIFICATE_SERVER_TIMEOUT";

		[SuppressThreadStaticFieldMessage]
		public static string WebCertificateServerTimeOutMsg = (NoResString)"Server timeout while retrieve the certificate content (https://qscd-dev.digitalsign.pt/totp/getCertificateByTOTPID URL).";

		public static IReadOnlyCollection<string> ErrorCodesToIgnoreForReporting => [BadResponse, WebException, PlaceholderSizeErrorPrefix, WebCertificateServerTimeOutCode];
	}
}

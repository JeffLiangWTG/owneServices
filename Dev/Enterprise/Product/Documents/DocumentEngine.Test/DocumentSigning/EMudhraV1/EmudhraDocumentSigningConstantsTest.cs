using NUnit.Framework;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class EmudhraDocumentSigningConstantsTest : TestCase
	{
		public void TestErrorCodesToIgnoreForReporting()
		{
			AssertContainsExactElementsInExactOrder(new[] { DocumentSigningConstants.BadResponse, DocumentSigningConstants.WebException, DocumentSigningConstants.PlaceholderSizeErrorPrefix, DocumentSigningConstants.WebCertificateServerTimeOutCode, }, DocumentSigningConstants.ErrorCodesToIgnoreForReporting);
		}
	}
}

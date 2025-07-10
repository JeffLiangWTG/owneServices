using System.Security.Cryptography.X509Certificates;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	sealed class CertificateValidationHelperTest : TestCase
	{
		public void TestGetInvalidCertificateChainErrors_ReturnsChainStatusErrors()
		{
			using var resourceRetriever = new EmbeddedResourceRetriever();
			var certificate = resourceRetriever.GetBytes("Certificate_HasExpiredDate.pfx");
			AssertMultilineASCIIEquals(
				"""
				1. The certificate is not valid due to an untrusted root certification.
				2. The certificate has expired. Please create a new certificate.
				""",
				CertificateValidationHelper.GetInvalidCertificateChainErrors(certificate, "1234"));
		}

		public void TestGetInvalidCertificateChainErrors_AllowsToOverrideVerificationFlags()
		{
			using var resourceRetriever = new EmbeddedResourceRetriever();
			var certificate = resourceRetriever.GetBytes("Certificate_HasExpiredDate.pfx");
			AssertEquals(string.Empty, CertificateValidationHelper.GetInvalidCertificateChainErrors(certificate, "1234", policy => policy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority | X509VerificationFlags.IgnoreNotTimeValid));
		}
	}
}

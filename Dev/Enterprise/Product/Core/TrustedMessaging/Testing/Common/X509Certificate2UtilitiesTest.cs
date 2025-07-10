using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using Enterprise.TrustedMessaging.Business;
using NUnit.Framework;

namespace Enterprise.TrustedMessaging.Testing
{
	public class X509Certificate2UtilitiesTest : TestCase
	{
		public void TestTryMakeCertificate()
		{
			var c1 = UserPortalClientConfigurationTest.NewCertificate();
			var c2 = UserPortalClientConfigurationTest.NewCertificate();
			var cert = c1.Export(X509ContentType.Cert);
			var pfx = c2.Export(X509ContentType.Pfx, "12345");

			var c1Reloaded = X509Certificate2Utilities.TryMakeCertificate(cert, null, false);
			var c2Reloaded = X509Certificate2Utilities.TryMakeCertificate(pfx, "12345", true);
			AssertEquals(c1.Thumbprint, c1Reloaded.Thumbprint);
			AssertEquals(c2.Thumbprint, c2Reloaded.Thumbprint);

			var badCert = X509Certificate2Utilities.TryMakeCertificate(new byte[] { 1, 2, 3 }, "12345", true);
			AssertNull(badCert);
#if NETFRAMEWORK
			AssertEquals("Trusted Messaging Error - Cannot find the requested object.\r\n", ErrorReporter.LastKeyReported);
#elif NET
			AssertEquals("Trusted Messaging Error - Cannot find the requested object.", ErrorReporter.LastKeyReported);
#endif
			AssertContains("Cannot find the requested object.", ErrorReporter.LastMessageReported);
			AssertContains("Cannot find the requested object.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();

			badCert = X509Certificate2Utilities.TryMakeCertificate(pfx, "12345xxx", true);
			AssertNull(badCert);
#if NETFRAMEWORK
			AssertEquals("Trusted Messaging Error - The specified network password is not correct.\r\n", ErrorReporter.LastKeyReported);
#elif NET
			AssertEquals("Trusted Messaging Error - The specified network password is not correct.", ErrorReporter.LastKeyReported);
#endif
			AssertContains("The specified network password is not correct.", ErrorReporter.LastMessageReported);
			AssertContains("The specified network password is not correct.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}
	}
}

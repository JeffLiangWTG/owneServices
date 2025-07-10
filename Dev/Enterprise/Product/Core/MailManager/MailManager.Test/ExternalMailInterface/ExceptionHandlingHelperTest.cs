using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using MailKit.Security;
using NUnit.Framework;
using static Enterprise.MailManager.ExternalMailInterface.ExceptionHandlingHelper;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class ExceptionHandlingHelperTest : TestCaseWithFactory
	{
		public void TestReportRootCertificateNotTrustedErrorContainsCertificateDetails()
		{
			var exception = new SslHandshakeException("A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.");

			try
			{
				ThrowFailedToConnectException(exception, exception);
			}
			catch (FailedToConnectException e)
			{
				AssertContains("Server Certificate:", e.Message);
				AssertContains("Root Certificate Authority: ", e.Message);
				AssertContains("Retry message:", e.Message);
			}
			finally
			{
				Assert("Should not report.", ErrorReporter.LastMessageReported.IsNullOrEmpty());
			}
		}

		[ExpectException(typeof(FailedToConnectException))]
		public void TestShouldNotReportRootCertificateNotTrustedError()
		{
			var exception = new SslHandshakeException("An error occurred while attempting to establish an SSL or TLS connection.");

			try
			{
				ThrowFailedToConnectException(exception, exception);
			}
			finally
			{
				AssertEquals("Should not report error.", ErrorReporter.LastMessageReported, "");
			}
		}
	}
}

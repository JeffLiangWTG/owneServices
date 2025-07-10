using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class SecurityCertificateTest : TransactionedTestCase
	{
		public void TestGranted()
		{
			AssertNotNull(SecurityCertificate.Granted);
			AssertEquals(SecurityCertificate.Granted, SecurityCertificate.Granted);
			AssertEquals(true, SecurityCertificate.Granted.IsAllowed);
		}

		public void TestDenied()
		{
			AssertNotNull(SecurityCertificate.Denied);
			AssertEquals(SecurityCertificate.Denied, SecurityCertificate.Denied);
			AssertEquals(false, SecurityCertificate.Denied.IsAllowed);
		}
	}
}

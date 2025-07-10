using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class AutoDenySecurityOverrideTest : TransactionedTestCase
	{
		public void TestPromptForGrantedConfirmation()
		{
			ISecurityOverrideProvider testProvider = new DefaultAccessSecurityProvider();
			AssertEquals(false, testProvider.ShouldPromptForGrantedConfirmation);
		}

		public void TestShouldPromptForGrantedConfirmation()
		{
			ISecurityOverrideProvider testProvider = new DefaultAccessSecurityProvider();
			AssertEquals(false, testProvider.ShouldPromptForGrantedConfirmation);
		}

		public void TestDefaultAccessSecurityProviderPromptForTemporaryAccess()
		{
			ISecurityOverrideProvider testProvider = new DefaultAccessSecurityProvider();
			bool commodity = EnvSecurity.Commodity.IsAllowed;
			bool profitShare = EnvSecurity.ProfitShare.IsAllowed;

			EnvSecurity.Commodity.IsAllowed = true;
			AssertEquals(SecurityCertificate.Granted, testProvider.PromptForTemporaryAccess(EnvSecurity.Commodity));
			EnvSecurity.Commodity.IsAllowed = false;
			AssertEquals(SecurityCertificate.Denied, testProvider.PromptForTemporaryAccess(EnvSecurity.Commodity));

			EnvSecurity.ProfitShare.IsAllowed = true;
			AssertEquals(SecurityCertificate.Granted, testProvider.PromptForTemporaryAccess(EnvSecurity.ProfitShare));
			EnvSecurity.ProfitShare.IsAllowed = false;
			AssertEquals(SecurityCertificate.Denied, testProvider.PromptForTemporaryAccess(EnvSecurity.ProfitShare));

			EnvSecurity.Commodity.IsAllowed = commodity;
			EnvSecurity.ProfitShare.IsAllowed = profitShare;
		}

		public void TestDefaultAccessSecurityProvider()
		{
			ISecurityOverrideProvider testProvider = new DefaultAccessSecurityProvider();
			bool commodity = EnvSecurity.Commodity.IsAllowed;
			bool profitShare = EnvSecurity.ProfitShare.IsAllowed;

			EnvSecurity.Commodity.IsAllowed = true;
			AssertEquals(SecurityCertificate.Granted, testProvider.SecurityCertificates[EnvSecurity.Commodity]);
			EnvSecurity.Commodity.IsAllowed = false;
			AssertEquals(SecurityCertificate.Denied, testProvider.SecurityCertificates[EnvSecurity.Commodity]);

			EnvSecurity.ProfitShare.IsAllowed = true;
			AssertEquals(SecurityCertificate.Granted, testProvider.SecurityCertificates[EnvSecurity.ProfitShare]);
			EnvSecurity.ProfitShare.IsAllowed = false;
			AssertEquals(SecurityCertificate.Denied, testProvider.SecurityCertificates[EnvSecurity.ProfitShare]);

			EnvSecurity.Commodity.IsAllowed = commodity;
			EnvSecurity.ProfitShare.IsAllowed = profitShare;
		}

		SecurityCore EnvSecurity
		{
			get { return EnvProxy.Instance.Security as SecurityCore; }
		}
	}
}

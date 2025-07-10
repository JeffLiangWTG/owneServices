using System.Net;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class ConfiguratorTest : TestCase
	{
		public void TestCredentialsWithProtectedPassword()
		{
			var protectedPwd = ProtectedDataHelper.Protect("proxypwd1");
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", true, "proxy1", 43, "proxyuser1", protectedPwd, false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var webProxy = (WebProxy)Configurator.GetProxy(webConfig1);
			var password = ((NetworkCredential)webProxy.Credentials).Password;
			CombineAssertions(() =>
			{
				AssertNotEquals("proxypwd1", protectedPwd);
				AssertEquals("proxypwd1", password);
			});
		}
	}
}

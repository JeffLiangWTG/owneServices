using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory.Registry;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ADIntegrationModeProviderTest : TransactionedTestCase
	{
		public void TestIsPullPushEnabled()
		{
			var provider = (IADRegistry)new ADRegistryProvider();

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			Assert(provider.IsIntegrationEnabled);

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			Assert(!provider.IsIntegrationEnabled);

			ActiveDirectoryRegistry.Instance.IsSingleSignOnEnabled = true;
			Assert(provider.IsSingleSignOnEnabled);

			ActiveDirectoryRegistry.Instance.IsSingleSignOnEnabled = false;
			Assert(!provider.IsSingleSignOnEnabled);
		}
	}
}

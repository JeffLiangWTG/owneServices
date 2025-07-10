using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class OrganisationsDataRegistryProviderTest : TestCase
	{
		public void TestInstance()
		{
			AssertNotNull(OrganisationsDataRegistryProvider.Instance);
		}
	}
}

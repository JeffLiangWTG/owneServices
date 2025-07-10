using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class TableProviderFactoryTest : TestCase
	{
		public void TestAllProvidersAreValid()
		{
			var tableProviderFactory = new TableProviderFactory();
			foreach (string providerName in tableProviderFactory.TableProviderTypesForTesting.Keys)
			{
				AssertNotNull(tableProviderFactory.GetProvider(providerName));
			}
		}
	}
}

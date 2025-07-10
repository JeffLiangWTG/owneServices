using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsBusinessProviderFactory))]
sealed class TSCustomsNumberViewStmNumsBusinessProviderFactoryTest : TestCaseWithFactory
{
	public void TestDefaultProviderValueResolverKey()
	{
		var factory = new TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest(Factory);
		AssertEquals("EUN", factory.DefaultProviderValueResolverKey_Exposed);
	}

	class TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest : TSCustomsNumberViewStmNumsBusinessProviderFactory
	{
		public TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public string DefaultProviderValueResolverKey_Exposed => DefaultProviderValueResolverKey;
	}
}

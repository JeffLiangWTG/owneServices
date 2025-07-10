using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderFetchStrategy))]
	sealed class AsycudaManifestHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var strategy = new AsycudaManifestHeaderFetchStrategy(header);
			strategy.FetchForLoad();
			AssertEquals(1, Factory.ActiveFetchHintsForTable(AsycudaBill.Schema.TableName));
		}
	}
}

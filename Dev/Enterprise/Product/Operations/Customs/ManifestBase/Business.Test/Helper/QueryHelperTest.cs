using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class QueryHelperTest : TestCaseWithFactory
	{
		public void TestQueryWithFetchOnlyFromLocalCache()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var query = QueryHelper.QueryWithFetchOnlyFromLocalCache(DummyBizoSchema.Z0_VarCharMax, "INVALID", dummy.IsInDatabase);
			AssertEquals(true, query.FetchOnlyFromLocalCache);
			AssertContains("Z0_VarCharMax = 'INVALID'", query.LiteralTextSqlFormatted);
			Factory.Save();
			query = QueryHelper.QueryWithFetchOnlyFromLocalCache(DummyBizoSchema.Z0_VarCharMax, "INVALID", dummy.IsInDatabase);
			AssertEquals(false, query.FetchOnlyFromLocalCache);
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class ZComparisonQueryProviderTest : TestCase
	{
		public void TestGetQuery()
		{
			ZComparisonQueryProvider provider = new ZComparisonQueryProvider(DummyBizoSchema.Z0_Code);
			ZQuery query = provider.GetQuery(SQLComparisonOperator.GreaterThan, "y");
			AssertEquals("Z0_Code > 'y'", query.LiteralTextADO);
		}

		public void TestWithNullColumn()
		{
			ZComparisonQueryProvider provider = new ZComparisonQueryProvider(null);
			ZQuery query = provider.GetQuery(SQLComparisonOperator.GreaterThan, "y");
			AssertEquals("Query is empty", "", query.LiteralTextADO);
		}
	}
}

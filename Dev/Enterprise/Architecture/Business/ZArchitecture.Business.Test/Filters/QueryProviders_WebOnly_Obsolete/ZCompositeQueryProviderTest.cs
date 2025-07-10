using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class ZCompositeQueryProviderTest : TestCase
	{
		public void TestGetQuery()
		{
			ZComparisonQueryProvider provider1 = new ZComparisonQueryProvider(DummyBizoSchema.Z0_Code);
			ZComparisonQueryProvider provider2 = new ZComparisonQueryProvider(DummyBizoSchema.Z0_FK_Code);
			SQLComparisonOperator[] operatorsArray = new SQLComparisonOperator[2];
			operatorsArray[0] = SQLComparisonOperator.Equal;
			operatorsArray[1] = SQLComparisonOperator.Contains;

			ZCompositeQueryProvider compositionProvider = new ZCompositeQueryProvider(operatorsArray, provider1, provider2);
			ZQuery query = compositionProvider.GetQuery(SQLComparisonOperator.Equal, "ABC");
			AssertEquals("Z0_Code = 'ABC' or Z0_FK_Code like '%ABC%'", query.LiteralTextADO);
		}
	}
}

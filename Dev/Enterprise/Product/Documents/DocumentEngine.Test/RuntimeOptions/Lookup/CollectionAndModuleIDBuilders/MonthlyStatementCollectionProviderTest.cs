using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(MonthlyStatementCollectionProvider))]
	sealed class MonthlyStatementCollectionProviderTest : StatementCollectionProviderTest
	{
		public void TestGetAdditionalQuery()
		{
			var additionalQuery = ((MonthlyStatementCollectionProvider)Provider).GetAdditionalQuery();
			Assert(!additionalQuery.IsEmpty);
		}
	}
}

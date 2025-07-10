using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(DailyStatementCollectionProvider))]
	sealed class DailyStatementCollectionProviderTest : StatementCollectionProviderTest
	{
		public void TestGetAdditionalQuery()
		{
			var additionalQuery = ((DailyStatementCollectionProvider)Provider).GetAdditionalQuery();
			Assert(!additionalQuery.IsEmpty);
		}
	}
}

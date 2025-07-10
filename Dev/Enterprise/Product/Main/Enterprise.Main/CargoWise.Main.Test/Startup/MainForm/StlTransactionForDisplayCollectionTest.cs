using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(StlTransactionForDisplayCollection))]
	public class StlTransactionForDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlTransactionForDisplayCollection>
	{
		protected override StlTransactionForDisplayCollection GetCollectionToTest()
		{
			return [];
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlTransactionForDisplay();
		}
	}
}

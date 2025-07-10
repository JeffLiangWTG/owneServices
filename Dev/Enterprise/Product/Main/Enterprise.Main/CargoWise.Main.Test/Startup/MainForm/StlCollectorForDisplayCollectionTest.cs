using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(StlCollectorForDisplayCollection))]
	public class StlCollectorForDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlCollectorForDisplayCollection>
	{
		protected override StlCollectorForDisplayCollection GetCollectionToTest()
		{
			return [];
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlCollectorForDisplay();
		}
	}
}

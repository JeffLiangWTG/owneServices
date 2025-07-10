using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class ManufacturersToProductsProviderTest : TestCaseWithFactory
	{
		public void TestLinks()
		{
			var foreignOperators = new List<ForeignOperator>() { };
			var dataProvider = ManufacturersToProductsProvider.New(foreignOperators);
			AssertNotNull("Identification should NOT be null", dataProvider);
			AssertEquals(0, dataProvider.Count);

			foreignOperators.Add(Factory.New<ForeignOperator>());
			foreignOperators.Add(Factory.New<ForeignOperator>());

			dataProvider = ManufacturersToProductsProvider.New(foreignOperators);
			AssertNotNull("Identification should NOT be null", dataProvider);
			AssertEquals(2, dataProvider.Count);
		}
	}
}

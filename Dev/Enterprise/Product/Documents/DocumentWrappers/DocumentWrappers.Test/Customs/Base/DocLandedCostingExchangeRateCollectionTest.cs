using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocLandedCostingExchangeRateCollection))]
	sealed class DocLandedCostingExchangeRateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocLandedCostingExchangeRateCollection>
	{
		protected override DocLandedCostingExchangeRateCollection GetCollectionToTest()
		{
			return new DocLandedCostingExchangeRateCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocLandedCostingExchangeRate(Factory, "USD", 0.4m);
		}
	}
}

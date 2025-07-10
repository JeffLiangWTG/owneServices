using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExchangeRateToleranceCollection))]
	public class ExchangeRateToleranceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ExchangeRateToleranceCollection>
	{
		public void TestFindValueWithFallback()
		{
			var collection = new ExchangeRateToleranceCollection();
			AssertEquals("Precondition, Empty Collection", 0, collection.Count);

			var itemUsd = collection.AddNew();
			itemUsd.Currency = "USD";
			itemUsd.ExchangeRateTolerancePercentage = 2m;

			var fallBackAud = collection.FindValueWithFallback("AUD");
			AssertEquals("Default Result", ExchangeRateToleranceLookups.AllCurrencyCode, fallBackAud.Currency);
			AssertEquals("Default Result", 0m, fallBackAud.ExchangeRateTolerancePercentage);

			var fallBackUsd = collection.FindValueWithFallback("USD");
			AssertEquals("Best Result", "USD", fallBackUsd.Currency);
			AssertEquals("Best Result", 2m, fallBackUsd.ExchangeRateTolerancePercentage);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ExchangeRateToleranceCollection GetCollectionToTest()
		{
			return new ExchangeRateToleranceCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExchangeRateTolerance();
		}

		#endregion
	}
}

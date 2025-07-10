using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ExchangeRateWrapperCollection))]
	sealed class ExchangeRateWrapperCollectionFromVoyageExRateTest : GenericWrapperCollectionTest<ExchangeRateWrapperCollection>
	{
		public void AddExchangeRates()
		{
			VoyageExRate exchangeRate1 = Factory.NewWithValidTestData<VoyageExRate>();
			VoyageExRate exchangeRate2 = Factory.NewWithValidTestData<VoyageExRate>();

			VoyageExRate[] exchangeRates = new VoyageExRate[] { exchangeRate1, exchangeRate2 };

			ExchangeRateWrapperCollection wrapperCollection = new ExchangeRateWrapperCollection(exchangeRates, Factory);

			AssertCollectionContains(new ExchangeRateWrapperFromVoyageExRate(exchangeRate1, Factory), wrapperCollection);
			AssertCollectionContains(new ExchangeRateWrapperFromVoyageExRate(exchangeRate2, Factory), wrapperCollection);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ExchangeRateWrapperFromVoyageExRate(null, Factory);
		}

		protected override ExchangeRateWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ExchangeRateWrapperCollection(System.Array.Empty<VoyageExRate>(), Factory);
		}
	}
}

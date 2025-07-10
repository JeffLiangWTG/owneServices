using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ExchangeRateWrapperCollection))]
	sealed class ExchangeRateWrapperCollectionFromExchangeRateTest : GenericWrapperCollectionTest<ExchangeRateWrapperCollection>
	{
		public void AddExchangeRates()
		{
			ExchangeRate exchangeRate1 = Factory.New<ExchangeRate>();
			ExchangeRate exchangeRate2 = Factory.New<ExchangeRate>();

			ExchangeRate[] exchangeRates = new ExchangeRate[] { exchangeRate1, exchangeRate2 };

			ExchangeRateWrapperCollection wrapperCollection = new ExchangeRateWrapperCollection(exchangeRates, Factory);

			AssertCollectionContains(new ExchangeRateWrapperFromExchangeRate(exchangeRate1, Factory), wrapperCollection);
			AssertCollectionContains(new ExchangeRateWrapperFromExchangeRate(exchangeRate2, Factory), wrapperCollection);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ExchangeRateWrapperFromExchangeRate(null, Factory);
		}

		protected override ExchangeRateWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ExchangeRateWrapperCollection(System.Array.Empty<ExchangeRate>(), Factory);
		}
	}
}

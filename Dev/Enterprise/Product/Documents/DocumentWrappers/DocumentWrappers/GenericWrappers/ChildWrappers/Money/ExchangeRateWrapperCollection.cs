using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ExchangeRateWrapperCollection : GenericWrapperCollection<ExchangeRateWrapper>
	{
		public ExchangeRateWrapperCollection(ExchangeRate[] exchangeRates, BusinessObjectFactory factory)
			: base(factory)
		{
			if (exchangeRates != null)
			{
				foreach (ExchangeRate exchangeRate in exchangeRates)
				{
					Add(new ExchangeRateWrapperFromExchangeRate(exchangeRate, Factory));
				}
			}
		}

		public ExchangeRateWrapperCollection(VoyageExRate[] exchangeRates, BusinessObjectFactory factory)
			: base(factory)
		{
			if (exchangeRates != null)
			{
				foreach (VoyageExRate exchangeRate in exchangeRates)
				{
					Add(new ExchangeRateWrapperFromVoyageExRate(exchangeRate, Factory));
				}
			}
		}
	}
}

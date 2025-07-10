using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ExchangeRateWrapperFromExchangeRate : ExchangeRateWrapper
	{
		public ExchangeRateWrapperFromExchangeRate(ExchangeRate exchangeRate, BusinessObjectFactory factory)
			: base(null, factory)
		{
			exchangeRateBO = exchangeRate ?? Factory.GetNull<ExchangeRate>();
		}

		readonly ExchangeRate exchangeRateBO;

		protected override CurrencyWrapper GetCurrency()
		{
			return new CurrencyWrapper(exchangeRateBO.RateCurrency, Factory);
		}

		protected override ZDecimal GetBuyRate()
		{
			return exchangeRateBO.JF_BaseRate;
		}

		protected override ZDecimal GetSellRate()
		{
			return exchangeRateBO.JF_SellRate;
		}

		protected override ZDecimal GetSellRateAgent()
		{
			return exchangeRateBO.JF_SellRate;
		}
	}
}

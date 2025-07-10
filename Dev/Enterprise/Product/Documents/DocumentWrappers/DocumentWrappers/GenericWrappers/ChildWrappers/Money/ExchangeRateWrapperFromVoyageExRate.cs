using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ExchangeRateWrapperFromVoyageExRate : ExchangeRateWrapper
	{
		public ExchangeRateWrapperFromVoyageExRate(VoyageExRate voyageExRate, BusinessObjectFactory factory)
			: base(voyageExRate, factory)
		{
			voyageExRateBO = voyageExRate ?? Factory.GetNull<VoyageExRate>();
		}

		readonly VoyageExRate voyageExRateBO;

		protected override CurrencyWrapper GetCurrency()
		{
			return new CurrencyWrapper(voyageExRateBO.ExCurrency, Factory);
		}

		protected override ZDecimal GetBuyRate()
		{
			return ZDecimal.Zero;
		}

		protected override ZDecimal GetSellRate()
		{
			return voyageExRateBO.E8_VoyageExchangeRate;
		}

		protected override ZDecimal GetSellRateAgent()
		{
			return ZDecimal.Zero;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyZExchangeRate : ZExchangeRate
	{
		public DummyZExchangeRate(BusinessObject bizObj, ExchangeRateType exchangeRateType, ZPropertyInfo rateInfo, ZPropertyInfoString currencyInfo)
			: base(bizObj, exchangeRateType, rateInfo, currencyInfo)
		{
		}

		public ZDecimal GetTodaysRate_Exposed(string currency)
		{
			return GetTodaysRate(currency);
		}
	}
}

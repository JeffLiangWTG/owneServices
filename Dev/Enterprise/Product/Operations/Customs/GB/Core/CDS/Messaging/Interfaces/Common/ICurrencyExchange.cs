using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ICurrencyExchange
	{
		ZDecimal RateNumeric { get; }
	}

	class CurrencyExchangeWrapper : ICurrencyExchange
	{
		CurrencyExchangeWrapper(ZDecimal rateNumeric)
		{
			this.rateNumeric = rateNumeric;
		}

		public static CurrencyExchangeWrapper New(ZDecimal exchangeRate)
		{
			return new CurrencyExchangeWrapper(exchangeRate);
		}

		ZDecimal ICurrencyExchange.RateNumeric => rateNumeric;

		readonly ZDecimal rateNumeric;
	}
}

using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IAmountAndCurrency
	{
		ZDecimal Amount { get; }
		ZString Currency { get; }
	}

	public class AmountAndCurrencyWrapper : IAmountAndCurrency
	{
		AmountAndCurrencyWrapper(ZDecimal amount, ZString currency)
		{
			this.amount = amount;
			this.currency = currency;
		}

		public static AmountAndCurrencyWrapper New(ZDecimal amount, ZString currency)
		{
			amount = amount.Round(noOfDecimalPlaces);
			ZDecimal amountWithTrailingZeros = decimal.Parse(amount.ToString("F" + noOfDecimalPlaces));
			return new AmountAndCurrencyWrapper(amountWithTrailingZeros, currency);
		}

		ZDecimal IAmountAndCurrency.Amount => amount;

		ZString IAmountAndCurrency.Currency => currency;

		readonly ZDecimal amount;
		readonly ZString currency;
		static int noOfDecimalPlaces => 2;
	}
}

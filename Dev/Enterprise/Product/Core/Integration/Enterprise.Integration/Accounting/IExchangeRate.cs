using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IExchangeRate
	{
		ZString CurrencyCode { get; }
		ZDecimal Rate { get; }

#if DEBUG
		void SetBuyRate_ForTestOnly(decimal rate);
#endif
	}
}

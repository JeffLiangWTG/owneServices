using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class EXDCurrencyChecker
	{
		public static bool IsValidCurrency(RefCurrency currency)
		{
			if (currency != null && currency.RX_Code.IndexOf('~') == -1)
			{
				return "AUD~CAD~CHF~CNY~DKK~EUR~FJD~GBP~HKD~IDR~EUR~ILS~INR~JPY~KRW~LKR~MYR~NOK~NZD~PGK~PHP~PKR~SBD~SEK~SGD~THB~TWD~USD~ZAR~BRL".IndexOf(currency.RX_Code) > -1;
			}
			else
			{
				return false;
			}
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters
{
	public class CurrencyMatcher
	{
		public CurrencyMatcher(ZString currencyCode, BusinessObjectFactory factory)
		{
			if (!currencyCode.IsEmpty)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
				if (currency != null)
				{
					MatchedPK = currency.PK;
					MatchedCode = currency.RX_Code;
				}
			}
		}
		public readonly ZGuid MatchedPK;
		public readonly ZString MatchedCode;
	}
}

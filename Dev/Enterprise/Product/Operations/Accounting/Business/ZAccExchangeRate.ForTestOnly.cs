#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public partial class ZAccExchangeRate
	{
		public ZDecimal GetTodaysRate_ForTestOnly(ZString currency)
		{
			return GetTodaysRate(currency);
		}

		public ZQuery GetExchangeRateFilter_ForTestOnly(ZString currency, bool includeOrderBy)
		{
			return GetExchangeRateFilter(currency, includeOrderBy);
		}

		public RefExchangeRate GetMostRecentExchangeRate_ForTestOnly(ZString currency)
		{
			return GetMostRecentExchangeRate(currency);
		}
	}
}

#endif

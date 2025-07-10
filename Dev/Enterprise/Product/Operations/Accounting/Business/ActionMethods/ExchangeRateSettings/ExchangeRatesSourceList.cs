using System;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business
{
	public class ExchangeRatesSourceList : ExchangeRatesSourceListBase
	{
		public ExchangeRatesSourceList(ExRateSourceType[] rateSources)
		{
			Clear();
			AddPair(Codes.CurrencyFile, Descriptions.CurrencyFile);
			if (rateSources != null)
			{
				if (Array.Exists(rateSources, c => c == ExRateSourceType.Voyage))
				{
					AddPair(Codes.SailingSchedule, Descriptions.SailingSchedule);
				}

				if (Array.Exists(rateSources, c => c == ExRateSourceType.BillingJob))
				{
					AddPair(Codes.BillingJobExRateRegistry, Descriptions.BillingJobExRateRegistry);
				}
			}
		}
	}
}


using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public class ExchangeRateToleranceLookups : ZLookups
	{
		public ExchangeRateToleranceLookups(ExchangeRateTolerance parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CurrencyList
		{
			get
			{
				if (currencyList == null)
				{
					currencyList = new CodeDescriptionPairList();
					currencyList.AddRange(new RefCurrencyCollection(new BusinessObjectFactory()).Cast<RefCurrency>()
						.Select(currency => new CodeDescriptionPair(currency.Code, currency.RX_Desc))
						.ToList()
					);
				}
				return currencyList;
			}
		}

		CodeDescriptionPairList currencyList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default Currency Code")]
		internal const string AllCurrencyCode = "All Currencies";
	}
}

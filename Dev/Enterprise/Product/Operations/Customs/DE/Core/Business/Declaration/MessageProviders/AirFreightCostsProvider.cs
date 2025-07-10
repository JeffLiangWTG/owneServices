using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class AirFreightCostsProvider : ImportCostsProvider, IAirFreightCosts
	{
		public AirFreightCostsProvider(decimal value, string currency, decimal rate, bool iata, ZDate exchangeRateDate, bool isFixed) : base(value, currency, rate, isFixed)
		{
			CurrencyRateDate = isFixed ? exchangeRateDate.ToNullableDateTime() : null;
			CurrencyRateIATA = iata;
		}

		public bool CurrencyRateIATA { get; }

		public DateTime? CurrencyRateDate { get; }
	}
}

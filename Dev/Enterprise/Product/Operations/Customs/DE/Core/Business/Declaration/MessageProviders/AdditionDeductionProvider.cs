using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class AdditionDeductionProvider : IAdditionDeduction
	{
		public AdditionDeductionProvider(string chargeType, ZDecimal amount, string currency, bool currencyRateIATA, ZDecimal exchangeRate, ZDate exchangeRateDate, bool exchangeRateUserEnterable, ZDecimal percentage)
		{
			Type = chargeType;
			CurrencyRateIATA = currencyRateIATA;
			CurrencyRateDate = exchangeRateUserEnterable ? exchangeRateDate.ToNullableDateTime()?.Date : null;
			Percentage = (chargeType == ImportChargeCodeList.Codes._010 || chargeType == ImportChargeCodeList.Codes._014) && percentage > 0 ? percentage.FormatDecimal(2) : ZDecimal.Zero;
			Value = amount.FormatDecimal(2);
			CurrencyCode = currency;
			CurrencyRateAgreedFlag = exchangeRateUserEnterable;
			CurrencyRate = exchangeRateUserEnterable ? exchangeRate.FormatDecimal(9) : ZDecimal.Zero;
		}

		public string Type { get; }

		public bool CurrencyRateIATA { get; }

		public DateTime? CurrencyRateDate { get; }

		public decimal Percentage { get; }

		public decimal Value { get; }

		public string CurrencyCode { get; }

		public bool CurrencyRateAgreedFlag { get; }

		public decimal CurrencyRate { get; }
	}
}

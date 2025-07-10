using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationMethodData : IValuationMethodData
	{
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal BaseAmount { get; set; }
		public string BaseAmountCurrency { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.ExchangeRate)]
		public decimal ExchangeRate { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal AmountInKRW { get; set; }
		public ChargeAmountKRW[] Deductions { get; set; }
		public ChargeAmountKRW[] Additions { get; set; }

		ZDecimal IValuationMethodData.BaseAmount => BaseAmount;
		ZString IValuationMethodData.BaseAmountCurrency => BaseAmountCurrency;
		ZDecimal IValuationMethodData.ExchangeRate => ExchangeRate;
		ZDecimal IValuationMethodData.AmountInKRW => AmountInKRW;
		IEnumerable<IChargeAmountKRW> IValuationMethodData.Deductions => Deductions;
		IEnumerable<IChargeAmountKRW> IValuationMethodData.Additions => Additions;
	}
}

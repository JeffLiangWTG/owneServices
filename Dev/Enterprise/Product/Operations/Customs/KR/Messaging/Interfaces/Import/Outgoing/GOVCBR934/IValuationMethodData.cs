using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IValuationMethodData
	{
		ZDecimal BaseAmount { get; }
		ZString BaseAmountCurrency { get; }
		ZDecimal ExchangeRate { get; }
		ZDecimal AmountInKRW { get; }
		IEnumerable<IChargeAmountKRW> Deductions { get; }
		IEnumerable<IChargeAmountKRW> Additions { get; }
	}
}

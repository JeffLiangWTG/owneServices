using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ChargeAmountKRW : IChargeAmountKRW
	{
		public string Type { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal Amount { get; set; }

		ZString IChargeAmountKRW.Type => Type;

		ZDecimal IChargeAmountKRW.Amount => Amount;
	}
}

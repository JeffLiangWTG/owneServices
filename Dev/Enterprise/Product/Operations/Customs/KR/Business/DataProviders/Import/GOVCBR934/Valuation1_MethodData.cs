using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Valuation1_MethodData : ValuationMethodData, IValuation1_MethodData
	{
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public decimal IndirectPaymentAmount { get; set; }

		ZDecimal IValuation1_MethodData.IndirectPaymentAmount => IndirectPaymentAmount;
	}
}

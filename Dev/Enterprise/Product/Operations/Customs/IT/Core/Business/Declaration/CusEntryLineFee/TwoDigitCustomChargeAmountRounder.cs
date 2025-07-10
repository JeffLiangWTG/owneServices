using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class TwoDigitCustomChargeAmountRounder : IFeeRounder
{
	public ZDecimal Round(ZDecimal chargeAmount) => FeeHelper.RoundChargeAmountIfNeeded(chargeAmount);
}

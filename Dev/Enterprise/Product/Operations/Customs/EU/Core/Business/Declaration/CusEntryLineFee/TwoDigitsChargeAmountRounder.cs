using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class TwoDigitsChargeAmountRounder : IFeeRounder
	{
		public ZDecimal Round(ZDecimal chargeAmount) => chargeAmount.Round(TwoDecimalPlaces);

		const int TwoDecimalPlaces = 2;
	}
}

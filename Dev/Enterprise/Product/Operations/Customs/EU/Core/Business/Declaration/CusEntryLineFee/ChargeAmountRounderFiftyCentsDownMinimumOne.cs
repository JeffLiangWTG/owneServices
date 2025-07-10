using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ChargeAmountRounderFiftyCentsDownMinimumOne : IFeeRounder
	{
		public ZDecimal Round(ZDecimal chargeAmount)
		{
			switch (chargeAmount)
			{
				case ZDecimal amount when amount == 0:
					return 0;
				case ZDecimal amount when amount < 1:
					return 1;
				case ZDecimal amount when amount == amount.Truncate() + 0.5m:
					return amount.Truncate();
				default:
					return chargeAmount.Round(0);
			}
		}
	}
}

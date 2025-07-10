using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PercentageChargeAmountCalculator : IChargeAmountCalculator
	{
		public PercentageChargeAmountCalculator(IChargeAmountCalculator chargeAmountCalculator)
		{
			BaseCalculator = Argument.NotNull(chargeAmountCalculator, nameof(chargeAmountCalculator));
		}

		IChargeAmountCalculator BaseCalculator { get; }

		ZDecimal IChargeAmountCalculator.Calculate() => BaseCalculator.Calculate() / 100;
	}
}

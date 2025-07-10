using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ChargeAmountCalculator : IChargeAmountCalculator
	{
		public ChargeAmountCalculator(CusEntryLineFee lineFee)
		{
			LineFee = Argument.NotNull(lineFee, nameof(lineFee));
		}

		CusEntryLineFee LineFee { get; }

		ZDecimal IChargeAmountCalculator.Calculate() => LineFee.CF_BaseValue * LineFee.CF_Rate;
	}
}

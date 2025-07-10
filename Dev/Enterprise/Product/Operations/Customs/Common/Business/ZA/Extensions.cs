using CargoWise.Types;

namespace Enterprise.Customs.Common.ZA
{
	public static class Extensions
	{
		public static ZDecimal RoundUsingCustomsValueRule(this ZDecimal originalValue)
		{
			return originalValue.RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(0.51m);
		}

		public static ZDecimal RoundDownIncludingToZeroUsingCustomsValueRule(this ZDecimal originalValue)
		{
			return originalValue.RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(0.51m);
		}
	}
}

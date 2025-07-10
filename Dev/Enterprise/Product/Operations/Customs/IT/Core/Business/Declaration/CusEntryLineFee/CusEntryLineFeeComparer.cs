using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineFeeComparer : IComparer<CusEntryLineFee>
{
	public int Compare(CusEntryLineFee x, CusEntryLineFee y)
	{
		var scoreCalculator = new CusEntryLineFeeScoreOrderCalculator();
		return scoreCalculator.GetScore(x).CompareTo(scoreCalculator.GetScore(y));
	}

	#region Implementation

	class CusEntryLineFeeScoreOrderCalculator
	{
		public ZDecimal GetScore(CusEntryLineFee lineFee)
		{
			var effectiveChargeType = lineFee.CF_ChargeType;
			var offset = 0;

			if (lineFee.IsSanMarinoDuty)
			{
				offset = GetOffset(-2);
			}
			else if (lineFee.IsDuty)
			{
				effectiveChargeType = RemoveFirstChar(effectiveChargeType);
				offset = GetOffset(-1);
			}
			else if (lineFee.IsVat || lineFee.IsVatExemption)
			{
				effectiveChargeType = RemoveFirstChar(effectiveChargeType);
				offset = GetOffset(2);
			}
			return offset + ZDecimal.ParseSafe(effectiveChargeType, 0m);
		}

		ZString RemoveFirstChar(ZString effectiveChargeType) => effectiveChargeType.SubstringSafe(1);

		int GetOffset(int factor) => OffsetAbsoluteValue * factor;

		const int OffsetAbsoluteValue = 10000;

		#endregion

	}
}

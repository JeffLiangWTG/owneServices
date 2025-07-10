using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSChargeTypeLevelCalculator
	{
		public CDSChargeTypeLevelCalculator(ZString chargeType)
		{
			this.chargeType = chargeType;
		}

		readonly ZString chargeType;

		public ZBool IsItemLevel => GetLevelForChargeType() == ChargeTypeLevel.ITEM;

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ChargeTypeLevel GetLevelForChargeType()
		{
			var result = ChargeTypeLevel.ITEM;
			switch (chargeType)
			{
				case CDSAdditionDeductionChargeTypeList.Codes.AK:
				case CDSAdditionDeductionChargeTypeList.Codes.AP:
				case CDSAdditionDeductionChargeTypeList.Codes.AQ:
				case CDSAdditionDeductionChargeTypeList.Codes.AR:
				case CDSAdditionDeductionChargeTypeList.Codes.AS:
				case CDSAdditionDeductionChargeTypeList.Codes.AV:
				case CDSAdditionDeductionChargeTypeList.Codes.AW:
				case CDSAdditionDeductionChargeTypeList.Codes.BA:
				case CDSAdditionDeductionChargeTypeList.Codes.BU:
				case CDSAdditionDeductionChargeTypeList.Codes.BR:
				case CDSAdditionDeductionChargeTypeList.Codes.BS:
					result = ChargeTypeLevel.HEADER;
					break;
			}

			return result;
		}
	}

	public enum ChargeTypeLevel
	{
		HEADER, ITEM
	}
}

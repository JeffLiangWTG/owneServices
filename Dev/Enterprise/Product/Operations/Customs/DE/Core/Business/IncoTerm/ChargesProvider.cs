using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business
{
	public static class ChargesProvider
	{
		public static CustomsChargeCode AdditionCharge => new CustomsChargeCode(ChargeCodeList.Codes.AdditionCharge, ChargeCodeList.Descriptions.AdditionCharge)
		{
			IsStatisticalValueApplicable = true
		};

		public static CustomsChargeCode EUBorderFreight => new CustomsChargeCode(ChargeCodeList.Codes.EUBorderFreight, ChargeCodeList.Descriptions.EUBorderFreight)
		{
			IsStatisticalValueApplicable = true
		};

		public static CustomsChargeCode DeductionCharge => new CustomsChargeCode(ChargeCodeList.Codes.DeductionCharge, ChargeCodeList.Descriptions.DeductionCharge)
		{
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true
		};

		public static CustomsChargeCode EUBorderInsurance => new CustomsChargeCode(ChargeCodeList.Codes.EUBorderInsurance, ChargeCodeList.Descriptions.EUBorderInsurance)
		{
			IsStatisticalValueApplicable = true
		};

		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(ChargeCodeList.Codes.OverseasFreight, ChargeCodeList.Descriptions.OverseasFreight);

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(ChargeCodeList.Codes.OverseasInsurance, ChargeCodeList.Descriptions.OverseasInsurance);
	}
}

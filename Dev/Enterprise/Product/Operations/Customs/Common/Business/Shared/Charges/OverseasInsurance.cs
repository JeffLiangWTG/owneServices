namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,

			IsPercentageApplicable = true
		};
	}
}


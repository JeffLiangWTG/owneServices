namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode Commission => new CustomsChargeCode(CustomsChargeTypeList.Codes.Commission, CustomsChargeTypeList.Descriptions.Commission)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsPercentageApplicable = true,
			IsIncoTermNeutral = true
		};
	}
}


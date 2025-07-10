namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode Discount => new CustomsChargeCode(CustomsChargeTypeList.Codes.Discount, CustomsChargeTypeList.Descriptions.Discount)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsPercentageApplicable = true,
			IsIncoTermNeutral = true
		};
	}
}

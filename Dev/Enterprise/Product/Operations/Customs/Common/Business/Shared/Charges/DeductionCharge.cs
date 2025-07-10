namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode DeductionCharge => new CustomsChargeCode(CustomsChargeTypeList.Codes.DeductionCharge, CustomsChargeTypeList.Descriptions.DeductionCharge)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = true,
			IsIncoTermNeutral = true
		};
	}
}


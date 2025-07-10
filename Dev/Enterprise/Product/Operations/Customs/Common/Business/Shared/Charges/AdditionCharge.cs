namespace Enterprise.Customs.Common
{
	public static partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode AdditionCharge => new CustomsChargeCode(CustomsChargeTypeList.Codes.AdditionCharge, CustomsChargeTypeList.Descriptions.AdditionCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true
		};
	}
}


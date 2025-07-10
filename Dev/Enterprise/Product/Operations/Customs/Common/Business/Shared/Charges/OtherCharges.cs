namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode OtherCharges => new CustomsChargeCode(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeTypeList.Descriptions.OtherCharges)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsIncoTermNeutral = true
		};
	}
}

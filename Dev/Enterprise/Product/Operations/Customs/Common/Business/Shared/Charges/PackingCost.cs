namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode PackingCost => new CustomsChargeCode(CustomsChargeTypeList.Codes.PackingCost, CustomsChargeTypeList.Descriptions.PackingCost)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};
	}
}

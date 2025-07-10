namespace Enterprise.Customs.Common
{
	static partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode ForeignInlandFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.ForeignInlandFreight, CustomsChargeTypeList.Descriptions.ForeignInlandFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};
	}
}

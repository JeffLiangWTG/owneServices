namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode ExWorks => new CustomsChargeCode(CustomsChargeTypeList.Codes.ExWorks, CustomsChargeTypeList.Descriptions.ExWorks)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true
		};
	}
}

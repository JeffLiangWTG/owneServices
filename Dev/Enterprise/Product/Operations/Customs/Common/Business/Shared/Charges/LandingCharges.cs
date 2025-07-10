namespace Enterprise.Customs.Common
{
	partial class CustomsChargeCodeProvider
	{
		public static CustomsChargeCode LandingCharges => new CustomsChargeCode(CustomsChargeTypeList.Codes.LandingCharges, CustomsChargeTypeList.Descriptions.LandingCharges)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			ConsiderIncotermWhenGroupChargeIsAppoorting = true,
			ConsiderIncotermWhenAppoorting = true
		};
	}
}

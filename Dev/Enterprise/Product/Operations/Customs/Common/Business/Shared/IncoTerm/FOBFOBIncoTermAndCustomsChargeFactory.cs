namespace Enterprise.Customs.Common
{
	public class FOBFOBIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true
		};

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;
		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;
	}
}

namespace Enterprise.Customs.Common
{
	public class CIFCIFIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = false
		};

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true
		};

		public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;
		public override CustomsChargeCode GetOverseasInsurance() => OverseasInsurance;
	}
}

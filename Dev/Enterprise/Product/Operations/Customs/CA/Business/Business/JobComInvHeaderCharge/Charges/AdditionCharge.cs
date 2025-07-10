using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode AdditionCharge => new CustomsChargeCode(CAChargeTypeList.Codes.AdditionCharge, CAChargeTypeList.Descriptions.AdditionCharge)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncoTermNeutral = true,
			IsPercentageApplicable = true
		};
	}
}

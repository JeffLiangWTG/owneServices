using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EdificeIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode EdificeOtherCommission => new CustomsChargeCode(AUChargeCodeList.Codes.OtherCommission, AUChargeCodeList.Descriptions.OtherCommission)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = false,
			IsPercentageApplicable = true,
			IsIncoTermNeutral = true
		};
	}
}

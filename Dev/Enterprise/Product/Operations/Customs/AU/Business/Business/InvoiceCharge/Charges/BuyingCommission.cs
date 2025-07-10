using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EdificeIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode EdificeBuyingCommission => new CustomsChargeCode(AUChargeCodeList.Codes.BuyingCommission, AUChargeCodeList.Descriptions.BuyingCommission)
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

using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	partial class IncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode Construction => new CustomsChargeCode(CAChargeTypeList.Codes.Construction, CAChargeTypeList.Descriptions.Construction)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = false,
			IsIncoTermNeutral = true
		};
	}
}

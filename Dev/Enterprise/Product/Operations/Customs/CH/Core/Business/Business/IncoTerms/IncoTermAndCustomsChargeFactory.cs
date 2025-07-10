using Enterprise.Customs.Common;

namespace Enterprise.Customs.CH.Business;

public class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
{
	public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = false,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true
	};

	public override CustomsChargeCode GetOverseasFreight() => OverseasFreight;
}

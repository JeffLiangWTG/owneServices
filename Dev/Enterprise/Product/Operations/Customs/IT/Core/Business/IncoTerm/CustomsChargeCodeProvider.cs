using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business;

public static class CustomsChargeCodeProvider
{
	public static CustomsChargeCode GetNewInsuranceCostsCharge() => new CustomsChargeCode(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCCustomsChargeTypeList.Descriptions.InsuranceCostsCharge)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncoTermNeutral = true,
		IsPercentageApplicable = true,
		ParentTypes = ChargeParentTypes.GroupInvoice
	};
}

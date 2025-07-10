using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business
{
	public static partial class ChargeCodeProvider
	{
		public static CustomsChargeCode StatisticalValue => new CustomsChargeCode(ChargeTypeList.Codes.StatisticalValue, ChargeTypeList.Descriptions.StatisticalValue)
		{
			IsDutiable = false,
			IsVATible = false,
			IsStatisticalValueApplicable = true,
			IsIncludedInITOTIfDeemed = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			IsStatisticalValueApplicableDeemed = true,
			ParentTypes = ChargeParentTypes.InvoiceLine
		};
	}
}

using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public static class ExportChargesProvider
	{
		#region CustomsChargeCodes

		public static CustomsChargeCode OverseasFreight => overseasFreight ?? (overseasFreight = new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasFreight;

		public static CustomsChargeCode OverseasInsurance => overseasInsurance ?? (overseasInsurance = new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsPercentageApplicable = true,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasInsurance;

		#endregion
	}
}

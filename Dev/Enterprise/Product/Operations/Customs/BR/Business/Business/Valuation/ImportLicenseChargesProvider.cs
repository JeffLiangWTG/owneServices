using System;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public static class ImportLicenseChargesProvider
	{
		#region CustomsChargeCodes

		public static CustomsChargeCode FreightInNationalTerritory => freightInNationalTerritory ?? (freightInNationalTerritory = new CustomsChargeCode(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ImportCustomsChargeTypeList.Descriptions.FreightInNationalTerritory)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = false,
			IsStatisticalValueApplicableDeemed = false,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
			IsIncoTermNeutral = false,
		});
		[ThreadStatic]
		static CustomsChargeCode freightInNationalTerritory;

		public static CustomsChargeCode OverseasInsurance => overseasInsurance ?? (overseasInsurance = new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = false,
			DistributeBy = ChargeDistributeByList.Codes.FOB,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasInsurance;

		#endregion

	}
}

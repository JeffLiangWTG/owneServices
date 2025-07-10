using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public static class ExportChargesProvider
	{
		public static CustomsChargeCode AdditionCharge => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.AdditionCharge, IEExportCustomsChargeTypeList.Descriptions.AdditionCharge)
		{
			IsStatisticalValueApplicableDeemed = true,
			IsStatisticalValueApplicable = true,
			DistributeBy = ChargeDistributeByList.Codes.Weight
		};

		public static CustomsChargeCode EUBorderFreight => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.EUBorderFreight, IEExportCustomsChargeTypeList.Descriptions.EUBorderFreight)
		{
			IsStatisticalValueApplicableDeemed = true,
			IsStatisticalValueApplicable = true,
			DistributeBy = ChargeDistributeByList.Codes.Weight
		};

		public static CustomsChargeCode DeductionCharge => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.DeductionCharge, IEExportCustomsChargeTypeList.Descriptions.DeductionCharge)
		{
			IsStatisticalValueApplicableDeemed = true,
			IsIncludedInITOTDeemedForThisCharge = true,
			DistributeBy = ChargeDistributeByList.Codes.Weight
		};

		public static CustomsChargeCode EUBorderInsurance => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.EUBorderInsurance, IEExportCustomsChargeTypeList.Descriptions.EUBorderInsurance)
		{
			IsStatisticalValueApplicableDeemed = true,
			IsStatisticalValueApplicable = true,
			DistributeBy = ChargeDistributeByList.Codes.Value
		};

		public static CustomsChargeCode OverseasFreight => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.OverseasFreight, IEExportCustomsChargeTypeList.Descriptions.OverseasFreight)
		{
			IsStatisticalValueApplicableDeemed = true,
			DistributeBy = ChargeDistributeByList.Codes.Weight
		};

		public static CustomsChargeCode OverseasInsurance => new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.OverseasInsurance, IEExportCustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsStatisticalValueApplicableDeemed = true,
			DistributeBy = ChargeDistributeByList.Codes.Value
		};
	}
}

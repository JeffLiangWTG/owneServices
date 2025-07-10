using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Valuation4_MethodData : ValuationMethodData, IValuation4_MethodData
	{
		[DecimalPlaces(Constants.DecimalPlacesConstants.CostRate)]
		public decimal DeductionGeneralCostPercentage { get; set; }
		public string DeductionGeneralCostPercentageType { get; set; }
		public string DeductionCustomsReferenceNo { get; set; }

		ZDecimal IValuation4_MethodData.DeductionGeneralCostPercentage => DeductionGeneralCostPercentage;
		ZString IValuation4_MethodData.DeductionGeneralCostPercentageType => DeductionGeneralCostPercentageType;
		ZString IValuation4_MethodData.DeductionCustomsReferenceNo => DeductionCustomsReferenceNo;
	}
}

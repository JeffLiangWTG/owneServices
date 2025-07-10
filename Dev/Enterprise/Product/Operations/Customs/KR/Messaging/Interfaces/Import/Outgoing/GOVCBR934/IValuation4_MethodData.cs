using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IValuation4_MethodData : IValuationMethodData
	{
		ZDecimal DeductionGeneralCostPercentage { get; }
		ZString DeductionGeneralCostPercentageType { get; }
		ZString DeductionCustomsReferenceNo { get; }
	}
}

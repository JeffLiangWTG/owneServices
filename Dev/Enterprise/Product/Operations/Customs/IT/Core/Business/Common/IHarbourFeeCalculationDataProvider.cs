using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business;

public interface IHarbourFeeCalculationDataProvider
{
	IHarbourRateProvider HarbourRateProvider { get; }

	IUniversalRateCalcData RateCalculationData { get; }
}

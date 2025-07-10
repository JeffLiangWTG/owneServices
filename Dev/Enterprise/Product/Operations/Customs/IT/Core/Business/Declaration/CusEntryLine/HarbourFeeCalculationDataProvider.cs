using System;
using CargoWise.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class HarbourFeeCalculationDataProvider : IHarbourFeeCalculationDataProvider
{
	public HarbourFeeCalculationDataProvider(CusEntryLine entryLine)
	{
		Argument.NotNull(entryLine, nameof(entryLine));

		lazyHarbourRateProvider = new Lazy<IHarbourRateProvider>(() => new JobComInvoiceLineHarbourRateProvider(entryLine.RandomLine));
		lazyRateCalculationData = new Lazy<IUniversalRateCalcData>(() => new HarbourFeeUniversalRateData(entryLine.EffectiveGrossWeight));
	}

	IHarbourRateProvider IHarbourFeeCalculationDataProvider.HarbourRateProvider => lazyHarbourRateProvider.Value;

	IUniversalRateCalcData IHarbourFeeCalculationDataProvider.RateCalculationData => lazyRateCalculationData.Value;

	readonly Lazy<IHarbourRateProvider> lazyHarbourRateProvider;
	readonly Lazy<IUniversalRateCalcData> lazyRateCalculationData;
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class HarbourFeeCalculator : IExtraFeeCalculator
{
	public HarbourFeeCalculator(IHarbourFeeCalculationDataProvider feeCalculationData)
	{
		Argument.NotNull(feeCalculationData, nameof(feeCalculationData));
		var harbourRateProvider = Argument.NotNull(feeCalculationData.HarbourRateProvider, nameof(feeCalculationData.HarbourRateProvider));

		harbourRate = harbourRateProvider.HarbourRate;
		rateCalculationData = feeCalculationData.RateCalculationData;
	}

	ZString IExtraFeeCalculator.RateCode => harbourRate?.ZXF_PortTaxType ?? ZString.Empty;

	IEnumerable<IDutyCalculationIntermediateResult> IExtraFeeCalculator.CalculateExtraFees()
	{
		if (harbourRate is null)
		{
			return Enumerable.Empty<DutyCalculationIntermediateResult>();
		}

		var participatingFees = RateCalculationVisitor.CalculateParticipatingFees(rateCalculationData, harbourRate.ZXF_RateFormula);
		return participatingFees.IntermediateResults
			.WhereNotNull();
	}

	readonly RefHarbourRate harbourRate;
	readonly IUniversalRateCalcData rateCalculationData;
}

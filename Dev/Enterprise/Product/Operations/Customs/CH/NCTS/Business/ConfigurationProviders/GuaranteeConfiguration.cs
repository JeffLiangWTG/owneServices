using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
{
	protected override int DefaultPercentageForLiabilityAmountCalculationCore => 10;

	protected override bool AllowDefaultLiabilityAmountCore => true;

	protected override decimal DefaultLiabilityAmountCore => 10000m;

	protected override bool UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsCore => false;

	protected override INctsGuaranteePhase5ValidationDecider GetGuaranteeeDeparturePhase5ValidationDecider() => new NctsGuaranteeDepartureValidationDecider();
}

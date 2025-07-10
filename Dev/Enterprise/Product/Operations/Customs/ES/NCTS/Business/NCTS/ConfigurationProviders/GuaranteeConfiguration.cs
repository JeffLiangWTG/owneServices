using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
	{
		protected override bool ApplySecurityToPW_OverrideCore(EU.NCTS.Business.NctsHeader header) => true;

		protected override int DefaultPercentageForLiabilityAmountCalculationCore => 100;

		protected override INctsGuaranteePhase5ValidationDecider GetGuaranteeeDeparturePhase5ValidationDecider() => new NctsGuaranteeDeparturePhase5ValidationDecider();
	}
}

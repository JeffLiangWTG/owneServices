namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
	{
		protected override int DefaultPercentageForLiabilityAmountCalculationCore => 100;
	}
}

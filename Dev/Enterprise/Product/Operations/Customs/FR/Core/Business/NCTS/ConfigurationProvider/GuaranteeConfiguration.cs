namespace Enterprise.Customs.FR.Business.NCTS
{
	public class GuaranteeConfiguration : EU.NCTS.Business.GuaranteeConfiguration
	{
		protected override EU.NCTS.Business.INctsGuaranteePhase5ValidationDecider GetGuaranteeeDeparturePhase5ValidationDecider() => new NctsGuaranteeDeparturePhase5ValidationDecider();
	}
}

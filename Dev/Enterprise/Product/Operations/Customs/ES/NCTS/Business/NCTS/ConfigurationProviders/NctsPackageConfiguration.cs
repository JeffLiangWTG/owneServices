namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsPackageConfiguration : EU.NCTS.Business.NctsPackageConfiguration
	{
		protected override EU.NCTS.Business.INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider()
			=> new NctsPackageDeparturePhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsPackagePhase5ValidationDecider GetArrivalPhase5ValidationDecider()
			=> new NctsPackageArrivalPhase5ValidationDecider();
	}
}

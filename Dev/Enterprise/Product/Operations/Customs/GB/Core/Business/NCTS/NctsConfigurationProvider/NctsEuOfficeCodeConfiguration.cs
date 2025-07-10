namespace Enterprise.Customs.GB.Business
{
	public class NctsEuOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
	{
		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsEuOfficeCodeDeparturePhase5ValidationDecider();
	}
}

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsIEOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
	{
		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsIEOfficeCodeDeparturePhase5ValidationDecider();
	}
}

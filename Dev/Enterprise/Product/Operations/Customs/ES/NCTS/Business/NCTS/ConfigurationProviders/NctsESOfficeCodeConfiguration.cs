namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsESOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
	{
		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsESOfficeCodeDeparturePhase5ValidationDecider();
	}
}

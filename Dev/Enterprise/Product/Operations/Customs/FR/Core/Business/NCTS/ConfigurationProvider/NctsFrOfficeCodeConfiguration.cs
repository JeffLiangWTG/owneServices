namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsFrOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
	{
		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsFrOfficeCodeDeparturePhase5ValidationDecider();
	}
}

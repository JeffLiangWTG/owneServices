namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsItOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
{
	protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsItOfficeCodeDeparturePhase5ValidationDecider();
}

namespace Enterprise.Customs.EU.NCTS.Business;

public class EnRouteIncidentConfiguration
{
	public ICusGoodsLocationValidationDecider GetGoodsLocationValidationDecider() => new ArrivalPhase5CusGoodsLocationValidationDecider();

	public IEnRouteIncidentValidationDecider GetEnRouteIncidentValidationDecider() => GetValidationDeciderCore();

	protected virtual IEnRouteIncidentValidationDecider GetValidationDeciderCore() => new EnRouteIncidentValidationDecider();
}

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface ICusSupplyChainActorReferenceValidationDecider
	{
		bool IsRuleR0840Active { get; }
	}
}

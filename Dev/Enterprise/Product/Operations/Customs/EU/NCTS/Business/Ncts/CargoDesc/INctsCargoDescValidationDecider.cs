namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsCargoDescValidationDecider
	{
		bool IsRuleNR0058Active { get; }

		bool IsRuleNR0059Active { get; }

		bool IsRuleNR0060Active { get; }
	}
}

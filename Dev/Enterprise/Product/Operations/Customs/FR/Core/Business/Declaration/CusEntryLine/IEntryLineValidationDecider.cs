namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IEntryLineValidationDecider : EU.Business.Declaration.IEntryLineValidationDecider
	{
		bool IsRuleNAT_175Active { get; }

		bool IsRuleNAT_184Active { get; }

		bool IsRuleNAT_188Active { get; }
	}
}

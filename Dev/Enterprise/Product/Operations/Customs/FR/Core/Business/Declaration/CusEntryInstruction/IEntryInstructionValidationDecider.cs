namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IEntryInstructionValidationDecider : EU.Business.Declaration.IEntryInstructionValidationDecider
	{
		bool IsRuleR0012Active { get; }
		bool IsRuleC0002Active { get; }
		bool IsRuleC0627Active { get; }
		bool IsRuleC0810_N01Active { get; }
		bool IsRuleC0834_N02Active { get; }
		bool IsRuleNAT_004BisActive { get; }
		bool IsRuleNAT_130BisActive { get; }
		bool IsRuleR0933_N03Active { get; }
		bool IsRuleNAT_030Active { get; }
	}
}

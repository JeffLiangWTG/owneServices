namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IEntryHeaderValidationDecider : EU.Business.Declaration.IEntryHeaderValidationDecider
	{
		bool IsRuleNAT_174Active { get; }
		bool IsRuleNAT_177Active { get; }
		bool IsRuleNAT_178Active { get; }
		bool IsRuleNAT_179Active { get; }
		bool IsRuleNAT_185Active { get; }

		bool IsRuleNAT_189Active { get; }
	}
}

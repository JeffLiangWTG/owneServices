namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IDeclarationValidationDecider
	{
		bool IsRuleC0002Active { get; }

		bool IsRuleC0211Active { get; }

		bool IsRuleC0623Active { get; }

		bool IsRuleC0646Active { get; }

		bool IsRuleC0729Active { get; }

		bool IsRuleC0738Active { get; }

		bool IsRuleC0841Active { get; }

		bool IsRuleC0843Active { get; }
	}
}

namespace Enterprise.Customs.EU.Business.Declaration
{
	sealed class UCC6ImportDeclarationValidationDecider : IDeclarationValidationDecider
	{
		public bool IsRuleC0002Active => true;

		public bool IsRuleC0211Active => false;

		public bool IsRuleC0623Active => true;

		public bool IsRuleC0646Active => true;

		public bool IsRuleC0729Active => true;

		public bool IsRuleC0738Active => true;

		public bool IsRuleC0841Active => false;

		public bool IsRuleC0843Active => false;
	}
}

using CargoWise.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	sealed class UCC6ImportDeclarationValidationDecider : IDeclarationValidationDecider
	{
		public UCC6ImportDeclarationValidationDecider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public bool IsRuleC0002Active => true;

		public bool IsRuleC0211Active => false;

		public bool IsRuleC0623Active => !declaration.HasSimplifiedEntry;

		public bool IsRuleC0646Active => true;

		public bool IsRuleC0729Active => false;

		public bool IsRuleC0738Active => false;

		public bool IsRuleC0810_N01Active => !declaration.HasSimplifiedEntry;

		public bool IsRuleC0841Active => false;

		public bool IsRuleC0843Active => false;

		public bool IsRuleNAT_020Active => true;

		public bool IsRuleNAT_021Active => true;

		public bool IsRuleNAT_130BisActive => true;

		public bool IsRuleNat_145BisActive => true;

		public bool IsRuleNAT_041QuinquiesActive => true;
	}
}

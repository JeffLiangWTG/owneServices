namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class UCC6ImportEntryHeaderValidationDecider : IEntryHeaderValidationDecider
	{
		public UCC6ImportEntryHeaderValidationDecider()
		{
		}

		public bool IsRuleNAT_174Active => true;

		public bool IsRuleNAT_177Active => true;

		public bool IsRuleNAT_178Active => true;

		public bool IsRuleNAT_179Active => true;

		public bool IsRuleNAT_185Active => true;

		public bool IsRuleNAT_189Active => true;
	}
}

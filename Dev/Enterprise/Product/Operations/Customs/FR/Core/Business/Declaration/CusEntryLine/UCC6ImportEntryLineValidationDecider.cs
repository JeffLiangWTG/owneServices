namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class UCC6ImportEntryLineValidationDecider : IEntryLineValidationDecider
	{
		public UCC6ImportEntryLineValidationDecider()
		{
		}

		public bool IsRuleNAT_175Active => true;

		public bool IsRuleNAT_184Active => true;

		public bool IsRuleNAT_188Active => true;
	}
}

namespace Enterprise.Customs.EU.Business.Declaration
{
	public sealed class UCC6ImportInvoiceHeaderValidationDecider : IInvoiceHeaderValidationDecider
	{
		public bool IsRuleC0002Active => true;

		public bool IsRuleC0624Active => true;

		public bool IsRuleC0627Active => true;

		public bool IsRuleC0729Active => true;

		public bool IsRuleC0728Active => true;

		public bool IsRuleC0738Active => true;

		public bool IsRuleR0012Active => true;
	}
}

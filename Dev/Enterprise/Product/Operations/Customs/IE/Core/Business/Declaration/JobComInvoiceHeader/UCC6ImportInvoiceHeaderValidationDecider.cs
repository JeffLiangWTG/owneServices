using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public sealed class UCC6ImportInvoiceHeaderValidationDecider : EU.Business.Declaration.IInvoiceHeaderValidationDecider, IRuleCD8051ForJZ_ValuationCodeDecider
	{
		public bool IsRuleC0002Active => true;

		public bool IsRuleC0624Active => true;

		public bool IsRuleC0627Active => true;

		public bool IsRuleC0729Active => true;

		public bool IsRuleC0728Active => true;

		public bool IsRuleC0738Active => true;

		public bool IsRuleR0012Active => true;

		bool IRuleCD8051ForJZ_ValuationCodeDecider.IsActive(JobComInvoiceHeader header) => true;
	}
}

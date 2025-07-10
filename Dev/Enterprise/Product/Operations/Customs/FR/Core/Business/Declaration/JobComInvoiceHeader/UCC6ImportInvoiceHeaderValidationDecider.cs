using CargoWise.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	sealed class UCC6ImportInvoiceHeaderValidationDecider : IInvoiceHeaderValidationDecider
	{
		public UCC6ImportInvoiceHeaderValidationDecider(JobComInvoiceHeader invoiceHeader)
		{
			this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}

		readonly JobComInvoiceHeader invoiceHeader;

		public bool IsRuleC0002Active => false;

		public bool IsRuleC0624Active => !(invoiceHeader.JobDeclaration?.HasSimplifiedEntry ?? false);

		public bool IsRuleC0627Active => !(invoiceHeader.JobDeclaration?.HasSimplifiedEntry ?? false);

		public bool IsRuleC0729Active => false;

		public bool IsRuleC0728Active => true;

		public bool IsRuleC0738Active => false;

		public bool IsRuleR0012Active => false;

		public bool IsRuleTNAT_078Active => true;

		public bool IsRuleNAT_240Active => true;

		public bool IsRuleNAT_154Active => true;

		public bool IsRuleNAT_237Active => true;
	}
}

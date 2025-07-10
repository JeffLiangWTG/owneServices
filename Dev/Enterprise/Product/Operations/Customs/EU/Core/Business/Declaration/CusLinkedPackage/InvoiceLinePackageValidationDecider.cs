namespace Enterprise.Customs.EU.Business.Declaration;

public sealed class InvoiceLinePackageValidationDecider : IInvoiceLinePackageValidationDecider
{
		public bool IsRuleR0219Active => true;

		public bool IsRuleR0220Active => true;

		public bool IsRuleR0364Active => true;
}


namespace Enterprise.Customs.EU.Business.Declaration;

public interface IInvoiceLinePackageValidationDecider
{
	bool IsRuleR0219Active { get; }

	bool IsRuleR0220Active { get; }

	bool IsRuleR0364Active { get; }
}


namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IInvoiceHeaderValidationDecider
	{
		bool IsRuleR0012Active { get; }

		bool IsRuleC0002Active { get; }

		bool IsRuleC0624Active { get; }

		bool IsRuleC0627Active { get; }

		bool IsRuleC0729Active { get; }

		bool IsRuleC0728Active { get; }

		bool IsRuleC0738Active { get; }
	}
}

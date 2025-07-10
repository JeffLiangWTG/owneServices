namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IInvoiceHeaderValidationDecider : EU.Business.Declaration.IInvoiceHeaderValidationDecider
	{
		bool IsRuleTNAT_078Active { get; }

		bool IsRuleNAT_240Active { get; }

		bool IsRuleNAT_154Active { get; }

		bool IsRuleNAT_237Active { get; }
	}
}

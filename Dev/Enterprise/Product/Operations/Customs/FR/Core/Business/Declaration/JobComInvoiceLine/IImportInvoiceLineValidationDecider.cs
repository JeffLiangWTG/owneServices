namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IImportInvoiceLineValidationDecider : EU.Business.Declaration.IImportInvoiceLineValidationDecider
	{
		bool IsRuleC0699_N01Active { get; }

		bool IsRuleC0699_N02Active { get; }

		bool IsRuleC0710_N01Active { get; }

		bool IsRuleC0834_N02Active { get; }

		bool IsRuleNAT_105Active { get; }

		bool IsRuleNAT_235Active { get; }

		bool IsRuleNAT_240Active { get; }

		bool IsRuleNAT_254Active { get; }

		bool ShouldValidateCPCAgainstEntryInstruction { get; }

		bool IsRuleNAT_030Active { get; }

		bool IsRuleNAT_154Active { get; }

		bool IsRuleNAT_237Active { get; }
	}
}

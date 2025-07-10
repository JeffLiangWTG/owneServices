using CargoWise.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	sealed class UCC6ImportInvoiceLineValidationDecider : IImportInvoiceLineValidationDecider
	{
		readonly JobComInvoiceLine invoiceLine;

		public UCC6ImportInvoiceLineValidationDecider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public bool ShouldValidateCPCAgainstEntryInstruction => true;

		public bool IsRuleC0834_N02Active => true;

		public bool IsRuleC0002Active => true;

		public bool IsRuleC0627Active => !(invoiceLine.EntryInstruction?.IsSimplified ?? false);

		public bool IsRuleC0820ActiveForJI_SupplementaryCode1 => true;

		public bool IsRuleC0820ActiveForJI_Tariff => true;

		public bool IsRuleCD0111Active => !(IsRuleNAT_240Active && invoiceLine.HasFreeGoods);

		public bool IsRuleCD5151ActiveForZG_CountryOfSupply => false;

		public bool IsRuleCD5161ActiveForJI_CountryOfOrigin => false;

		public bool IsRuleCD9102Active => true;

		public bool IsRuleC0699_N01Active => true;

		public bool IsRuleC0699_N02Active => true;

		public bool IsRuleC0710ActiveForJI_LinePrice => !(IsRuleNAT_240Active && invoiceLine.HasFreeGoods);

		public bool IsRuleC0710ActiveForJI_CountryOfOrigin => false;

		public bool IsRuleC0710ActiveForZG_CountryOfSupply => false;

		public bool IsRuleC0710_N01Active => true;

		public bool IsRuleC0919Active => false;

		public bool IsRuleR0012Active => true;

		public bool IsRuleR0222Active => false;

		public bool IsRuleR0224Active => true;

		public bool IsRuleC0624Active => !(invoiceLine.EntryInstruction?.IsSimplified ?? false);

		public bool IsRuleC0936Active => true;

		public bool IsRuleC0728Active => true;

		public bool AllowMultipleRequestedProcedure => true;

		public bool IsRuleNAT_105Active => true;

		public bool IsRuleNAT_235Active => true;

		public bool IsRuleNAT_240Active => true;

		public bool IsRuleNAT_254Active => true;

		public bool IsRuleR0223Active => true;

		public bool IsRuleNAT_030Active => true;

		public bool IsRuleNAT_154Active => true;

		public bool IsRuleNAT_237Active => true;
	}
}

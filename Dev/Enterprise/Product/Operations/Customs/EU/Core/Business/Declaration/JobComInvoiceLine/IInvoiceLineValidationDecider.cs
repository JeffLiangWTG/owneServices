namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IInvoiceLineValidationDecider
	{
		bool IsRuleR0222Active { get; }

		bool IsRuleR0223Active { get; }

		bool IsRuleR0224Active { get; }
	}

	public interface IImportInvoiceLineValidationDecider : IInvoiceLineValidationDecider
	{
		bool IsRuleC0002Active { get; }

		bool IsRuleC0627Active { get; }

		bool IsRuleC0820ActiveForJI_SupplementaryCode1 { get; }

		bool IsRuleC0820ActiveForJI_Tariff { get; }

		bool IsRuleCD0111Active { get; }

		bool IsRuleCD5151ActiveForZG_CountryOfSupply { get; }

		bool IsRuleCD5161ActiveForJI_CountryOfOrigin { get; }

		bool IsRuleCD9102Active { get; }

		bool IsRuleC0710ActiveForJI_LinePrice { get; }

		bool IsRuleC0710ActiveForJI_CountryOfOrigin { get; }

		bool IsRuleC0710ActiveForZG_CountryOfSupply { get; }

		bool IsRuleC0919Active { get; }

		bool IsRuleR0012Active { get; }

		bool IsRuleC0624Active { get; }

		bool IsRuleC0936Active { get; }

		bool IsRuleC0728Active { get; }

		bool AllowMultipleRequestedProcedure { get; }
	}

	public interface IExportInvoiceLineValidationDecider : IInvoiceLineValidationDecider
	{
	}
}

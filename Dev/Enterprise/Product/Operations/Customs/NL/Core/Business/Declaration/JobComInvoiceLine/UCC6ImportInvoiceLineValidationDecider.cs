using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration
{
	public sealed class UCC6ImportInvoiceLineValidationDecider : IImportInvoiceLineValidationDecider
	{
		public bool IsRuleC0002Active => true;

		public bool IsRuleC0627Active => true;

		public bool IsRuleC0820ActiveForJI_SupplementaryCode1 => true;

		public bool IsRuleC0820ActiveForJI_Tariff => true;

		public bool IsRuleCD0111Active => true;

		public bool IsRuleCD5151ActiveForZG_CountryOfSupply => false;

		public bool IsRuleCD5161ActiveForJI_CountryOfOrigin => false;

		public bool IsRuleCD9102Active => true;

		public bool IsRuleC0710ActiveForJI_LinePrice => true;

		public bool IsRuleC0710ActiveForJI_CountryOfOrigin => true;

		public bool IsRuleC0710ActiveForZG_CountryOfSupply => true;

		public bool IsRuleC0919Active => true;

		public bool IsRuleR0012Active => true;

		public bool IsRuleR0224Active => true;

		public bool IsRuleC0624Active => true;

		public bool IsRuleC0936Active => true;

		public bool IsRuleC0728Active => true;

		public bool AllowMultipleRequestedProcedure => true;

		public bool IsRuleR0222Active => true;

		public bool IsRuleR0223Active => true;
	}
}

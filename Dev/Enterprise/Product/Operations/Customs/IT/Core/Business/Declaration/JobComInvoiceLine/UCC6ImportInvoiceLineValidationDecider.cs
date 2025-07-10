using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class UCC6ImportInvoiceLineValidationDecider : IImportInvoiceLineValidationDecider
{
	public bool IsRuleC0002Active => false;

	public bool IsRuleC0728Active => false;

	public bool IsRuleC0820ActiveForJI_SupplementaryCode1 => false;

	public bool IsRuleC0820ActiveForJI_Tariff => false;

	public bool IsRuleCD0111Active => true;

	public bool IsRuleCD5151ActiveForZG_CountryOfSupply => false;

	public bool IsRuleCD5161ActiveForJI_CountryOfOrigin => false;

	public bool IsRuleCD9102Active => true;

	public bool IsRuleC0627Active => false;

	public bool IsRuleC0710ActiveForJI_LinePrice => false;

	public bool IsRuleC0710ActiveForJI_CountryOfOrigin => false;

	public bool IsRuleC0710ActiveForZG_CountryOfSupply => false;

	public bool IsRuleC0919Active => false;

	public bool IsRuleR0012Active => false;

	public bool IsRuleR0222Active => false;

	public bool IsRuleR0224Active => true;

	public bool IsRuleC0624Active => false;

	public bool AllowMultipleRequestedProcedure => true;

	public bool IsRuleC0936Active => false;

	public bool IsRuleR0223Active => true;
}

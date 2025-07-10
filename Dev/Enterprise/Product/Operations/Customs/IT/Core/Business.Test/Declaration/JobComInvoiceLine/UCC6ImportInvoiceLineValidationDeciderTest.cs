using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(UCC6ImportInvoiceLineValidationDecider))]
sealed class UCC6ImportInvoiceLineValidationDeciderTest : EU.Business.Declaration.Testing.ImportInvoiceLineValidationDeciderTest<UCC6ImportInvoiceLineValidationDecider>
{
	protected override bool ExpectedIsRuleC0002Active => false;

	protected override bool ExpectedIsRuleC0627Active => false;

	protected override bool ExpectedIsRuleC0820ActiveForJI_SupplementaryCode1 => false;

	protected override bool ExpectedIsRuleC0820ActiveForJI_Tariff => false;

	protected override bool ExpectedIsRuleCD0111Active => true;

	protected override bool ExpectedIsRuleCD5151ActiveForZG_CountryOfSupply => false;

	protected override bool ExpectedIsRuleCD5161ActiveForJI_CountryOfOrigin => false;

	protected override bool ExpectedIsRuleCD9102Active => true;

	protected override bool ExpectedIsRuleC0710ActiveForJI_LinePrice => false;

	protected override bool ExpectedIsRuleC0710ActiveForJI_CountryOfOrigin => false;

	protected override bool ExpectedIsRuleC0710ActiveForZG_CountryOfSupply => false;

	protected override bool ExpectedIsRuleC0919Active => false;

	protected override bool ExpectedIsRuleR0012Active => false;

	protected override bool ExpectedIsRuleR0224Active => true;

	protected override bool ExpectedIsRuleC0624Active => false;

	protected override bool ExpectedIsRuleC0936Active => false;

	protected override bool ExpectedIsRuleC0728Active => false;

	protected override bool ExpectedAllowMultipleRequestedProcedure => true;

	protected override bool ExpectedIsRuleR0222Active => false;

	protected override bool ExpectedIsRuleR0223Active => true;
}

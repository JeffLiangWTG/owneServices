using NUnit.Framework;
namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceLineValidationDecider))]
	sealed class UCC6ImportInvoiceLineValidationDeciderTest : ImportInvoiceLineValidationDeciderTest<UCC6ImportInvoiceLineValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0627Active => true;

		protected override bool ExpectedIsRuleC0820ActiveForJI_SupplementaryCode1 => true;

		protected override bool ExpectedIsRuleC0820ActiveForJI_Tariff => true;

		protected override bool ExpectedIsRuleCD0111Active => true;

		protected override bool ExpectedIsRuleCD5151ActiveForZG_CountryOfSupply => false;

		protected override bool ExpectedIsRuleCD5161ActiveForJI_CountryOfOrigin => false;

		protected override bool ExpectedIsRuleCD9102Active => true;

		protected override bool ExpectedIsRuleC0710ActiveForJI_LinePrice => true;

		protected override bool ExpectedIsRuleC0710ActiveForJI_CountryOfOrigin => true;

		protected override bool ExpectedIsRuleC0710ActiveForZG_CountryOfSupply => true;

		protected override bool ExpectedIsRuleC0919Active => true;

		protected override bool ExpectedIsRuleR0012Active => true;

		protected override bool ExpectedIsRuleR0224Active => true;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0936Active => true;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedAllowMultipleRequestedProcedure => true;

		protected override bool ExpectedIsRuleR0222Active => false;

		protected override bool ExpectedIsRuleR0223Active => true;
	}
}

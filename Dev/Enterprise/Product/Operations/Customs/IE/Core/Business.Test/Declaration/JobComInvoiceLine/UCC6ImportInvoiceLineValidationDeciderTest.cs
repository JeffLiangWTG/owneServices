using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceLineValidationDecider))]
	sealed class UCC6ImportInvoiceLineValidationDeciderTest : EU.Business.Declaration.Testing.ImportInvoiceLineValidationDeciderTest<UCC6ImportInvoiceLineValidationDecider>
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

		protected override bool ExpectedIsRuleR0222Active => false;

		protected override bool ExpectedIsRuleR0224Active => true;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0936Active => true;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedAllowMultipleRequestedProcedure => false;

		protected override bool ExpectedIsRuleR0223Active => true;

		public void TestIsRuleCD5151ActiveForJI_CountryOfOrigin()
		{
			AssertEquals(true, decider.IsRuleCD5151ActiveForJI_CountryOfOrigin);
		}

		public void TestIsRuleCD5161ActiveForZG_CountryOfSupply()
		{
			AssertEquals(true, decider.IsRuleCD5161ActiveForZG_CountryOfSupply);
		}
	}
}

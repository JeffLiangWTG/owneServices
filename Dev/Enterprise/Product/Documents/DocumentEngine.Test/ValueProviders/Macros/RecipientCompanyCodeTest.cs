using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientCompanyCode))]
	sealed class RecipientCompanyCodeTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientCompanyCode();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient company code>", new RecipientCompanyCode().IsResponsibleForReplacing("<recipient company code>", Passes.FirstPass));
			Assert("Should not replace", !new RecipientCompanyCode().IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "blah blah";
			fred.OrgHeaderPK = org.PK;
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("blah blah", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WTG";
			fred.OrgHeaderPK = org.PK;
			((IReportForUnitTesting)Report).DeliveryContact = fred;
		}
	}
}

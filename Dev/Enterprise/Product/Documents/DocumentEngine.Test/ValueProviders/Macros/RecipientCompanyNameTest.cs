using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientCompanyName))]
	sealed class RecipientCompanyNameTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientCompanyName();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient company name>", new RecipientCompanyName().IsResponsibleForReplacing("<recipient company name>", Passes.FirstPass));
			Assert("Should not replace", !new RecipientCompanyName().IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.CompanyName = "Company";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("Company", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var wtg = new DocDeliveryContact(new BusinessObjectFactory());
			wtg.CompanyName = "WiseTech Global";
			((IReportForUnitTesting)Report).DeliveryContact = wtg;
		}
	}
}

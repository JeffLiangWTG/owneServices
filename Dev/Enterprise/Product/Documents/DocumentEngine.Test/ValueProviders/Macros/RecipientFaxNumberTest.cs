using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientFaxNumber))]
	sealed class RecipientFaxNumberTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientFaxNumber();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient fax number>", ValueProviderToTest.IsResponsibleForReplacing("<recipient fax number>", Passes.FirstPass));
			Assert("Should replace <recipient fax number>", ValueProviderToTest.IsResponsibleForReplacing("<recipient fax number>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Fax = "1234 5678";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("1234 5678", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Fax = "+61 2 9025 1199";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientPhoneNumber))]
	sealed class RecipientPhoneNumberTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientPhoneNumber();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient phone number>", ValueProviderToTest.IsResponsibleForReplacing("<recipient phone number>", Passes.FirstPass));
			Assert("Should replace <recipient phone number>", ValueProviderToTest.IsResponsibleForReplacing("<recipient phone number>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Vincent Vega <recipient phone number>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Phone = "1234 5678";
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
			fred.Phone = "+61 2 9025 1100";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
		}
	}
}

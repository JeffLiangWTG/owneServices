using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientEmailAddress))]
	sealed class RecipientEmailAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientEmailAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient email address>", ValueProviderToTest.IsResponsibleForReplacing("<recipient email address>", Passes.FirstPass));
			Assert("Should replace <recipient email address>", ValueProviderToTest.IsResponsibleForReplacing("<recipient email address>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Email = "zappoo@zip.com.au";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("zappoo@zip.com.au", ValueProviderToTest.GetReplacement("", Report));
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
			fred.Email = "mail@mail.com";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
		}
	}
}

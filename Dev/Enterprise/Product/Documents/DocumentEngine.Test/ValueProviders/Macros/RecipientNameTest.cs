using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientName))]
	sealed class RecipientNameTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientName();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient name>", new RecipientName().IsResponsibleForReplacing("<recipient name>", Passes.FirstPass));
			Assert("Should not replace", !new RecipientName().IsResponsibleForReplacing("Vincent Vega", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Name = "Fred";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("Fred", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var john = new DocDeliveryContact(new BusinessObjectFactory());
			john.Name = "John Doe";
			((IReportForUnitTesting)Report).DeliveryContact = john;
		}
	}
}

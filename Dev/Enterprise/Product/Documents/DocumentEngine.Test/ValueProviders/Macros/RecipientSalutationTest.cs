using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientSalutation))]
	sealed class RecipientSalutationTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientSalutation();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient salutation>", new RecipientSalutation().IsResponsibleForReplacing("<recipient salutation>", Passes.FirstPass));
			Assert("Should not replace", !new RecipientSalutation().IsResponsibleForReplacing("Vincent Vega Vastes", Passes.FirstPass));
		}

		public void TestReplacementWithSalutation()
		{
			PrepareRenderer();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Fred Jones";
			contact.OC_JobCategory = "CTO";
			contact.OC_Salutation = "Hi [Name] [JobCategory]";
			contact.OC_OH = org.PK;
			Factory.Save();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.OrgHeaderPK = org.PK;
			fred.Name = "Fred Jones";

			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("Hi Fred Jones CTO", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWithNoSalutation()
		{
			PrepareRenderer();
			var fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Name = "Fred Jones";
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("Returns name as no salutation specified", "Fred Jones", ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Doe";
			contact.OC_Salutation = "Dear [Name]";
			contact.OC_OH = org.PK;
			Factory.Save();
			var john = new DocDeliveryContact(new BusinessObjectFactory());
			john.OrgHeaderPK = org.PK;
			john.Name = "John Doe";
			((IReportForUnitTesting)Report).DeliveryContact = john;
		}
	}
}

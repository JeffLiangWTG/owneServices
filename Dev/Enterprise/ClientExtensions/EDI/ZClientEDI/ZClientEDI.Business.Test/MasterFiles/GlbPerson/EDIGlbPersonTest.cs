using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIGlbPerson))]
	public class EDIGlbPersonTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeletePersonMergeQueue()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var item = Factory.New<EdiPersonMergeQueue>();
			item.EMQ_PER_RetainPerson = person1.PK;
			item.EMQ_PER_DissolvePerson = person2.PK;
			Factory.Save();

			person1.Delete();
			Factory.Save();

			AssertEquals(false, Factory.ExistsInDatabase(EdiPersonMergeQueueSchema.Constants.TableName, new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_DissolvePerson, person2.PK)));
		}

		[TestDate(2020, 01, 01)]
		public void TestSkipPersonalEmailPrompt()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";

			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			Factory.Save();

			var personAsEDI = newContact.Person as EDIGlbPerson;
			personAsEDI.StorePersonalEmailPromptSkip();
			Factory.Save();

			AssertEquals("Should skip the personal password prompt for 7 days", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals("Should skip the personal password prompt for 7 days", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals("Should skip the personal password prompt for 7 days", false, personAsEDI.ShouldSkipPersonalEmailPrompt());

			personAsEDI.StorePersonalEmailPromptSkip();
			Factory.Save();

			AssertEquals("Should skip the personal password prompt for 7 days", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals("Should skip the personal password prompt for 7 days", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals("Should skip the personal password prompt for 7 days", false, personAsEDI.ShouldSkipPersonalEmailPrompt());
		}

		[TestDate(2020, 01, 01)]
		public void TestSkipPersonalEmailPromptPermanently()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";

			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			Factory.Save();

			var personAsEDI = newContact.Person as EDIGlbPerson;
			personAsEDI.StorePersonalEmailPromptSkip(true);

			AssertEquals("Should skip the personal password prompt permanently", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(7);
			AssertEquals("Should skip the personal password prompt permanently", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
			TestDateAttribute.AddDays(1);
			AssertEquals("Should skip the personal password prompt permanently", true, personAsEDI.ShouldSkipPersonalEmailPrompt());
		}
	}
}

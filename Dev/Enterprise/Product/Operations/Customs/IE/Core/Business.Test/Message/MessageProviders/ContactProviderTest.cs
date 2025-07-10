using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class ContactProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null - OrgContact", ContactProvider.New((OrgContact)null));
				AssertNull("Null - OrgHeader", ContactProvider.New((OrgHeader)null));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestNew_JobDocAddress()
		{
			CombineAssertions(() =>
			{
				JobDocAddress docAddress = null;
				AssertNull("Null - JobDocAddress", ContactProvider.New(docAddress));
				docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_Contact = "BOB THE BUILDER";
				docAddress.E2_Phone = "0290203040";
				docAddress.E2_Email = "bob@where.com";
				var provider = ContactProvider.New(docAddress);
				AssertEquals("Name", "BOB THE BUILDER", provider.Name);
				AssertEquals("PhoneNumber", "0290203040", provider.PhoneNumber);
				AssertEquals("EmailAddress", "bob@where.com", provider.EmailAddress);
			});
		}

		public void TestName()
		{
			orgContact.OC_ContactName = "TestContactName";
			AssertEquals("Name", "TestContactName", GetProvider().Name);
		}

		public void TestPhoneNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_Phone = "+61 (2) 1111 1111";
			orgContact.OC_OH = orgHeader.PK;
			AssertEquals("PhoneNumber", "+61 (2) 1111 1111", GetProvider().PhoneNumber);

			orgContact.OC_Phone = "+61 (2) 2222 2222";
			AssertEquals("PhoneNumber", "+61 (2) 2222 2222", GetProvider().PhoneNumber);
		}

		public void TestEmailAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_Email = "emailMainAddress@test.com";
			orgContact.OC_OH = orgHeader.PK;
			AssertEquals("EmailAddress", "emailMainAddress@test.com", GetProvider().EmailAddress);

			orgContact.OC_Email = "emailContact@test.com";
			AssertEquals("EmailAddress", "emailContact@test.com", GetProvider().EmailAddress);
		}

		ContactProvider GetProvider(OrgContact contact) => ContactProvider.New(contact);
		protected override ContactProvider GetProvider() => GetProvider(orgContact);

		protected override void SetUp()
		{
			base.SetUp();

			orgContact = Factory.New<OrgContact>();
		}
		OrgContact orgContact;
	}
}

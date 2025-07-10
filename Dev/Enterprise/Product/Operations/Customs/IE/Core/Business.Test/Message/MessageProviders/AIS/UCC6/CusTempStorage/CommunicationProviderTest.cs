using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class CommunicationProviderTest : DataProviderTestCase<CommunicationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("Arguments == null", CommunicationProvider.New(null, null));
				AssertNotNull("Valid phone argument", CommunicationProvider.New(null, "+441234232"));
				AssertNotNull("Valid contact argument", CommunicationProvider.New(orgContact, null));
				AssertNotNull("Valid arguments", Provider);
			});
		}

		public void TestType()
		{
			AssertEquals("Phone", "TE", Provider.Type);

			orgContact.OC_Email = "test@contact.ie";
			AssertEquals("Email", "EM", GetProvider().Type);

			overridePhone = "+44000000";
			AssertEquals("Override Phone", "TE", GetProvider().Type);
		}

		public void TestId()
		{
			AssertEquals("Phone number", "+35312345678", Provider.Id);

			orgContact.OC_Email = "test@contact.ie";
			AssertEquals("Email address", "test@contact.ie", GetProvider().Id);

			overridePhone = "+44000000";
			AssertEquals("Override Phone number", "+44000000", GetProvider().Id);
		}

		protected override CommunicationProvider GetProvider() => CommunicationProvider.New(orgContact, overridePhone);

		protected override void SetUp()
		{
			base.SetUp();
			orgContact = Factory.New<OrgContact>();
			orgContact.OC_Phone = "+35312345678";
		}

		OrgContact orgContact;
		ZString overridePhone;
	}
}

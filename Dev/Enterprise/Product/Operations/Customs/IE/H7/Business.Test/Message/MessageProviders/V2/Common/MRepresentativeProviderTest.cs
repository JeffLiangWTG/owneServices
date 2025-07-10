using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MRepresentativeProviderTest : DataProviderTestCase<MRepresentativeProvider>
	{
		public void TestId()
		{
			AssertEquals("Id", "IE1234412", mRepresentativeProvider.Id);
		}

		public void TestContactPerson()
		{
			var contactPerson = mRepresentativeProvider.ContactPerson;
			CombineAssertions("contact Person details", () =>
			{
				AssertNotNull(contactPerson);
				AssertEquals("Name", "Name", contactPerson.Name);
				AssertEquals("Phone Number", "123456789", contactPerson.PhoneNumber);
				AssertEquals("Email Address", "123@test.org", contactPerson.EmailAddress);
			});
		}

		public void TestStatus()
		{
			manifestHeader.AMA_AgentType = "DIR";
			AssertEquals("Status is 2 when Rep. Status is Direct", "2", mRepresentativeProvider.Status);

			manifestHeader.AMA_AgentType = "IND";
			mRepresentativeProvider = MRepresentativeProvider.NewOrNull(manifestHeader);
			AssertEquals("Status is 3 when Rep. Status is Indirect", "3", mRepresentativeProvider.Status);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<OrgHeader>();
			var address = header.Addresses.AddNew();

			var customsCode = address.CustomsCodes.AddNew();
			customsCode.OK_CodeType = "EOR";
			customsCode.OK_CustomsRegNo = "1234412";

			var contact = header.AllocatedContacts.AddNew();
			contact.OC_ContactName = "Name";
			contact.OC_Phone = "123456789";
			contact.OC_Email = "123@test.org";

			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CUS";

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_OA_Representative = address.PK;

			mRepresentativeProvider = MRepresentativeProvider.NewOrNull(manifestHeader);
		}

		AsycudaManifestHeader manifestHeader;
		MRepresentativeProvider mRepresentativeProvider;

		protected override MRepresentativeProvider GetProvider()
		{
			return mRepresentativeProvider;
		}
	}
}

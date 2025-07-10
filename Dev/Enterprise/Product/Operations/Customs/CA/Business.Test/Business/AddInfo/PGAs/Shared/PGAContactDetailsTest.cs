using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGAContactDetailsTest : TestCaseWithFactory
	{
		public void TestGetContactDetails()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader1 = factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = factory.NewWithValidTestData<OrgHeader>();

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "jaime";
			staff.GS_EmailAddress = "jaime.lannister@winterfall.com";
			staff.GS_WorkPhone = "02588888888";

			CreateContactForAllocation(orgHeader1, "jonh.snow@winterfall.com", OrgConstants.ContactAllocationType.CAPGA, "02512345678", "jonh");
			CreateContactForAllocation(orgHeader2, "jonh.snow@winterfall.com", OrgConstants.ContactAllocationType.USPGA, "02512345678", "jonh");

			var contactDetails1 = new PGAContactDetails(orgHeader1);
			var contactDetails2 = new PGAContactDetails(orgHeader2);
			var contactDetails3 = new PGAContactDetails(orgHeader1, staff);

			AssertEquals("jonh.snow@winterfall.com", contactDetails1.EmailAddress);
			AssertEquals("02512345678", contactDetails1.PhoneNumber);
			AssertEquals("jonh", contactDetails1.ContactName);

			AssertEquals(string.Empty, contactDetails2.EmailAddress);
			AssertEquals(string.Empty, contactDetails2.PhoneNumber);
			AssertEquals(string.Empty, contactDetails2.ContactName);

			AssertEquals("jaime.lannister@winterfall.com", contactDetails3.EmailAddress);
			AssertEquals("02588888888", contactDetails3.PhoneNumber);
			AssertEquals("jaime", contactDetails3.ContactName);
		}

		void CreateContactForAllocation(OrgHeader org, string email, string type, string phone, string contactName)
		{
			var contact = org.Contacts.AddNew();
			contact.OC_Email = email;
			contact.OC_Phone = phone;
			contact.OC_ContactName = contactName;

			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = type;
		}
	}
}

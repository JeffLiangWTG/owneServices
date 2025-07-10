using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EDIOrgContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestInactiveAddress_Existing()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 Test Rd";

			var contact = org.Contacts.AddNew() as EDIOrgContact;
			contact.OC_ContactName = "Contact1";
			contact.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			address2.OA_IsActive = false;

			contact.Validation.ValidateAll();
			AssertEquals(false, contact.HasErrors());
			AssertEquals(true, contact.HasWarnings());
		}

		public void TestInactiveAddress_Set()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 Test Rd";

			var contact = org.Contacts.AddNew() as EDIOrgContact;
			contact.OC_ContactName = "Contact1";
			contact.OC_OA_OrgAddress = address2.PK;

			Factory.Save();

			address1.OA_IsActive = false;

			Factory.Save();

			contact.OC_OA_OrgAddress = address1.PK;

			contact.Validation.ValidateAll();
			AssertEquals(true, contact.HasErrors());
		}

		public void TestCheckOC_OA_OrgAddress()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var address = org.Addresses.AddNew();
			address.OA_Address1 = "1 Test Rd";

			var contact = org.Contacts.AddNew() as EDIOrgContact;
			contact.OC_ContactName = "Contact1";
			contact.OC_OA_OrgAddress = address.PK;
			var validation = contact.Validation;

			Factory.Save();

			contact.OC_IsActive = true;
			address.OA_IsActive = false;
			validation.ValidateOC_OA_OrgAddress();
			AssertNoError(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertHasWarning(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");

			Factory.Save();
			validation.ValidateOC_OA_OrgAddress();
			AssertNoError("No error for existing saved data", contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertHasWarning(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");

			var clientBranch = Factory.NewWithValidTestData<ClientBranch>();
			clientBranch.LCB_OA = address.PK;
			Factory.Save();
			validation.ValidateOC_OA_OrgAddress();
			AssertHasWarning(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertNoError(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");

			contact.OC_IsActive = true;
			address.OA_IsActive = true;
			validation.ValidateOC_OA_OrgAddress();
			AssertNoWarning(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			AssertNoError(contact.OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
		}
	}
}

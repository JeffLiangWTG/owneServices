using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class AccountConfirmationRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact = Factory.New<OrgContact>();
			var descriptor = new AccountConfirmationRoutingDescriptor(contact);
			AssertEquals("Routing page url", "~/Login/AccountConfirmation.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			var user = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user.EUA_OC_WebAccessContact = contact.PK;
			user.Database.LD_OH_WebAccessOrg = contact.OC_OH;
			Factory.Save();
			Assert("Precondition", user.EUA_IsActive);
			Assert("Precondition", user.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition", null, (contact as EDIOrgContact)?.GetMostRecentUnlinkedUserAccount());
			var descriptor = new AccountConfirmationRoutingDescriptor(contact);
			AssertEquals("Not required as user account is linked", false, descriptor.IsRoutingRequired);
			var user2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			user2.EUA_OC_WebAccessContact = contact.PK;
			user2.EUA_IsContactRelationshipActive = false;
			user2.Database.LD_OH_WebAccessOrg = contact.OC_OH;
			Factory.Save();
			AssertNotEquals("Precondition", null, (contact as EDIOrgContact)?.GetMostRecentUnlinkedUserAccount());
			descriptor = new AccountConfirmationRoutingDescriptor(contact);
			AssertEquals("Is required", true, descriptor.IsRoutingRequired);
		}
	}
}
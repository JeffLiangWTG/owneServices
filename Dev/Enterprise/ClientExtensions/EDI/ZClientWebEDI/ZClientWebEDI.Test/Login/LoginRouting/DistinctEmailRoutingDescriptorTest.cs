using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class DistinctEmailRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var user = Factory.New<EdiCustomerUserAccount>();
			var descriptor = new DistinctEmailRoutingDescriptor(user);
			AssertEquals("Routing page url", "~/Login/DistinctEmailRequired.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "~code~";
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "name";
			contact.OC_OH = org.PK;
			Factory.Save();
			var ld = Factory.NewWithValidTestData<LicenceDatabase>();
			var user1 = Factory.New<EdiCustomerUserAccount>();
			user1.EUA_OC_WebAccessContact = contact.PK;
			user1.EUA_UserID = "some user";
			user1.EUA_LD = ld.PK;
			Factory.Save();
			var descriptor1 = new DistinctEmailRoutingDescriptor(user1);
			AssertEquals(false, descriptor1.IsRoutingRequired);
			var user2 = Factory.New<EdiCustomerUserAccount>();
			user2.EUA_OC_WebAccessContact = contact.PK;
			user2.EUA_UserID = "other user";
			user2.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.EmailChanged;
			user2.EUA_LD = ld.PK;
			Factory.Save();
			var descriptor2 = new DistinctEmailRoutingDescriptor(user2);
			AssertEquals(false, descriptor2.IsRoutingRequired);
			user1.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.DistinctEmailRequired;
			Factory.Save();
			AssertEquals(true, descriptor1.IsRoutingRequired);
			AssertEquals(false, descriptor2.IsRoutingRequired);
		}
	}
}
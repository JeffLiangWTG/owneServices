using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class LoginOptionsRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var user = Factory.New<EdiCustomerUserAccount>();
			var contact = Factory.New<OrgContact>();
			var descriptor = new LoginOptionsRoutingDescriptor(user, contact);
			AssertEquals("Routing page url", "~/Login/LoginOptions.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var user = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			var descriptor = new LoginOptionsRoutingDescriptor(user, contact);
			AssertEquals("Not required as registry is not turned on", false, descriptor.IsRoutingRequired);
			EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Not required as user has no need to verify", false, descriptor.IsRoutingRequired);
			user.EUA_IsContactRelationshipActive = false;
			Factory.Save();
			descriptor = new LoginOptionsRoutingDescriptor(user, contact);
			AssertEquals("Is required", true, descriptor.IsRoutingRequired);
		}
	}
}
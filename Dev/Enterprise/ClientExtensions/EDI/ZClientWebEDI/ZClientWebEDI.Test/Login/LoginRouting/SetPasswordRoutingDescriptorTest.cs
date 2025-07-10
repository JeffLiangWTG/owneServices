using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class SetPasswordRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact = Factory.New<OrgContact>();
			var descriptor = new SetPasswordRoutingDescriptor(contact);
			AssertEquals("Routing page url", "~/Login/InitialLoginSetPassword.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			contact.OC_PasswordHash = ZBlob.Empty;
			Factory.Save();
			var descriptor = new SetPasswordRoutingDescriptor(contact);
			Assert("Precondition", !contact.HasPassword);
			AssertEquals("Contact password is empty", true, descriptor.IsRoutingRequired);
			contact.SetHashedPassword("1234");
			Factory.Save();
			descriptor = new SetPasswordRoutingDescriptor(contact);
			Assert("Precondition", contact.HasPassword);
			AssertEquals("Contact password is not empty", false, descriptor.IsRoutingRequired);
		}
	}
}
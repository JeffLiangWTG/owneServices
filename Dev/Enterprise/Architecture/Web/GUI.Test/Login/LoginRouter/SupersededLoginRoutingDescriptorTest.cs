using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[HttpContextEnabledTest]
	sealed class SupersededLoginRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			contact2.Person.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			contact2.Person.PER_PasswordHashIterations = 9239;
			contact2.Person.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var descriptor1 = new SupersededLoginRoutingDescriptor(contact1);
			var descriptor2 = new SupersededLoginRoutingDescriptor(contact2);
			AssertDescriptorUrl(descriptor1, false);
			AssertDescriptorUrl(descriptor2, true);
		}

		void AssertDescriptorUrl(SupersededLoginRoutingDescriptor descriptor, bool hasPersonPassword)
		{
			var page = hasPersonPassword ? "LoginRedirection.aspx" : "LoginSuperseded.aspx";
			AssertStartsWith("Routing page url", FormattableString.Invariant($"~/Login/{page}"), descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var supersededHelper = new OrgContactSupersededHelper(contact);
			AssertEquals("Precondition", false, supersededHelper.HasValidContacts);
			var descriptor = new SupersededLoginRoutingDescriptor(contact);
			AssertEquals("Not required as contact is not superseded", false, descriptor.IsRoutingRequired);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_PER = contact.OC_PER;
			contact.SupersedeWebAccess();
			Factory.Save();
			AssertEquals("Precondition", true, supersededHelper.HasValidContacts);
			AssertEquals("Required as contact is superseded and has valid login contact", true, descriptor.IsRoutingRequired);
		}
	}
}

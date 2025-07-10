using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class SetMasterPasswordRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_IsActive = false;
			contact1.OC_PER = person.PK;
			contact2.OC_PER = person.PK;
			var descriptor = new SetMasterPasswordRoutingDescriptor(contact1);
			AssertEquals("Routing page url", "~/Login/RegisterMasterPassword.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_PER = person.PK;
			contact1.SetHashedPassword("1234");
			Factory.Save();
			var descriptor = new SetMasterPasswordRoutingDescriptor(contact1);
			Assert("Precondition", !contact1.Person.HasPassword);
			AssertEquals("Precondition", 1, contact1.Person.ContactCollection.Count);
			AssertEquals("Should not register if only 1 contact in the relationship", false, descriptor.IsRoutingRequired);
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_IsActive = false;
			contact2.OC_PER = person.PK;
			contact2.SetHashedPassword("5678");
			Factory.Save();
			descriptor = new SetMasterPasswordRoutingDescriptor(contact1);
			Assert("Precondition", !contact1.Person.HasPassword);
			AssertEquals("Precondition", 2, contact1.Person.ContactCollection.Count);
			AssertEquals("Should register if more than 1 contact is in the relationship (even if related contact is inactive or has no web access)", true, descriptor.IsRoutingRequired);
			contact1.RemovePasswordAndHash();
			contact2.RemovePasswordAndHash();
			Factory.Save();
			descriptor = new SetMasterPasswordRoutingDescriptor(contact1);
			AssertEquals("Should not register when all linked contacts have no password", false, descriptor.IsRoutingRequired);
			contact1.SetHashedPassword("1234");
			contact2.SetHashedPassword("5678");
			contact1.Person.SetHashedPassword("3333");
			Factory.Save();
			descriptor = new SetMasterPasswordRoutingDescriptor(contact1);
			Assert("Precondition", contact1.Person.HasPassword);
			AssertEquals("Precondition", 2, contact1.Person.ContactCollection.Count);
			AssertEquals("Should not register if person password already set", false, descriptor.IsRoutingRequired);
		}
	}
}
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZContactBusinessObject))]
	sealed class ZContactBusinessObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestSelectedContactPK()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.AddNew();
			Factory.Save();
			AssertEquals("Contacts count", 1, org.Contacts.Count);

			ZContactBusinessObject contact = new ZContactBusinessObject();
			AssertEquals("Default Org PK", GlbCompany.CurrentCompany.GC_OH_OrgProxy, contact.SelectedOrganisationPK);

			contact.SelectedContactPK = org.Contacts[0].PK;
			AssertEquals("New Org PK", org.PK, contact.SelectedOrganisationPK);
		}
	}
}

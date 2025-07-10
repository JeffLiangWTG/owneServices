using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	public class SupportRequestContactValueObjectHelperTest : ContactValueObjectHelperTest
	{
		public void TestDoNotDeactivateExistingContact()
		{
			Xsd.OrgContactCollection collection = new Xsd.OrgContactCollection();
			Xsd.OrgContact xsdContact = collection.AddNew();
			xsdContact.Name = "NewContact";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var systemGeneratedContact = org.Contacts.AddNew();
			systemGeneratedContact.OC_ContactName = "SystemGeneratedContact";
			systemGeneratedContact.OC_SystemCreateUser = "~BP";

			var helper = new SupportRequestContactValueObjectHelper("");
			helper.ImportFromValueObjectCollection(collection, org, new ValueObjectImportContext(Factory, new NotificationBuffer()));

			AssertEquals(2, org.Contacts.Count);
			Assert(org.Contacts[0].OC_IsActive);
			Assert(org.Contacts[1].OC_IsActive);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	[TestedType(typeof(CustomSupportIncident))]
	public class CustomSupportIncidentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CustomSupportIncident(Factory.NewWithValidTestData<SupportIncident>());

		public void TestPopulateProperties()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			incident.IM_OC_Contact = Factory.NewWithValidTestData<OrgContact>().PK;
			var customIncident = new CustomSupportIncident(incident);

			AssertEquals(customIncident.ClientCode, incident.Client.OH_Code);
			AssertEquals(customIncident.ContactName, incident.Contact.OC_ContactName);
			AssertEquals(customIncident.IncidentNumber, incident.IM_IncidentNumber);
			AssertEquals(customIncident.Description, incident.IM_Description);
			AssertEquals(customIncident.Product, incident.IM_Product);
			AssertEquals(customIncident.ProductArea, incident.ProductArea);
			AssertEquals(customIncident.Priority, incident.IM_Priority);
			AssertEquals(customIncident.EConversation, incident.EConversation);
		}
	}
}

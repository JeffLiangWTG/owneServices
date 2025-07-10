using System.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgContactFilterBusinessObject))]
	[HttpContextEnabledTest]
	sealed class OrgContactFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		OrgContactFilterBusinessObject FilterObject
		{
			get
			{
				if (filterObject == null)
				{
					filterObject = (OrgContactFilterBusinessObject)GetNewBusinessObject();
				}
				return filterObject;
			}
		}
		OrgContactFilterBusinessObject filterObject;

		public void TestFilterForOrgContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var activeContact = org.Contacts.AddNew();
			activeContact.OC_IsActive = true;
			activeContact.OC_ContactName = "Active Contact";

			var inActiveContact = org.Contacts.AddNew();
			inActiveContact.OC_IsActive = false;
			inActiveContact.OC_ContactName = "Inactive Contact";

			Factory.Save();

			HttpContext.Current.Request.QueryString[ZFilterPage.ParentPKQuery] = org.PK.ToString();

			var activeMatchingContacts = new OrgContactCollection(Factory, FilterObject.Filter);
			activeMatchingContacts.Load();

			AssertEquals("Shoud Return one Active Contact", 1, activeMatchingContacts.Count);

			AssertEquals(activeMatchingContacts[0].OC_ContactName, "Active Contact");
		}
	}
}

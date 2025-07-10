using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrgContactsFilterBusinessObject))]
	public class EDIOrgContactsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestClientStaffFilter()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA", false);
			var db = licence.Database;
			db.LD_DatabaseNumber = 1234;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~123";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "name 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "name 2";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "name 3";
			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			userAccount1.EUA_UserID = "~11";
			userAccount1.EUA_FullName = "full name 1";
			userAccount1.EUA_Email = "11@gmail.com";
			userAccount1.EUA_LD = db.PK;
			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;
			userAccount3.EUA_UserID = "~33";
			userAccount3.EUA_FullName = "full name 33";
			userAccount3.EUA_Email = "33@gmail.com";
			userAccount3.EUA_LD = db.PK;
			Factory.Save();
			var filterBizO = new EDIOrgContactsFilterBusinessObject();
			var nameFilter = filterBizO["Name"] as ModuleTextFilter;
			nameFilter.Property = "name ";
			nameFilter.IsActive = true;
			var contacts = Factory.Load<EDIOrgContact>(filterBizO.Filter);
			AssertEquals(3, contacts.Length);
			var userAccountFilter = (ModuleFlagsFilter)filterBizO["Has Client Staff"];
			userAccountFilter.Property0 = true;
			userAccountFilter.IsActive = true;
			contacts = Factory.Load<EDIOrgContact>(filterBizO.Filter);
			AssertEquals(2, contacts.Length);
			userAccountFilter.Property0 = false;
			contacts = Factory.Load<EDIOrgContact>(filterBizO.Filter);
			AssertEquals(1, contacts.Length);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIOrgContactsFilterBusinessObject();
		}
	}
}

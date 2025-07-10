using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(MyAccountContactFilterStripBusinessObject))]
	public class MyAccountContactFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new MyAccountContactFilterStripBusinessObjectForTest(Factory.NewWithValidTestData<OrgHeader>());
		public void TestNoClosestPort()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "c1";
			contact.OC_Email = "c1@cw1.com";
			var collection = new OrgContactCollection(Factory);
			var filterStrip = new MyAccountContactFilterStripBusinessObjectForTest(org);
			var filter = filterStrip["CompanyName"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertNoExceptionThrown(delegate
			{
				filter.Property = "CompanyName";
			});
		}

		public void TestFilters()
		{
			var org0 = Factory.NewWithValidTestData<OrgHeader>();
			var contact0 = org0.Contacts.AddNew();
			contact0.OC_ContactName = "c1";
			contact0.OC_Email = "c1@cw1.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "c1";
			contact1.OC_Email = "c1@cw1.com";
			contact1.OC_OA_OrgAddress = Factory.New<OrgAddress>().PK;
			contact1.OrgAddress.OA_OH = org1.PK;
			contact1.OrgAddress.OA_Code = "CM1";
			contact1.OrgAddress.OA_Address1 = "OA_Address1";
			contact1.OrgAddress.CompanyName = "CompanyName1";
			contact1.OrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			contact1.IsCustomerServiceContact = true;
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "c2";
			contact2.OC_Email = "c2@cw1.com";
			contact2.OC_OA_OrgAddress = Factory.New<OrgAddress>().PK;
			contact2.OrgAddress.OA_OH = org1.PK;
			contact2.OrgAddress.OA_Code = "CM2";
			contact2.OrgAddress.OA_Address1 = "OA_Address2";
			contact2.OrgAddress.CompanyName = "CompanyName2";
			contact2.OrgAddress.OA_RL_NKRelatedPortCode = "AU2CO";
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "c3";
			contact3.OC_Email = "c3@cw1.com";
			contact3.OC_IsActive = false;
			Factory.Save();
			var collection = new OrgContactCollection(Factory);
			var filterStrip = new MyAccountContactFilterStripBusinessObjectForTest(org1);
			collection.Load(filterStrip.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(contact1, collection);
			AssertCollectionContains(contact2, collection);
			var filter = filterStrip["ContactName"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "c2";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact2, collection);
			filter.IsActive = false;
			filter = filterStrip["Email"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "c1@cw1.com";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact1, collection);
			filter.IsActive = false;
			filter = filterStrip["CompanyName"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "CompanyName2";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact2, collection);
			filter.IsActive = false;
			filter = filterStrip["Branch"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "OA_Address1";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact1, collection);
			filter.IsActive = false;
			var nkFilter = filterStrip["UNLOCO"] as ModuleNkFilter;
			nkFilter.IsActive = true;
			nkFilter.Property = "AU2CO";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact2, collection);
		}

		public void TestNotificationRoleFilter()
		{
			var org0 = Factory.NewWithValidTestData<OrgHeader>();
			var contact0 = org0.Contacts.AddNew();
			contact0.OC_ContactName = "c1";
			contact0.OC_Email = "c1@cw1.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "c1";
			contact1.OC_Email = "c1@cw1.com";
			contact1.IsCustomerServiceContact = true;
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "c2";
			contact2.OC_Email = "c2@cw1.com";
			contact2.IsCustomerServiceContact = false;
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "c3";
			contact3.OC_Email = "c3@cw1.com";
			Factory.Save();
			var collection = new OrgContactCollection(Factory);
			var filterStrip = new MyAccountContactFilterStripBusinessObjectForTest(org1);
			collection.Load(filterStrip.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(contact1, collection);
			AssertCollectionContains(contact2, collection);
			var filter = filterStrip["NotificationRole"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = "CSV";
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact1, collection);
		}
	}
}

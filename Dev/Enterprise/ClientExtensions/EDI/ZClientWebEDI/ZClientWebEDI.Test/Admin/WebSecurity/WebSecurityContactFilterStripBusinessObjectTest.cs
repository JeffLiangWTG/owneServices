using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityContactFilterStripBusinessObject))]
	public class WebSecurityContactFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WebSecurityContactFilterStripBusinessObjectForTest(Factory.NewWithValidTestData<OrgHeader>());
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
			contact1.OC_WebAccessEnabled = true;
			var securityName = contact1.SecurityRightsView[0].SecurityItemName;
			contact1.SecurityRightsView[0].OZ_Granted = true;
			contact1.IsCustomerServiceContact = false;
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "c2";
			contact2.OC_Email = "c2@cw1.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.SecurityRightsView[0].OZ_Granted = false;
			contact2.IsCustomerServiceContact = true;
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "c3";
			contact3.OC_Email = "c3@cw1.com";
			contact3.OC_WebAccessEnabled = false;
			Factory.Save();
			var collection = new OrgContactCollection(Factory);
			var filterStrip = new WebSecurityContactFilterStripBusinessObjectForTest(org1);
			collection.Load(filterStrip.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(contact1, collection);
			AssertCollectionContains(contact2, collection);
			var filter = filterStrip["Security"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = securityName;
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(contact1, collection);
		}

		public void TestSecurityFilter()
		{
			//security 1: default granted
			//org 1: granted
			//contact 1.1: granted
			//contact 1.2: denied
			//org 2: denied
			//contact 2.1: granted
			//contact 2.2: denied
			//org 3: granted self-managed
			//contact 3.1: granted
			//contact 3.2: denied
			//org 4: denied self-managed
			//contact 4.1: granted
			//contact 4.2: denied

			//security 2: default denied
			//org 5: granted
			//contact 5.3: granted
			//contact 5.4: denied
			//org 6: denied
			//contact 6.3: granted
			//contact 6.4: denied
			//org 7: granted self-managed
			//contact 7.3: granted
			//contact 7.4: denied
			//org 8: denied self-managed
			//contact 8.3: granted
			//contact 8.4: denied

			var security1 = EDIWebSecurityRightsList.WiseTechAcademy;
			AssertEquals(true, security1.IsGrantedByDefault);

			AssertOrgContactSecurity(security1.SecurityItemName, 1, isSelfManaged: false, isOrgSecurityGranted: true, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security1.SecurityItemName, 2, isSelfManaged: false, isOrgSecurityGranted: false, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security1.SecurityItemName, 3, isSelfManaged: true, isOrgSecurityGranted: true, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security1.SecurityItemName, 4, isSelfManaged: true, isOrgSecurityGranted: false, isContact1SecurityGranted: true, isContact2SecurityGranted: false);

			var security2 = EDIWebSecurityRightsList.Downloads;
			AssertEquals(false, security2.IsGrantedByDefault);

			AssertOrgContactSecurity(security2.SecurityItemName, 5, isSelfManaged: false, isOrgSecurityGranted: true, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security2.SecurityItemName, 6, isSelfManaged: false, isOrgSecurityGranted: false, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security2.SecurityItemName, 7, isSelfManaged: true, isOrgSecurityGranted: true, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
			AssertOrgContactSecurity(security2.SecurityItemName, 8, isSelfManaged: true, isOrgSecurityGranted: false, isContact1SecurityGranted: true, isContact2SecurityGranted: false);
		}

		void AssertOrgContactSecurity(string securityName, short orgIndex, bool isSelfManaged, bool isOrgSecurityGranted, bool isContact1SecurityGranted, bool isContact2SecurityGranted)
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var orgSecurity = org.SecurityRightsView.OfType<OrgSecurity>().Single(x => x.SecurityItemNameForDisplay == securityName);
			orgSecurity.OX_IsCustomerManaged = isSelfManaged;
			orgSecurity.OX_Granted = isOrgSecurityGranted;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = $"contact {orgIndex}.1";
			contact1.OC_Email = $"contact{orgIndex}1@test.abc";
			contact1.OC_WebAccessEnabled = true;
			contact1.SecurityRightsForBindingOnly.OfType<OrgSecurityContacts>().Single(x => x.SecurityItemName == securityName).OZ_Granted = isContact1SecurityGranted;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = $"contact {orgIndex}.2";
			contact2.OC_Email = $"contact{orgIndex}2@test.abc";
			contact2.OC_WebAccessEnabled = true;
			contact2.SecurityRightsForBindingOnly.OfType<OrgSecurityContacts>().Single(x => x.SecurityItemName == securityName).OZ_Granted = isContact2SecurityGranted;

			Factory.Save();

			var filterFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgForFiltering = filterFactory.Load<OrgHeader>(org.PK);

			var collection = new OrgContactCollection(filterFactory);

			var filterStrip = new WebSecurityContactFilterStripBusinessObjectForTest(orgForFiltering);
			var filter = filterStrip["Security"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = securityName;
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals($"contact {orgIndex}.1", collection[0].OC_ContactName);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection.Load(filterStrip.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals($"contact {orgIndex}.2", collection[0].OC_ContactName);
		}

		public void TestSecurityFilter_DbHits()
		{
			var securityName = EDIWebSecurityRightsList.Downloads.SecurityItemName;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SecurityRights.OfType<OrgSecurity>().Single(x => x.SecurityItemNameForDisplay == securityName).OX_Granted = false;

			for (int i = 0; i < 20; i++)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"contact {i}";
				contact.OC_Email = $"c{i}@cw1.com";
				contact.OC_WebAccessEnabled = true;
				if (i % 2 == 0)
				{
					contact.SecurityRightsForBindingOnly.OfType<OrgSecurityContacts>().Single(x => x.SecurityItemName == securityName).OZ_Granted = true;
				}
			}

			Factory.Save();

			var filterFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgForFiltering = filterFactory.Load<OrgHeader>(org.PK);
			var collection = new OrgContactCollection(filterFactory);
			var filterStrip = new WebSecurityContactFilterStripBusinessObjectForTest(orgForFiltering);
			var filter = filterStrip["Security"] as ModuleTextFilter;
			filter.IsActive = true;
			filter.Property = securityName;
			collection.Load(filterStrip.Filter);
			AssertEquals(10, collection.Count);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgSecuritySchema.Constants.TableName, 1 },
				{ StmMenuItemSchema.Constants.TableName,1 },
			};
			AssertDbHits(expectedHitCounts, filterFactory);
		}
	}
}

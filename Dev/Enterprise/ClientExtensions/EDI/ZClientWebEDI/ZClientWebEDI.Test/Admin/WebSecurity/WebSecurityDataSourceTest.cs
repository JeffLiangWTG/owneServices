using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class WebSecurityDataSourceTest : TestCaseWithFactory
	{
		public static WebSecurityDataSource GetWrapperToTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "a@cw1.com";
			contact.OC_WebAccessEnabled = true;
			org.SecurityRightsView[0].OX_IsCustomerManaged = true;
			var wrapper = new WebSecurityDataSource(factory, org.PK);
			wrapper.SelectContact(contact);
			return wrapper;
		}

		public void TestBulkUpdate_All()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "~code";
			OrgSecurity security1 = (OrgSecurity)org1.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security1.OX_IsCustomerManaged = true;
			foreach (OrgSecurity sec in org1.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "aaa 1";
			contact1.OC_Email = "c1@org.com";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "bbb 2";
			contact2.OC_Email = "c2@org.com";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "ccc 3";
			contact3.OC_Email = "c3@org.com";
			contact3.OC_WebAccessEnabled = true;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "~code2";
			foreach (OrgSecurity sec in org2.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			OrgSecurity security2 = (OrgSecurity)org2.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security2.OX_IsCustomerManaged = true;
			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "aaa 4";
			contact4.OC_Email = "c4@org.com";
			contact4.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertEquals(false, contact1.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact2.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact3.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact4.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			var wrapper = new WebSecurityDataSource(Factory, org1.PK);
			wrapper.SelectContact(contact1);
			wrapper.InitBulkUpdate(0, 0);
			var securityItem = wrapper.BulkUpdateProfile.Items.Cast<SecurityItem>().FirstOrDefault(i => i.SecurityKey == WebSecurityRightsList.WebAccreditationsViewAll.Code);
			securityItem.Granted = true;
			securityItem.Skip = false;
			wrapper.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, "aaa"));
			AssertContactSecurity(contact1, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, true);
			AssertContactSecurity(contact2, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
			AssertContactSecurity(contact3, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
			AssertContactSecurity(contact4, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, true);
		}

		public void TestBulkUpdate_Delete()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code";
			OrgSecurity security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security.OX_IsCustomerManaged = true;
			foreach (OrgSecurity sec in org.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "aaa 1";
			contact.OC_Email = "c1@org.com";
			contact.OC_WebAccessEnabled = true;
			var securityDelete = Factory.New<OrgSecurityContacts>();
			securityDelete.OZ_OC = contact.PK;
			securityDelete.OZ_OX = security.PK;
			securityDelete.OZ_Granted = true;
			Factory.Save();
			var wrapper = new WebSecurityDataSource(Factory, org.PK);
			wrapper.SelectContact(contact);
			wrapper.InitBulkUpdate(0, 0);
			var securityItem = wrapper.BulkUpdateProfile.Items.Cast<SecurityItem>().FirstOrDefault(i => i.SecurityKey == WebSecurityRightsList.WebAccreditationsViewAll.Code);
			securityItem.Granted = false;
			securityItem.Skip = false;
			wrapper.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, "aaa"));
			AssertEquals(0, contact.SecurityRightsNoDummies.Count);
			AssertContactSecurity(contact, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
		}

		public void TestBulkUpdate_Skip()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code";
			OrgSecurity security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security.OX_IsCustomerManaged = true;
			foreach (OrgSecurity sec in org.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "aaa 1";
			contact.OC_Email = "c1@org.com";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			var wrapper = new WebSecurityDataSource(Factory, org.PK);
			wrapper.SelectContact(contact);
			wrapper.InitBulkUpdate(0, 0);
			var securityItem = wrapper.BulkUpdateProfile.Items.Cast<SecurityItem>().FirstOrDefault(i => i.SecurityKey == WebSecurityRightsList.WebAccreditationsViewAll.Code);
			securityItem.Granted = false;
			securityItem.Skip = false;
			wrapper.BulkUpdateAll(new ZQuery(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, "aaa"));
			AssertEquals(1, contact.SecurityRightsNoDummies.Count);
			AssertEquals(false, contact.SecurityRightsNoDummies[0].IsSavedByFactory);
			AssertContactSecurity(contact, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
		}

		public void TestBulkUpdate_Selected()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "~code";
			OrgSecurity security1 = (OrgSecurity)org1.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security1.OX_IsCustomerManaged = true;
			foreach (OrgSecurity sec in org1.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "aaa 1";
			contact1.OC_Email = "c1@org.com";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "bbb 2";
			contact2.OC_Email = "c2@org.com";
			contact2.OC_WebAccessEnabled = true;
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "ccc 3";
			contact3.OC_Email = "c3@org.com";
			contact3.OC_WebAccessEnabled = true;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "~code2";
			foreach (OrgSecurity sec in org2.SecurityRights)
			{
				sec.OX_Granted = false;
			}

			OrgSecurity security2 = (OrgSecurity)org2.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebAccreditationsViewAll.Code))[0];
			security2.OX_IsCustomerManaged = true;
			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "aaa 4";
			contact4.OC_Email = "c4@org.com";
			contact4.OC_WebAccessEnabled = true;
			Factory.Save();
			AssertEquals(false, contact1.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact2.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact3.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			AssertEquals(false, contact4.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebAccreditationsViewAll));
			var wrapper = new WebSecurityDataSource(Factory, org1.PK);
			wrapper.SelectContact(contact1);
			wrapper.InitBulkUpdate(0, 0);
			var securityItem = wrapper.BulkUpdateProfile.Items.Cast<SecurityItem>().FirstOrDefault(i => i.SecurityKey == WebSecurityRightsList.WebAccreditationsViewAll.Code);
			securityItem.Granted = true;
			securityItem.Skip = false;
			var collection = new OrgContactCollection(Factory);
			collection.Add(contact1);
			collection.Add(contact2);
			collection.Add(contact3);
			collection.Add(contact4);
			wrapper.BulkUpdateSelected(collection, new[] { contact2.PK, contact3.PK });
			AssertContactSecurity(contact1, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
			AssertContactSecurity(contact2, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, true);
			AssertContactSecurity(contact3, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, true);
			AssertContactSecurity(contact4, WebSecurityRightsList.WebAccreditationsViewAll.SecurityItemName, false);
		}

		void AssertContactSecurity(OrgContact contact, string name, bool hasGranted)
		{
			var securities = contact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>();
			bool found = false;
			foreach (OrgSecurityContacts sec in securities)
			{
				if (sec.SecurityItemName == name && sec.OZ_Granted)
				{
					found = true;
				}
			}

			AssertEquals(hasGranted, found);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}

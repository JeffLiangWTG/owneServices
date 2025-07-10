using System;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(OrgContactSupersededHelper))]
	[HttpContextEnabledTest]
	public class OrgContactSupersededHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasValidContacts()
		{
			var redirectContact = Factory.NewWithValidTestData<OrgContact>();
			redirectContact.SupersedeWebAccess();
			redirectContact.SetHashedPassword("1234");
			Factory.Save();

			var invalidHelper = GetSupersededHelper(null);
			AssertEquals("No redirect contact", false, invalidHelper.HasValidContacts);
			var validHelper = GetSupersededHelper(redirectContact);
			AssertEquals("No valid login contacts", false, validHelper.HasValidContacts);

			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = redirectContact.OC_PER;
			Factory.Save();
			validHelper.ContactsForLogin.Load();
			AssertEquals("Contact is not active", false, validHelper.HasValidContacts);

			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = redirectContact.OC_PER;
			Factory.Save();
			validHelper.ContactsForLogin.Load();
			AssertEquals("Contact has no web access", false, validHelper.HasValidContacts);

			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_WebAccessEnabled = true;
			Factory.Save();
			validHelper.ContactsForLogin.Load();
			AssertEquals("Contact is unrelated", false, validHelper.HasValidContacts);

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = redirectContact.OC_PER;
			Factory.Save();
			validHelper.ContactsForLogin.Load();
			AssertEquals("Contact is valid for login", true, validHelper.HasValidContacts);

			var activeHelper = GetSupersededHelper(activeContact);
			AssertEquals("Original contact must have web access superseded", false, activeHelper.HasValidContacts);
		}

		public void TestContactsForLogin()
		{
			var redirectContact = Factory.NewWithValidTestData<OrgContact>();
			redirectContact.SupersedeWebAccess();
			redirectContact.SetHashedPassword("1234");
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = redirectContact.OC_PER;
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = redirectContact.OC_PER;
			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = redirectContact.OC_PER;
			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_WebAccessEnabled = true;
			Factory.Save();

			Assert("Precondition", activeContact.OC_IsActive);
			Assert("Precondition", !inactiveContact.OC_IsActive);
			Assert("Precondition", webAccessDisabledContact.OC_IsActive);
			Assert("Precondition", unrelatedContact.OC_IsActive);
			Assert("Precondition", activeContact.OC_WebAccessEnabled);
			Assert("Precondition", inactiveContact.OC_WebAccessEnabled);
			Assert("Precondition", !webAccessDisabledContact.OC_WebAccessEnabled);
			Assert("Precondition", unrelatedContact.OC_WebAccessEnabled);
			AssertNotEquals("Precondition", redirectContact.OC_PER, unrelatedContact.OC_PER);

			var redirectContactHelper = GetSupersededHelper(redirectContact);
			AssertEquals("Only active related web enabled contacts are valid for login", 1, redirectContactHelper.ContactsForLogin.Count);
			AssertEquals("Active contact", activeContact.PK, redirectContactHelper.ContactsForLogin.First().PK);
		}

		public void TestContactsForDeactivation()
		{
			var redirectContact = Factory.NewWithValidTestData<OrgContact>();
			redirectContact.SupersedeWebAccess();
			redirectContact.SetHashedPassword("1234");
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = redirectContact.OC_PER;
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = redirectContact.OC_PER;
			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = redirectContact.OC_PER;
			var otherRedirectContact = Factory.NewWithValidTestData<OrgContact>();
			otherRedirectContact.SupersedeWebAccess();
			otherRedirectContact.OC_PER = redirectContact.OC_PER;
			var unrelatedRedirectContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedRedirectContact.SupersedeWebAccess();
			Factory.Save();

			Assert("Precondition", !activeContact.WebAccessSuperseded);
			Assert("Precondition", !inactiveContact.WebAccessSuperseded);
			Assert("Precondition", !webAccessDisabledContact.WebAccessSuperseded);
			Assert("Precondition", otherRedirectContact.WebAccessSuperseded);
			Assert("Precondition", unrelatedRedirectContact.WebAccessSuperseded);
			AssertNotEquals("Precondition", redirectContact.OC_PER, unrelatedRedirectContact.OC_PER);

			var redirectContactHelper = GetSupersededHelper(redirectContact);
			AssertEquals("All related contacts that are marked for redirection", 2, redirectContactHelper.ContactsForDeactivation.Count);
			AssertContainsExactElementsInAnyOrder(new[] { redirectContact.PK, otherRedirectContact.PK }, redirectContactHelper.ContactsForDeactivation.Select(x => x.PK));
		}

		public void TestDefaultLoginContact()
		{
			var redirectContact = Factory.NewWithValidTestData<OrgContact>();
			redirectContact.SupersedeWebAccess();
			redirectContact.SetHashedPassword("1234");
			redirectContact.OC_Email = "carrot@rabbit.com";
			Factory.Save();

			var contactWithDifferentEmail = Factory.NewWithValidTestData<OrgContact>();
			contactWithDifferentEmail.OC_WebAccessEnabled = true;
			contactWithDifferentEmail.OC_PER = redirectContact.OC_PER;
			contactWithDifferentEmail.OC_Email = "other@cover.com";
			var contactWithMatchingEmail = Factory.NewWithValidTestData<OrgContact>();
			contactWithMatchingEmail.OC_WebAccessEnabled = true;
			contactWithMatchingEmail.OC_PER = redirectContact.OC_PER;
			contactWithMatchingEmail.OC_Email = redirectContact.OC_Email;
			var primaryContact = Factory.NewWithValidTestData<OrgContact>();
			primaryContact.OC_WebAccessEnabled = true;
			primaryContact.OC_PER = redirectContact.OC_PER;
			primaryContact.OC_Email = "other@cover.com";
			redirectContact.Person.SetPrimaryRelationship(primaryContact);
			Factory.Save();

			var redirectContactHelper = GetSupersededHelper(redirectContact);
			AssertEquals("Precondition", 3, redirectContactHelper.ContactsForLogin.Count);
			AssertEquals("First priority is primary contact", primaryContact.PK, redirectContactHelper.DefaultLoginContact.Contact.PK);
			redirectContact.Person.PrimaryRelationship.Delete();
			Factory.Save();
			primaryContact.Delete();
			Factory.Save();
			redirectContactHelper = GetSupersededHelper(redirectContact);
			AssertEquals("Fallback is contact with matching email", contactWithMatchingEmail.PK, redirectContactHelper.DefaultLoginContact.Contact.PK);
			contactWithMatchingEmail.Delete();
			Factory.Save();
			redirectContactHelper = GetSupersededHelper(redirectContact);
			AssertEquals("Fallback to first contact", contactWithDifferentEmail.PK, redirectContactHelper.DefaultLoginContact.Contact.PK);
		}

		public void TestSendActiveContactAuthenticationEmail()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();

			Assert("Precondition", activeContact.OC_IsActive);
			Assert("Precondition", activeContact.OC_WebAccessEnabled);

			var supersededContact = Factory.NewWithValidTestData<OrgContact>();
			supersededContact.OC_Email = "trench@coat.com";
			supersededContact.OC_WebAccessEnabled = true;
			supersededContact.OC_PER = activeContact.OC_PER;
			supersededContact.SupersedeWebAccess();
			supersededContact.SetHashedPassword("1234");
			Assert("Precondition", supersededContact.OC_WebAccessEnabled);
			Assert("Precondition", supersededContact.WebAccessSuperseded);

			activeContact.OC_Email = "camo@mile.com";
			Factory.Save();

			var contactLoginHelper = GetSupersededHelper(supersededContact);
			var passwordBasePageUrl = new Uri("https://google.com");
			var originalUrl1 = new Uri("https://yahoo.com");
			var url = contactLoginHelper.GenerateSetMasterPasswordUrl(passwordBasePageUrl, originalUrl1, LoginRouterIdentityManager.GenerateToken(supersededContact));

			var values = HttpUtility.ParseQueryString(url.Query);
			var queryString = new SecureQueryString(WebUtility.UrlDecode(values[SecureQueryString.QueryStringKey]));
			var token = queryString[WebUserAdminManager.SetMasterPasswordKey];

			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			var accessTokens = Factory.Load<StmAccessToken>(query);

			AssertEquals(1, accessTokens.Length);
			var activeContactToken = accessTokens.FirstOrDefault(x => x.SAT_Scope == originalUrl1.AbsoluteUri);
			AssertNotNull(activeContactToken);

			AssertEquals("Token in url should match the saved token in the db", token, activeContactToken.SAT_Token);
		}

		public void TestSupersedeRedirectionContacts()
		{
			var redirectContact = Factory.NewWithValidTestData<OrgContact>();
			redirectContact.SupersedeWebAccess();
			redirectContact.SetHashedPassword("1234");
			Factory.Save();

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			activeContact.OC_PER = redirectContact.OC_PER;
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_WebAccessEnabled = true;
			inactiveContact.OC_PER = redirectContact.OC_PER;
			var webAccessDisabledContact = Factory.NewWithValidTestData<OrgContact>();
			webAccessDisabledContact.OC_WebAccessEnabled = false;
			webAccessDisabledContact.OC_PER = redirectContact.OC_PER;
			var otherRedirectContact = Factory.NewWithValidTestData<OrgContact>();
			otherRedirectContact.SupersedeWebAccess();
			otherRedirectContact.OC_PER = redirectContact.OC_PER;
			var unrelatedRedirectContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedRedirectContact.SupersedeWebAccess();
			Factory.Save();

			AssertEquals("Precondition", false, activeContact.WebAccessSuperseded);
			AssertEquals("Precondition", true, activeContact.OC_IsActive);
			AssertEquals("Precondition", false, inactiveContact.WebAccessSuperseded);
			AssertEquals("Precondition", false, inactiveContact.OC_IsActive);
			AssertEquals("Precondition", false, webAccessDisabledContact.WebAccessSuperseded);
			AssertEquals("Precondition", true, webAccessDisabledContact.OC_IsActive);
			AssertEquals("Precondition", true, otherRedirectContact.WebAccessSuperseded);
			AssertEquals("Precondition", false, otherRedirectContact.OC_IsActive);
			AssertEquals("Precondition", true, unrelatedRedirectContact.WebAccessSuperseded);
			AssertEquals("Precondition", false, unrelatedRedirectContact.OC_IsActive);
			AssertNotEquals("Precondition", redirectContact.OC_PER, unrelatedRedirectContact.OC_PER);

			var redirectContactHelper = GetSupersededHelper(redirectContact);
			redirectContactHelper.DeactivateRedirectionContacts();
			AssertEquals("Should be updated to hard inactive", false, redirectContact.WebAccessSuperseded);
			AssertEquals("Should be updated to hard inactive", false, redirectContact.OC_IsActive);
			AssertEquals("Should be updated to hard inactive", false, otherRedirectContact.WebAccessSuperseded);
			AssertEquals("Should be updated to hard inactive", false, otherRedirectContact.OC_IsActive);
			AssertEquals("Should be unaffected", false, activeContact.WebAccessSuperseded);
			AssertEquals("Should be unaffected", true, activeContact.OC_IsActive);
			AssertEquals("Should be unaffected", false, inactiveContact.WebAccessSuperseded);
			AssertEquals("Should be unaffected", false, inactiveContact.OC_IsActive);
			AssertEquals("Should be unaffected", false, webAccessDisabledContact.WebAccessSuperseded);
			AssertEquals("Should be unaffected", true, webAccessDisabledContact.OC_IsActive);
			AssertEquals("Should be unaffected", true, unrelatedRedirectContact.WebAccessSuperseded);
			AssertEquals("Should be unaffected", false, unrelatedRedirectContact.OC_IsActive);
		}

		#region Implementation

		protected virtual OrgContactSupersededHelper GetSupersededHelper(OrgContact contact)
		{
			return new OrgContactSupersededHelper(contact);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			return GetSupersededHelper(contact);
		}

		#endregion
	}
}

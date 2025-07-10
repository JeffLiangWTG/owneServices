using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
#if NETFRAMEWORK
using System.IO;
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public class OrgContactWebUserTest : WebUserTestCase
	{
		#region Setup

		protected override WebUser GetNewWebUser()
		{
			return new OrgContactWebUser();
		}

		protected OrgHeader Company
		{
			get { return fCompany ?? (fCompany = CreateNewCompany()); }
		}

		OrgHeader fCompany;

		protected virtual OrgHeader CreateNewCompany()
		{
			OrgHeader result = (OrgHeader)Factory.New(typeof(OrgHeader));
			result.OH_Code = "XXXXXX";
			result.OH_RL_NKClosestPort = "BFDOR";
			if (result.CompanyDataCollection.Count == 0)
			{
				result.CompanyDataCollection.AddNew();
			}
			result.CompanyDataCollection[0].OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			return result;
		}

		protected override IContactable CreateNewContact(string username, string email, string password)
		{
			return CreateNewContact(username, email, password, Company);
		}

		protected OrgContact CreateNewContact(string username, string email, string password, OrgHeader company)
		{
			OrgContact result = company.Contacts.AddNew();
			result.OC_ContactName = username;
			result.OC_Email = email;
			result.SetHashedPassword(password);
			result.OC_WebAccessEnabled = true;
			return result;
		}

		OrgContact CreateNewContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = CreateNewContact("test", "test@test.com", "test", org);

			return contact;
		}

		protected new OrgContactWebUser User
		{
			get { return (OrgContactWebUser)base.User; }
		}

		protected new OrgContact Contact
		{
			get { return (OrgContact)base.Contact; }
		}

		protected override string AffiliationCode
		{
			get { return Company.OH_Code; }
		}

		protected override string AffiliationName
		{
			get { return Company.OH_FullNameTruncated; }
		}

		#endregion

		[HttpContextEnabledTest]
		public void TestAllUserRelatedOrgs()
		{
			var contact1 = CreateNewContact();
			var contact2 = CreateNewContact();
			var contact3 = CreateNewContact();
			var contact4 = CreateNewContact();

			contact1.OC_IsActive = false;
			contact3.Header.OH_IsActive = false;
			Factory.Save();

			User.Login(contact1.Header.OH_Code, "test@test.com", "test");
			AssertAllUserRelatedOrgs(contact2.Header.PK, contact4.Header.PK);
		}

		protected virtual void AssertAllUserRelatedOrgs(params ZGuid[] expectedOrgsPK)
		{
			var relatedOrgs = User.AllUserRelatedOrgs;
			AssertEquals(2, relatedOrgs.Count);
			Assert(relatedOrgs.All(h => expectedOrgsPK.Contains(h.PK)));
		}

		[HttpContextEnabledTest]
		public void TestOrganisationRelatedOrgPKs()
		{
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader header0 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader header1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = header0.PK;
			relatedParty.PR_OH_RelatedParty = header1.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			OrgContact contact = header1.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			contact.OC_Email = "email@email.em";
			contact.SetHashedPassword("pswrd");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			User.Login(header1.OH_Code, "email@email.em", "pswrd");
			var result = new List<ZGuid>(User.OrganisationRelatedOrgAddressPKs);
			Assert(result.Contains(header0.MainAddress.PK));
			Assert(result.Contains(header1.MainAddress.PK));

			result = new List<ZGuid>(User.OrganisationRelatedOrgPKs);
			Assert(result.Contains(header0.PK));
			Assert(result.Contains(header1.PK));

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		[HttpContextEnabledTest]
		public void TestGetRelatedOrgQuery()
		{
			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header0 = Factory.NewWithValidTestData<OrgHeader>();
				var header1 = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = Factory.New<OrgRelatedParty>();
				relatedParty.PR_OH_Parent = header0.PK;
				relatedParty.PR_OH_RelatedParty = header1.PK;
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
				Factory.Save();

				var query = OrgContactWebUser.GetRelatedOrgQuery(header1.PK);

				var relatedOrgsPks = Factory.Load<OrgHeader>(query).Select(x => x.PK).ToList();

				AssertCollectionContains(header0.PK, relatedOrgsPks);
				AssertCollectionContains(header1.PK, relatedOrgsPks);
				AssertEquals(2, relatedOrgsPks.Count);
			}
		}

		public void TestContactRelatedAccountOrgs()
		{
			var commonPassword = "ChangeMe123!";
			var commonEmail = "email@common.com";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.SetHashedPassword(commonPassword);
			var contactA = Factory.NewWithValidTestData<OrgContact>();
			contactA.OC_PER = person.PK;
			contactA.OC_WebAccessEnabled = true;
			contactA.OC_Email = commonEmail;
			var samePersonContact = Factory.NewWithValidTestData<OrgContact>();
			samePersonContact.OC_PER = person.PK;
			samePersonContact.OC_WebAccessEnabled = true;
			samePersonContact.OC_Email = "other@email.com";
			var samePersonNonWebAccessContact = Factory.NewWithValidTestData<OrgContact>();
			samePersonNonWebAccessContact.OC_PER = person.PK;
			samePersonNonWebAccessContact.OC_WebAccessEnabled = false;
			samePersonNonWebAccessContact.OC_Email = "other@email.com";
			var samePersonInactiveContact = Factory.NewWithValidTestData<OrgContact>();
			samePersonInactiveContact.OC_PER = person.PK;
			samePersonInactiveContact.OC_WebAccessEnabled = true;
			samePersonInactiveContact.OC_IsActive = false;
			samePersonInactiveContact.OC_Email = "other@email.com";
			var samePersonContactWithInactiveOrg = Factory.NewWithValidTestData<OrgContact>();
			samePersonContactWithInactiveOrg.OC_PER = person.PK;
			samePersonContactWithInactiveOrg.OC_Email = "other@email.com";
			samePersonContactWithInactiveOrg.OC_WebAccessEnabled = true;
			samePersonContactWithInactiveOrg.Header.OH_IsActive = false;
			var sameEmailContact = Factory.NewWithValidTestData<OrgContact>();
			sameEmailContact.OC_Email = commonEmail;
			sameEmailContact.SetHashedPassword("blahBlahWhatever");
			sameEmailContact.OC_WebAccessEnabled = true;
			var sameEmailNonWebAccessContact = Factory.NewWithValidTestData<OrgContact>();
			sameEmailNonWebAccessContact.OC_Email = commonEmail;
			sameEmailNonWebAccessContact.OC_WebAccessEnabled = false;
			var sameEmailInactiveContact = Factory.NewWithValidTestData<OrgContact>();
			sameEmailInactiveContact.OC_Email = commonEmail;
			sameEmailInactiveContact.OC_WebAccessEnabled = true;
			sameEmailInactiveContact.OC_IsActive = false;
			var sameEmailContactWithInactiveOrg = Factory.NewWithValidTestData<OrgContact>();
			sameEmailContactWithInactiveOrg.OC_Email = commonEmail;
			sameEmailContactWithInactiveOrg.OC_WebAccessEnabled = true;
			sameEmailContactWithInactiveOrg.Header.OH_IsActive = false;
			var unrelatedContact = Factory.NewWithValidTestData<OrgContact>();
			unrelatedContact.OC_Email = "other@email.com";
			unrelatedContact.OC_WebAccessEnabled = true;
			Factory.Save();

			User.Login(contactA.OrganisationCode, commonEmail, commonPassword);
			var result = User.ContactRelatedAccounts.Select(x => x.PK).ToArray();
			Assert("Should contain org of contacts on the same person as logged in contact", result.Contains(contactA.PK));
			Assert("Should contain org of contacts on the same person as logged in contact", result.Contains(samePersonContact.PK));
			Assert("Should contain org of contacts with the same email as logged in contact", result.Contains(sameEmailContact.PK));
			Assert("Should not contain org of contacts without web access", !result.Contains(samePersonNonWebAccessContact.PK));
			Assert("Should not contain org of contacts without web access", !result.Contains(sameEmailNonWebAccessContact.PK));
			Assert("Should not contain org of inactive contacts", !result.Contains(samePersonInactiveContact.PK));
			Assert("Should not contain org of inactive contacts", !result.Contains(sameEmailInactiveContact.PK));
			Assert("Should not contain inactive org", !result.Contains(samePersonContactWithInactiveOrg.PK));
			Assert("Should not contain inactive org", !result.Contains(sameEmailContactWithInactiveOrg.PK));
			Assert("Should not contain org of unrelated contacts", !result.Contains(unrelatedContact.PK));
		}

		[HttpContextEnabledTest]
		public void TestSecurityItemGuidIsChecked()
		{
			var reportRights = ReportsWebSecurityRights.New(Factory);
			if (!reportRights.Any())
			{
				Assert(true);
				return;
			}

			var item = reportRights.First();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var right = orgHeader.SecurityRights.AddNew();
			right.OX_SU = item.SecurityGuid;
			right.OX_Granted = false;

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			contact.OC_Email = "email@email.em";
			contact.SetHashedPassword("pswrd");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			User.Login(orgHeader.OH_Code, "email@email.em", "pswrd");

			Assert("When OX_Granted is false, AreSecurityRightsGranted should return false", !User.AreSecurityRightsGranted(item));
			User.Logout();

			right.OX_Granted = true;
			Factory.Save();
			User.Login(orgHeader.OH_Code, "email@email.em", "pswrd");

			Assert("When OX_Granted is true, AreSecurityRightsGranted should return true", User.AreSecurityRightsGranted(item));
		}

		public void TestLoginWithPasswordHash()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact = header.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			contact.OC_Email = "email@email.em";
			contact.OC_WebAccessEnabled = true;

			UserSecretsContext.DefaultContext.SaveSecret("pswrd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10, contact.GetPasswordAdapter());

			Factory.Save();

			User.Login(header.OH_Code, "email@email.em", "pswrd");
			AssertEquals("Login worked", contact.PK, User.LoggedInWebContact.PK);
			User.Logout();

			contact.OC_PasswordHashIterations = 100;
			Factory.Save();

			User.Login(header.OH_Code, "email@email.em", "pswrd");
			AssertNull("Login failed", User.LoggedInWebContact);
		}

		[HttpContextEnabledTest]
		public void TestCheckingSecurityRightDoesntCreateLotsOfBizos_SecurityItemDoesntExist()
		{
			var webUser = new ContactWithExposedRightCheck();
			OrgHeader header;
			OrgContact contact;
			CreateUserAndLogIn(webUser, out header, out contact);
			var loginFactory = webUser.LoggedInWebContact.Factory;
			var preCount = SecurityItemsInCacheCount(loginFactory);

			var itemsLoaded = SecurityItemsInCacheCount(loginFactory) - preCount;
			var numberOfItems = AllWebSecurityRights.New(Factory).Count;
			Assert($"Only load the security items we need. Loaded: {itemsLoaded}. Nb of items: {numberOfItems}", itemsLoaded < numberOfItems);

			webUser.IsRightGranted(WebSecurityRightsList.eRequestPortalViewAll);

			itemsLoaded = SecurityItemsInCacheCount(loginFactory) - preCount;
			numberOfItems = AllWebSecurityRights.New(Factory).Count;
			Assert($"Only load the security items we need. Loaded: {itemsLoaded}. Nb of items: {numberOfItems}", itemsLoaded < numberOfItems);
		}

		class ContactWithExposedRightCheck : OrgContactWebUser
		{
			public new bool IsRightGranted(WebSecurityRight right)
			{
				return base.IsRightGranted(right);
			}
		}

		void CreateUserAndLogIn(OrgContactWebUser webUser, out OrgHeader header, out OrgContact contact)
		{
			var anotherFactory = new BusinessObjectFactory();
			header = anotherFactory.NewWithValidTestData<OrgHeader>();
			contact = header.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			contact.OC_Email = "email@email.em";
			contact.SetHashedPassword("pswrd");
			contact.OC_WebAccessEnabled = true;

			anotherFactory.Save();

			webUser.Login(header.OH_Code, "email@email.em", "pswrd");

			AssertEquals("PRE: Login worked", contact.PK, webUser.LoggedInWebContact.PK);
		}

		int SecurityItemsInCacheCount(BusinessObjectFactory factory)
		{
			return ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects.OfType<OrgSecurity>().Count();
		}

		[HttpContextEnabledTest]
		public void TestOrganisationRelatedOrgAddressPKs()
		{
			bool oldRegistryValue = WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value;

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader header0 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader header1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = header0.PK;
			relatedParty.PR_OH_RelatedParty = header1.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			OrgContact contact = header1.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			contact.OC_Email = "email@email.em";
			contact.SetHashedPassword("pswrd");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			User.Login(header1.OH_Code, "email@email.em", "pswrd");

			var result = new List<ZGuid>(User.OrganisationRelatedOrgAddressPKs);

			AssertEquals(result.Count, 2);
			Assert(result.Contains(header1.MainAddress.PK));

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			User.Login(header1.OH_Code, "email@email.em", "pswrd");
			result = new List<ZGuid>(User.OrganisationRelatedOrgAddressPKs);
			AssertEquals(result.Count, 1);

			WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
		}

		protected override void AssertNotLoggedIn(WebUser user)
		{
			OrgContactWebUser orgContactUser = (OrgContactWebUser)user;
			base.AssertNotLoggedIn(orgContactUser);
			AssertNull(orgContactUser.LoggedInWebOrganisation);
			AssertEquals(ZString.Empty, orgContactUser.CompanyName);
		}

		protected override void AssertSuccessfulLogin(WebUser user, string expectedUsername)
		{
			OrgContactWebUser orgContactUser = (OrgContactWebUser)user;
			base.AssertSuccessfulLogin(orgContactUser, expectedUsername);
			AssertEquals(Company.PK, orgContactUser.LoggedInWebOrganisation.PK);
			AssertEquals(Company.OH_FullName, orgContactUser.CompanyName);
		}

		#region TestLoginAndLogout

		public void TestLoginAndLogOut_Old()
		{
			User.Login("XXX", "XXX", "XXX");
			Assert(!User.IsLoggedIn);
			AssertEquals(null, User.LoggedInWebOrganisation);
			AssertEquals(ZString.Empty, User.LoggedInUserName);
			User.Login(Company.OH_Code, Email, Password);

			Assert(User.IsLoggedIn);
			AssertEquals(Company.PK, User.LoggedInWebOrganisation.PK);
			AssertEquals(UserName, User.LoggedInUserName);
			AssertEquals(Company.OH_FullName, User.CompanyName);

			User.Logout();

			Assert(!User.IsLoggedIn);
			AssertEquals(null, User.LoggedInWebOrganisation);
			AssertEquals("", User.CompanyName);
			AssertEquals("", User.LoggedInUserName);
		}

		public void TestCannotLoginIfInactiveLogin()
		{
			Company.OH_IsActive = ZBool.False;
			Factory.Save();
			User.Login(Company.OH_Code, Email, Password);

			Assert(!User.IsLoggedIn);

			Company.OH_IsActive = ZBool.True;
			Factory.Save();
			User.Login(Company.OH_Code, Email, Password);
			Assert(User.IsLoggedIn);

			User.Logout();
			Assert(!User.IsLoggedIn);

			Contact.OC_IsActive = ZBool.False;
			Factory.Save();
			User.Login(Company.OH_Code, Email, Password);
			Assert(!User.IsLoggedIn);

			Contact.OC_IsActive = ZBool.True;
			Factory.Save();
			User.Login(Company.OH_Code, Email, Password);
			Assert(User.IsLoggedIn);
		}

		void CreateLockoutUserRecord(OrgContact contact, string companyCode = "")
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = string.IsNullOrEmpty(companyCode) ? contact.OC_Email : (ZString)(contact.OC_Email + " " + companyCode);
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		public void TestCannotLoginIfLockedOut()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Contact.OC_PER = person.PK;
			CreateLockoutUserRecord(Contact, Company.OH_Code);
			Factory.Save();

			User.Login(Company.OH_Code, Email, Password);

			Assert(!User.IsLoggedIn);
			Assert(User.IsLockedOut);
		}

		public void TestIsLockedOutResetBetweenAttempts()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			Contact.OC_PER = person.PK;
			CreateLockoutUserRecord(Contact, Company.OH_Code);
			Factory.Save();

			User.Login(Company.OH_Code, Email, Password);

			Assert(User.IsLockedOut);

			new OrgContactLoginAttemptRecorder().Unlock(Company.OH_Code, Email, true);
			Factory.Save();

			User.Login(Company.OH_Code, Email, Password);

			Assert(!User.IsLockedOut);
		}

		public void TestRecordsLoginAttemptOnFailure()
		{
			var contactUsername = string.Empty;
			var mockLoginAttemptRecorder = new Mock<IOrgContactLoginAttemptRecorder>();
			mockLoginAttemptRecorder
				.Setup(x => x.RecordLoginAttempt(Company.OH_Code, Email, null))
				.Callback<string, string, byte[]>((companyCode, username, hash) => contactUsername = username);
			ObjectFactory.Substitute(mockLoginAttemptRecorder.Object);

			User.Login(Company.OH_Code, Email, "wrongPassword");

			mockLoginAttemptRecorder.Verify(x => x.RecordLoginAttempt(Company.OH_Code, Email, null));
			AssertEquals("Contact's login attempts recorded", Email, contactUsername);
		}

		public void TestShouldRecordLoginAttemptOnFailureForSupersededContact()
		{
			var contactUsername = string.Empty;
			var mockLoginAttemptRecorder = new Mock<IOrgContactLoginAttemptRecorder>();
			mockLoginAttemptRecorder
				.Setup(x => x.RecordLoginAttempt(Company.OH_Code, Email, null))
				.Callback<string, string, byte[]>((companyCode, username, hash) => contactUsername = username);
			ObjectFactory.Substitute(mockLoginAttemptRecorder.Object);
			Assert("Precondition", Contact.OC_WebAccessEnabled);
			Contact.SupersedeWebAccess();

			User.Login(Company.OH_Code, Email, "wrongPassword");

			mockLoginAttemptRecorder.Verify(x => x.RecordLoginAttempt(Company.OH_Code, Email, null));
			AssertEquals("Contact's login attempts recorded", Email, contactUsername);
		}

		public void TestLoginWithEdiSupportToken()
		{
			User.Login(Company.OH_Code, Email, "");
			Assert(!User.IsLoggedIn);
			AssertEquals(null, User.LoggedInWebOrganisation);
			AssertEquals("", User.CompanyName);
			AssertEquals("", User.LoggedInUserName);

			User.Login(Company.OH_Code, Email, CWSupportLoginToken.TokenForTest);
			Assert("Support token doesn't support normal account any more", !User.IsLoggedIn);
			AssertEquals(null, User.LoggedInWebOrganisation);
			AssertEquals("", User.CompanyName);
			AssertEquals("", User.LoggedInUserName);

			User.Login(Company.OH_Code, Environment.User.SupportUserName, CWSupportLoginToken.TokenForTest);
			Assert(User.IsLoggedIn);
			AssertEquals(Company.PK, User.LoggedInWebOrganisation.PK);
			AssertEquals(Environment.User.SupportUserName, User.LoggedInUserName);
			AssertEquals(Company.OH_FullName, User.CompanyName);
		}

		#endregion

		#region TestLoginForWebServices

		public void TestLoginForWebServices_Old()
		{
			Assert(!User.IsLoggedIn);

			AssertEquals("WebServiceUsername should be empty by default", "", WebDataRegistry.Instance.WebServiceUsername.Value);
			AssertEquals("WebServicePassword should be empty by default", "", WebDataRegistry.Instance.WebServicePassword.Value);

			User.Login(Company.OH_Code, "", "");
			AssertEquals("User should not be logged in with blank username / password", false, User.IsLoggedIn);

			try
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServiceUsername");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServicePassword");

				User.Login(Company.OH_Code, "TestWebServiceUsername", "");
				AssertEquals("User should not be logged in with blank password", false, User.IsLoggedIn);

				User.Login(Company.OH_Code, "", "TestWebServicePassword");
				AssertEquals("User should not be logged in with blank username", false, User.IsLoggedIn);

				User.Login(Company.OH_Code, "IncorrectUsername", "TestWebServicePassword");
				AssertEquals("User should not be logged in with incorrect username", false, User.IsLoggedIn);

				User.Login(Company.OH_Code, "TestWebServiceUsername", "IncorrectPassword");
				AssertEquals("User should not be logged in with incorrect password", false, User.IsLoggedIn);

				User.Login(Company.OH_Code, "TestWebServiceUsername", "TestWebServicePassword");
				Assert(User.IsLoggedIn);
				AssertEquals("WebServicesUser", User.LoggedInUserName);
				AssertEquals(Company.OH_FullName, User.CompanyName);
			}
			finally
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServiceUsername.Value))
				{
					ErrorReporter.ReportOnce("WebServiceUsername should be reset to empty string");
				}
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServicePassword.Value))
				{
					ErrorReporter.ReportOnce("WebServicePassword should be reset to empty string");
				}
			}
		}
		#endregion

		#region TestLoginForEDISupport
		public void TestLoginForEDISupport_Old()
		{
			Assert(!User.IsLoggedIn);

			User.LoginSupportForTest(Company.OH_Code);
			Assert(User.IsLoggedIn);
			AssertEquals(Environment.User.SupportUserName, User.LoggedInUserName);
			AssertEquals(Company.OH_FullName, User.CompanyName);
		}
		#endregion

		#region TestIsSuperUser
		public void TestIsSuperUser_Old()
		{
			Assert(!User.IsLoggedIn);
			Assert(!User.IsSuperUser);
			User.LoginSupportForTest(Company.OH_Code);
			Assert(User.IsLoggedIn);
			Assert(User.IsSuperUser);
		}
		#endregion

		#region TestContactAndCompanyReference
		public void TestContactAndCompanyReference()
		{
			User.LoginSupportForTest(Company.OH_Code);
			Assert(User.IsLoggedIn);
			AssertEquals("ContactAndCompanyReference for support", String.Format("{0} ({1})", Environment.User.SupportUserName, Company.OH_Code), User.ContactAndCompanyReference);
			User.Logout();

			User.Login(Company.OH_Code, Email, Password);
			Assert(User.IsLoggedIn);
			AssertEquals("ContactAndCompanyReference for Test Company", ZString.Format("user@user ({0})", Company.OH_Code), User.ContactAndCompanyReference);

			User.Logout();

			try
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServiceUsername");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServicePassword");

				User.Login(Company.OH_Code, "TestWebServiceUsername", "TestWebServicePassword");
				Assert(User.IsLoggedIn);
				AssertEquals("ContactAndCompanyReference for WebServices", ZString.Format("WebServicesUser ({0})", Company.OH_Code), User.ContactAndCompanyReference);
			}
			finally
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServiceUsername.Value))
				{
					ErrorReporter.ReportOnce("WebServiceUsername should be reset to empty string");
				}
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServicePassword.Value))
				{
					ErrorReporter.ReportOnce("WebServicePassword should be reset to empty string");
				}
			}
		}
		#endregion

		#region TestCurrentOrg

		[ExpectNoExceptions]
		public void TestCurrentOrg()
		{
			User.LoginSupportForTest(Company.OH_Code);

			AssertEquals("User should be logged in", true, User.IsLoggedIn);
			AssertEquals("CurrentOrg should be valid", true, User.CurrentOrg.IsValid);
			AssertEquals("CurrentOrg should be PK of the LoggedInOrg", Company.PK, User.CurrentOrg);

			User.Logout();

			AssertEquals("User should be logged out", false, User.IsLoggedIn);
			AssertEquals("CurrentOrg should be invalid", false, User.CurrentOrg.IsValid);
		}
		#endregion

		#region TestGetCountryCode
		public void TestGetCountryCode()
		{
			User.LoginSupportForTest(Company.OH_Code);

			AssertEquals(Company.Branch.Company.Country.Code, User.GetCountryCode());
			Company.CompanyDataCollection[0].OB_GB_ControllingBranch = ZGuid.Empty;
			Factory.Save();
			User.LoginSupportForTest(Company.OH_Code);
			AssertEquals(Company.CountryCode, User.GetCountryCode());
		}
		#endregion

		#region TestAllRelatedContacts

		public virtual void TestAllRelatedContactsAndOrgs()
		{
			Company.OH_FullName = "Borland";

			var company2 = CreateNewCompany();
			company2.OH_Code = "YYYYY";
			company2.OH_FullName = "Apple";
			CreateNewContact("user one", Email, Password, company2);
			var contact = CreateNewContact("user two", "other@email", "other", company2);
			Factory.Save();

			User.Login(Company.OH_Code, Email, Password);

			Assert(User.IsLoggedIn);

			AssertNotNull("AllRelatedOrgs", User.AllUserRelatedOrgs);
			AssertEquals("AllRelatedOrgs.Count", 2, User.AllUserRelatedOrgs.Count);
			AssertNotNull("Should contain organization Borland", User.AllUserRelatedOrgs.FindByPK(Company.PK));
			AssertNotNull("Should contain organization Apple", User.AllUserRelatedOrgs.FindByPK(company2.PK));
			Assert("Organizations should be sorted in alphabetical order by name", string.Compare(User.AllUserRelatedOrgs[0].OH_FullName, User.AllUserRelatedOrgs[1].OH_FullName) <= 0);

			User.Login(company2.OH_Code, contact.Email, Password);
			AssertNotNull("AllRelatedOrgs", User.AllUserRelatedOrgs);
			AssertEquals("AllRelatedOrgs.Count", 1, User.AllUserRelatedOrgs.Count);
			AssertNotNull("Should contain organization Apple", User.AllUserRelatedOrgs.FindByPK(company2.PK));
		}

		#endregion

		public void TestLoginFactoryCanBeGarbageCollected()
		{
			var userAndFactory = CreateLoggedUserInSeparateFunctionSoGarbageCollectionCanCollectLocalVars();
			GC.Collect();
			GC.WaitForFullGCComplete();
			GC.WaitForPendingFinalizers();
			AssertEquals("login factory is collected", false, userAndFactory.Item2.TryGetTarget(out var factory));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		Tuple<OrgContactWebUser, WeakReference<BusinessObjectFactory>> CreateLoggedUserInSeparateFunctionSoGarbageCollectionCanCollectLocalVars()
		{
			var user = User;
			Factory.RefreshEnabled = false;

			var helper = new ZWebTestHelper(Factory);
			Factory.Save();

			user.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			Assert("PRE", user.IsLoggedIn);
			var factoryRef = new WeakReference<BusinessObjectFactory>(user.LoggedInUser.Factory);
			return Tuple.Create(user, factoryRef);
		}

		public override void TestAreSecurityRightsGranted()
		{
			var webSecurityRight = AllWebSecurityRights.New(Factory).First();

			var helper = new ZWebTestHelper(Factory);
			var orgRight = helper.TestOrg.SecurityRights
				.Cast<OrgSecurity>()
				.First(right => right.OX_SecurityItemName == webSecurityRight.Code);

			orgRight.OX_Granted = false;

			var contactRight = helper.TestContact.SecurityRightsForBindingOnly[0];
			contactRight.OZ_OX = orgRight.PK;
			contactRight.OZ_Granted = false;

			Factory.Save();

			User.LoginSupportForTest(helper.TestOrg.OH_Code);
			Assert("Should always return true for SuperUser", User.AreSecurityRightsGranted(webSecurityRight));

			User.Logout();
			User.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert(!User.AreSecurityRightsGranted(webSecurityRight));

			orgRight.OX_Granted = true;
			contactRight.OZ_Granted = false;
			Factory.Save();
			User.Logout();
			User.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert(!User.AreSecurityRightsGranted(webSecurityRight));

			orgRight.OX_Granted = false;
			contactRight.OZ_Granted = true;
			Factory.Save();
			User.Logout();
			User.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert(User.AreSecurityRightsGranted(webSecurityRight));
		}

		public void TestCanPublishLayoutsWhenNotLoggedIn_ShouldNotThrowException()
		{
			var user = new OrgContactWebUser();
			Assert("The user should not be logged in.", !user.IsLoggedIn);
			var canPublish = user.CanPublishLayouts;
			Assert("Can publish did not return false for a user who is not logged in.", !canPublish);
		}

		[HttpContextEnabledTest]
		public virtual void TestCanPublishCompanyLayouts()
		{
			User.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);

			Assert(!User.CanPublishLayouts);
			Assert(!User.CanPublishCompanyLayouts);

			User.Logout();
			User.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);

			//Grant publish right, so it can publish to org but not to company
			var publishRight = User.LoggedInWebContact.GetContact().SecurityRightsForBindingOnly
				.Cast<OrgSecurityContacts>()
				.FirstOrDefault(right => right.Security.SecurityKey == WebSecurityRightsList.WebPublishLayouts.Code);

			AssertNotNull(publishRight);
			publishRight.OZ_Granted = true;

			Assert(User.CanPublishLayouts);
			Assert(!User.CanPublishCompanyLayouts);

			//Login company to be ProxyOrg and user can now publish to company
			((GlbCompany)EnvProxy.Instance.CurrentCompany).GC_OH_OrgProxy = Helper.TestOrg.PK;
			Assert(User.CanPublishLayouts);
			Assert(User.CanPublishCompanyLayouts);
		}

		#region TestCanAddNewOrganisations

		public virtual void TestCanAddNewOrganisations()
		{
			Helper.TestContact.OC_WebAccessEnabled = true;
			OrgSecurity orgRight = Helper.TestOrg.SecurityRights.AddNew();
			orgRight.OX_Granted = false;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.WebAddNewOrganisations.Code;
			Factory.Save();

			User.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);

			WebDataRegistry.Instance.AllowToAddNewOrganisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Assert("Should be false due to the registry setting", !User.CanAddNewOrganisations);

			WebDataRegistry.Instance.AllowToAddNewOrganisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Assert("Should be false due to the web security right", !User.CanAddNewOrganisations);

			orgRight.OX_Granted = true;
			Factory.Save();

			User.Logout();
			User.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);

			Assert("Should be true if both the registry setting and the web security right allow", User.CanAddNewOrganisations);
		}

		#endregion

		[TestDate(2018, 8, 14, 15, 30, 0)] // valid smalldatetime
		public void TestLoggedInWebOrganisation_GetHeader()
		{
			var user = User;
			user.Login(Company.OH_Code, Email, Password);
			Assert(user.IsLoggedIn);
			var webOrg = (WebOrg)user.LoggedInWebOrganisation;
			var firstFactoryInstance = webOrg.Factory._Instance;
			webOrg.DiscardAnyFactoryReferenceForTest();
#if NETFRAMEWORK
			var testContext = new HttpContext(new HttpRequest(string.Empty, "http://localhost", string.Empty), new HttpResponse(new StringWriter()));
#elif NET
			var testContext = new DefaultHttpContext();
			testContext.Request.Scheme = "http";
			testContext.Request.Host = new HostString("localhost");
#endif
			try
			{
#if NETFRAMEWORK
				HttpContext.Current = testContext;
#elif NET
				var mockAccessor = new Mock<IHttpContextAccessor>();
				var context = new DefaultHttpContext();
				context.Session = new DummySession();
				mockAccessor.Setup(a => a.HttpContext).Returns(context);
				WebEnv.HttpContextAccessor = mockAccessor.Object;
#endif

				var org = webOrg.GetHeader();
				AssertNotEquals("PRE: new factory", firstFactoryInstance, org.Factory._Instance);
				AssertArrayEqualsByElements(((INeedRow)Company).Row.ItemArray, ((INeedRow)org).Row.ItemArray);
				var expectedDbHits = new Dictionary<string, int>();
				expectedDbHits.Add(OrgHeaderSchema.Constants.TableName, 0);
				AssertDbHits(expectedDbHits, org.Factory);
			}
			finally
			{
#if NETFRAMEWORK
				HttpContext.Current = null;
#elif NET
				WebEnv.HttpContextAccessor = null;
#endif
			}
		}

		public void TestLoginSpecialUser_DbHitsForOrgWithMultipleContacts()
		{
			var org = Company;
			for (int i = 1; i <= 5; ++i)
			{
				var name = "user" + i.ToString(CultureInfo.InvariantCulture);
				CreateNewContact(name, name + "@test.org", name);
			}
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
				};
			var user = User;
			user.LoginSupportForTest(Company.OH_Code);
			AssertDbHits(expectedDbHits, user.LoggedInOrgContact.Factory);
		}

		public void TestLoginSupersededContact()
		{
			var org = Company;
			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			Factory.Save();

			var supersededWebAccessContact = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact.OC_WebAccessEnabled = true;
			supersededWebAccessContact.SupersedeWebAccess();
			supersededWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			supersededWebAccessContact.SetHashedPassword("1234");

			Factory.Save();

			User.Login(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not log in superseded contact", false, User.IsLoggedIn);
		}

		public void TestIsSpecialLogin()
		{
			WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");
			WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "there");
			var org = Company;
			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			Factory.Save();

			AssertEquals(false, User.IsSpecialLogin(string.Empty, WebDataRegistry.Instance.WebServicePassword.Value));
			AssertEquals(false, User.IsSpecialLogin(WebDataRegistry.Instance.WebServiceUsername.Value, string.Empty));
			AssertEquals(false, User.IsSpecialLogin(string.Empty, string.Empty));
			AssertEquals(false, User.IsSpecialLogin(activeWebAccessContact.OC_Email, "1234"));
			AssertEquals(true, User.IsSpecialLogin(WebDataRegistry.Instance.WebServiceUsername.Value, WebDataRegistry.Instance.WebServicePassword.Value));
			AssertEquals(true, User.IsSpecialLogin(Enterprise.ZArchitecture.Environment.User.WebUserName, Enterprise.ZArchitecture.Environment.User.WebTransientPassword));
			AssertEquals(true, User.IsSpecialLogin(Enterprise.ZArchitecture.Environment.User.SupportUserName, CWSupportLoginToken.TokenForTest));
		}

		#region GetLoginContact

		public void TestGetLoginContact()
		{
			var org = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			var activeNonWebAccessContact = CreateNewContact("user2", "user@2.com", "1234", org);
			activeNonWebAccessContact.OC_WebAccessEnabled = false;
			activeNonWebAccessContact.SetHashedPassword("1234");
			var inactiveWebAccessContact = CreateNewContact("user3", "user@3.com", "1234", org);
			inactiveWebAccessContact.OC_WebAccessEnabled = true;
			inactiveWebAccessContact.OC_IsActive = false;
			inactiveWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var loginContact = User.GetLoginContact(org.OH_Code, activeWebAccessContact.OC_Email, "1234");
			AssertEquals("Should get contact", activeWebAccessContact.PK, loginContact.PK);
			loginContact = User.GetLoginContact(org2.OH_Code, activeWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get contact with different company code", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, activeWebAccessContact.OC_Email, "4352");
			AssertEquals("Should not get contact with incorrect password", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, activeNonWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get contact with no web access", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, inactiveWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get inactive contact", null, loginContact);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactSupersededContact()
		{
			var org = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var activeNonWebAccessContact = CreateNewContact("user2", "user@2.com", "1234", org);
			activeNonWebAccessContact.OC_WebAccessEnabled = false;
			activeNonWebAccessContact.SetHashedPassword("1234");
			var inactiveWebAccessContact = CreateNewContact("user3", "user@3.com", "1234", org);
			inactiveWebAccessContact.OC_WebAccessEnabled = true;
			inactiveWebAccessContact.OC_IsActive = false;
			inactiveWebAccessContact.SetHashedPassword("1234");
			var inactiveNonWebAccessContact = CreateNewContact("user4", "user@4.com", "1234", org);
			inactiveNonWebAccessContact.OC_WebAccessEnabled = false;
			inactiveNonWebAccessContact.OC_IsActive = false;
			inactiveNonWebAccessContact.SetHashedPassword("1234");
			var supersededWebAccessContact1 = CreateNewContact("user10", "user@10.com", "9999", org);
			supersededWebAccessContact1.OC_WebAccessEnabled = true;
			supersededWebAccessContact1.SetHashedPassword("9999");
			var supersededWebAccessContact2 = CreateNewContact("user6", "user@5.com", "9999", org);
			supersededWebAccessContact2.OC_WebAccessEnabled = true;
			supersededWebAccessContact2.SetHashedPassword("9999");
			var supersededNoWebAccessContact = CreateNewContact("user7", "user@7.com", "1234", org);
			supersededNoWebAccessContact.OC_WebAccessEnabled = false;
			supersededNoWebAccessContact.SetHashedPassword("1234");
			supersededNoWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			var supersededWebAccessContact3 = CreateNewContact("user58", "user@5.com", "1234", org2);
			supersededWebAccessContact3.OC_WebAccessEnabled = true;
			supersededWebAccessContact3.SetHashedPassword("1234");
			supersededWebAccessContact3.OC_PER = activeNonWebAccessContact.OC_PER;
			var supersededWebAccessContact4 = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact4.OC_WebAccessEnabled = true;
			supersededWebAccessContact4.SetHashedPassword("1234");
			supersededWebAccessContact4.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact1.SupersedeWebAccess();
			supersededWebAccessContact2.SupersedeWebAccess();
			supersededNoWebAccessContact.SupersedeWebAccess();
			supersededWebAccessContact3.SupersedeWebAccess();
			supersededWebAccessContact4.SupersedeWebAccess();
			Factory.Save();

			var loginContact = User.GetLoginContact(org.OH_Code, supersededNoWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get contact if no web access", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact1.OC_Email, "9999");
			AssertEquals("Should not get contact if no contact linked via person", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact2.OC_Email, "9999");
			AssertEquals("Should not get contact if contact linked via person is inactive", null, loginContact);
			loginContact = User.GetLoginContact(org2.OH_Code, supersededWebAccessContact3.OC_Email, "1234");
			AssertEquals("Should not get contact if contact linked via person has no web access", null, loginContact);
			loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact4.OC_Email, "1234");
			AssertEquals("Should get superseded contact", supersededWebAccessContact4.PK, loginContact.PK);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactExpiredSupersededContact()
		{
			WebDataRegistry.Instance.ContactRedirectionExpiryDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			var org = Company;

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var supersededWebAccessContact = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact.OC_WebAccessEnabled = true;
			supersededWebAccessContact.SetHashedPassword("1234");
			supersededWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			var loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Should get superseded contact", supersededWebAccessContact.PK, loginContact.PK);

			TestDateAttribute.AddDays(5);

			loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Supersede should have expired", null, loginContact);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactExpiredThenReSupersededContact()
		{
			WebDataRegistry.Instance.ContactRedirectionExpiryDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			var org = Company;

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var supersededWebAccessContact = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact.OC_WebAccessEnabled = true;
			supersededWebAccessContact.SetHashedPassword("1234");
			supersededWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			var loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Should get superseded contact", supersededWebAccessContact.PK, loginContact.PK);

			TestDateAttribute.AddDays(5);

			supersededWebAccessContact.CancelWebAccessSuperseded();
			Factory.Save();
			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			loginContact = User.GetLoginContact(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Supersede should not be expired since it has been refreshed with a new log", supersededWebAccessContact.PK, loginContact.PK);
		}

		public void TestGetLoginContactShouldReturnNullIfUsernameBlank()
		{
			var org = Company;
			var activeWebAccessContact = CreateNewContact("user1", string.Empty, string.Empty, org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			Factory.Save();

			AssertEquals("Should return null", null, User.GetLoginContact(org.OH_Code, string.Empty, string.Empty));
		}

		#endregion

		#region GetLoginContacts

		public void TestGetLoginContacts()
		{
			var org = Company;

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			var activeNonWebAccessContact = CreateNewContact("user2", "user@2.com", "1234", org);
			activeNonWebAccessContact.OC_WebAccessEnabled = false;
			activeNonWebAccessContact.SetHashedPassword("1234");
			var inactiveWebAccessContact = CreateNewContact("user3", "user@3.com", "1234", org);
			inactiveWebAccessContact.OC_WebAccessEnabled = true;
			inactiveWebAccessContact.OC_IsActive = false;
			inactiveWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(org.OH_Code, activeWebAccessContact.OC_Email, "1234");
			AssertEquals("Should return single contact", LoginContactsResult.Success, result);
			AssertEquals("Should return single contact", 1, loginContacts.Length);
			AssertEquals("Should get contact", activeWebAccessContact.PK, loginContacts[0].PK);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, activeWebAccessContact.OC_Email, "4352");
			AssertEquals("Should not get any contacts", LoginContactsResult.Failure, result);
			AssertEquals("Should not get contacts with incorrect password", 0, loginContacts.Length);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, activeNonWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get any contacts", LoginContactsResult.Failure, result);
			AssertEquals("Should not get contact with no web access", 0, loginContacts.Length);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, inactiveWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get any contacts", LoginContactsResult.Failure, result);
			AssertEquals("Should not get inactive contact", 0, loginContacts.Length);
		}

		public void TestGetLoginContactsShouldReturnAllMatchingContacts()
		{
			var org1 = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_IsActive = true;
			contactMatch1.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_IsActive = true;
			contactMatch2.SetHashedPassword(commonPassword);
			var contactNonMatch3 = CreateNewContact("user1", commonEmail, string.Empty, org3);
			contactNonMatch3.OC_WebAccessEnabled = true;
			contactNonMatch3.OC_IsActive = true;
			contactNonMatch3.SetHashedPassword("DifferentPass123!");
			var contactNonMatch4 = CreateNewContact("user1", "different@email.com", string.Empty, org4);
			contactNonMatch4.OC_WebAccessEnabled = true;
			contactNonMatch4.OC_IsActive = true;
			contactNonMatch4.SetHashedPassword(commonPassword);
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(org1.OH_Code, commonEmail, commonPassword);
			AssertEquals("Should return first and second contact", LoginContactsResult.Success, result);
			AssertEquals("Should return first and second contact", 2, loginContacts.Length);
			var loginContactPKs = loginContacts.Select(x => x.PK);
			AssertContainsExactElementsInAnyOrder("Should return first and second contact", new[] { contactMatch1.PK, contactMatch2.PK }, loginContactPKs);
		}

		public void TestGetLoginContactsMoreThan5Persons()
		{
			var org1 = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			var org6 = Factory.NewWithValidTestData<OrgHeader>();
			var org7 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = Factory.NewWithValidTestData<GlbPerson>();
			var person4 = Factory.NewWithValidTestData<GlbPerson>();
			var person5 = Factory.NewWithValidTestData<GlbPerson>();
			var person6 = Factory.NewWithValidTestData<GlbPerson>();
			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_PER = person1.PK;
			contactMatch1.Person.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_PER = person2.PK;
			contactMatch2.Person.SetHashedPassword("differentPassword1!");
			var contactMatch3 = CreateNewContact("user1", commonEmail, string.Empty, org3);
			contactMatch3.OC_WebAccessEnabled = true;
			contactMatch3.OC_PER = person3.PK;
			contactMatch3.Person.SetHashedPassword(commonPassword);
			var contactMatch4 = CreateNewContact("user1", commonEmail, string.Empty, org4);
			contactMatch4.OC_WebAccessEnabled = true;
			contactMatch4.OC_PER = person4.PK;
			contactMatch4.Person.SetHashedPassword(commonPassword);
			var contactMatch5 = CreateNewContact("user1", commonEmail, string.Empty, org5);
			contactMatch5.OC_WebAccessEnabled = true;
			contactMatch5.OC_PER = person5.PK;
			contactMatch5.Person.SetHashedPassword(commonPassword);
			var supersededContactMatch6 = CreateNewContact("user1", commonEmail, string.Empty, org6);
			supersededContactMatch6.OC_WebAccessEnabled = true;
			supersededContactMatch6.OC_PER = person6.PK;
			supersededContactMatch6.Person.SetHashedPassword(commonPassword);

			var unrelatedActiveContactForSupersededLogin = CreateNewContact("user1", "otherEmail@work.com", string.Empty, org7);
			unrelatedActiveContactForSupersededLogin.OC_WebAccessEnabled = true;
			unrelatedActiveContactForSupersededLogin.OC_IsActive = true;
			unrelatedActiveContactForSupersededLogin.SetHashedPassword(commonPassword);
			unrelatedActiveContactForSupersededLogin.OC_PER = person6.PK;
			Factory.Save();

			supersededContactMatch6.SupersedeWebAccess();
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(string.Empty, commonEmail, commonPassword);
			AssertEquals("Should not return success because there are too many matches to hash all the passwords", LoginContactsResult.TooManyContacts, result);
			AssertEquals("Should return all contacts without matching passwords (including superseded)", 6, loginContacts.Length);
		}

		public void TestGetLoginContactsShouldCallRecordAndVerifyLoginOncePerContactWithNoPersonPassword()
		{
			var org1 = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_PER = person1.PK;
			contactMatch1.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_PER = person1.PK;
			contactMatch2.SetHashedPassword(commonPassword);
			Factory.Save();

			var webUser = GetNewWebUserForRecordAndVerifyCountTest();
			var (result, loginContacts) = webUser.GetLoginContacts(string.Empty, commonEmail, commonPassword);
			AssertEquals("Should return both contacts", LoginContactsResult.Success, result);
			AssertEquals("Should return both contacts", 2, loginContacts.Length);
			AssertEquals("Should call RecordAndVerify once per contact password", 2, webUser.RecordAndVerifyCallCount);
		}

		public void TestGetLoginContactsShouldCallRecordAndVerifyLoginOncePerPerson()
		{
			var org1 = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var person3 = Factory.NewWithValidTestData<GlbPerson>();

			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_PER = person1.PK;
			contactMatch1.Person.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_PER = person1.PK;
			contactMatch2.SetHashedPassword(commonPassword);
			var contactMatch3 = CreateNewContact("user1", commonEmail, string.Empty, org3);
			contactMatch3.OC_WebAccessEnabled = true;
			contactMatch3.OC_PER = person2.PK;
			contactMatch3.Person.SetHashedPassword(commonPassword);
			var contactMatch4 = CreateNewContact("user1", commonEmail, string.Empty, org4);
			contactMatch4.OC_WebAccessEnabled = true;
			contactMatch4.OC_PER = person3.PK;
			contactMatch4.SetHashedPassword(commonPassword);
			Factory.Save();

			var webUser = GetNewWebUserForRecordAndVerifyCountTest();
			var (result, loginContacts) = webUser.GetLoginContacts(string.Empty, commonEmail, commonPassword);
			AssertEquals("Should return all the contacts", LoginContactsResult.Success, result);
			AssertEquals("Should return all the contacts", 4, loginContacts.Length);
			AssertEquals("Should call RecordAndVerify once per contact password", 3, webUser.RecordAndVerifyCallCount);
		}

		OrgContactWebUserForTest GetNewWebUserForRecordAndVerifyCountTest() => new OrgContactWebUserForTest();

		public void TestGetLoginContactsShouldReturnNullIfUsernameBlank()
		{
			var org = Company;
			var activeWebAccessContact = CreateNewContact("user1", string.Empty, string.Empty, org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(string.Empty, string.Empty, string.Empty);
			AssertEquals("Should not return any contacts", LoginContactsResult.Failure, result);
			AssertEquals("Should not return any contacts", 0, loginContacts.Length);
		}

		class OrgContactWebUserForTest : OrgContactWebUser
		{
			protected override bool RecordAndVerifyLogin(OrgContact contact, string password, OrgContactCollection passwordFailures, string companyCode, byte[] loginHash)
			{
				RecordAndVerifyCallCount += 1;
				return base.RecordAndVerifyLogin(contact, password, passwordFailures, companyCode, loginHash);
			}

			public int RecordAndVerifyCallCount;
		}

		#region Login Attempts

		public void TestGetLoginContactsShouldRecordLoginAttemptForUsernameOnFailure()
		{
			var org1 = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			const string commonEmail = "user@1.com";
			const string commonPassword = "ChangeMe123!";

			var contactMatch1 = CreateNewContact("user1", commonEmail, string.Empty, org1);
			contactMatch1.OC_WebAccessEnabled = true;
			contactMatch1.OC_IsActive = true;
			contactMatch1.SetHashedPassword(commonPassword);
			var contactMatch2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contactMatch2.OC_WebAccessEnabled = true;
			contactMatch2.OC_IsActive = true;
			contactMatch2.SetHashedPassword(commonPassword);
			var contactNonMatch3 = CreateNewContact("user1", commonEmail, string.Empty, org3);
			contactNonMatch3.OC_WebAccessEnabled = true;
			contactNonMatch3.OC_IsActive = true;
			contactNonMatch3.SetHashedPassword("DifferentPass123!");
			var contactNonMatch4 = CreateNewContact("user1", "different@email.com", string.Empty, org4);
			contactNonMatch4.OC_WebAccessEnabled = true;
			contactNonMatch4.OC_IsActive = true;
			contactNonMatch4.SetHashedPassword(commonPassword);
			Factory.Save();

			var contactUsername = string.Empty;

			var mockLoginAttemptRecorder = new Mock<IOrgContactLoginAttemptRecorder>();
			mockLoginAttemptRecorder
				.Setup(x => x.RecordLoginAttempt(string.Empty, commonEmail, null))
				.Callback<string, string, byte[]>((companyCode, username, hash) => contactUsername = username);
			ObjectFactory.Substitute(mockLoginAttemptRecorder.Object);

			var (result, loginContacts) = User.GetLoginContacts(string.Empty, commonEmail, "nonMatchingPass123!");
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			AssertEquals("Should not return any contacts", 0, loginContacts.Length);

			mockLoginAttemptRecorder.Verify(x => x.RecordLoginAttempt(string.Empty, commonEmail, null));

			AssertEquals("Should record an attempt for the login email", commonEmail, contactUsername);
		}

		public void TestGetLoginContactsShouldNotReturnLockedOutContacts()
		{
			var org = Company;
			const string email = "user@1.com";
			const string password = "ChangeMe123!";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = CreateNewContact("user1", email, string.Empty, org);
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.SetHashedPassword(password);
			contact.OC_PER = person.PK;
			CreateLockoutUserRecord(contact);
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(string.Empty, email, password);
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			AssertEquals("Locked out contact should not be returned", 0, loginContacts.Length);
		}

		#endregion

		#region Superseded Contacts

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactsSupersededContact()
		{
			var org = Company;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var activeNonWebAccessContact = CreateNewContact("user2", "user@2.com", "1234", org);
			activeNonWebAccessContact.OC_WebAccessEnabled = false;
			activeNonWebAccessContact.SetHashedPassword("1234");
			var inactiveWebAccessContact = CreateNewContact("user3", "user@3.com", "1234", org);
			inactiveWebAccessContact.OC_WebAccessEnabled = true;
			inactiveWebAccessContact.OC_IsActive = false;
			inactiveWebAccessContact.SetHashedPassword("1234");
			var inactiveNonWebAccessContact = CreateNewContact("user4", "user@4.com", "1234", org);
			inactiveNonWebAccessContact.OC_WebAccessEnabled = false;
			inactiveNonWebAccessContact.OC_IsActive = false;
			inactiveNonWebAccessContact.SetHashedPassword("1234");
			var supersededWebAccessContact1 = CreateNewContact("user10", "user@10.com", "9999", org);
			supersededWebAccessContact1.OC_WebAccessEnabled = true;
			supersededWebAccessContact1.SetHashedPassword("9999");
			var supersededWebAccessContact2 = CreateNewContact("user6", "user@5.com", "9999", org);
			supersededWebAccessContact2.OC_WebAccessEnabled = true;
			supersededWebAccessContact2.SetHashedPassword("9999");
			var supersededNoWebAccessContact = CreateNewContact("user7", "user@7.com", "1234", org);
			supersededNoWebAccessContact.OC_WebAccessEnabled = false;
			supersededNoWebAccessContact.SetHashedPassword("1234");
			supersededNoWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			var supersededWebAccessContact3 = CreateNewContact("user58", "user@5.com", "1234", org2);
			supersededWebAccessContact3.OC_WebAccessEnabled = true;
			supersededWebAccessContact3.SetHashedPassword("1234");
			supersededWebAccessContact3.OC_PER = activeNonWebAccessContact.OC_PER;
			var supersededWebAccessContact4 = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact4.OC_WebAccessEnabled = true;
			supersededWebAccessContact4.SetHashedPassword("1234");
			supersededWebAccessContact4.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact1.SupersedeWebAccess();
			supersededWebAccessContact2.SupersedeWebAccess();
			supersededNoWebAccessContact.SupersedeWebAccess();
			supersededWebAccessContact3.SupersedeWebAccess();
			supersededWebAccessContact4.SupersedeWebAccess();
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededNoWebAccessContact.OC_Email, "1234");
			AssertEquals("Should not get contact if no web access", 0, loginContacts.Length);
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact1.OC_Email, "9999");
			AssertEquals("Should not get contact if no contact linked via person", 0, loginContacts.Length);
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact2.OC_Email, "9999");
			AssertEquals("Should not get contact if contact linked via person is inactive", 0, loginContacts.Length);
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			(result, loginContacts) = User.GetLoginContacts(org2.OH_Code, supersededWebAccessContact3.OC_Email, "1234");
			AssertEquals("Should not get contact if contact linked via person has no web access", 0, loginContacts.Length);
			AssertEquals("Should fail", LoginContactsResult.Failure, result);
			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact4.OC_Email, "1234");
			AssertEquals("Should get superseded contact", 1, loginContacts.Length);
			AssertEquals("Should get superseded contact", LoginContactsResult.Success, result);
			AssertEquals("Should get superseded contact", supersededWebAccessContact4.PK, loginContacts[0].PK);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactsExpiredSupersededContact()
		{
			WebDataRegistry.Instance.ContactRedirectionExpiryDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			var org = Company;

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var supersededWebAccessContact = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact.OC_WebAccessEnabled = true;
			supersededWebAccessContact.SetHashedPassword("1234");
			supersededWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Should get superseded contact", 1, loginContacts.Length);
			AssertEquals("Should get superseded contact", LoginContactsResult.Success, result);
			AssertEquals("Should get superseded contact", supersededWebAccessContact.PK, loginContacts[0].PK);

			TestDateAttribute.AddDays(5);

			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Supersede should have expired", 0, loginContacts.Length);
			AssertEquals(LoginContactsResult.Failure, result);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetLoginContactsExpiredThenReSupersededContact()
		{
			WebDataRegistry.Instance.ContactRedirectionExpiryDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			var org = Company;

			var activeWebAccessContact = CreateNewContact("user1", "user@1.com", "1234", org);
			activeWebAccessContact.OC_WebAccessEnabled = true;
			activeWebAccessContact.OC_IsActive = true;
			activeWebAccessContact.SetHashedPassword("1234");
			Factory.Save();

			var supersededWebAccessContact = CreateNewContact("user5", "user@6.com", "1234", org);
			supersededWebAccessContact.OC_WebAccessEnabled = true;
			supersededWebAccessContact.SetHashedPassword("1234");
			supersededWebAccessContact.OC_PER = activeWebAccessContact.OC_PER;
			Factory.Save();

			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			var (result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Should get superseded contact", 1, loginContacts.Length);
			AssertEquals("Should get superseded contact", LoginContactsResult.Success, result);
			AssertEquals("Should get superseded contact", supersededWebAccessContact.PK, loginContacts[0].PK);

			TestDateAttribute.AddDays(5);

			supersededWebAccessContact.CancelWebAccessSuperseded();
			Factory.Save();
			supersededWebAccessContact.SupersedeWebAccess();
			Factory.Save();

			(result, loginContacts) = User.GetLoginContacts(org.OH_Code, supersededWebAccessContact.OC_Email, "1234");
			AssertEquals("Supersede should not be expired since it has been refreshed with a new log", 1, loginContacts.Length);
			AssertEquals("Supersede should not be expired since it has been refreshed with a new log", LoginContactsResult.Success, result);
			AssertEquals("Supersede should not be expired since it has been refreshed with a new log", supersededWebAccessContact.PK, loginContacts[0].PK);
		}

		#endregion

		[TestDate(2022, 2, 25)]
		public void TestSetUserWebContractSignedAndSaveToDb()
		{
			User.Login(Company.OH_Code, Email, Password);
			Assert("Precondition", User.LoggedInOrgContact.OC_WebContractSignedDate.IsEmpty);
			Assert("Precondition", User.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty);

			User.SetUserWebContractSignedAndSaveToDb();

			var expectedSignedDate = new ZDateTime(2022, 2, 25);
			AssertEquals(expectedSignedDate, User.LoggedInOrgContact.OC_WebContractSignedDate);
			AssertEquals(expectedSignedDate, User.LoggedInWebContact.OC_WebContractSignedDate);

			TestDateAttribute.AddDays(5);
			User.SetUserWebContractSignedAndSaveToDb();

			var expectedUpdatedSignedDate = expectedSignedDate.AddDays(5);
			AssertEquals(expectedUpdatedSignedDate, User.LoggedInOrgContact.OC_WebContractSignedDate);
			AssertEquals(expectedUpdatedSignedDate, User.LoggedInWebContact.OC_WebContractSignedDate);
		}

		#endregion
	}
}

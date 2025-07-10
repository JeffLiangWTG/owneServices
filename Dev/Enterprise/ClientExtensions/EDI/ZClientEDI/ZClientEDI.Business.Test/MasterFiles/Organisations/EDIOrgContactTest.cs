using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Certification.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgContact))]
	internal class EDIOrgContactTest : OrgContactTest
	{
		public void TestCompanyName()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_FullName = "Org Name";

			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = org.PK;

			var address = org.Addresses.AddNew();
			address.OA_CompanyNameOverride = "Company Name";

			AssertEquals("Org Name", contact.CompanyName);

			contact.OC_OA_OrgAddress = address.PK;
			AssertEquals("Company Name", contact.CompanyName);
		}

		public void TestTypeDecider()
		{
			AssertEquals(typeof(EDIOrgContact), Factory.New<OrgContact>().GetType());
		}

		public void TestRelatedCertificateApplicant()
		{
			var contact = Factory.New<EDIOrgContact>();
			contact.OC_Email = "newuser@cargowise.com";
			AssertNull(contact.RelatedCertificateApplicant);

			var applicant = Factory.New<CertificateApplicant>();
			applicant.HA_EmailAddress = "olduser@cargowise.com";
			AssertNull("Should not match", contact.RelatedCertificateApplicant);

			applicant.HA_EmailAddress = "newuser@cargowise.com";
			AssertEquals(applicant, contact.RelatedCertificateApplicant);
		}

		public void TestLogMyAccountDisclaimerAcknowledgementIfRequired()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var enterprise = database.LicEnterprise;

			var org = database.LicEnterprise.Organisation;
			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "User 1";

			Factory.Save();

			var logs = contact.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecutedCode);
			AssertEquals(0, logs.Count());

			contact.LogMyAccountDisclaimerAcknowledgementIfRequired(database);
			Factory.Save();
			logs = contact.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecutedCode);
			AssertEquals(1, logs.Count());
			AssertEquals("MyAccount Disclaimer Authentication Staff Profile Collection Acknowledged by User via CargoWise One System DDD-SYD", logs.First().SL_Reference);

			contact.LogMyAccountDisclaimerAcknowledgementIfRequired(database);
			Factory.Save();
			logs = contact.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecutedCode);
			AssertEquals(1, logs.Count());
		}

		public void TestAdditionalProperties()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			AssertEquals("Header", contact.CompanyNameForBindingOnly);
			AssertEquals("#1", contact.BranchForBindingOnly);
		}

		public new void TestGenerateResetPasswordUrl()
		{
			var otherRelatedContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			var orgContact = Factory.NewWithValidTestData<EDIOrgContact>();
			orgContact.OC_PER = otherRelatedContact.OC_PER;
			orgContact.Person.ContactCollection.Reload(false);
			AssertExceptionThrown<WebSiteUrlNotSetException>(() => orgContact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Reset));

			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/myaccount");
			var url = orgContact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Reset);
			AssertEquals("http://localhost/myaccount/Admin/ResetPassword.aspx?ResetKey=111", url);

			Factory.Save();
			orgContact.Person.SetHashedPassword("1234");
			url = orgContact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Reset);
			AssertEquals("http://localhost/myaccount/Admin/ResetMasterPassword.aspx?ResetKey=111", url);
		}

		public new void TestGenerateSetPasswordUrl()
		{
			var relatedContact = Factory.NewWithValidTestData<EDIOrgContact>();
			Factory.Save();
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			var orgContact = Factory.NewWithValidTestData<EDIOrgContact>();
			orgContact.OC_PER = relatedContact.OC_PER;
			orgContact.Person.ContactCollection.Reload(false);
			AssertExceptionThrown<WebSiteUrlNotSetException>(() => orgContact.GeneratePasswordInstructionUrl("", PasswordInstructionType.Set));

			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/myaccount");
			var url = orgContact.GeneratePasswordInstructionUrl("111", PasswordInstructionType.Set);
			AssertEquals("http://localhost/myaccount/Admin/SetPassword.aspx?SetKey=111", url);
		}

		public new void TestPasswordInstructionUrl()
		{
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/myaccount");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			contact1.OC_Email = "email1@testing.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_ContactName = "contact2";
			contact2.OC_Email = "email2@testing.com";

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Parent = contact1.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
				passwordLogContact1.SL_Table = OrgContactSchema.Constants.TableName;
			}

			var passwordLogContact2 = Factory.New<StmALog>();
			using (passwordLogContact2.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact2.SL_Parent = contact2.PK;
				passwordLogContact2.SL_SE_NKEvent = string.Empty;
				passwordLogContact2.SL_Table = OrgContactSchema.Constants.TableName;
			}

			Factory.Save();

			AssertEquals("Precondition", PasswordInstructionType.Reset, contact1.GetPasswordInstructionType());
			AssertEquals("Precondition", PasswordInstructionType.Set, contact2.GetPasswordInstructionType());

			AssertEquals(FormattableString.Invariant($"http://localhost/myaccount/Admin/ResetMasterPassword.aspx?ResetKey={OrgContact.TokenMacro}"), contact1.PasswordInstructionMacroUrl);
			AssertEquals(FormattableString.Invariant($"http://localhost/myaccount/Admin/SetMasterPassword.aspx?SetKey={OrgContact.TokenMacro}"), contact2.PasswordInstructionMacroUrl);
		}

		public new void TestGenerateResetPasswordUrlWhenBranchOrCompanyInactive()
		{
			Assert(true);
		}

		public void TestNotificationRoles()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			AssertNotificationRoles(contact);

			AssertNotificationRoles(contact, isCSV: (contact.IsCustomerServiceContact = true));
			contact.IsCustomerServiceContact = false;
			AssertNotificationRoles(contact, isANR: (contact.IsAccountsReceivableContact = true));
			contact.IsAccountsReceivableContact = false;
			AssertNotificationRoles(contact, isBOR: (contact.IsBorderWiseAdministrator = true));
			contact.IsBorderWiseAdministrator = false;
			AssertNotificationRoles(contact, isERA: (contact.IsERequestApprover = true));
			contact.IsERequestApprover = false;
			AssertNotificationRoles(contact, isIST: (contact.IsInformationServicesTechnicalAdministrator = true));
			contact.IsInformationServicesTechnicalAdministrator = false;
			AssertNotificationRoles(contact, isCCP: (contact.IsCertificationProgramContact = true));
			contact.IsCertificationProgramContact = false;
		}

		public void TestGetMostRecentUnlinkedUserAccount()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.WebAccessOrg;
			var contact1 = (EDIOrgContact)org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "u1@cw1.com";
			var contact2 = (EDIOrgContact)org.Contacts.AddNew();
			contact2.OC_ContactName = "User 2";
			contact2.OC_Email = "u2@cw1.com";
			var contact3 = (EDIOrgContact)org.Contacts.AddNew();
			contact3.OC_ContactName = "User 3";
			contact3.OC_Email = "u3@cw1.com";
			Factory.Save();

			AssertNull(contact1.GetMostRecentUnlinkedUserAccount());
			AssertNull(contact2.GetMostRecentUnlinkedUserAccount());
			AssertNull(contact3.GetMostRecentUnlinkedUserAccount());

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "U01";
			userAccount1.EUA_FullName = "User 1";
			userAccount1.EUA_Email = "u1@cw1.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "U02";
			userAccount2.EUA_FullName = "User 2";
			userAccount2.EUA_Email = "u2@cw1.com";
			userAccount2.EUA_IsContactRelationshipActive = false;
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();

			AssertNull(contact1.GetMostRecentUnlinkedUserAccount());
			AssertEquals(userAccount2, contact2.GetMostRecentUnlinkedUserAccount());
			AssertNull(contact3.GetMostRecentUnlinkedUserAccount());
		}

		public void TestGetMostRecentUnlinkedUserAccountShouldExcludeSelfDeactivation()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.WebAccessOrg;
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "u1@cw1.com";
			Factory.Save();

			AssertNull(contact.GetMostRecentUnlinkedUserAccount());
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "U01";
			userAccount.EUA_FullName = "User 1";
			userAccount.EUA_Email = "u1@cw1.com";
			userAccount.EUA_IsContactRelationshipActive = false;
			userAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			AssertNull(contact.GetMostRecentUnlinkedUserAccount());
		}

		public void TestCanSendResetPasswordEmail()
		{
			var enterpriseCode = "DDD";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "ABC", "SYD");
			var database = licence.Database;
			var org = database.WebAccessOrg;
			var contact = (EDIOrgContact)org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "u1@cw1.com";
			Factory.Save();

			AssertEquals("Reset should be allowed when no user accounts", true, contact.CanSendResetPasswordEmail);

			var sdaUserAccount = Factory.New<EdiCustomerUserAccount>();
			sdaUserAccount.EUA_LD = database.PK;
			sdaUserAccount.EUA_UserID = "U01";
			sdaUserAccount.EUA_FullName = "User 1";
			sdaUserAccount.EUA_Email = "u1@cw1.com";
			sdaUserAccount.EUA_IsContactRelationshipActive = false;
			sdaUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.SelfDeactivation;
			sdaUserAccount.EUA_OC_WebAccessContact = contact.PK;
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			var testDatabase = licenceEnterprise.Databases.AddNew();
			testDatabase.LD_ServerCode = "TD1";
			testDatabase.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			var otherUserAccount = Factory.New<EdiCustomerUserAccount>();
			otherUserAccount.EUA_LD = testDatabase.PK;
			otherUserAccount.EUA_UserID = "U01";
			otherUserAccount.EUA_FullName = "User 1";
			otherUserAccount.EUA_Email = "u1@cw1.com";
			otherUserAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();

			AssertEquals("Reset should be allowed if the contact is not linked to any other contacts", true, contact.CanSendResetPasswordEmail);

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var linkedContact = otherOrg.Contacts.AddNew();
			linkedContact.OC_ContactName = "User 1";
			linkedContact.OC_Email = "u1@cw1.com";
			linkedContact.OC_PER = contact.OC_PER;
			Factory.Save();

			AssertEquals("Reset should be allowed if the contact has an active user account", true, contact.CanSendResetPasswordEmail);

			otherUserAccount.EUA_IsContactRelationshipActive = false;
			otherUserAccount.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();

			AssertEquals("Reset should not be allowed if the contact has no active user accounts and has an SDA", false, contact.CanSendResetPasswordEmail);
		}

		public void TestProperties()
		{
			var contact = Factory.New<EDIOrgContact>();
			contact.OC_PasswordHash = null;

			AssertNull(contact.Person);
			AssertEquals("Not Set", contact.PersonalRecoveryEmail);
			AssertEquals("Not Set", contact.PersonPassword);
			AssertEquals("Not Set", contact.ContactPassword);

			var person = Factory.New<GlbPerson>();
			person.PER_EmailAddress = "";
			person.PER_PasswordHash = null;
			contact.OC_PER = person.PK;

			AssertNotNull(contact.Person);
			AssertEquals("Not Set", contact.PersonalRecoveryEmail);
			AssertEquals("Not Set", contact.PersonPassword);
			AssertEquals("Not Set", contact.ContactPassword);

			person.PER_EmailAddress = "test@wisetech.com";
			UserSecretsContext.DefaultContext.SaveSecret("pswrd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10, person.GetPasswordAdapter());
			UserSecretsContext.DefaultContext.SaveSecret("pswrd", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10, contact.GetPasswordAdapter());

			AssertEquals("Set", contact.PersonalRecoveryEmail);
			AssertEquals("Set", contact.PersonPassword);
			AssertEquals("Set", contact.ContactPassword);
		}

		public void TestSetIsActive_InsertLog()
		{
			var logSearchQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.SetToInactiveCode);
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Hello World";
			contact.OC_IsActive = true;
			Factory.Save();

			contact.OC_IsActive = false;
			Factory.Save();
			AssertNull("INA event should not be added when the contact is not a tech contact", database.Logs.Find(logSearchQuery).FirstOrDefault());

			contact.OC_IsActive = true;
			Factory.Save();
			AssertNull("INA event should not be added when the contact is not a tech contact", database.Logs.Find(logSearchQuery).FirstOrDefault());

			database.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			database2.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			Factory.Save();

			contact.OC_IsActive = false;
			contact.OC_IsActive = true;
			Factory.Save();
			AssertNull("(1)INA event should not be added if the final result of IsActive is true", database.Logs.Find(logSearchQuery).FirstOrDefault());
			AssertNull("(2)INA event should not be added if the final result of IsActive is true", database2.Logs.Find(logSearchQuery).FirstOrDefault());

			contact.OC_IsActive = false;
			contact.OC_IsActive = false;
			contact.OC_IsActive = true;
			contact.OC_IsActive = false;
			Factory.Save();

			var logs = database.Logs.Find(logSearchQuery);
			var logs2 = database2.Logs.Find(logSearchQuery);
			var expectedReference = $"Tech Contact {contact.OC_ContactName} set to Inactive.";
			AssertEquals("(1)Only one event should be added after saving", 1, logs.Length);
			AssertEquals("(2)Only one event should be added after saving", 1, logs2.Length);

			var log = logs.FirstOrDefault();
			var log2 = logs2.FirstOrDefault();
			AssertEquals("(1) Reference doesn't match", expectedReference, log.SL_Reference);
			AssertEquals("(2) Reference doesn't match", expectedReference, log2.SL_Reference);

			contact.OC_IsActive = true;
			Factory.Save();
			AssertEquals("(1)INA event should not be added when trying to active contact", 1, database.Logs.Find(logSearchQuery).Length);
			AssertEquals("(2)INA event should not be added when trying to active contact", 1, database2.Logs.Find(logSearchQuery).Length);
		}

		public void TestSetISTInactiveShouldAddLogAtOrg()
		{
			var logSearchQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.SetToInactiveCode);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Hello World";
			contact.OC_OH = orgHeader.PK;
			contact.OC_IsActive = true;
			contact.IsInformationServicesTechnicalAdministrator = true;
			Factory.Save();

			contact.OC_IsActive = false;
			Factory.Save();

			var orgLogs = orgHeader.Logs.Find(logSearchQuery);
			var log = orgLogs.FirstOrDefault();
			var expectedReference = $"InformationServicesTechnicalAdministrator {contact.OC_ContactName} set to Inactive.";
			AssertEquals("log doesn't match", expectedReference, log.SL_Reference);
		}

		public void TestUnassignedISTShouldAddLogAtOrg()
		{
			var logSearchQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.SetToInactiveCode);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Hello World";
			contact.OC_OH = orgHeader.PK;
			contact.OC_IsActive = true;
			contact.IsInformationServicesTechnicalAdministrator = true;
			Factory.Save();

			contact.IsInformationServicesTechnicalAdministrator = false;
			Factory.Save();

			var orgLogs = orgHeader.Logs.Find(logSearchQuery);
			var log = orgLogs.FirstOrDefault();
			var expectedReference = $"Active Contact: {contact.OC_ContactName} unassigned InformationServicesTechnicalAdministrator.";
			AssertEquals("log doesn't match", expectedReference, log.SL_Reference);
		}

		public void TestSetIsActive_InsertLogByVersionLog()
		{
			var factory = new BusinessObjectFactory();

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AAAAA";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User 1";
			contact.OC_Email = "a@a.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = false;

			var staff = factory.New<GlbStaff>();
			staff.GS_FullName = contact.OC_ContactName;
			staff.GS_EmailAddress = contact.OC_Email;
			staff.GS_IsActive = false;

			var database = factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			database.LD_OH_WebAccessOrg = org.PK;
			factory.Save();

			var staffReport = new StaffReport(staff);
			var contactInfo = new ContactImportValue(staffReport);

			var importer = new VersionReportContactImporterForTest(Factory, database);
			importer.ImportSingleContactForTest(contactInfo);
			importer.ImportFactory_Exposed.Save();

			var expectedReference = $"Tech Contact {contact.OC_ContactName} set to Inactive via Version Report.";
			var database1 = new BusinessObjectFactory().Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.PK, database.PK)).FirstOrDefault();
			var expectedLog = database1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedReference));
			AssertEquals(1, expectedLog.Length);
			AssertEquals(AutoEvents.SetToInactiveCode, expectedLog.FirstOrDefault().SL_SE_NKEvent);
		}

		class VersionReportContactImporterForTest : VersionReportContactImporter
		{
			public VersionReportContactImporterForTest(BusinessObjectFactory factory, LicenceDatabase database) : base(factory, database)
			{
			}

			public void ImportSingleContactForTest(ContactImportValue contactValue, bool shouldUpdateContactRelationship = true)
			{
				this.ImportSingleContact(contactValue, shouldUpdateContactRelationship);
			}
		}

		public void TestShouldNotSupersedeMasterOrgContact()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;
			var enterprise = database.LicEnterprise;

			var nonMasterCompanyCode = "XYZ";
			var nonMasterOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			nonMasterOrg.OH_RL_NKClosestPort = "AUSYD";
			nonMasterOrg.OH_FullName = "DDD" + nonMasterCompanyCode + " Company";
			nonMasterOrg.OH_Code = "DDD" + nonMasterCompanyCode;

			var nonMasterCompany = enterprise.Companies.AddNew();
			nonMasterCompany.LC_OH = nonMasterOrg.PK;
			nonMasterCompany.LC_CompanyCode = "ABD";
			nonMasterCompany.LC_LE = enterprise.PK;

			var nonMasterLicence = Factory.New<LicenceHeader>();
			nonMasterLicence.LA_LD = database.PK;
			nonMasterLicence.LA_LC = nonMasterCompany.PK;
			nonMasterLicence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			var masterOrg = licence1.Database.WebAccessOrg;
			masterOrg.Contacts.RemoveAndDeleteAll();

			var nonMasterOrgContact = nonMasterOrg.Contacts.AddNew() as EDIOrgContact;
			nonMasterOrgContact.OC_ContactName = "User One";
			nonMasterOrgContact.OC_Email = "user.one@test.org";
			nonMasterOrgContact.OC_WebAccessEnabled = true;

			var nonMasterOrgUserAccount = Factory.New<EdiCustomerUserAccount>();
			nonMasterOrgUserAccount.EUA_LD = database.PK;
			nonMasterOrgUserAccount.EUA_UserID = "US1";
			nonMasterOrgUserAccount.EUA_FullName = "User One";
			nonMasterOrgUserAccount.EUA_Email = "user.one@test.org";
			nonMasterOrgUserAccount.EUA_OC_WebAccessContact = nonMasterOrgContact.PK;

			var masterOrgContact = masterOrg.Contacts.AddNew();
			masterOrgContact.OC_ContactName = "User Two";
			masterOrgContact.OC_Email = "user.two@test.org";
			masterOrgContact.OC_WebAccessEnabled = true;

			var masterOrgUserAccount = Factory.New<EdiCustomerUserAccount>();
			masterOrgUserAccount.EUA_LD = database.PK;
			masterOrgUserAccount.EUA_UserID = "US2";
			masterOrgUserAccount.EUA_FullName = "User Two";
			masterOrgUserAccount.EUA_Email = "user.two@test.org";
			masterOrgUserAccount.EUA_OC_WebAccessContact = masterOrgContact.PK;

			Factory.Save();

			nonMasterOrgContact.SupersedeWebAccess();
			masterOrgContact.SupersedeWebAccess();
			AssertEquals("Should supersede non master org contact", true, nonMasterOrgContact.WebAccessSuperseded);
			AssertEquals("Should not supersede master org contact", false, masterOrgContact.WebAccessSuperseded);
		}

		public void TestSupersedeWebAccessShouldSupersedeContactsWithNoUserAccounts()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;
			var enterprise = database.LicEnterprise;

			var nonMasterCompanyCode = "XYZ";
			var nonMasterOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			nonMasterOrg.OH_RL_NKClosestPort = "AUSYD";
			nonMasterOrg.OH_FullName = "DDD" + nonMasterCompanyCode + " Company";

			nonMasterOrg.OH_Code = "DDD" + nonMasterCompanyCode;

			var nonMasterCompany = enterprise.Companies.AddNew();
			nonMasterCompany.LC_OH = nonMasterOrg.PK;
			nonMasterCompany.LC_CompanyCode = "ABD";
			nonMasterCompany.LC_LE = enterprise.PK;

			var nonMasterLicence = Factory.New<LicenceHeader>();
			nonMasterLicence.LA_LD = database.PK;
			nonMasterLicence.LA_LC = nonMasterCompany.PK;
			nonMasterLicence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			var masterOrg = licence1.Database.WebAccessOrg;
			masterOrg.Contacts.RemoveAndDeleteAll();

			var masterOrgContactWithNoUserAccount = masterOrg.Contacts.AddNew();
			masterOrgContactWithNoUserAccount.OC_ContactName = "User Two";
			masterOrgContactWithNoUserAccount.OC_Email = "user.two@test.org";
			masterOrgContactWithNoUserAccount.OC_WebAccessEnabled = true;

			Factory.Save();

			masterOrgContactWithNoUserAccount.SupersedeWebAccess();
			AssertEquals("Should supersede master org contact with no user account", true, masterOrgContactWithNoUserAccount.WebAccessSuperseded);
		}

		void AssertNotificationRoles(EDIOrgContact contact, bool isCSV = false, bool isANR = false, bool isBOR = false, bool isERA = false, bool isIST = false, bool isCCP = false)
		{
			AssertEquals(isCSV, contact.IsCustomerServiceContact);
			AssertEquals(isCSV, (ZBool)contact.IsCustomerServiceContactInfo.OriginalValue);
			AssertEquals(isANR, contact.IsAccountsReceivableContact);
			AssertEquals(isANR, (ZBool)contact.IsAccountsReceivableContactInfo.OriginalValue);
			AssertEquals(isBOR, contact.IsBorderWiseAdministrator);
			AssertEquals(isBOR, (ZBool)contact.IsBorderWiseAdministratorInfo.OriginalValue);
			AssertEquals(isERA, contact.IsERequestApprover);
			AssertEquals(isERA, (ZBool)contact.IsERequestApproverInfo.OriginalValue);
			AssertEquals(isIST, contact.IsInformationServicesTechnicalAdministrator);
			AssertEquals(isIST, (ZBool)contact.IsInformationServicesTechnicalAdministratorInfo.OriginalValue);
			AssertEquals(isCCP, contact.IsCertificationProgramContact);
			AssertEquals(isCCP, (ZBool)contact.IsCertificationProgramContactInfo.OriginalValue);
		}

		protected override bool ExpectedDefaultSecurityRightAccess
		{
			get { return false; }
		}
	}
}

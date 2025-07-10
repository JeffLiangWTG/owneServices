using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	class ContactClonerTest : TestCaseWithFactory
	{
		public void TestDuplicatesInBothOrgs()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database1 = licence1.Database;
			var database2 = BillingTestHelper.CreateAnotherDatabase(licence1.Company, "MEL").Database;
			var licence2 = BillingTestHelper.CreateAnotherLicence(database1, "DEF");

			var person1 = Factory.NewWithPrimaryKey<GlbPerson>(Guid.Parse("BCE2EE40-4CF6-4832-8CFF-7C0A51366D0A"));
			person1.PER_FullName = "Hariprasath Periyasamy";
			person1.PER_EmailAddress = "hariprasath.periyasamy@tvslsl.com";

			var person2 = Factory.NewWithPrimaryKey<GlbPerson>(Guid.Parse("9CB5738B-D8E5-49EE-97AD-908533969C31"));
			person2.PER_FullName = "Hariprasath Periyasamy";

			var person3 = Factory.NewWithPrimaryKey<GlbPerson>(Guid.Parse("46A980B0-650D-444E-B1BD-864FFE889EE7"));
			person3.PER_FullName = "Hariprasath Periyasamy";

			var oldMasterOrg = licence2.Company.Header;
			oldMasterOrg.Contacts.RemoveAndDeleteAll();

			database1.LD_OH_WebAccessOrg = oldMasterOrg.PK;
			database2.LD_OH_WebAccessOrg = oldMasterOrg.PK;

			//var contact0 = oldMasterOrg.Contacts.AddNew();
			var contact0 = Factory.NewWithPrimaryKey<OrgContact>(Guid.Parse("A1E06BA1-1EA1-472A-84CD-F6724A095D09"));
			contact0.OC_OH = oldMasterOrg.PK;
			contact0.OC_ContactName = "Hariprasath Periyasamy";
			contact0.OC_Email = "hariprasath.periyasamy@tvslsl.com";
			contact0.OC_IsActive = true;
			contact0.OC_WebAccessEnabled = false;
			contact0.OC_PER = person3.PK;

			//var contact1 = oldMasterOrg.Contacts.AddNew();
			var contact1 = Factory.NewWithPrimaryKey<OrgContact>(Guid.Parse("1CBBE762-BF5D-4100-A84B-AB3D0D4B15BA"));
			contact1.OC_OH = oldMasterOrg.PK;
			contact1.OC_ContactName = "Hariprasath Periyasamy (1)";
			contact1.OC_Email = "hariprasath.p@tvsscs.com";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_PER = person1.PK;

			//var contact2 = oldMasterOrg.Contacts.AddNew();
			var contact2 = Factory.NewWithPrimaryKey<OrgContact>(Guid.Parse("D3F7DF84-54D3-46BB-8252-D0BBE7CCD03B"));
			contact2.OC_OH = oldMasterOrg.PK;
			contact2.OC_ContactName = "Hariprasath Periyasamy (1) (1)";
			contact2.OC_Email = string.Empty;
			contact2.OC_IsActive = false;
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_PER = person2.PK;

			var newMasterOrg = database1.LicEnterprise.Organisation;

			//var contact3 = newMasterOrg.Contacts.AddNew();
			var contact3 = Factory.NewWithPrimaryKey<OrgContact>(Guid.Parse("8FC0247F-37F1-45C0-973A-AA3182317C4E"));
			contact3.OC_OH = newMasterOrg.PK;
			contact3.OC_ContactName = "Hariprasath Periyasamy";
			contact3.OC_Email = "hariprasath.p@tvsscs.com";
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = person1.PK;

			//var contact4 = newMasterOrg.Contacts.AddNew();
			var contact4 = Factory.NewWithPrimaryKey<OrgContact>(Guid.Parse("B2ADFD1D-F56D-4582-85A1-D68C03990961"));
			contact4.OC_OH = newMasterOrg.PK;
			contact4.OC_ContactName = "Hariprasath Periyasamy (1)";
			contact4.OC_Email = string.Empty;
			contact4.OC_IsActive = false;
			contact4.OC_WebAccessEnabled = false;
			contact4.OC_PER = person2.PK;

			var userAccount1 = Factory.NewWithPrimaryKey<EdiCustomerUserAccount>(Guid.Parse("9C576E88-3DDF-43B7-917F-18FB39C34160"));
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "HPP";
			userAccount1.EUA_FullName = "Hariprasath Periyasamy";
			userAccount1.EUA_Email = "hariprasath.p@tvsscs.com";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.NewWithPrimaryKey<EdiCustomerUserAccount>(Guid.Parse("FEA2AF4A-1E39-450D-8335-16FB0663A377"));
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_UserID = "fea2af4a-1e39-450d-8335-16fb0663a377";
			userAccount2.EUA_FullName = "Hariprasath Periyasamy";
			userAccount2.EUA_Email = "hariprasath.periyasamy@tvslsl.com";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount3 = Factory.NewWithPrimaryKey<EdiCustomerUserAccount>(Guid.Parse("97717127-6522-443D-A30B-046902AADBA4"));
			userAccount3.EUA_LD = database1.PK;
			userAccount3.EUA_UserID = "HAP";
			userAccount3.EUA_FullName = "Hariprasath Periyasamy";
			userAccount3.EUA_Email = "hariprasath.periyasamy@tvslsl.com";
			userAccount3.EUA_OC_WebAccessContact = contact2.PK;
			userAccount3.EUA_IsActive = false;
			userAccount3.EUA_IsContactRelationshipActive = false;
			userAccount3.EUA_ContactRelationshipStatus = "DER";

			Factory.Save();

			oldMasterOrg.Contacts.Reload(true);
			newMasterOrg.Contacts.Reload(true);

			var logs = new ProcessStatusForTest();
			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var cloner = new ContactCloner(cloneFactory, logs);

			var oldMasterOrgInCloneFactory = cloneFactory.Load<OrgHeader>(oldMasterOrg.PK);
			var newMasterOrgInCloneFactory = cloneFactory.Load<OrgHeader>(newMasterOrg.PK);
			oldMasterOrgInCloneFactory.Contacts.Reload(true);
			newMasterOrgInCloneFactory.Contacts.Reload(true);

			cloner.CloneContacts(oldMasterOrgInCloneFactory, newMasterOrgInCloneFactory, database1);
			cloner.CloneContacts(oldMasterOrgInCloneFactory, newMasterOrgInCloneFactory, database2);
			cloner.MergeContactsPerson();
			cloneFactory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var contact3reloaded = newFactory.Load<OrgContact>(contact3.PK);
			var contact4reloaded = newFactory.Load<OrgContact>(contact4.PK);

			AssertEquals("Names on target org shouldn't change", contact3.OC_ContactName, contact3reloaded.OC_ContactName);
			AssertEquals("Names on target org shouldn't change", contact4.OC_ContactName, contact4reloaded.OC_ContactName);

			AssertEquals(3, contact3reloaded.ParentOrg.Contacts.Count);

			foreach (OrgContact contact in contact3reloaded.ParentOrg.Contacts)
			{
				if (contact.PK != contact3.PK && contact.PK != contact4.PK)
				{
					AssertNotEquals("should not re-use existing contact names", contact.OC_ContactName, contact3.OC_ContactName);
					AssertNotEquals("should not re-use existing contact names", contact.OC_ContactName, contact4.OC_ContactName);
					AssertEquals(1, contact3reloaded.ParentOrg.Contacts.Cast<OrgContact>().Count(c => c.OC_ContactName == contact.OC_ContactName));
				}
			}
		}

		public void TestCloneContacts()
		{
			var logs = new ProcessStatusForTest();
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;
			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");

			var oldMasterOrg = licence2.Company.Header;
			var newMasterOrg = database.LicEnterprise.Organisation;

			oldMasterOrg.Contacts.RemoveAndDeleteAll();

			var address1 = oldMasterOrg.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";

			var contact1a = oldMasterOrg.Contacts.AddNew() as EDIOrgContact;
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = address1.PK;
			contact1a.LogMyAccountDisclaimerAcknowledgementIfRequired(database);
			contact1a.IsAccountsReceivableContact = true;
			contact1a.IsBorderWiseAdministrator = true;
			contact1a.IsCustomerServiceContact = true;
			contact1a.IsERequestApprover = true;
			contact1a.IsInformationServicesTechnicalAdministrator = true;
			contact1a.IsCertificationProgramContact = true;
			contact1a.AddDocumentGroup(ContactType.Consignor.Code, true, null);

			var orgRight1 = oldMasterOrg.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.CustomerService.Code))[0] as OrgSecurity;
			var contactRightQuery1 = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight1.PK);
			contactRightQuery1.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact1a.PK);
			var contactRight1 = contact1a.SecurityRightsForBindingOnly.Find(contactRightQuery1)[0] as OrgSecurityContacts;
			orgRight1.OX_Granted = false;
			contactRight1.OZ_Granted = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "AAA";
			clientBranch1.LCB_Name = "Branch A";
			clientBranch1.LCB_OA = address1.PK;

			newMasterOrg.OH_Code = "SACAWAGA";
			newMasterOrg.Contacts.RemoveAndDeleteAll();

			var contact2 = newMasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			Factory.Save();

			var cloner = new ContactCloner(Factory, logs);
			cloner.CloneContacts(oldMasterOrg, newMasterOrg, database);
			Factory.Save();

			contact1a.Reload();
			AssertEquals("Should be deactivated with redirection", false, contact1a.OC_IsActive);
			AssertEquals("Should have web access superseded", true, contact1a.WebAccessSuperseded);

			address1.Reload();
			AssertEquals(false, address1.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg2 = loadFactory.Load<OrgHeader>(newMasterOrg.PK);

			var address2 = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.OA_CompanyNameOverride == "Company AAA");

			var loadedClientBranch1 = loadFactory.Load<ClientBranch>(clientBranch1.PK);
			AssertEquals(address2.PK, loadedClientBranch1.LCB_OA);

			AssertEquals(2, loadedOrg2.Contacts.Count);

			var contact2a = loadedOrg2.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Email", "user.one@test.org", contact2a.OC_Email);
			AssertEquals("Web access", true, contact2a.OC_WebAccessEnabled);
			AssertEquals("Password", true, contact2a.VerifyPassword("123456"));
			AssertEquals("Address", address2.PK, contact2a.OC_OA_OrgAddress);
			AssertEquals("Should be linked to Person of source contact", contact1a.OC_PER, contact2a.OC_PER);

			var contact1aMyAccountLog = contact1a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecuted.Code);
			var contact2aMyAccountLog = contact2a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.ClickThroughAgreementExecuted.Code);
			AssertNotNull("Should have myaccount disclaimer acknowledge log", contact2aMyAccountLog);
			AssertEquals("Log event time should copy over", contact1aMyAccountLog.SL_EventTime, contact2aMyAccountLog.SL_EventTime);

			var contact2b = loadedOrg2.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User Two");
			AssertEquals(true, contact2b.OC_IsActive);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(userAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);

			var orgRight2 = loadedOrg2.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.CustomerService.Code))[0] as OrgSecurity;
			var contactRightQuery2 = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight2.PK);
			contactRightQuery2.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact2a.PK);
			var contactRight2 = contact2a.SecurityRightsForBindingOnly.Find(contactRightQuery2)[0] as OrgSecurityContacts;
			AssertEquals(false, orgRight2.OX_Granted);
			AssertEquals(true, contactRight2.OZ_Granted);

			AssertEquals(contact2a.Person.PrimaryRelationship.PPR_PrimaryId, contact2a.PK);
			AssertEquals(true, contact2a.IsAccountsReceivableContact);
			AssertEquals(true, contact2a.IsBorderWiseAdministrator);
			AssertEquals(true, contact2a.IsCustomerServiceContact);
			AssertEquals(true, contact2a.IsERequestApprover);
			AssertEquals(true, contact2a.IsInformationServicesTechnicalAdministrator);
			AssertEquals(true, contact2a.IsCertificationProgramContact);
			AssertEquals(true, contact2a.HasDocumentGroup(ContactType.Consignor.Code));

			AssertEquals(contact2b.Person.PrimaryRelationship.PPR_PrimaryId, contact2b.PK);
			AssertEquals(@"Cloning Org Securities... 1 / 35 - 2
Cloning Org Securities... 2 / 35 - 5
Cloning Org Securities... 3 / 35 - 8
Cloning Org Securities... 4 / 35 - 11
Cloning Org Securities... 5 / 35 - 14
Cloning Org Securities... 6 / 35 - 17
Cloning Org Securities... 7 / 35 - 20
Cloning Org Securities... 8 / 35 - 22
Cloning Org Securities... 9 / 35 - 25
Cloning Org Securities... 10 / 35 - 28
Cloning Org Securities... 11 / 35 - 31
Cloning Org Securities... 12 / 35 - 34
Cloning Org Securities... 13 / 35 - 37
Cloning Org Securities... 14 / 35 - 40
Cloning Org Securities... 15 / 35 - 42
Cloning Org Securities... 16 / 35 - 45
Cloning Org Securities... 17 / 35 - 48
Cloning Org Securities... 18 / 35 - 51
Cloning Org Securities... 19 / 35 - 54
Cloning Org Securities... 20 / 35 - 57
Cloning Org Securities... 21 / 35 - 60
Cloning Org Securities... 22 / 35 - 62
Cloning Org Securities... 23 / 35 - 65
Cloning Org Securities... 24 / 35 - 68
Cloning Org Securities... 25 / 35 - 71
Cloning Org Securities... 26 / 35 - 74
Cloning Org Securities... 27 / 35 - 77
Cloning Org Securities... 28 / 35 - 80
Cloning Org Securities... 29 / 35 - 82
Cloning Org Securities... 30 / 35 - 85
Cloning Org Securities... 31 / 35 - 88
Cloning Org Securities... 32 / 35 - 91
Cloning Org Securities... 33 / 35 - 94
Cloning Org Securities... 34 / 35 - 97
Cloning Org Securities... 35 / 35 - 100
Cloning Contact Addresses... 1 / 2 - 50
Cloning Contact Addresses... 2 / 2 - 100
Cloning Contacts... 1 / 1 - 100
", logs.Logs.ToString());
		}

		public void TestCloneContacts_ExistingContact()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.OH_Code = "CAWAGA";
			org1.Contacts.RemoveAndDeleteAll();

			var address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";

			var contact1a = org1.Contacts.AddNew() as EDIOrgContact;
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = address1.PK;
			contact1a.OC_Phone = "+61 2 8888 8888";
			contact1a.OC_Title = "Developer";
			contact1a.OC_WebAccessEnabled = true;

			contact1a.IsAccountsReceivableContact = true;
			contact1a.IsBorderWiseAdministrator = true;
			contact1a.IsCustomerServiceContact = true;
			contact1a.IsERequestApprover = true;
			contact1a.IsInformationServicesTechnicalAdministrator = true;
			contact1a.IsCertificationProgramContact = true;

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User Two";
			contact1b.OC_Email = "user.two@test.org";

			var contact1c = org1.Contacts.AddNew();
			contact1c.OC_ContactName = "User Two (1)";
			contact1c.OC_Email = "user.two1@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1b.PK;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "AAA";
			clientBranch1.LCB_Name = "Branch A";
			clientBranch1.LCB_OA = address1.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew() as EDIOrgContact;
			contact2a.OC_ContactName = "User ONE (1)";
			contact2a.OC_Email = "user.one@test.org";
			contact2a.OC_WebAccessEnabled = true;

			var contact2b = org2.Contacts.AddNew() as EDIOrgContact;
			contact2b.OC_ContactName = "User tWO";
			contact2b.OC_Email = "abc@test.org";
			contact2b.OC_IsActive = false;
			contact2b.IsAccountsReceivableContact = true;
			contact2b.IsBorderWiseAdministrator = true;
			contact2b.IsCustomerServiceContact = true;
			contact2b.IsERequestApprover = true;
			contact2b.IsInformationServicesTechnicalAdministrator = true;
			contact2b.IsCertificationProgramContact = true;
			contact2b.AddDocumentGroup(ContactType.Consignor.Code, true, null);

			var contact2c = org2.Contacts.AddNew();
			contact2c.OC_ContactName = "User oNe";
			contact2c.OC_Email = "user.one@test.org";
			contact2c.OC_IsActive = false;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg2 = loadFactory.Load<OrgHeader>(org2.PK);

			var address2 = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.OA_CompanyNameOverride == "Company AAA");

			AssertEquals(4, loadedOrg2.Contacts.Count);

			var loadedContact2a = loadedOrg2.Contacts.Cast<OrgContact>().Single(x => x.PK == contact2a.PK) as EDIOrgContact;
			AssertEquals("User ONE (1)", loadedContact2a.OC_ContactName);
			AssertEquals("user.one@test.org", loadedContact2a.OC_Email);
			AssertEquals(true, loadedContact2a.OC_WebAccessEnabled);
			AssertEquals(true, loadedContact2a.VerifyPassword("123456"));
			AssertEquals("+61 2 8888 8888", loadedContact2a.OC_Phone);
			AssertEquals("Developer", loadedContact2a.OC_Title);
			AssertEquals(address2.PK, loadedContact2a.OC_OA_OrgAddress);
			AssertEquals(contact1a.OC_PER, loadedContact2a.OC_PER);

			AssertEquals(true, loadedContact2a.IsAccountsReceivableContact);
			AssertEquals(true, loadedContact2a.IsBorderWiseAdministrator);
			AssertEquals(true, loadedContact2a.IsCustomerServiceContact);
			AssertEquals(true, loadedContact2a.IsERequestApprover);
			AssertEquals(true, loadedContact2a.IsInformationServicesTechnicalAdministrator);
			AssertEquals(true, loadedContact2a.IsCertificationProgramContact);

			var loadedContact2b1 = loadedOrg2.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User tWO") as EDIOrgContact;
			AssertEquals("abc@test.org", loadedContact2b1.OC_Email);
			AssertNotEquals(contact1b.OC_PER, loadedContact2b1.OC_PER);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsAccountsReceivableContact);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsBorderWiseAdministrator);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsCustomerServiceContact);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsERequestApprover);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsInformationServicesTechnicalAdministrator);
			AssertEquals("should not be deleted", true, loadedContact2b1.IsCertificationProgramContact);
			AssertEquals(true, loadedContact2b1.HasDocumentGroup(ContactType.Consignor.Code));

			var loadedContact2b2 = loadedOrg2.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User Two (1)");
			AssertEquals("user.two@test.org", loadedContact2b2.OC_Email);
			AssertEquals(contact1b.OC_PER, loadedContact2b2.OC_PER);

			loadedContact2b2.Person.PER_FullName = "User One";
			loadFactory.Save();

			var loadedContact1b = loadFactory.Load<OrgContact>(contact1b.PK);
			AssertEquals("User One (1)", loadedContact1b.OC_ContactName);
			AssertEquals("User One (2)", loadedContact2b2.OC_ContactName);
		}

		public void TestCloneContacts_SameEmailAddressContact()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "Support";
			contact1b.OC_Email = "user.one@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "USP";
			userAccount2.EUA_FullName = "Support";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1b.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "User.One@test.org";

			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "Support";
			contact2b.OC_Email = "user.one@test.org";

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			userAccount1.Reload();
			userAccount2.Reload();
			AssertEquals(contact2a.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertEquals(contact2b.PK, userAccount2.EUA_OC_WebAccessContact);

			cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org2ForClone, org1ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			userAccount1.Reload();
			userAccount2.Reload();
			AssertEquals(contact1a.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertEquals(contact1b.PK, userAccount2.EUA_OC_WebAccessContact);
		}

		public void TestCloneContacts_SimilarContact()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = false;

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "Support";
			contact1b.OC_Email = "user.one@test.org";
			contact1b.OC_WebAccessEnabled = false;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "USP";
			userAccount2.EUA_FullName = "Support";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1b.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			userAccount1.Reload();
			userAccount2.Reload();
			AssertEquals(contact2a.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertNotEquals("Should be a new contact", contact2a.PK, userAccount2.EUA_OC_WebAccessContact);

			AssertEquals("user.one@test.org", userAccount1.WebAccessContact.OC_Email);

			cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org2ForClone, org1ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			userAccount1.Reload();
			userAccount2.Reload();
			AssertEquals(contact1a.PK, userAccount1.EUA_OC_WebAccessContact);
			AssertEquals(contact1b.PK, userAccount2.EUA_OC_WebAccessContact);
		}

		public void TestCloneContacts_ExistingContactSamePerson()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;

			var person = Factory.New<GlbPerson>();

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.OC_PER = person.PK;
			person.UpdateFromContact(contact1a);

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";
			contact2a.OC_WebAccessEnabled = false;
			contact2a.OC_PER = person.PK;

			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one@test.org";
			contact2b.OC_WebAccessEnabled = true;
			contact2b.OC_PER = person.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();

			userAccount1.Reload();
			AssertEquals(contact2b.PK, userAccount1.EUA_OC_WebAccessContact);
		}

		public void TestCloneContacts_ExistingContactSamePerson_Inactive()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;

			var person1 = Factory.New<GlbPerson>();
			var person2 = Factory.New<GlbPerson>();

			var org1 = database.LicEnterprise.Organisation;
			org1.OH_Code = "CAWAGA";
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.OC_PER = person1.PK;
			person1.UpdateFromContact(contact1a);

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one1@test.org";
			contact2a.OC_WebAccessEnabled = false;
			contact2a.OC_PER = person1.PK;
			contact2a.OC_IsActive = false;

			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one2@test.org";
			contact2b.OC_WebAccessEnabled = true;
			contact2b.OC_PER = person1.PK;
			contact2b.OC_IsActive = false;

			var contact3 = org2.Contacts.AddNew();
			contact3.OC_ContactName = "User Two";
			contact3.OC_Email = "user.one@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = person2.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();

			userAccount1.Reload();
			AssertEquals("Should be a new one since contact is inactive", contact3.PK, userAccount1.EUA_OC_WebAccessContact);
		}

		public void TestCloneContacts_ContactPersonMerge()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";

			var contact1b = org1.Contacts.AddNew();
			contact1b.OC_ContactName = "User Two";
			contact1b.OC_Email = "user.two@test.org";

			var contact1c = org1.Contacts.AddNew();
			contact1c.OC_ContactName = "User Three";
			contact1c.OC_Email = "user.three@test.org";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US2";
			userAccount2.EUA_FullName = "User Two";
			userAccount2.EUA_Email = "user.two@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1b.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User Three";
			userAccount3.EUA_Email = "user.three@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact1c.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";

			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User Two";
			contact2b.OC_Email = "user.two@test.org";

			Factory.Save();

			var applicant2a = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2a.HA_FullName = "User One";
			applicant2a.HA_EmailAddress = "user.one@test.org";
			applicant2a.HA_PER = contact2a.OC_PER;

			var applicant2b = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2b.HA_FullName = "User Two";
			applicant2b.HA_EmailAddress = "user.two@test.org";
			applicant2b.HA_PER = contact2b.OC_PER;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			var loadFactory = new BusinessObjectFactory();
			var loadedContact2a = loadFactory.Load<OrgContact>(contact2a.PK);
			var loadedContact2b = loadFactory.Load<OrgContact>(contact2b.PK);
			var loadedApplicant2a = loadFactory.Load<HRJobApplicant>(applicant2a.PK);
			var loadedApplicant2b = loadFactory.Load<HRJobApplicant>(applicant2b.PK);

			AssertEquals("Should be no merge failures", false, cloner.HasPersonMergeFailure);
			AssertEquals("Target org contact should be merged to source org contact person", contact1a.OC_PER, loadedContact2a.OC_PER);
			AssertEquals("Target org contact should be merged to source org contact person", contact1b.OC_PER, loadedContact2b.OC_PER);
			AssertEquals("Target org contact related applicant should be merged to source org contact person", contact1a.OC_PER, loadedApplicant2a.HA_PER);
			AssertEquals("Target org contact related applicant should be merged to source org contact person", contact1b.OC_PER, loadedApplicant2b.HA_PER);

			cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org2ForClone, org1ForClone, database);
			cloneFactory.Save();
			cloner.MergeContactsPerson();

			AssertEquals("Should be no merge failures", false, cloner.HasPersonMergeFailure);
			AssertEquals("The person is same as before", contact1a.OC_PER, loadedContact2a.OC_PER);
			AssertEquals("The person is same as before", contact1b.OC_PER, loadedContact2b.OC_PER);
			AssertEquals("The person is same as before", contact1a.OC_PER, loadedApplicant2a.HA_PER);
			AssertEquals("The person is same as before", contact1b.OC_PER, loadedApplicant2b.HA_PER);
		}

		public void TestCloneContacts_ExistingAddress()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var address1a = org1.Addresses.AddNew();
			address1a.OA_Address1 = "1 Test Rd";
			address1a.OA_Address2 = "Building A";
			address1a.OA_City = "Sydney";
			address1a.OA_State = "NSW";
			address1a.OA_PostCode = "2000";
			address1a.OA_RL_NKRelatedPortCode = "AUSYD";
			address1a.OA_CompanyNameOverride = "Company AAA";
			address1a.OA_Code = "Branch A (AAA)";
			address1a.OA_IsActive = false;

			var address1b = org1.Addresses.AddNew();
			address1b.OA_Address1 = "33 Debug St";
			address1b.OA_Address2 = "Shop 948";
			address1b.OA_City = "Melbourne";
			address1b.OA_State = "VIC";
			address1b.OA_PostCode = "3000";
			address1b.OA_RL_NKRelatedPortCode = "AUMEL";
			address1b.OA_CompanyNameOverride = "Company AAA";
			address1b.OA_Code = "Branch B (BBB)";

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = address1a.PK;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var clientBranch1a = Factory.New<ClientBranch>();
			clientBranch1a.LCB_LD = database.PK;
			clientBranch1a.LCB_Code = "AAA";
			clientBranch1a.LCB_Name = "Branch A";
			clientBranch1a.LCB_OA = address1a.PK;

			var clientBranch1b = Factory.New<ClientBranch>();
			clientBranch1b.LCB_LD = database.PK;
			clientBranch1b.LCB_Code = "BBB";
			clientBranch1b.LCB_Name = "Branch B";
			clientBranch1b.LCB_OA = address1b.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var address2a = org2.Addresses.AddNew();
			address2a.OA_Address1 = address1a.OA_Address1;
			address2a.OA_Address2 = address1a.OA_Address2;
			address2a.OA_City = address1a.OA_City;
			address2a.OA_State = address1a.OA_State;
			address2a.OA_PostCode = address1a.OA_PostCode;
			address2a.OA_RL_NKRelatedPortCode = address1a.OA_RL_NKRelatedPortCode;
			address2a.OA_CompanyNameOverride = address1a.OA_CompanyNameOverride;
			address2a.OA_Code = address1a.OA_Code;

			var address2b = org2.Addresses.AddNew();
			address2b.OA_Address1 = "00 Nowhere";
			address2b.OA_City = "City";
			address2b.OA_State = "AAA";
			address2b.OA_PostCode = "1111";
			address2b.OA_RL_NKRelatedPortCode = "AUBNE";
			address2b.OA_Code = "Branch B (BBB)";

			Factory.Save();

			var cloner = new ContactCloner(Factory);
			cloner.CloneContacts(org1, org2, database);
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg2 = loadFactory.Load<OrgHeader>(org2.PK);

			AssertEquals(4, loadedOrg2.Addresses.Count);
			var loadedAddress2a = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.OA_Code == "Branch A (AAA)");
			var loadedAddress2b1 = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.OA_Code == "Branch B (BBB)");
			var loadedAddress2b2 = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.OA_Code == "Branch B_1 (BBB)");

			var loadedClientBranch1 = loadFactory.Load<ClientBranch>(clientBranch1a.PK);
			AssertEquals(loadedAddress2a.PK, loadedClientBranch1.LCB_OA);
			AssertEquals(false, loadedAddress2a.OA_IsActive);

			AssertEquals(1, org2.Contacts.Count);
			var contact2a = org2.Contacts[0];
			AssertEquals(loadedAddress2a.PK, contact2a.OC_OA_OrgAddress);
		}

		public void TestCloneContacts_ExistingSameAddress()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var address1a = org1.Addresses.AddNew();
			address1a.OA_Address1 = "1 Test Rd";
			address1a.OA_Address2 = "Building A";
			address1a.OA_City = "Sydney";
			address1a.OA_State = "NSW";
			address1a.OA_PostCode = "2000";
			address1a.OA_RL_NKRelatedPortCode = "AUSYD";
			address1a.OA_CompanyNameOverride = "Company AAA";
			address1a.OA_Code = "Branch A (AAA)";

			var address1b = org1.Addresses.AddNew();
			address1b.OA_Address1 = "1 Test Rd";
			address1b.OA_Address2 = "Building A";
			address1b.OA_City = "Sydney";
			address1b.OA_State = "NSW";
			address1b.OA_PostCode = "2000";
			address1b.OA_RL_NKRelatedPortCode = "AUSYD";
			address1b.OA_CompanyNameOverride = "Company AAA";
			address1b.OA_Code = "Branch B (BBB)";

			var clientBranch1a = Factory.New<ClientBranch>();
			clientBranch1a.LCB_LD = database.PK;
			clientBranch1a.LCB_Code = "AAA";
			clientBranch1a.LCB_Name = "Branch A";
			clientBranch1a.LCB_OA = address1a.PK;

			var clientBranch1b = Factory.New<ClientBranch>();
			clientBranch1b.LCB_LD = database.PK;
			clientBranch1b.LCB_Code = "BBB";
			clientBranch1b.LCB_Name = "Branch B";
			clientBranch1b.LCB_OA = address1b.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var address2a = org2.Addresses.AddNew();
			address2a.OA_Address1 = address1a.OA_Address1;
			address2a.OA_Address2 = address1a.OA_Address2;
			address2a.OA_City = address1a.OA_City;
			address2a.OA_State = address1a.OA_State;
			address2a.OA_PostCode = address1a.OA_PostCode;
			address2a.OA_RL_NKRelatedPortCode = address1a.OA_RL_NKRelatedPortCode;
			address2a.OA_CompanyNameOverride = address1a.OA_CompanyNameOverride;
			address2a.OA_Code = address1a.OA_Code;

			Factory.Save();

			var cloner = new ContactCloner(Factory);
			cloner.CloneContacts(org1, org2, database);
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg2 = loadFactory.Load<OrgHeader>(org2.PK);

			var address2aBranches = loadFactory.Load<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_OA, address2a.PK));
			AssertEquals("Org address should only link to one client branch", 1, address2aBranches.Length);
		}

		public void TestCloneContacts_ExistingMainAddress()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var address1a = org1.Addresses.AddNew();
			address1a.OA_Address1 = "1 Test Rd";
			address1a.OA_Address2 = "Building A";
			address1a.OA_City = "Sydney";
			address1a.OA_State = "NSW";
			address1a.OA_PostCode = "2000";
			address1a.OA_RL_NKRelatedPortCode = "AUSYD";
			address1a.OA_CompanyNameOverride = "Company AAA";
			address1a.OA_Code = "Branch A (AAA)";

			var clientBranch1a = Factory.New<ClientBranch>();
			clientBranch1a.LCB_LD = database.PK;
			clientBranch1a.LCB_Code = "AAA";
			clientBranch1a.LCB_Name = "Branch A";
			clientBranch1a.LCB_OA = address1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var address2a = org2.Addresses.MainAddress;
			address2a.OA_Address1 = address1a.OA_Address1;
			address2a.OA_Address2 = address1a.OA_Address2;
			address2a.OA_City = address1a.OA_City;
			address2a.OA_State = address1a.OA_State;
			address2a.OA_PostCode = address1a.OA_PostCode;
			address2a.OA_RL_NKRelatedPortCode = address1a.OA_RL_NKRelatedPortCode;
			address2a.OA_CompanyNameOverride = address1a.OA_CompanyNameOverride;
			address2a.OA_Code = address1a.OA_Code;

			Factory.Save();

			var cloner = new ContactCloner(Factory);
			cloner.CloneContacts(org1, org2, database);
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg2 = loadFactory.Load<OrgHeader>(org2.PK);

			AssertEquals(2, loadedOrg2.Addresses.Count);
			var loadedAddress2b = loadedOrg2.Addresses.Cast<OrgAddress>().Single(x => x.PK != address2a.PK);

			var loadedClientBranch1 = loadFactory.Load<ClientBranch>(clientBranch1a.PK);
			AssertEquals("Should not match to main address", loadedAddress2b.PK, loadedClientBranch1.LCB_OA);
		}

		public void TestCloneContacts_PersonalInfo()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_Gender = Core.Constants.Genders.Man;
			contact1a.OC_Birthday = ZDate.BrettsBirthday;
			contact1a.OC_RN_NKNationality = "AU";
			contact1a.OC_PersonalInfo = "It is a secrect.";

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";

			Factory.Save();

			var originalSecurityRight = Env.Security.OrgContactViewPersonalInformation.IsAllowed;
			try
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;

				var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
				var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
				var cloner = new ContactCloner(cloneFactory);
				cloner.CloneContacts(org1ForClone, org2ForClone, database);
				cloneFactory.Save();
			}
			finally
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = originalSecurityRight;
			}

			var loadFactory = new BusinessObjectFactory();
			var loadedContact2a = loadFactory.Load<OrgContact>(contact2a.PK);
			AssertEquals("Gender is not copied", Core.Constants.Genders.NotSpecified, loadedContact2a.OC_Gender);
			AssertEquals("Birthday is not copied", ZDate.Empty, loadedContact2a.OC_Birthday);
			AssertEquals("Nationality is not copied", ZString.Empty, loadedContact2a.OC_RN_NKNationality);
			AssertEquals("Personal info is not copied", ZString.Empty, loadedContact2a.OC_PersonalInfo);
		}

		public void TestCloneContacts_PersonPrimaryRelationship()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var enterprise = database.LicEnterprise;

			var person = Factory.New<GlbPerson>();

			var org1 = enterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_PER = person.PK;
			person.UpdateFromContact(contact1);

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "User One";
			contact2.OC_Email = "user.one@test.org";
			contact2.OC_PER = person.PK;

			person.SetPrimaryRelationship(contact1);

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();

			var primaryRelationship = person.PrimaryRelationship;
			primaryRelationship.Reload();
			AssertEquals("Should be destination contact", contact2.PK, primaryRelationship.PPR_PrimaryId);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			person.SetPrimaryRelationship(staff);
			Factory.Save();

			cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org2ForClone, org1ForClone, database);
			cloneFactory.Save();

			primaryRelationship.Reload();
			AssertEquals("Should still be staff", staff.PK, primaryRelationship.PPR_PrimaryId);
		}

		[ExpectNoExceptions]
		public void TestCloneContacts_MultipleUserAccountsLinked()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db1 = licence1.Database;
			var licence2 = BillingTestHelper.CreateAnotherDatabase(licence1, "PRD");
			var db2 = licence2.Database;
			var enterprise = db1.LicEnterprise;

			var org1 = enterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence3 = BillingTestHelper.CreateAnotherLicence(db1, "DEF");
			var org2 = licence3.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_WebAccessEnabled = true;

			var orgRight1 = org1.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.CustomerService.Code))[0] as OrgSecurity;
			var contactRightQuery1 = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight1.PK);
			contactRightQuery1.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact1.PK);
			var contactRight1 = contact1.SecurityRightsForBindingOnly.Find(contactRightQuery1)[0] as OrgSecurityContacts;
			orgRight1.OX_Granted = false;
			contactRight1.OZ_Granted = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = db1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = db2.PK;
			userAccount2.EUA_UserID = "AAA";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, db1);
			cloner.CloneContacts(org1ForClone, org2ForClone, db2);
			cloneFactory.Save();
		}

		[ExpectNoExceptions]
		public void TestCloneContacts_MultipleUserAccountsLinked_SameDatabase()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db1 = licence1.Database;
			var enterprise = db1.LicEnterprise;
			var org1 = enterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence3 = BillingTestHelper.CreateAnotherLicence(db1, "DEF");
			var org2 = licence3.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_WebAccessEnabled = true;

			var orgRight1 = org1.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, EDIWebSecurityRightsList.CustomerService.Code))[0] as OrgSecurity;
			var contactRightQuery1 = new ZQuery(OrgSecurityContactsSchema.OZ_OX, orgRight1.PK);
			contactRightQuery1.AddToFilter(OrgSecurityContactsSchema.OZ_OC, contact1.PK);
			var contactRight1 = contact1.SecurityRightsForBindingOnly.Find(contactRightQuery1)[0] as OrgSecurityContacts;
			orgRight1.OX_Granted = false;
			contactRight1.OZ_Granted = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = db1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = db1.PK;
			userAccount2.EUA_UserID = "AAA";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, db1);
			cloneFactory.Save();
		}

		public void TestCloneContacts_NoMatchedContactsDBHits()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			const int staffCount = 5;

			for (int i = 0; i < staffCount; i++)
			{
				var contact = org1.Contacts.AddNew();
				contact.OC_ContactName = $"User {i}";
				contact.OC_Email = $"user{i}@test.org";
				contact.OC_OA_OrgAddress = org1.MainAddress.PK;
				contact.OC_DetailsVerified = ZDateTime.Now;

				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = $"US{i}";
				userAccount.EUA_FullName = $"User {i}";
				userAccount.EUA_Email = $"user{i}@test.org";
				userAccount.EUA_OC_WebAccessContact = contact.PK;
			}

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedOrg1 = cloneFactory.Load<OrgHeader>(org1.PK);
			var loadedOrg2 = cloneFactory.Load<OrgHeader>(org2.PK);

			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(loadedOrg1, loadedOrg2, database);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ EdiCustomerUserAccountSchema.Constants.TableName, 2 },
				{ ClientBranchSchema.Constants.TableName, 1 },
				{ LicenceDatabaseSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgContactSchema.Constants.TableName, 3 },
				{ OrgDocumentSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgSecuritySchema.Constants.TableName, 2 },
				{ OrgSecurityContactsSchema.Constants.TableName, 1 },
				{ GlbPersonSchema.Constants.TableName, 1 },
				{ GlbPersonPrimaryRelationshipSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedHitCounts, cloneFactory);
		}

		public void TestCloneContacts_WithMatchedContactsDBHits()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");

			var nonMasterOrg = licence2.Company.Header;
			nonMasterOrg.Contacts.RemoveAndDeleteAll();

			var masterOrg = database.LicEnterprise.Organisation;
			masterOrg.OH_Code = "SACAWAGA";
			masterOrg.Contacts.RemoveAndDeleteAll();

			const int staffCount = 5;

			for (int i = 0; i < staffCount; i++)
			{
				var contact1 = nonMasterOrg.Contacts.AddNew();
				contact1.OC_ContactName = $"User {i}";
				contact1.OC_Email = $"user{i}@test.org";
				contact1.OC_OA_OrgAddress = nonMasterOrg.MainAddress.PK;
				contact1.OC_DetailsVerified = ZDateTime.Now;

				var contact2 = masterOrg.Contacts.AddNew();
				contact2.OC_ContactName = $"User {i}";
				contact2.OC_Email = $"user{i}@test.org";

				var userAccount = Factory.New<EdiCustomerUserAccount>();
				userAccount.EUA_LD = database.PK;
				userAccount.EUA_UserID = $"US{i}";
				userAccount.EUA_FullName = $"User {i}";
				userAccount.EUA_Email = $"user{i}@test.org";
				userAccount.EUA_OC_WebAccessContact = contact1.PK;
			}

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedOrg1 = cloneFactory.Load<OrgHeader>(nonMasterOrg.PK);
			var loadedOrg2 = cloneFactory.Load<OrgHeader>(masterOrg.PK);

			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(loadedOrg1, loadedOrg2, database);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ EdiCustomerUserAccountSchema.Constants.TableName, 1 },
				{ ClientBranchSchema.Constants.TableName, 1 },
				{ LicenceDatabaseSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgContactSchema.Constants.TableName, 3 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgSecuritySchema.Constants.TableName, 2 },
				{ OrgSecurityContactsSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ OrgDocumentSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedHitCounts, cloneFactory);
		}

		public void TestCloneContacts_ShouldMatchEmailBeforePerson()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "user1@test.org";
			contact1.OC_OA_OrgAddress = org1.MainAddress.PK;
			contact1.OC_DetailsVerified = ZDateTime.Now;

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "User 1";
			userAccount.EUA_Email = "user1@test.org";
			userAccount.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			var contact2A = org2.Contacts.AddNew();
			contact2A.OC_ContactName = "User 2";
			contact2A.OC_Email = "user1@test.org";

			var contact2B = org2.Contacts.AddNew();
			contact2B.OC_ContactName = "User 3";
			contact2B.OC_Email = "user2@test.org";
			contact2B.OC_PER = contact1.OC_PER;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedOrg1 = cloneFactory.Load<OrgHeader>(org1.PK);
			var loadedOrg2 = cloneFactory.Load<OrgHeader>(org2.PK);

			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(loadedOrg1, loadedOrg2, database);
			var loadedUserAccount = cloneFactory.Load<EdiCustomerUserAccount>(userAccount.PK);
			AssertEquals("Should have matched to contact with same email", contact2A.PK, loadedUserAccount.EUA_OC_WebAccessContact);
		}

		public void TestCloneContacts_ShouldSupersedeContactsWithNoUserAccountsRemaining()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var unrelatedLicence = BillingTestHelper.CreateLicence(Factory, "EEE", "CDE", "SYD");

			var database = licence.Database;
			var database2 = unrelatedLicence.Database;
			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");

			var nonMasterOrg = licence2.Company.Header;
			nonMasterOrg.Contacts.RemoveAndDeleteAll();

			var masterOrg = database.LicEnterprise.Organisation;
			masterOrg.OH_Code = "SACAWAGA";
			masterOrg.Contacts.RemoveAndDeleteAll();

			var contact1 = nonMasterOrg.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			contact1.OC_Email = "user1@test.org";
			contact1.OC_OA_OrgAddress = nonMasterOrg.MainAddress.PK;
			contact1.OC_DetailsVerified = ZDateTime.Now;

			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_LD = database.PK;
			userAccount.EUA_UserID = "US1";
			userAccount.EUA_FullName = "User 1";
			userAccount.EUA_Email = "user1@test.org";
			userAccount.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_UserID = "U1V";
			userAccount2.EUA_FullName = "User 2";
			userAccount2.EUA_Email = "user2@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			var contact2 = nonMasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User 3";
			contact2.OC_Email = "user3@test.org";
			contact2.OC_OA_OrgAddress = nonMasterOrg.MainAddress.PK;
			contact2.OC_DetailsVerified = ZDateTime.Now;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User 3";
			userAccount3.EUA_Email = "user3@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact2.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedOrg1 = cloneFactory.Load<OrgHeader>(nonMasterOrg.PK);
			var loadedOrg2 = cloneFactory.Load<OrgHeader>(masterOrg.PK);

			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(loadedOrg1, loadedOrg2, database);

			var contact1Reloaded = cloneFactory.Load<OrgContact>(contact1.PK);
			var contact2Reloaded = cloneFactory.Load<OrgContact>(contact2.PK);
			AssertEquals("Should not be superseded since it still has an active user account in a different db", true, contact1Reloaded.OC_IsActive);
			AssertEquals("Should not be superseded since it still has an active user account in a different db", false, contact1Reloaded.WebAccessSuperseded);
			AssertEquals("Should have web access superseded since there are no user accounts left on the contact", false, contact2Reloaded.OC_IsActive);
			AssertEquals("Should have web access superseded since there are no user accounts left on the contact", true, contact2Reloaded.WebAccessSuperseded);
		}

		public void TestCloneContacts_ShouldKeepIsActive()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;

			var person1 = Factory.New<GlbPerson>();
			var person2 = Factory.New<GlbPerson>();

			var org1 = database.LicEnterprise.Organisation;
			org1.OH_Code = "CAWAGA";
			org1.Contacts.RemoveAndDeleteAll();

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.OC_PER = person1.PK;
			contact1a.OC_IsActive = false;
			person1.UpdateFromContact(contact1a);

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "SACAWAGA";
			org2.Contacts.RemoveAndDeleteAll();

			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one1@test.org";
			contact2a.OC_WebAccessEnabled = false;
			contact2a.OC_PER = person1.PK;

			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one2@test.org";
			contact2b.OC_WebAccessEnabled = true;
			contact2b.OC_PER = person1.PK;

			var contact3 = org2.Contacts.AddNew();
			contact3.OC_ContactName = "User Two";
			contact3.OC_Email = "user.one@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = person2.PK;

			Factory.Save();

			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var org1ForClone = cloneFactory.Load<OrgHeader>(org1.PK);
			var org2ForClone = cloneFactory.Load<OrgHeader>(org2.PK);
			var cloner = new ContactCloner(cloneFactory);
			cloner.CloneContacts(org1ForClone, org2ForClone, database);
			cloneFactory.Save();

			contact3.Reload();
			AssertEquals("Should be false as the contact it is copying from is false", false, contact3.OC_IsActive);
		}

		public void TestCloneContactAddressFromDifferentDatabase()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database1 = licence1.Database;

			var licenceOrg1 = licence1.Company.Header;

			var address1 = licenceOrg1.Addresses.AddNew();
			address1.OA_Address1 = "1 Test Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_CompanyNameOverride = "Company AAA";
			address1.OA_Code = "branch a (aaa)";

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database1.PK;
			clientBranch1.LCB_Code = "AAA";
			clientBranch1.LCB_Name = "Branch A";
			clientBranch1.LCB_OA = address1.PK;

			var licence2 = BillingTestHelper.CreateLicence(Factory, "BBB", "DEF", "NJT");
			var database2 = licence2.Database;

			var licenceOrg2 = licence2.Company.Header;

			var address2 = licenceOrg2.Addresses.AddNew();
			address2.OA_Address1 = "1 Test Rd";
			address2.OA_Address2 = "Building A";
			address2.OA_City = "Sydney";
			address2.OA_State = "NSW";
			address2.OA_PostCode = "2000";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.OA_CompanyNameOverride = "Company AAA";

			var clientBranch11 = Factory.New<ClientBranch>();
			clientBranch11.LCB_LD = database1.PK;
			clientBranch11.LCB_Code = "AAA";
			clientBranch11.LCB_Name = "BRANCH A";
			clientBranch11.LCB_OA = address2.PK;

			Factory.Save();

			var logs = new ProcessStatusForTest();
			var cloneFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var cloner = new ContactCloner(cloneFactory, logs);

			var sourceOrg = cloneFactory.Load<OrgHeader>(licenceOrg2.PK);
			var targetOrg = cloneFactory.Load<OrgHeader>(licenceOrg1.PK);
			targetOrg.Contacts.Reload(true);
			sourceOrg.Contacts.Reload(true);

			cloner.CloneContacts(sourceOrg, targetOrg, database1);
			AssertNoExceptionThrown(() => cloneFactory.Save());

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var targetOrgAfterClone = newFactory.Load<OrgHeader>(targetOrg.PK);
			AssertEquals("should contain new address with OA_CODE 'BRANCH A_1 (AAA)'", true, targetOrgAfterClone.Addresses.Cast<OrgAddress>().Any(x => x.OA_Code == "BRANCH A_1 (AAA)"));
		}

		#region MoveContactToMasterOrg

		public void TestMoveContactToMasterOrg()
		{
			SetupForMasterOrgMoveTest();

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Email", "user.one@test.org", contact2a.OC_Email);
			AssertEquals("Web access", true, contact2a.OC_WebAccessEnabled);
			AssertEquals("Password", true, contact2a.VerifyPassword("123456"));
			AssertEquals("Address", ZGuid.Empty, contact2a.OC_OA_OrgAddress);
			AssertEquals("Should be linked to Person of source contact", NonMasterOrgContact.OC_PER, contact2a.OC_PER);

			var contact1aMyAccountLog = NonMasterOrgContact.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			var contact2aMyAccountLog = contact2a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			AssertNotNull("Should have myaccount disclaimer acknowledge log", contact2aMyAccountLog);
			AssertEquals("Log event time should copy over", contact1aMyAccountLog.SL_EventTime, contact2aMyAccountLog.SL_EventTime);

			var contact2b = loadedMasterOrg.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User Two");
			AssertEquals(true, contact2b.OC_IsActive);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);

			AssertContactProperties(contact2a);
			AssertEquals(contact2b.Person.PrimaryRelationship.PPR_PrimaryId, contact2b.PK);
		}

		public void TestMoveProductionContactToMasterOrg_ExistingContact()
		{
			SetupForMasterOrgMoveTest();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.three@test.org";
			contact2.OC_PER = NonMasterOrgContact.OC_PER;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Email should be updated", "user.one@test.org", contact2a.OC_Email);
			AssertEquals("Web access", true, contact2a.OC_WebAccessEnabled);
			AssertEquals("Password", true, contact2a.VerifyPassword("123456"));
			AssertEquals("Address", ZGuid.Empty, contact2a.OC_OA_OrgAddress);
			AssertEquals("Should be linked to Person of source contact", NonMasterOrgContact.OC_PER, contact2a.OC_PER);

			var contact1aMyAccountLog = NonMasterOrgContact.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			var contact2aMyAccountLog = contact2a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			AssertNotNull("Should have myaccount disclaimer acknowledge log", contact2aMyAccountLog);
			AssertEquals("Log event time should copy over", contact1aMyAccountLog.SL_EventTime, contact2aMyAccountLog.SL_EventTime);

			var contact2b = loadedMasterOrg.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User Two");
			AssertEquals(true, contact2b.OC_IsActive);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);

			AssertContactProperties(contact2a);
			AssertEquals(contact2b.Person.PrimaryRelationship.PPR_PrimaryId, contact2b.PK);
		}

		public void TestMoveProductionContactToMasterOrg_ShouldMatchOnEmailBeforePerson()
		{
			SetupForMasterOrgMoveTest();

			var contact3 = MasterOrg.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.one@test.org";

			var contact4 = MasterOrg.Contacts.AddNew();
			contact4.OC_ContactName = "User X";
			contact4.OC_Email = "user.x@test.org";
			contact4.OC_PER = NonMasterOrgContact.OC_PER;
			Factory.Save();

			AssertEquals("Precondition", 3, MasterOrg.Contacts.Count);

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(3, loadedMasterOrg.Contacts.Count);

			AssertEquals("Should be the returned contact", clonedContact.PK, contact3.PK);
			AssertEquals("Should be linked to Person of source contact", NonMasterOrgContact.OC_PER, contact3.OC_PER);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact3.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestMoveNonProductionContactToMasterOrg_ExistingContact()
		{
			SetupForMasterOrgMoveTest();

			Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			Factory.Save();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.one@test.org";
			contact2.OC_WebAccessEnabled = false;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;
			Factory.Save();

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User Three");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Email on master org contact should be retained", "user.one@test.org", contact2a.OC_Email);
			AssertEquals("Web access should be copied across", true, contact2a.OC_WebAccessEnabled);
			AssertEquals("Password should be copied across", true, contact2a.VerifyPassword("123456"));
			AssertEquals("Address", ZGuid.Empty, contact2a.OC_OA_OrgAddress);
			AssertEquals("Should be linked to Person of source contact", NonMasterOrgContact.OC_PER, contact2a.OC_PER);

			var contact1aMyAccountLog = NonMasterOrgContact.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			var contact2aMyAccountLog = contact2a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			AssertNotNull("Should have myaccount disclaimer acknowledge log", contact2aMyAccountLog);
			AssertEquals("Log event time should copy over", contact1aMyAccountLog.SL_EventTime, contact2aMyAccountLog.SL_EventTime);

			var contact2b = loadedMasterOrg.Contacts.Cast<OrgContact>().Single(x => x.OC_ContactName == "User Two");
			AssertEquals(true, contact2b.OC_IsActive);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);

			AssertContactProperties(contact2a);
			AssertEquals(contact2b.Person.PrimaryRelationship.PPR_PrimaryId, contact2b.PK);
		}

		public void TestMoveContactToMasterOrg_ExistingContactWithPassword()
		{
			SetupForMasterOrgMoveTest();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.three@test.org";
			contact2.SetHashedPassword("abcde");
			contact2.OC_PER = NonMasterOrgContact.OC_PER;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Password should be retained from master org contact", true, contact2a.VerifyPassword("abcde"));

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestMoveContactToMasterOrg_ExistingContactNoPasswords()
		{
			SetupForMasterOrgMoveTest();
			NonMasterOrgContact.RemovePasswordAndHash();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.three@test.org";
			contact2.OC_PER = NonMasterOrgContact.OC_PER;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("No Password", false, contact2a.HasPassword);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Should not require email verification", false, loadedEdiCustomerUserAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Contact relationship should be active", true, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
		}

		public void TestMoveContactToMasterOrg_ExistingContactNoPasswordsInactiveContactRelationship()
		{
			SetupForMasterOrgMoveTest();

			UserAccount1.EUA_IsContactRelationshipActive = false;
			UserAccount1.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			NonMasterOrgContact.RemovePasswordAndHash();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.three@test.org";
			contact2.OC_PER = NonMasterOrgContact.OC_PER;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("No Password", false, contact2a.HasPassword);

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Should require email verification", true, loadedEdiCustomerUserAccount1.EUA_IsEmailVerificationRequired);
			AssertEquals("Contact relationship should still be inactive", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Contact relationship should be unchanged", ContactRelationshipStatusList.Codes.AccountReactivated, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestMoveProductionContactToMasterOrg_ExistingSameAddress()
		{
			SetupForMasterOrgMoveTest();

			var masterOrgAddress = MasterOrg.Addresses.AddNew();
			masterOrgAddress.OA_Address1 = NonMasterOrgAddress.OA_Address1;
			masterOrgAddress.OA_Address2 = NonMasterOrgAddress.OA_Address2;
			masterOrgAddress.OA_City = NonMasterOrgAddress.OA_City;
			masterOrgAddress.OA_State = NonMasterOrgAddress.OA_State;
			masterOrgAddress.OA_PostCode = NonMasterOrgAddress.OA_PostCode;
			masterOrgAddress.OA_RL_NKRelatedPortCode = NonMasterOrgAddress.OA_RL_NKRelatedPortCode;
			masterOrgAddress.OA_CompanyNameOverride = NonMasterOrgAddress.OA_CompanyNameOverride;
			masterOrgAddress.OA_Code = NonMasterOrgAddress.OA_Code;
			Factory.Save();

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Address should be reassigned to matched address", masterOrgAddress.PK, contact2a.OC_OA_OrgAddress);
		}

		public void TestMoveProductionContactToMasterOrg_ExistingMainAddress()
		{
			SetupForMasterOrgMoveTest();

			var masterOrgAddress = MasterOrg.MainAddress;
			masterOrgAddress.OA_Address1 = NonMasterOrgAddress.OA_Address1;
			masterOrgAddress.OA_Address2 = NonMasterOrgAddress.OA_Address2;
			masterOrgAddress.OA_City = NonMasterOrgAddress.OA_City;
			masterOrgAddress.OA_State = NonMasterOrgAddress.OA_State;
			masterOrgAddress.OA_PostCode = NonMasterOrgAddress.OA_PostCode;
			masterOrgAddress.OA_RL_NKRelatedPortCode = NonMasterOrgAddress.OA_RL_NKRelatedPortCode;
			masterOrgAddress.OA_CompanyNameOverride = NonMasterOrgAddress.OA_CompanyNameOverride;
			masterOrgAddress.OA_Code = NonMasterOrgAddress.OA_Code;
			Factory.Save();

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Address FK should be removed since it's the main address", ZGuid.Empty, contact2a.OC_OA_OrgAddress);
		}

		public void TestMoveContactToMasterOrg_ExistingContactSameEmailAddress()
		{
			SetupForMasterOrgMoveTest();

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.one@test.org";
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Should be the returned contact", contact2.PK, UserAccount1.EUA_OC_WebAccessContact);
		}

		public void TestMoveContactToMasterOrg_ExistingContactSamePersonInactive()
		{
			SetupForMasterOrgMoveTest();
			var person1 = NonMasterOrgContact.Person;
			var person2 = Factory.New<GlbPerson>();

			var contact2a = MasterOrg.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one1@test.org";
			contact2a.OC_WebAccessEnabled = false;
			contact2a.OC_PER = person1.PK;
			contact2a.OC_IsActive = false;

			var contact2b = MasterOrg.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one2@test.org";
			contact2b.OC_WebAccessEnabled = true;
			contact2b.OC_PER = person1.PK;
			contact2b.OC_IsActive = false;

			var contact3 = MasterOrg.Contacts.AddNew();
			contact3.OC_ContactName = "User Two";
			contact3.OC_Email = "user.one@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = person2.PK;

			var userAccount2a = Factory.New<EdiCustomerUserAccount>();
			userAccount2a.EUA_LD = Database.PK;
			userAccount2a.EUA_UserID = "USA";
			userAccount2a.EUA_FullName = "User One";
			userAccount2a.EUA_Email = "user.one1@test.org";
			userAccount2a.EUA_OC_WebAccessContact = contact2a.PK;

			var userAccount2b = Factory.New<EdiCustomerUserAccount>();
			userAccount2b.EUA_LD = Database.PK;
			userAccount2b.EUA_UserID = "USB";
			userAccount2b.EUA_FullName = "User One (1)";
			userAccount2b.EUA_Email = "user.one2@test.org";
			userAccount2b.EUA_OC_WebAccessContact = contact2b.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = Database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User Two";
			userAccount3.EUA_Email = "user.one@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			UserAccount1.Reload();
			AssertEquals("Should be a new one since contact is inactive", contact3.PK, UserAccount1.EUA_OC_WebAccessContact);
		}

		public void TestMoveContactToMasterOrg_ExistingContactSameEmailInactiveShouldReactivateContact()
		{
			SetupForMasterOrgMoveTest();

			var contact3 = MasterOrg.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.one@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = Database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User Three";
			userAccount3.EUA_Email = "user.one@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			UserAccount1.Reload();
			AssertEquals("Should use the inactive contact", contact3.PK, UserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Should reactivate the inactive contact", true, contact3.OC_IsActive);
			Assert("Should inherit dissolved contact's password", contact3.VerifyPassword("123456"));
			AssertEquals("Should have inactive relationship", false, UserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Should have DCP status", ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, UserAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestMoveContactToMasterOrg_ExistingContactSamePersonInactiveShouldReactivateContact()
		{
			SetupForMasterOrgMoveTest();
			var person1 = NonMasterOrgContact.Person;

			var contact3 = MasterOrg.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.one1@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			contact3.OC_PER = person1.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = Database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User Three";
			userAccount3.EUA_Email = "user.one1@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			Factory.Save();

			ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			UserAccount1.Reload();
			AssertEquals("Should use the inactive contact", contact3.PK, UserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Should reactivate the inactive contact", true, contact3.OC_IsActive);
			Assert("Should inherit dissolved contact's password", contact3.VerifyPassword("123456"));
			AssertEquals("Should have inactive relationship", false, UserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Should have DCP status", ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, UserAccount1.EUA_ContactRelationshipStatus);
		}

		public void TestMoveContactToMasterOrg_ShouldPrioritiseActiveContactOnPersonMatch()
		{
			SetupForMasterOrgMoveTest();
			var person1 = NonMasterOrgContact.Person;

			var contact3 = MasterOrg.Contacts.AddNew();
			contact3.OC_ContactName = "User Three";
			contact3.OC_Email = "user.one1@test.org";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = false;
			contact3.OC_PER = person1.PK;

			var contact4 = MasterOrg.Contacts.AddNew();
			contact4.OC_ContactName = "User Four";
			contact4.OC_Email = "user.one2@test.org";
			contact4.OC_WebAccessEnabled = true;
			contact4.OC_IsActive = true;
			contact4.OC_PER = person1.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = Database.PK;
			userAccount3.EUA_UserID = "US3";
			userAccount3.EUA_FullName = "User Three";
			userAccount3.EUA_Email = "user.one1@test.org";
			userAccount3.EUA_OC_WebAccessContact = contact3.PK;

			var userAccount4 = Factory.New<EdiCustomerUserAccount>();
			userAccount4.EUA_LD = Database.PK;
			userAccount4.EUA_UserID = "US4";
			userAccount4.EUA_FullName = "User Four";
			userAccount4.EUA_Email = "user.one2@test.org";
			userAccount4.EUA_OC_WebAccessContact = contact4.PK;

			Factory.Save();

			ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			UserAccount1.Reload();
			AssertEquals("Should use the active contact on the person", contact4.PK, UserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact should remain active", true, contact4.OC_IsActive);
			AssertEquals("Contact should remain inactive", false, contact3.OC_IsActive);
		}

		public void TestMoveContactToMasterOrg_MultipleUserAccountsLinked()
		{
			SetupForMasterOrgMoveTest();
			var licence2 = BillingTestHelper.CreateAnotherDatabase(Licence1, "PRD");
			var database2 = licence2.Database;

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Three";
			contact2.OC_Email = "user.three@test.org";
			contact2.SetHashedPassword("abcde");
			contact2.OC_PER = NonMasterOrgContact.OC_PER;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = Database.PK;
			userAccount2.EUA_UserID = "US3";
			userAccount2.EUA_FullName = "User Three";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact2.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_LD = database2.PK;
			userAccount3.EUA_UserID = "US4";
			userAccount3.EUA_FullName = "User Four";
			userAccount3.EUA_Email = "user.one@test.org";
			userAccount3.EUA_OC_WebAccessContact = NonMasterOrgContact.PK;

			var clonedContact = ContactCloner.MoveContactToMasterOrg(NonMasterOrgContact, Database);
			Factory.Save();

			NonMasterOrgContact.Reload();
			AssertEquals("Should not be deactivated if it still has live user accounts referencing it", true, NonMasterOrgContact.OC_IsActive);

			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);

			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);

			AssertEquals(2, loadedMasterOrg.Contacts.Count);

			var contact2a = loadedMasterOrg.Contacts.Cast<EDIOrgContact>().Single(x => x.OC_ContactName == "User One");
			AssertEquals("Should be the returned contact", clonedContact.PK, contact2a.PK);
			AssertEquals("Password should be retained from master org contact", true, contact2a.VerifyPassword("abcde"));

			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account",
				ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);
			var loadedEdiCustomerUserAccount3 = loadFactory.Load<EdiCustomerUserAccount>(userAccount3.PK);
			AssertEquals("Should not have moved", NonMasterOrgContact.PK, loadedEdiCustomerUserAccount3.EUA_OC_WebAccessContact);
		}

		#region Implementation

		LicenceHeader Licence1;
		LicenceDatabase Database;
		OrgHeader MasterOrg;
		OrgAddress NonMasterOrgAddress;
		EDIOrgContact NonMasterOrgContact;
		EdiCustomerUserAccount UserAccount1;

		void SetupForMasterOrgMoveTest()
		{
			Licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			Database = Licence1.Database;
			var enterprise = Database.LicEnterprise;

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
			nonMasterLicence.LA_LD = Database.PK;
			nonMasterLicence.LA_LC = nonMasterCompany.PK;
			nonMasterLicence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);

			MasterOrg = Licence1.Database.WebAccessOrg;
			MasterOrg.Contacts.RemoveAndDeleteAll();

			NonMasterOrgAddress = nonMasterOrg.Addresses.AddNew();
			NonMasterOrgAddress.OA_Address1 = "1 Test Rd";
			NonMasterOrgAddress.OA_Address2 = "Building A";
			NonMasterOrgAddress.OA_City = "Sydney";
			NonMasterOrgAddress.OA_State = "NSW";
			NonMasterOrgAddress.OA_PostCode = "2000";
			NonMasterOrgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			NonMasterOrgAddress.OA_CompanyNameOverride = "Company AAA";

			NonMasterOrgContact = nonMasterOrg.Contacts.AddNew() as EDIOrgContact;
			NonMasterOrgContact.OC_ContactName = "User One";
			NonMasterOrgContact.OC_Email = "user.one@test.org";
			NonMasterOrgContact.OC_WebAccessEnabled = true;
			NonMasterOrgContact.SetHashedPassword("123456");
			NonMasterOrgContact.OC_OA_OrgAddress = NonMasterOrgAddress.PK;
			NonMasterOrgContact.LogMyAccountDisclaimerAcknowledgementIfRequired(Database);
			NonMasterOrgContact.IsAccountsReceivableContact = true;
			NonMasterOrgContact.IsBorderWiseAdministrator = true;
			NonMasterOrgContact.IsCustomerServiceContact = true;
			NonMasterOrgContact.IsERequestApprover = true;
			NonMasterOrgContact.IsInformationServicesTechnicalAdministrator = true;
			NonMasterOrgContact.IsCertificationProgramContact = true;
			NonMasterOrgContact.AddDocumentGroup(ContactType.Consignor.Code, true, null);

			UserAccount1 = Factory.New<EdiCustomerUserAccount>();
			UserAccount1.EUA_LD = Database.PK;
			UserAccount1.EUA_UserID = "US1";
			UserAccount1.EUA_FullName = "User One";
			UserAccount1.EUA_Email = "user.one@test.org";
			UserAccount1.EUA_OC_WebAccessContact = NonMasterOrgContact.PK;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = Database.PK;
			clientBranch1.LCB_Code = "AAA";
			clientBranch1.LCB_Name = "Branch A";
			clientBranch1.LCB_OA = NonMasterOrgAddress.PK;

			var contact2 = MasterOrg.Contacts.AddNew();
			contact2.OC_ContactName = "User Two";
			contact2.OC_Email = "user.two@test.org";

			Factory.Save();
		}

		void AssertContactProperties(EDIOrgContact contact)
		{
			AssertEquals(contact.Person.PrimaryRelationship.PPR_PrimaryId, contact.PK);
			AssertEquals(true, contact.IsAccountsReceivableContact);
			AssertEquals(true, contact.IsBorderWiseAdministrator);
			AssertEquals(true, contact.IsCustomerServiceContact);
			AssertEquals(true, contact.IsERequestApprover);
			AssertEquals(true, contact.IsInformationServicesTechnicalAdministrator);
			AssertEquals(true, contact.IsCertificationProgramContact);
			AssertEquals(true, contact.HasDocumentGroup(ContactType.Consignor.Code));
		}

		#endregion

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}

	class ProcessStatusForTest : IProcessStatus
	{
		public readonly ZStringBuilder Logs = new ZStringBuilder();

		void IProcessStatus.UpdateStatus(string status, int progressValue)
		{
			Logs.AppendLine($"{status} - {progressValue}");
		}
	}
}

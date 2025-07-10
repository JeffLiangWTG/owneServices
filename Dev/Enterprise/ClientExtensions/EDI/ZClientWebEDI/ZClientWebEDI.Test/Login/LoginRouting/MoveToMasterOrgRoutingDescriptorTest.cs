using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class MoveToMasterOrgRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact = Factory.New<EDIOrgContact>();
			var database = Factory.New<LicenceDatabase>();
			var descriptor = new AccountReconfigurationRoutingDescriptor(contact, database, null);
			AssertEquals("No routing page url", null, descriptor.RoutingUrl);
		}

		public void TestIsRoutingRequired()
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
			nonMasterOrgContact.SetHashedPassword("123456");
			var masterOrgContact = masterOrg.Contacts.AddNew() as EDIOrgContact;
			masterOrgContact.OC_ContactName = "User Two";
			masterOrgContact.OC_Email = "user.two@test.org";
			masterOrgContact.OC_WebAccessEnabled = true;
			masterOrgContact.SetHashedPassword("123456");
			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = nonMasterOrgContact.PK;
			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database.PK;
			userAccount2.EUA_UserID = "US1";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = masterOrgContact.PK;
			var token1 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount1);
			var identityManager1 = new MyAccountLoginRouterIdentityManager(Factory);
			identityManager1.PopulatePropertiesFromToken(token1);
			var token2 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount2);
			var identityManager2 = new MyAccountLoginRouterIdentityManager(Factory);
			identityManager2.PopulatePropertiesFromToken(token2);
			var descriptor1 = new AccountReconfigurationRoutingDescriptor(nonMasterOrgContact, database, identityManager1);
			AssertEquals("Routing is required since it's not linked to the master org", true, descriptor1.IsRoutingRequired);
			var descriptor2 = new AccountReconfigurationRoutingDescriptor(masterOrgContact, database, identityManager2);
			AssertEquals("No routing required since it's already linked to the master org", false, descriptor2.IsRoutingRequired);
		}

		public void TestRoutingAction()
		{
			SetupForMasterOrgMoveTest();
			AssertEquals("Precondition", NonMasterOrgContact.Person.PrimaryRelationship.PPR_PrimaryId, NonMasterOrgContact.PK);
			var token1 = MyAccountLoginRouterIdentityManager.GenerateToken(UserAccount1);
			var identityManager = new MyAccountLoginRouterIdentityManager(Factory);
			identityManager.PopulatePropertiesFromToken(token1);
			Factory.Save();
			var descriptor = new AccountReconfigurationRoutingDescriptor(NonMasterOrgContact, UserAccount1.Database, identityManager);
			AssertEquals("Precondition", true, descriptor.IsRoutingRequired);
			descriptor.RoutingAction();
			NonMasterOrgContact.Reload();
			AssertEquals(false, NonMasterOrgContact.OC_IsActive);
			NonMasterOrgAddress.Reload();
			AssertEquals("Shouldn't change original address", true, NonMasterOrgAddress.OA_IsActive);
			var loadFactory = new BusinessObjectFactory();
			var loadedMasterOrg = loadFactory.Load<OrgHeader>(MasterOrg.PK);
			AssertEquals(1, loadedMasterOrg.Contacts.Count);
			var contact2a = loadedMasterOrg.Contacts[0];
			AssertEquals("Email", "user.one@test.org", contact2a.OC_Email);
			AssertEquals("Web access", true, contact2a.OC_WebAccessEnabled);
			AssertEquals("Password", true, contact2a.VerifyPassword("123456"));
			AssertEquals("Address", ZGuid.Empty, contact2a.OC_OA_OrgAddress);
			AssertEquals("Should be linked to Person of source contact", NonMasterOrgContact.OC_PER, contact2a.OC_PER);
			var contact1aMyAccountLog = NonMasterOrgContact.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			var contact2aMyAccountLog = contact2a.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.ClickThroughAgreementExecuted.Code);
			AssertNotNull("Should have myaccount disclaimer acknowledge log", contact2aMyAccountLog);
			AssertEquals("Log event time should copy over", contact1aMyAccountLog.SL_EventTime, contact2aMyAccountLog.SL_EventTime);
			var loadedEdiCustomerUserAccount1 = loadFactory.Load<EdiCustomerUserAccount>(UserAccount1.PK);
			AssertEquals(contact2a.PK, loadedEdiCustomerUserAccount1.EUA_OC_WebAccessContact);
			AssertEquals("Contact relationship should be deactivated", false, loadedEdiCustomerUserAccount1.EUA_IsContactRelationshipActive);
			AssertEquals("Status should be set to prompt login options so the user becomes aware of changes to their account", ContactRelationshipStatusList.Codes.DissolvedContactWithPassword, loadedEdiCustomerUserAccount1.EUA_ContactRelationshipStatus);
			AssertEquals("Primary relationship should be updated", contact2a.Person.PrimaryRelationship.PPR_PrimaryId, contact2a.PK);
			AssertNotEquals("IdentityManager token should have been regenerated", token1, identityManager.Token);
			AssertEquals("IdentityManager user account should remain unchanged", UserAccount1.PK, identityManager.UserAccount.PK);
			AssertEquals("IdentityManager contact should be updated", contact2a.PK, identityManager.Contact.PK);
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
			Factory.Save();
		}
		#endregion
	}
}
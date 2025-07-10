using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class UserAccountRelationshipControlTest : TestCaseWithFactory
	{
		public void TestPageLoadShouldHideCheckboxesForContacts()
		{
			SetupUsers();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var organisationText = ((ZTextLabel)repeater.Items[0].FindControl("Organisation")).Text;
			var contactCheckBox = (ZCheckBox)repeater.Items[0].FindControl("IsContactRelationshipActive");
			AssertEquals("Precondition", repeaterDataSource[0].Organisation, organisationText);
			AssertEquals("Checkbox should not be visible for contact wrapper", false, contactCheckBox.Visible);
			var userAccountOrganisationText = ((ZTextLabel)repeater.Items[1].FindControl("Organisation")).Text;
			var userAccountCheckBox = (ZCheckBox)repeater.Items[1].FindControl("IsContactRelationshipActive");
			AssertEquals("Precondition", string.Empty, userAccountOrganisationText);
			AssertEquals("Checkbox should be visible for user account wrapper", true, userAccountCheckBox.Visible);
		}

		public void TestPageLoadShouldSetCheckBoxesForUserAccounts()
		{
			SetupUsers();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var trainingUserLicenceType = ((ZTextLabel)repeater.Items[2].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Training), repeaterDataSource[2].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[2].LicenceType, trainingUserLicenceType);
			var trainingUserRelationshipCheckBox = (ZCheckBox)repeater.Items[2].FindControl("IsContactRelationshipActive");
			AssertEquals("Training user's contact relationship is inactive so box should not be checked", false, trainingUserRelationshipCheckBox.Checked);
			var testUserLicenceType = ((ZTextLabel)repeater.Items[3].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Test), repeaterDataSource[3].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[3].LicenceType, testUserLicenceType);
			var testUserRelationshipCheckBox = (ZCheckBox)repeater.Items[3].FindControl("IsContactRelationshipActive");
			AssertEquals("Test user's contact relationship is active so box should be checked", true, testUserRelationshipCheckBox.Checked);
		}

		public void TestSave()
		{
			SetupUsers();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var testUserLicenceType = ((ZTextLabel)repeater.Items[3].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Test), repeaterDataSource[3].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[3].LicenceType, testUserLicenceType);
			var testUserRelationshipCheckBox = (ZCheckBox)repeater.Items[3].FindControl("IsContactRelationshipActive");
			AssertEquals(true, testUserRelationshipCheckBox.Checked);
			testUserRelationshipCheckBox.Checked = false;
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			var newFactory = new BusinessObjectFactory();
			var productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			var testUser1Reloaded = newFactory.Load<EdiCustomerUserAccount>(TestUser.PK);
			var testUser2Reloaded = newFactory.Load<EdiCustomerUserAccount>(TrainingUser.PK);
			AssertEquals("Should remain unchanged", true, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be updated", false, testUser1Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be updated", ContactRelationshipStatusList.Codes.SelfDeactivation, testUser1Reloaded.EUA_ContactRelationshipStatus);
			AssertEquals("Should remain unchanged", false, testUser2Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain unchanged", ContactRelationshipStatusList.Codes.AccountReactivated, testUser2Reloaded.EUA_ContactRelationshipStatus);
		}

		public void TestSaveShouldDeactivateContactsWhereNoActiveUserAccountRelationshipsExist()
		{
			SetupUsers();
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = Contact.OC_Email;
			otherContact.OC_PER = Contact.OC_PER;
			otherContact.SetHashedPassword("1234");
			Factory.Save();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(otherContact.OrgCode, otherContact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var testUserLicenceType = ((ZTextLabel)repeater.Items[3].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Test), repeaterDataSource[3].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[3].LicenceType, testUserLicenceType);
			var testUserRelationshipCheckBox = (ZCheckBox)repeater.Items[3].FindControl("IsContactRelationshipActive");
			AssertEquals(true, testUserRelationshipCheckBox.Checked);
			testUserRelationshipCheckBox.Checked = false;
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			var newFactory = new BusinessObjectFactory();
			var productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			var testUser1Reloaded = newFactory.Load<EdiCustomerUserAccount>(TestUser.PK);
			var testUser2Reloaded = newFactory.Load<EdiCustomerUserAccount>(TrainingUser.PK);
			AssertEquals("Should remain unchanged", true, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be updated", false, testUser1Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain unchanged", false, testUser2Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain active since it still has an active user account", true, Contact.OC_IsActive);
			var productionUserLicenceType = ((ZTextLabel)repeater.Items[1].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Production), repeaterDataSource[1].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[1].LicenceType, productionUserLicenceType);
			var productionUserRelationshipCheckBox = (ZCheckBox)repeater.Items[1].FindControl("IsContactRelationshipActive");
			AssertEquals(true, productionUserRelationshipCheckBox.Checked);
			productionUserRelationshipCheckBox.Checked = false;
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			AssertEquals("Should be updated", false, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain unchanged", false, testUser1Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain unchanged", false, testUser2Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be deactivated since it has no more active user accounts", false, Contact.OC_IsActive);
		}

		public void TestSaveShouldDeactivateContactsWhereNoActiveUserAccountsExist()
		{
			SetupUsers();
			TestUser.EUA_IsActive = false;
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			otherContact.OC_WebAccessEnabled = true;
			otherContact.OC_Email = Contact.OC_Email;
			otherContact.OC_PER = Contact.OC_PER;
			otherContact.SetHashedPassword("1234");
			Factory.Save();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(otherContact.OrgCode, otherContact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var productionUserLicenceType = ((ZTextLabel)repeater.Items[1].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Production), repeaterDataSource[1].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[1].LicenceType, productionUserLicenceType);
			var productionUserRelationshipCheckBox = (ZCheckBox)repeater.Items[1].FindControl("IsContactRelationshipActive");
			AssertEquals(true, productionUserRelationshipCheckBox.Checked);
			productionUserRelationshipCheckBox.Checked = false;
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			var newFactory = new BusinessObjectFactory();
			var productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			var testUser1Reloaded = newFactory.Load<EdiCustomerUserAccount>(TestUser.PK);
			var testUser2Reloaded = newFactory.Load<EdiCustomerUserAccount>(TrainingUser.PK);
			AssertEquals("Should be updated", false, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Precondition", false, testUser1Reloaded.EUA_IsActive);
			AssertEquals("Should remain unchanged", false, testUser2Reloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be deactivated since it has no more active user accounts", false, Contact.OC_IsActive);
		}

		public void TestSaveShouldPromptUserWhenDeactivatingLoginContact()
		{
			SetupUsers();
			TestUser.EUA_IsActive = false;
			Factory.Save();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var productionUserLicenceType = ((ZTextLabel)repeater.Items[1].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Production), repeaterDataSource[1].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[1].LicenceType, productionUserLicenceType);
			var productionUserRelationshipCheckBox = (ZCheckBox)repeater.Items[1].FindControl("IsContactRelationshipActive");
			AssertEquals(true, productionUserRelationshipCheckBox.Checked);
			productionUserRelationshipCheckBox.Checked = false;
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			AssertEquals("Should show confirmation div", "CiModalOn", page.UserAccountRelationship.ConfirmationDivExposed.CssClass);
			page.UserAccountRelationship.ConfirmationNo_ClickExposed();
			AssertEquals("Should show hide confirmation div", "CiModalOff", page.UserAccountRelationship.ConfirmationDivExposed.CssClass);
			var newFactory = new BusinessObjectFactory();
			var productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			AssertEquals("Should remain unchanged", true, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should remain unchanged", true, Contact.OC_IsActive);
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			AssertEquals("Should show confirmation div", "CiModalOn", page.UserAccountRelationship.ConfirmationDivExposed.CssClass);
			page.UserAccountRelationship.ConfirmationYes_ClickExposed();
			AssertEquals("Should show hide confirmation div", "CiModalOff", page.UserAccountRelationship.ConfirmationDivExposed.CssClass);
			newFactory = new BusinessObjectFactory();
			productionUserReloaded = newFactory.Load<EdiCustomerUserAccount>(ProductionUser.PK);
			AssertEquals("Should be updated", false, productionUserReloaded.EUA_IsContactRelationshipActive);
			AssertEquals("Should be deactivated since it has no more active user accounts", false, Contact.OC_IsActive);
			AssertEquals(page.AppInstance.LoginPage, page.Response.RedirectLocation);
		}

		public void TestSaveShouldNotDeactivateContactIfUserAccountsWereAlreadyDeactivated()
		{
			SetupUsers();
			TestUser.EUA_IsActive = false;
			ProductionUser.EUA_IsContactRelationshipActive = false;
			ProductionUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();
			Contact.OC_IsActive = true;
			Factory.Save();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			AssertEquals("Precondition", true, Contact.OC_IsActive);
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			AssertNotEquals("Should not show confirmation div", "CiModalOn", page.UserAccountRelationship.ConfirmationDivExposed.CssClass);
			AssertEquals("Should remain unchanged", true, Contact.OC_IsActive);
		}

		public void TestHasChanges()
		{
			SetupUsers();
			var page = GetPageForTest();
			var repeaterDataSource = new UserAccountDeactivationManager(Contact.Person).ContactUserAccountWrappers;
			page.UserAccountRelationship.BindRepeater(repeaterDataSource);
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var repeater = page.UserAccountRelationship.ContactUserAccountGroupingRepeaterExposed;
			var testUserLicenceType = ((ZTextLabel)repeater.Items[3].FindControl("LicenceType")).Text;
			AssertEquals("Precondition: 4th element should be the TST user account", new DatabaseTypes().GetDescriptionFromCode(DatabaseTypes.Codes.Test), repeaterDataSource[3].LicenceType);
			AssertEquals("Precondition", repeaterDataSource[3].LicenceType, testUserLicenceType);
			var testUserRelationshipCheckBox = (ZCheckBox)repeater.Items[3].FindControl("IsContactRelationshipActive");
			AssertEquals(true, testUserRelationshipCheckBox.Checked);
			AssertEquals(false, page.UserAccountRelationship.HasChanges);
			testUserRelationshipCheckBox.Checked = false;
			AssertEquals(true, page.UserAccountRelationship.HasChanges);
			page.UserAccountRelationship.SaveChangesButton_ClickExposed();
			AssertEquals(false, page.UserAccountRelationship.HasChanges);
		}

		OrgContact Contact;
		EdiCustomerUserAccount ProductionUser;
		EdiCustomerUserAccount TestUser;
		EdiCustomerUserAccount TrainingUser;
		void SetupUsers()
		{
			var enterpriseCode = "ENT";
			var licence = BillingTestHelper.CreateLicence(Factory, enterpriseCode, "COM", "SRV");
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			var org = licence.Company.Header;
			org.OH_Code = "SCWAAASYD";
			Contact = org.Contacts.AddNew();
			Contact.OC_Email = "sam@test.com";
			Contact.OC_WebAccessEnabled = true;
			Contact.SetHashedPassword("1234");
			ProductionUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ProductionUser.EUA_LD = licence.Database.PK;
			ProductionUser.EUA_OC_WebAccessContact = Contact.PK;
			ProductionUser.EUA_IsContactRelationshipActive = true;
			ProductionUser.EUA_IsActive = true;
			var testDatabase = licenceEnterprise.Databases.AddNew();
			testDatabase.LD_ServerCode = "TD1";
			testDatabase.LD_LicenceType = DatabaseTypes.Codes.Test;
			testDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			var trainingDatabase = licenceEnterprise.Databases.AddNew();
			trainingDatabase.LD_ServerCode = "TD2";
			trainingDatabase.LD_LicenceType = DatabaseTypes.Codes.Training;
			trainingDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			TestUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			TestUser.EUA_LD = testDatabase.PK;
			TestUser.EUA_UserID = "US1";
			TestUser.EUA_Email = Contact.OC_Email;
			TestUser.EUA_OC_WebAccessContact = Contact.PK;
			TestUser.EUA_IsContactRelationshipActive = true;
			TrainingUser = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			TrainingUser.EUA_LD = trainingDatabase.PK;
			TrainingUser.EUA_UserID = "US2";
			TrainingUser.EUA_Email = Contact.OC_Email;
			TrainingUser.EUA_OC_WebAccessContact = Contact.PK;
			TrainingUser.EUA_IsContactRelationshipActive = false;
			TrainingUser.EUA_ContactRelationshipStatus = ContactRelationshipStatusList.Codes.AccountReactivated;
			Factory.Save();
		}

		PageForTest GetPageForTest()
		{
			var page = new PageForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.InitialiseControls();
			return page;
		}
	}
}

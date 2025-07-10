using System.Collections.Generic;
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

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class PasswordChangeRequirementsControlTest : TestCaseWithFactory
	{
		public void TestPageLoadScripts_AllValidations()
		{
			SetupUsers();
			var page = GetPageForTest();
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");
			page.DoPageLoad();

			var scripts = page.PasswordChangeRequirements.PasswordRequirementValidationScriptBlock;
			Assert(scripts.Contains("lowerResult"));
			Assert(scripts.Contains("LowerPass"));
			Assert(scripts.Contains("upperResult"));
			Assert(scripts.Contains("UpperPass"));
			Assert(scripts.Contains("numberResult"));
			Assert(scripts.Contains("NumberPass"));
			Assert(scripts.Contains("specialResult"));
			Assert(scripts.Contains("SpecialCharPass"));
			Assert(scripts.Contains("lengthResult"));
			Assert(scripts.Contains("MinLengthPass"));
		}

		public void TestPageLoadScripts_SomeValidations()
		{
			SetupUsers();
			var page = GetPageForTest();
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");

			var newRequirements = new Dictionary<ContactPasswordValidator.PasswordRequirements, int>();
			newRequirements.Add(ContactPasswordValidator.PasswordRequirements.MinLength, 33);
			newRequirements.Add(ContactPasswordValidator.PasswordRequirements.ContainsUpperCaseLetter, 1);
			newRequirements.Add(ContactPasswordValidator.PasswordRequirements.ContainsNumber, 1);
			page.PasswordChangeRequirements.SetPasswordRequirements(newRequirements);

			page.DoPageLoad();

			var scripts = page.PasswordChangeRequirements.PasswordRequirementValidationScriptBlock;
			Assert(!scripts.Contains("lowerResult"));
			Assert(!scripts.Contains("LowerPass"));
			Assert(scripts.Contains("upperResult"));
			Assert(scripts.Contains("UpperPass"));
			Assert(scripts.Contains("numberResult"));
			Assert(scripts.Contains("NumberPass"));
			Assert(!scripts.Contains("specialResult"));
			Assert(!scripts.Contains("SpecialCharPass"));
			Assert(scripts.Contains("lengthResult"));
			Assert(scripts.Contains("MinLengthPass"));
			Assert(scripts.Contains("passwordInputValue.length >= 33"));
		}

		public void TestPageLoadScripts_AtLeastThreeOfTheFollowingValidation()
		{
			SetupUsers();
			var page = GetPageForTest();
			page.SiteUser.LoginForTest(Contact.OrgCode, Contact.OC_Email, "1234");

			var newRequirements = new Dictionary<ContactPasswordValidator.PasswordRequirements, int>();
			newRequirements.Add(ContactPasswordValidator.PasswordRequirements.MinLength, 25);
			newRequirements.Add(ContactPasswordValidator.PasswordRequirements.ContainsAtLeastThreeOfTheFollowing, 1);
			page.PasswordChangeRequirements.SetPasswordRequirements(newRequirements);

			page.DoPageLoad();

			var scripts = page.PasswordChangeRequirements.PasswordRequirementValidationScriptBlock;
			Assert(scripts.Contains("lowerResult"));
			Assert(!scripts.Contains("LowerPass"));
			Assert(scripts.Contains("upperResult"));
			Assert(!scripts.Contains("UpperPass"));
			Assert(scripts.Contains("numberResult"));
			Assert(!scripts.Contains("NumberPass"));
			Assert(scripts.Contains("specialResult"));
			Assert(!scripts.Contains("SpecialCharPass"));
			Assert(scripts.Contains("lengthResult"));
			Assert(scripts.Contains("MinLengthPass"));
			Assert(scripts.Contains("passwordInputValue.length >= 25"));

			Assert(scripts.Contains("AtLeastThreePass"));
			Assert(scripts.Contains("lengthResult && passedValidations >= 3"));
			Assert(scripts.Contains("function hideRequirementFields()"));
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

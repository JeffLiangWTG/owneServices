using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI
{
	[TestedType(typeof(LoginManager))]
	[HttpContextEnabledTest]
	public class LoginManagerTest : NonPersistentBusinessObjectTestCase
	{
		#region set up

		protected override void SetUp()
		{
			base.SetUp();
			TestLoginManager = (LoginManager)GetNewBusinessObject();
		}

		protected LoginManager TestLoginManager;

		#endregion

		public void TestIsCompanyCodeRequired()
		{
			AssertEquals("CompanyCode should be required by default", true, TestLoginManager.IsCompanyCodeRequired);

			TestLoginManager.IsCompanyCodeRequired = false;
			AssertEquals("CompanyCode should be as set", false, TestLoginManager.IsCompanyCodeRequired);

			TestLoginManager.IsCompanyCodeRequired = true;
			AssertEquals("CompanyCode should be as set", true, TestLoginManager.IsCompanyCodeRequired);
		}

		public void TestCompanyCode()
		{
			TestLoginManager.CompanyCode = ZString.Empty;
			AssertEquals("CompanyCode should be required by default", true, TestLoginManager.IsCompanyCodeRequired);
			AssertEquals("Company code should have an error", 1, TestLoginManager.CompanyCodeInfo.GetErrors().Count());
			AssertEquals("Company code should have an error", "Company code is required", TestLoginManager.CompanyCodeInfo.GetErrors().GetFirstMessage());

			TestLoginManager.CompanyCode = "Abc";
			AssertEquals("Company code should NOT have errors", 0, TestLoginManager.CompanyCodeInfo.GetErrors().Count());

			AssertEquals(TestLoginManager.CompanyCodeInfo.MaxLength, OrgHeaderSchema.OH_Code.MaxLength);

			TestLoginManager.IsCompanyCodeRequired = false;
			TestLoginManager.CompanyCode = ZString.Empty;
			AssertEquals("Company code should NOT have errors", 0, TestLoginManager.CompanyCodeInfo.GetErrors().Count());
		}

		public void TestUserName()
		{
			TestLoginManager.UserName = ZString.Empty;
			AssertEquals("User name should have an error", 1, TestLoginManager.UserNameInfo.GetErrors().Count());
			AssertEquals("User name should have an error", "User name is required", TestLoginManager.UserNameInfo.GetErrors().GetFirstMessage());

			TestLoginManager.UserName = "Abc";
			AssertEquals("User name should NOT have errors", 0, TestLoginManager.UserNameInfo.GetErrors().Count());

			AssertEquals(TestLoginManager.UserNameInfo.MaxLength, OrgContactSchema.OC_Email.MaxLength);
		}

		public void TestPassword()
		{
			TestLoginManager.Password = ZString.Empty;
			AssertEquals("Password should have an error", 1, TestLoginManager.PasswordInfo.GetErrors().Count());
			AssertEquals("Password should have an error", "Password is required", TestLoginManager.PasswordInfo.GetErrors().GetFirstMessage());

			TestLoginManager.Password = "Abc";
			AssertEquals("Password should NOT have errors", 0, TestLoginManager.PasswordInfo.GetErrors().Count());

			AssertPasswordMaxLength();
		}

		protected virtual void AssertPasswordMaxLength()
		{
			AssertEquals(OrgContact.PasswordMaxLength, TestLoginManager.PasswordInfo.MaxLength);
		}

		public void TestRememberMe()
		{
			TestLoginManager.RememberMe = ZBool.False;
			AssertEquals("RememberMe should have NO errors", 0, TestLoginManager.PasswordInfo.GetErrors().Count());
			AssertEquals(ZBool.False, TestLoginManager.RememberMe);

			TestLoginManager.RememberMe = ZBool.True;
			AssertEquals(ZBool.True, TestLoginManager.RememberMe);
		}

		public void TestRunPreSaveValidation()
		{
			AssertEquals(0, TestLoginManager.CompanyCodeInfo.GetErrors().Count());
			AssertEquals(0, TestLoginManager.UserNameInfo.GetErrors().Count());
			AssertEquals(0, TestLoginManager.PasswordInfo.GetErrors().Count());

			TestLoginManager.RunPreSaveValidation();

			AssertEquals(1, TestLoginManager.CompanyCodeInfo.GetErrors().Count());
			AssertEquals(1, TestLoginManager.UserNameInfo.GetErrors().Count());
			AssertEquals(1, TestLoginManager.PasswordInfo.GetErrors().Count());
		}

		public void TestCompanyCodeInfo()
		{
			AssertEquals(OrgHeaderSchema.OH_Code.MaxLength, TestLoginManager.CompanyCodeInfo.MaxLength);
			AssertEquals("By default should not be read-only", false, TestLoginManager.CompanyCodeInfo.ReadOnly);

			TestLoginManager.AllowCompanyCodeAndUsernameToBeChanged = false;
			AssertEquals("Should be read-only", true, TestLoginManager.CompanyCodeInfo.ReadOnly);
		}

		public void TestUserNameInfo()
		{
			AssertEquals(OrgContactSchema.OC_Email.MaxLength, TestLoginManager.UserNameInfo.MaxLength);
			AssertEquals("By default should not be read-only", false, TestLoginManager.UserNameInfo.ReadOnly);

			TestLoginManager.AllowCompanyCodeAndUsernameToBeChanged = false;
			AssertEquals("Should be read-only", true, TestLoginManager.UserNameInfo.ReadOnly);
		}

		public void TestAllowCompanyCodeAndUsernameToBeChanged()
		{
			AssertEquals("By default should be true", true, TestLoginManager.AllowCompanyCodeAndUsernameToBeChanged);

			TestLoginManager.AllowCompanyCodeAndUsernameToBeChanged = false;
			AssertEquals(false, TestLoginManager.AllowCompanyCodeAndUsernameToBeChanged);
		}

		public void TestSiteUser()
		{
			AssertNotNull("Precondition AppInstance should be not null", WebEnv.AppInstance);
			AssertNotNull("Precondition AppInstance.SiteUser should be not null", WebEnv.AppInstance.SiteUser);
			AssertNotNull("SiteUser", TestLoginManager.SiteUser);
			AssertEquals("SiteUser", WebEnv.AppInstance.SiteUser, TestLoginManager.SiteUser);
		}

		public void TestMaxLengthTruncates()
		{
			var expectedCompanyCode = new string('A', TestLoginManager.CompanyCodeInfo.MaxLength);
			var expectedUserName = new string('B', TestLoginManager.UserNameInfo.MaxLength);
			var expectedPassword = new string('C', TestLoginManager.PasswordInfo.MaxLength);

			TestLoginManager.CompanyCode = expectedCompanyCode + "A";
			TestLoginManager.UserName = expectedUserName + "B";
			TestLoginManager.Password = expectedPassword + "C";

			CombineAssertions(() =>
			{
				AssertEquals(expectedCompanyCode, TestLoginManager.CompanyCode);
				AssertEquals(expectedUserName, TestLoginManager.UserName);
				AssertEquals(expectedPassword, TestLoginManager.Password);
			});
		}
	}
}

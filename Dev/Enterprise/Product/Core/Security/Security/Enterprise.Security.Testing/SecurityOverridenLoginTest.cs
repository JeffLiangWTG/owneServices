using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	[TestedType(typeof(SecurityOverridenLogin))]
	sealed class SecurityOverridenLoginTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateSupportToken()
		{
			var securityLogin = new SecurityOverridenLogin(Factory)
			{
				Login = EnvProxy.Instance.CurrentUser.LoginName,
				Password = User.MasterPassword
			};
			AssertNotNull("UserSecurity should be not be null", securityLogin.UserSecurity);
		}

		public void TestValidateLogin()
		{
			SecurityOverridenLogin login = new SecurityOverridenLogin(Factory);
			login.Login = "";
			login.ValidateLogin();
			AssertHasErrors(login.LoginInfo);

			login.Login = "Bob";
			AssertNoErrors(login.LoginInfo);
		}

		public void TestValidatePassword()
		{
			SecurityOverridenLogin login = new SecurityOverridenLogin(Factory);
			login.Password = "";
			login.ValidatePassword();
			AssertHasErrors(login.PasswordInfo);

			login.Password = "BLAH";
			AssertNoErrors(login.LoginInfo);
		}

		public void TestLogin_ExpiredUserPassword()
		{
			var entityProvider = new Mock<IADEntityProvider>() { CallBase = true };
			var adUser = new Mock<IADUser>() { CallBase = true };
			ObjectFactory.Substitute(entityProvider.Object);

			var staff = CreateStaff("TestUser", "password", "TST");
			entityProvider.Setup(m => m.GetADUser(staff)).Returns(adUser.Object);
			adUser.Setup(m => m.PasswordExpired).Returns(true);
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				Assert(staff.ADPasswordExpired);

				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				Assert(mustChange);
			}

			var securityLogin = new SecurityOverridenLogin(Factory)
			{
				Login = "TestUser",
				Password = "password"
			};
			AssertNull("UserSecurity should be null", securityLogin.UserSecurity);
			entityProvider.VerifyAll();
			adUser.VerifyAll();
		}

		GlbStaff CreateStaff(string loginName, string password, string code)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = password;
			staff.GS_CanLogin = true;
			staff.GS_IsActive = true;
			staff.GS_IsOperational = true;
			staff.GS_ActiveDirectoryObjectGuid = new ZGuid(staff.PK);

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();
			return staff;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SecurityOverridenLogin();
		}

		#endregion
	}
}

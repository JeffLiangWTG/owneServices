using System;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Security.Testing
{
	sealed class AlternativeCredentialsTest : TestCaseWithFactory
	{
		public void TestValidateSecurityOverrideToken_UserNotFound()
		{
			ValidateSecurityOverrideToken(() =>
			{
				var securityLogin = new AlternativeCredentials("testuser", "token1");

				CombineAssertions(() =>
				{
					AssertNull("UserSecurity should be null", securityLogin.UserSecurity);
					AssertEquals("Should not be validated.", false, securityLogin.LoginAuthentication.LoginValidated);
					AssertEquals(LoginAuthenticationInfo.Status.UserNotFound, securityLogin.LoginAuthentication.State);
					AssertEquals("Invalid user login name.", securityLogin.LoginAuthentication.FailureMessage);
				});
			});
		}

		public void TestValidateSecurityOverrideToken_UserDoesNotMatchToken()
		{
			ValidateSecurityOverrideToken(() =>
			{
				CreateStaff("testuser", "password", "TST");

				var securityLogin = new AlternativeCredentials("testuser", "token1");

				CombineAssertions(() =>
				{
					AssertNull("UserSecurity should be null", securityLogin.UserSecurity);
					AssertEquals("Should not be validated.", false, securityLogin.LoginAuthentication.LoginValidated);
					AssertEquals(LoginAuthenticationInfo.Status.Failure, securityLogin.LoginAuthentication.State);
					AssertEquals("Invalid Security Override Token", securityLogin.LoginAuthentication.FailureMessage);
				});

				var stmAccessToken = Factory.LoadTop1<StmAccessToken>(new CargoWise.EntityFramework.ZQuery());
				AssertEquals("token should not be used.", 1, stmAccessToken.SAT_RemainingUseCount);
			});
		}

		public void TestValidateSecurityOverrideToken_ValidToken()
		{
			ValidateSecurityOverrideToken(() =>
			{
				var securityLogin = new AlternativeCredentials(GlbStaff.CurrentUser.GS_LoginName, "token1");

				CombineAssertions(() =>
				{
					AssertNotNull("UserSecurity should have value", securityLogin.UserSecurity);
					AssertEquals("Should be validated.", true, securityLogin.LoginAuthentication.LoginValidated);
				});
			});
		}

		void ValidateSecurityOverrideToken(Action action)
		{
			var mockIOIDCConfig = new Mock<IOIDCConfig>();
			mockIOIDCConfig.Setup(m => m.IsOIDCEnabled).Returns(true);
			using (ObjectFactory.Substitute(mockIOIDCConfig.Object))
			using (SystemDataRegistry.Instance.EnableSecurityOverrideToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var stmAccessToken = Factory.New<StmAccessToken>();
				stmAccessToken.SAT_Type = AccessTokenTypes.SecurityOverrideToken;
				stmAccessToken.SAT_ParentTableCode = GlbStaffSchema.Constants.Prefix;
				stmAccessToken.SAT_ParentId = GlbStaff.CurrentUser.PK;
				stmAccessToken.SAT_ExpiresAt = ZDateTime.Now.AddMinutes(10);
				stmAccessToken.SAT_RemainingUseCount = 1;
				stmAccessToken.SAT_Token = "token1";
				Factory.Save();

				action();
			}
		}

		public void TestValidLogin()
		{
			SecurityTestObject.CreateTestUser(true, "", "tst", "testuser", "password");

			AlternativeCredentials securityLogin = new AlternativeCredentials("testuser", "password");
			AssertNotNull("UserSecurity should not be null", securityLogin.UserSecurity);
		}

		public void TestInvalidLogin()
		{
			AlternativeCredentials securityLogin = new AlternativeCredentials("invaliduserid", "");
			AssertNull("UserSecurity should be null", securityLogin.UserSecurity);
		}

		public void TestLogin_ExpiredUserPassword()
		{
			var staff = CreateStaff("TestUser", "password", "TST");

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				Assert(mustChange);
			}

			var securityLogin = new AlternativeCredentials("TestUser", "password");
			AssertNull("UserSecurity should be null", securityLogin.UserSecurity);
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
			staff.GS_LastPasswordChangeDate = DateTime.Now.AddYears(-1);

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();
			return staff;
		}
	}
}

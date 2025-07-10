using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProductRegistration.Client;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.Security.Testing
{
	sealed class UserLoginControllerTest : TestCaseWithFactory
	{
		public void TestGetSecurityForUserDonnotAlwaysLoadFromDatabase()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var loginSecurity1 = Factory.New<GlbSecurity>();
			loginSecurity1.GU_GB = GlbBranch.CurrentBranch.PK;
			loginSecurity1.GU_GE = GlbDepartment.CurrentDepartment.PK;
			loginSecurity1.GU_GS = staff1.PK;
			loginSecurity1.GU_SecurityRight = Env.Security.Login.Code;
			loginSecurity1.GU_SecurityItemIsAllowed = true;

			var loginSecurity2 = Factory.New<GlbSecurity>();
			loginSecurity2.GU_GB = branch.PK;
			loginSecurity2.GU_GE = department.PK;
			loginSecurity2.GU_GS = staff1.PK;
			loginSecurity2.GU_SecurityRight = Env.Security.Login.Code;
			loginSecurity2.GU_SecurityItemIsAllowed = false;

			var loginSecurity3 = Factory.New<GlbSecurity>();
			loginSecurity3.GU_GB = branch.PK;
			loginSecurity3.GU_GE = department.PK;
			loginSecurity3.GU_GS = staff2.PK;
			loginSecurity3.GU_SecurityRight = Env.Security.Login.Code;
			loginSecurity3.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var controller = new UserLoginController();
				var security1 = controller.GetSecurityForUser(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				Assert(security1.Login.IsAllowed);

				var security2 = controller.GetSecurityForUser(staff1.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				Assert(!security2.Login.IsAllowed);

				var security3 = controller.GetSecurityForUser(staff2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
				Assert(security3.Login.IsAllowed);
				var factory1 = (security1.GlbSecurityCollection as BusinessObjectCollection).Factory;
				var factory2 = (security2.GlbSecurityCollection as BusinessObjectCollection).Factory;
				var factory3 = (security3.GlbSecurityCollection as BusinessObjectCollection).Factory;
				Assert(factory1._Instance == factory2._Instance);
				Assert(factory1._Instance != factory3._Instance);
			}
		}

		public void TestValidateCountrySpecificDatabases()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(user);
			var controller = new UserLoginController();

			var countriesWithRefDbs = new[]
				{
					Enterprise.Core.Constants.CountryCodes.Australia,
					Enterprise.Core.Constants.CountryCodes.Canada,
					Enterprise.Core.Constants.CountryCodes.NewZealand,
					Enterprise.Core.Constants.CountryCodes.UnitedStates,
				};

			foreach (var country in countriesWithRefDbs)
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(country))
				{
					var userContext = new UserContext(user, GlbCompany.CurrentCompany.FirstActiveBranch.PK.ToGuid(), Guid.Empty, loginAuthenticationInfo: authenticatedUser);
					AssertNoExceptionThrown("Country " + country + " should be OK but it failed - maybe your RefDB or its synonyms don't exist.", () => controller.ValidateCountrySpecificDatabases(userContext));
				}
			}

			var countriesWithoutRefDbs = new[]
				{
					Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
					Enterprise.Core.Constants.CountryCodes.Singapore
				};

			foreach (var country in countriesWithoutRefDbs)
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(country))
				{
					var userContext = new UserContext(user, GlbCompany.CurrentCompany.FirstActiveBranch.PK.ToGuid(), Guid.Empty, loginAuthenticationInfo: authenticatedUser);
					AssertNoExceptionThrown("Country " + country + " should be ignored but it failed.", () => controller.ValidateCountrySpecificDatabases(userContext));
				}
			}

			AssertNoExceptionThrown("Handles no country.", () => controller.ValidateCountrySpecificDatabases(new UserContext(user, Guid.Empty, Guid.Empty, loginAuthenticationInfo: authenticatedUser)));
		}

		public void TestValidateCountrySpecificDatabases_ElevatedUser()
		{
			var sql = @"
DECLARE @sql NVARCHAR(MAX) = (SELECT 'DROP SYNONYM [' + name + '];' FROM sys.synonyms
WHERE name LIKE 'RefDbTrfAU_%' FOR XML PATH (''))
EXEC (@sql);
";
			Db.Connection.ExecuteNonQuery(sql);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(user);
			var controller = new UserLoginController();

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Enterprise.Core.Constants.CountryCodes.Australia))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				var userContext = new UserContext(user, GlbCompany.CurrentCompany.FirstActiveBranch.PK.ToGuid(), Guid.Empty, loginAuthenticationInfo: authenticatedUser);
				var ex = AssertExceptionThrown<DatabaseMissingException>("Standard User throws Exception. Cannot log in.", () => controller.ValidateCountrySpecificDatabases(userContext));
				AssertEquals("Country/Region [AU] requires the [Tariff] reference database to be installed.\r\nTo correct this issue run Help > Database Administration > Recreate Database Synonyms.", ex.Message);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				user.GS_IsController = true;
				AssertNoExceptionThrown("'Controller' User does not throw an Exception. Can log in.", () => controller.ValidateCountrySpecificDatabases(userContext));
				AssertEquals("Country/Region [AU] requires the [Tariff] reference database to be installed.\r\nTo correct this issue run Help > Database Administration > Recreate Database Synonyms.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestSemaphoreProvider]
		public void TestLoginWithSupportToken()
		{
			CreateStaff("testuser", "42034793", "tst", true, false);

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			bool result = Controller.Login("testuser", CWSupportLoginToken.TokenForTest, CurrentBranch, CurrentDepartment);
			AssertEquals("we don't support login with support token any more unless the user is CWSupport.", false, result);

			result = Controller.Login("testuser", "42034793", CurrentBranch, CurrentDepartment);
			Assert("login with password", result);

			result = Controller.Login("CWSupport", CWSupportLoginToken.TokenForTest, CurrentBranch, CurrentDepartment);
			Assert("login CWSupport with support token", result);
		}

		[TestSemaphoreProvider]
		public void TestLoginWithSupportTokenHasLoginToken()
		{
			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			bool result = Controller.Login("CWSupport", CWSupportLoginToken.TokenWithNameAndRolesForTest, CurrentBranch, CurrentDepartment);
			Assert("login CWSupport with support token", result);
			AssertEquals(true, Env.CurrentUser.LoginToken.LoggedInWithSupportToken);
			AssertEquals("ABC", Env.CurrentUser.LoginToken.SupportTokenUserCode);
			AssertEquals("UserName", Env.CurrentUser.LoginToken.SupportTokenUserName);
		}

		public void TestGenerateTemporaryPassword()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			Factory.Save();
			staff.LocalPasswordMustBeReset = true;
			staff.GS_EmailAddress = string.Empty;
			Factory.Save();

			AssertNull("Precondition", Controller.TemporaryPasswordCache);

			//Invalid email
			AssertEquals("Invalid email message", "Your Staff record does not have a valid email address or your email address is used on multiple Staff records. Please contact your system admin to reset your password.", Controller.GenerateAndSendTemporaryPassword(staff.GS_LoginName));
			AssertNull("TemporarayPasswordCache", Controller.TemporaryPasswordCache);

			//Valid email
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();
			AssertEquals("Success message", "A temporary password has been sent to your email address.", Controller.GenerateAndSendTemporaryPassword(staff.GS_LoginName));
			AssertNotNull("TemporarayPasswordCache", Controller.TemporaryPasswordCache);
			AssertEquals(staff.GS_LoginName, Controller.TemporaryPasswordCache.User.LoginName);

			//Email use by multiple users
			var staff2 = CreateStaff("testuser2", "testpassword", "ts2", true, false);
			staff2.GS_EmailAddress = staff.GS_EmailAddress;
			Factory.Save();
			AssertEquals("Invalid email message", "Your Staff record does not have a valid email address or your email address is used on multiple Staff records. Please contact your system admin to reset your password.", Controller.GenerateAndSendTemporaryPassword(staff.GS_LoginName));
			AssertNull("TemporarayPasswordCache", Controller.TemporaryPasswordCache);

			//user not exist
			AssertEquals("user not exist", string.Empty, Controller.GenerateAndSendTemporaryPassword("bogan"));
			AssertNull("TemporarayPasswordCache", Controller.TemporaryPasswordCache);
		}

		public void TestValidateUserLoginAndPassword_WithTempPassword()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			Factory.Save();
			staff.LocalPasswordMustBeReset = true;
			staff.GS_EmailAddress = "bill.gates@microsoft.com";
			Factory.Save();

			Assert("LocalPasswordMustBeReset", staff.LocalPasswordMustBeReset);
			AssertNull("TemporaryPasswordCache is null", Controller.TemporaryPasswordCache);

			//Before generating temp password
			var loginResult = Controller.ValidateUserLoginAndPassword("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.TempPasswordRequired, loginResult.State);

			//Temp password generated, wrong password
			Controller.GenerateAndSendTemporaryPassword("testuser");
			loginResult = Controller.ValidateUserLoginAndPassword("testuser", "wrong");
			AssertEquals("Wrong temp password", LoginAuthenticationInfo.Status.TempPasswordRequired, loginResult.State);
			AssertEquals("Wrong temp password message", "The password is invalid or has expired. Verify that you have entered the password correctly or request a new temporary password.", loginResult.FailureMessage);

			//Temp password generated, correct password
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var tempPasswordFromEmail = email.Body.Split(System.Environment.NewLine.ToCharArray())[0];
			loginResult = Controller.ValidateUserLoginAndPassword("testuser", tempPasswordFromEmail);
			AssertEquals("Correct temp password", LoginAuthenticationInfo.Status.TempPasswordLoginSuccessfully, loginResult.State);
		}

		public void TestValidateUserLoginAndPassword_WithPasswordChangedFromDifferentInstance()
		{
			var oldPassword = "oldPa$$word";
			var newPassword = "newP@ssword";
			var staff = CreateStaff("testuser", oldPassword, "tst", true, false);
			Factory.Save();

			using (RowFactory.SetCachedTables(GlbStaff.Schema.TableName))
			{
				//Before password change
				var loginResult = Controller.ValidateUserLoginAndPassword("testuser", oldPassword);
				AssertEquals("Old password", LoginAuthenticationInfo.Status.OK, loginResult.State);

				//password changed from another instance or outside CW1
				var iterationCount = 10;
				var (_, newPasswordHash, newSalt, _) = UserSecretsContext.DefaultContext.DeriveRawComponents(newPassword, UserSecretHashAlgorithm.Pbkdf2HmacSha1, iterationCount);
				var query = $@"UPDATE dbo.GlbStaff SET GS_PasswordHash = @passwordHash, GS_PasswordSalt = @passwordSalt, GS_PasswordHashIterations = @iterationCount, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_PK = '{staff.PK}'";
				using (var cmd = TestConnection.Command(query))
				{
					cmd.AddParameter("@passwordHash", System.Data.SqlDbType.VarBinary, newPasswordHash);
					cmd.AddParameter("@passwordSalt", System.Data.SqlDbType.VarBinary, newSalt);
					cmd.AddParameter("@iterationCount", System.Data.SqlDbType.Int, iterationCount);
					cmd.ExecuteNonQuery();
				}

				loginResult = Controller.ValidateUserLoginAndPassword("testuser", newPassword);
				AssertEquals("New password", LoginAuthenticationInfo.Status.OK, loginResult.State);

				loginResult = Controller.ValidateUserLoginAndPassword("testuser", oldPassword);
				AssertEquals("Old password no longer valid", LoginAuthenticationInfo.Status.PasswordInvalid, loginResult.State);
			}
		}

		public void TestValidateUserLoginAndPassword_WithSupport2FA()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.StaffPlainTextPassword = "password";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.IsTwoFactorAuthenticationEnabled = true;
			staff.GS_EmailAddress = "someone@somewhere.com";
			Factory.Save();

			var loginResult = Controller.ValidateUserLoginAndPassword("testuser", "password", support2FA: true);
			AssertEquals("Login is validated (right password)", true, loginResult.LoginValidated);
			AssertEquals("Status is not OK as support2FA is set", false, loginResult.IsOK);
			AssertEquals("Expecting 2FA when validate with support2FA", LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired, loginResult.State);
		}

		public void TestValidateUserLoginAndPassword_WithoutSupport2FA()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.StaffPlainTextPassword = "password";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.IsTwoFactorAuthenticationEnabled = true;
			staff.GS_EmailAddress = "someone@somewhere.com";
			Factory.Save();

			var loginResult = Controller.ValidateUserLoginAndPassword("testuser", "password");
			AssertEquals("Login is validated (right password)", true, loginResult.LoginValidated);
			AssertEquals("Status is OK as support2FA is not set", true, loginResult.IsOK);
			AssertNotEquals("Not expecting 2FA when validate without support2FA", LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired, loginResult.State);
		}

		public void TestValidateUserLoginAndPassword_With2FACode_ShouldDisregardSupport2FA()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "testuser";
			staff.StaffPlainTextPassword = "password";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.IsTwoFactorAuthenticationEnabled = true;
			staff.GS_EmailAddress = "someone@somewhere.com";
			Factory.Save();

			Controller.TwoFactorAuthenticationCodeTable[staff.PK.ToGuid()] = "bla-bla-bla";
			var loginResult = Controller.ValidateUserLoginAndPassword("testuser", "password", twoFactorAuthenticationCode: "blur-blur-blur", support2FA: false);
			AssertEquals("Login is validated (right password)", true, loginResult.LoginValidated);
			AssertEquals("Status is not OK with wrong 2FA code even support2FA is not set", false, loginResult.IsOK);
			AssertEquals("Should validate with 2FA code when it is supplied and should disregard support2FA", LoginAuthenticationInfo.Status.TwoFactorAuthenticationFailed, loginResult.State);
		}

		[TestSemaphoreProvider]
		public void TestLogin()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", false, false);
			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			bool result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("User Inactive. Login should fail.", !result);

			staff.GS_IsActive = true;
			Factory.Save();

			var initialUserContext = Env.CurrentUserContext;

			result = Controller.Login("testuser", "badpassword", CurrentBranch, CurrentDepartment);
			AssertEquals("Login should fail - Bad password", false, result);

			result = Controller.Login("testuser", null, CurrentBranch, CurrentDepartment);
			AssertEquals("Login should fail - Bad password", false, result);

			result = Controller.Login("baduser", "testpassword", CurrentBranch, CurrentDepartment);
			AssertEquals("Login should fail - Bad user", false, result);

			result = Controller.Login(null, "testpassword", CurrentBranch, CurrentDepartment);
			AssertEquals("Login should fail - Bad user", false, result);

			result = Controller.Login("testuser", staff.GS_PasswordHash.ToString(), CurrentBranch, CurrentDepartment);
			AssertEquals("Login should fail- Using encrypted password", false, result);

			AssertSame(initialUserContext, Env.CurrentUserContext);
			staff.Reload();
			AssertEquals("no login yet", ZDateTime.Empty, staff.GS_LastActivityDate);

			result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			AssertEquals("Login should succeed", true, result);
			AssertEquals("UserLogin", "testuser", Env.CurrentUser.LoginName);
			AssertEquals("CurrentDepartment", CurrentDepartment, CurrentDepartment);
			AssertEquals("CurrentBranch", CurrentBranch, CurrentBranch);
			staff.Reload();
			Assert(string.Format(CultureInfo.InvariantCulture, "Last Login Date filled out ({0} vs {1})", ZDateTime.UtcNow, staff.GS_LastActivityDate), (staff.GS_LastActivityDate - ZDateTime.UtcNow) < TimeSpan.FromMinutes(30));
		}

		LoginAuthenticationInfo UserLoginInfo(bool isSystemAccount)
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			var semaphoreProvider = (SemaphoreProviderWithCurrentUserForTesting)TestSemaphoreProviderAttribute.TestProvider;
			semaphoreProvider.RemoteClientName = "machine1";

			var userInfo = Controller.LoginUser("testuser", "testpassword");
			var loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);

			semaphoreProvider.RemoteClientName = "machine2";
			staff.GS_IsSystemAccount = isSystemAccount;
			Factory.Save();

			userInfo = Controller.LoginUser("testuser", "testpassword");
			loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);

			return loginResult;
		}

		[TestSemaphoreProvider]
		public void TestSkipLoginOnAnotherMachineInSystemAccount()
		{
			var loginResult = UserLoginInfo(true);
			Assert("System account can login on multiple machines", loginResult.IsOK);
		}

		[TestSemaphoreProvider]
		public void TestLoginOnAnotherMachineEnforced()
		{
			var loginResult = UserLoginInfo(false);
			Assert("System account cannot login on multiple machines", !loginResult.IsOK);
		}

		[TestSemaphoreProvider]
		public void TestLogin_PasswordHash()
		{
			var staff = CreateStaff("testuser", "", "tst", true, false);
			var (_, hash, salt, iterations) = UserSecretsContext.DefaultContext.DeriveRawComponents("testpassword", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10);
			staff.GS_PasswordHash = hash;
			staff.GS_PasswordHashIterations = iterations;
			staff.GS_PasswordSalt = salt;
			Factory.Save();
			var result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff);
			Assert("Login should succeed", result);

			staff.GS_PasswordHashIterations = 100;
			Factory.Save();
			result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("Login should fail", !result);
		}

		[TestSemaphoreProvider]
		public void TestLogin_Resource()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, true);

			bool result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("Is resource. Login should fail.", !result);

			staff = CreateStaff("testuser9", "testpassword", "ts9", true, false);
			Factory.Save();

			result = Controller.Login("testuser9", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("Not a resource. Login should pass.", result);
		}

		[TestSemaphoreProvider]
		public void TestLogin_CLUser()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", active: true, resource: false, isOperational: true, isController: false, canLogin: true);

			var result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff);
			Assert("this user Can Login", result);

			staff.GS_CanLogin = false;
			Factory.Save();

			result = Controller.Login("testuser9", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("This user Cannot Login", !result);
		}

		[TestSemaphoreProvider]
		public void TestLogin_IsDeviceOnly()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			staff.GS_IsDevice = true;
			Factory.Save();

			bool result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("Is Device Only. Login should fail.", !result);

			staff.GS_IsDevice = false;
			Factory.Save();

			result = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);
			Assert("Not Device Only. Login should pass.", result);
		}

		[TestSemaphoreProvider]
		public void TestLogin_DeviceUserAndIsDeviceOnly()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			staff.GS_IsDevice = true;
			Factory.Save();

			bool deviceOnlyFromDevice = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff, true);
			Assert("Is Device Only. Login from Device should pass.", deviceOnlyFromDevice);

			bool deviceOnly = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff, false);
			Assert("Is Device Only. Login should not pass.", !deviceOnly);

			staff.GS_IsDevice = false;
			Factory.Save();

			bool notDeviceOnlyFromDevice = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff, true);
			Assert("Not Device Only. Login from Device should pass.", notDeviceOnlyFromDevice);

			bool notDeviceOnly = Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff, false);
			Assert("Not Device Only. Login should pass.", notDeviceOnly);
		}

		public void TestGetAnyBranch()
		{
			GlbCompany activeCompany = Factory.NewWithValidTestData<GlbCompany>();
			activeCompany.GC_IsActive = true;
			GlbCompany inactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			inactiveCompany.GC_IsActive = false;

			GlbBranch branch11 = Factory.NewWithValidTestData<GlbBranch>();
			branch11.GB_GC = inactiveCompany.PK;
			branch11.GB_BranchName = "000";
			branch11.GB_IsActive = true;

			GlbBranch branch12 = Factory.NewWithValidTestData<GlbBranch>();
			branch12.GB_GC = inactiveCompany.PK;
			branch12.GB_BranchName = "001";
			branch12.GB_IsActive = false;

			GlbBranch branch21 = Factory.NewWithValidTestData<GlbBranch>();
			branch21.GB_GC = activeCompany.PK;
			branch21.GB_BranchName = "002";
			branch21.GB_IsActive = true;

			GlbBranch branch22 = Factory.NewWithValidTestData<GlbBranch>();
			branch22.GB_GC = activeCompany.PK;
			branch22.GB_BranchName = "003";
			branch22.GB_IsActive = false;

			Factory.Save();

			AssertEquals(branch21.PK.ToGuid(), Controller.GetAnyBranch(Factory));

			branch12.GB_IsActive = true;
			branch21.GB_IsActive = false;
			branch22.GB_IsActive = true;

			Factory.Save();

			AssertEquals(branch22.PK.ToGuid(), Controller.GetAnyBranch(Factory));
		}

		public void TestLoginDetailsInDatabase()
		{
			EnterpriseActiveLoginSemaphore semaphore = new EnterpriseActiveLoginSemaphore();
			ISemaphoreInfo[] loggedInUsersInfos = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphore);
			bool isCurrentUserLoginDetailsInDb = false;
			foreach (ISemaphoreInfo info in loggedInUsersInfos)
			{
				if (EnvProxy.Instance.CurrentUser.PK == info.OwnerSession.UserPk)
				{
					isCurrentUserLoginDetailsInDb = true;
					break;
				}
			}
			Assert("Current user login details should be in Db", isCurrentUserLoginDetailsInDb);
		}

		public void TestLogLoginTime()
		{
			DateTime loginTime = Env.Time.CurrentLocalDateTime;

			TestUserLoginController testUser = new TestUserLoginController();
			testUser.LogLoginTime(loginTime);

			string sqlText =
				"SELECT count(*) FROM dbo.StmALog" +
				" WHERE SL_Table = '" + GlbStaffSchema.Constants.TableName + "'" +
				" AND SL_Parent = '" + Env.CurrentUser.PK.ToString() + "'" +
				" AND SL_GS_NKUser = '" + Env.CurrentUser.Initials + "'" +
				" AND SL_SE_NKEvent = 'LGI'" +
				" AND SL_Reference = '" + testUser.GetLogInOutReference(true) + "'" +
				" AND SL_EventTime = '" + SqlFormatInfo.ToSqlDateTimeString(loginTime) + "'";
			int rowCount = (int)ExecuteScalar(sqlText);
			AssertEquals("Login event should have been logged", 1, rowCount);
		}

		public void TestLogForcedLogin()
		{
			TestUserLoginController testUser = new TestUserLoginController();
			var loginToken = Env.CurrentUser.LoginToken as LoginToken;
			loginToken.ForcedRemoteLogoff = false;
			AssertNotContains("FORCED", testUser.GetLogInOutReference(true));
			loginToken.ForcedRemoteLogoff = true;
			AssertContains("FORCED", testUser.GetLogInOutReference(true));
		}

		public void TestLogSupportToken()
		{
			var testUser = new TestUserLoginController();
			var loginToken = Env.CurrentUser.LoginToken as LoginToken;
			AssertNotContains("INC000001, BAS", testUser.GetLogInOutReference(true));
			loginToken.SupportTokenIncident = "INC000001";
			loginToken.SupportTokenUserCode = "BAS";
			AssertContains("INC000001, BAS", testUser.GetLogInOutReference(true));
		}

		public void TestLogWithDummyToken()
		{
			var testUser = new TestUserLoginController();
			var loginToken = Env.CurrentUser.LoginToken as LoginToken;
			AssertNotContains("Original User, Original.User@dummy.com", testUser.GetLogInOutReference(true));
			loginToken.DummyTokenOriginalUserName = "Original User";
			loginToken.DummyTokenOriginalUserEmail = "Original.User@dummy.com";
			AssertContains("Original User, Original.User@dummy.com", testUser.GetLogInOutReference(true));
		}

		public void TestLogLogoutTime()
		{
			DateTime logoutTime = Env.Time.CurrentLocalDateTime;

			TestUserLoginController testUser = new TestUserLoginController();
			testUser.LogLogoutTime(logoutTime);

			string sqlText =
				"SELECT count(*) FROM dbo.StmALog" +
				" WHERE SL_Table = '" + GlbStaffSchema.Constants.TableName + "'" +
				" AND SL_Parent = '" + Env.CurrentUser.PK.ToString() + "'" +
				" AND SL_GS_NKUser = '" + Env.CurrentUser.Initials + "'" +
				" AND SL_SE_NKEvent = 'LGO'" +
				" AND SL_Reference = '" + testUser.GetLogInOutReference(false) + "'" +
				" AND SL_EventTime = '" + SqlFormatInfo.ToSqlDateTimeString(logoutTime) + "'";
			int rowCount = (int)ExecuteScalar(sqlText);
			AssertEquals("Logout event should have been logged", 1, rowCount);
		}

		[TestSemaphoreProvider]
		public void TestLastLogonBranchDepartment()
		{
			GlbStaff staff = CreateStaff("testuser1", "password", "ts1", true, false);

			AssertEquals("Staff LastLogon branch should not be set", ZGuid.Empty, staff.GS_GB_LastLogonBranch);
			AssertEquals("Staff LastLogon department should not be set", ZGuid.Empty, staff.GS_GE_LastLogonDepartment);

			UserLoginController controller = new UserLoginController();
			Assert("Login should be successful", controller.Login("testuser1", "password", CurrentBranch, CurrentDepartment, staff));
			AssertEquals("Staff LastLogon branch should be CurrentBranch", CurrentBranch, staff.GS_GB_LastLogonBranch);
			AssertEquals("Staff LastLogon department should be CurrentDepartment", CurrentDepartment, staff.GS_GE_LastLogonDepartment);

			Guid demoBranchGuid = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "DEM");
			Guid cEADepartmentGuid = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "CER");

			staff.GS_GB_LastLogonBranch = demoBranchGuid;
			staff.GS_GE_LastLogonDepartment = cEADepartmentGuid;
			Factory.Save();

			Assert("Login should be successful", controller.Login("testuser1", "password", CurrentBranch, CurrentDepartment, staff));
			AssertEquals("Staff LastLogon branch should be CurrentBranch", CurrentBranch, staff.GS_GB_LastLogonBranch);
			AssertEquals("Staff LastLogon department should be CurrentDepartment", CurrentDepartment, staff.GS_GE_LastLogonDepartment);

			GlbStaff systemAccountStaff = CreateStaff("testuser2", "password", "ts2", true, false);
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "name";
			person.PER_RN_NKCountry = "AU";
			systemAccountStaff.GS_PER = person.PK;
			systemAccountStaff.GS_IsSystemAccount = true;

			Factory.Save();

			AssertEquals("Staff LastLogon branch should not be set", ZGuid.Empty, systemAccountStaff.GS_GB_LastLogonBranch);
			AssertEquals("Staff LastLogon department should not be set", ZGuid.Empty, systemAccountStaff.GS_GE_LastLogonDepartment);

			Assert("Login should be successful", controller.Login("testuser2", "password", CurrentBranch, CurrentDepartment));
			AssertEquals("Staff is system account - LastLogon branch should not be set", ZGuid.Empty, systemAccountStaff.GS_GB_LastLogonBranch);
			AssertEquals("Staff is system account - Home department should not be set", ZGuid.Empty, systemAccountStaff.GS_GE_LastLogonDepartment);
		}

		[TestSemaphoreProvider]
		public void TestLastLogonBranchDepartment_Concurrency()
		{
			GlbStaff staff = CreateStaff("testuser1", "password", "ts1", true, false);
			Guid dept1Pk = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "CER");
			AssertNotEquals(CurrentDepartment, dept1Pk);
			UserLoginController controller = new UserLoginController();
			LoginAuthenticationInfo authenticatedUser = controller.ValidateUserLoginAndPassword("testuser1", "password");
			Assert("PRE: login ok", controller.LoginLocation(authenticatedUser, CurrentBranch, CurrentDepartment, staff));
			AssertEquals("PRE: Staff LastLogon department", CurrentDepartment, staff.GS_GE_LastLogonDepartment);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staffInFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffInFactory2.Reload();
			AssertNotEquals(ZGuid.Empty, staffInFactory2.GS_GE_LastLogonDepartment);
			staffInFactory2.GS_GE_LastLogonDepartment = ZGuid.Empty;
			factory2.Save();

			Assert("can login when staff record in DB is more recent", controller.LoginLocation(authenticatedUser, CurrentBranch, dept1Pk));
		}

		[TestSemaphoreProvider]
		public void TestLogonBranchCompanyDepartmentInactive()
		{
			var staff = CreateStaff("testuser1", "password", "ts1", true, false);

			var activeCompany = staff.Factory.NewWithValidTestData<GlbCompany>();
			activeCompany.GC_Code = "ACP";
			activeCompany.GC_IsActive = true;

			var inactiveCompany = staff.Factory.NewWithValidTestData<GlbCompany>();
			inactiveCompany.GC_Code = "CMP";
			inactiveCompany.GC_IsActive = false;

			var branch1 = staff.Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = inactiveCompany.PK;
			branch1.GB_BranchName = "001";
			branch1.GB_Code = "TST";
			branch1.GB_IsActive = false;

			var branch2 = staff.Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = inactiveCompany.PK;
			branch2.GB_BranchName = "002";
			branch2.GB_Code = "TS2";
			branch2.GB_IsActive = true;

			var branch3 = staff.Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = activeCompany.PK;
			branch3.GB_BranchName = "003";
			branch3.GB_Code = "TS3";
			branch3.GB_IsActive = true;

			var department = staff.Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "ACP";
			department.GE_IsActive = false;

			staff.Factory.Save();

			var controller = new UserLoginController();
			var authenticatedUser = controller.ValidateUserLoginAndPassword("testuser1", "password");

			var loginResult = controller.LoginLocationEx(authenticatedUser, branch1.GB_Code, Env.CurrentDepartment.Code);
			AssertEquals("Branch login fails", false, loginResult.IsOK);
			AssertEquals("Branch error message", "The branch '" + branch1.GB_Code + "' is inactive.", loginResult.FailureMessage);

			loginResult = controller.LoginLocationEx(authenticatedUser, branch2.GB_Code, Env.CurrentDepartment.Code);
			AssertEquals("Company login fails", false, loginResult.IsOK);
			AssertEquals("Company error message", "The company '" + inactiveCompany.GC_Code + "' is inactive.", loginResult.FailureMessage);

			loginResult = controller.LoginLocationEx(authenticatedUser, branch3.GB_Code, department.GE_Code);
			AssertEquals("Department login fails", false, loginResult.IsOK);
			AssertEquals("Department error message", "The department '" + department.GE_Code + "' is inactive.", loginResult.FailureMessage);

			staff.GS_GB_LastLogonBranch = branch1.PK;
			staff.Factory.Save();
			loginResult = controller.LoginLocationEx(authenticatedUser, string.Empty, Env.CurrentDepartment.Code);
			AssertEquals("Login fail", false, loginResult.IsOK);
			AssertEquals("Error message", "The branch '" + branch1.GB_Code + "' is inactive.", loginResult.FailureMessage);
		}

		public void TestValidateAndRegisterUserForUpgrade()
		{
			AssertNull("[PRE-CONDITION] UpgradeLogonStaffCode", Controller.UpgradeLogonStaffCode);
			AssertNull("[PRE-CONDITION] UpgradeLogonName", Controller.UpgradeLogonName);

			bool isController = Controller.ValidateAndRegisterUserForUpgrade(User.SupportUserName, CWSupportLoginToken.TokenForTest, out _);
			Assert("Developer login should be controller", isController);
			AssertEquals("UpgradeLogonName", User.SupportUserName, Controller.UpgradeLogonName);
			AssertEquals("UpgradeLogonStaffCode", "E", Controller.UpgradeLogonStaffCode);

			// non-controller
			var staff = CreateStaff("testuser", "", "tst", true, false, isController: false);
			var (_, hash, salt, iterations) = UserSecretsContext.DefaultContext.DeriveRawComponents("testpassword", UserSecretHashAlgorithm.Pbkdf2HmacSha1, 10);
			staff.GS_PasswordHash = hash;
			staff.GS_PasswordHashIterations = iterations;
			staff.GS_PasswordSalt = salt;
			Factory.Save();

			Assert("Staff login should exist", Controller.ValidateUserLoginAndPassword("testuser", "testpassword").LoginValidated);

			isController = Controller.ValidateAndRegisterUserForUpgrade("testuser", "testpassword", out var errorMessage);
			Assert("Non controller login should fail", !isController);
			AssertEquals("System Controller permission is required.", errorMessage);

			// controller
			staff.GS_IsController = true;
			Factory.Save();

			isController = Controller.ValidateAndRegisterUserForUpgrade("testuser", "testpassword", out errorMessage);
			Assert("Login should succeed", isController);
			AssertEquals("testuser", Controller.UpgradeLogonName);
			AssertEquals(string.Empty, errorMessage);

			staff.GS_PasswordHashIterations = 100;
			Factory.Save();
			isController = Controller.ValidateAndRegisterUserForUpgrade("testuser", "testpassword", out errorMessage);
			Assert("Login failed", !isController);
			AssertEquals(UserLoginValidation.LoginFailMsg, errorMessage);
		}

		public void TestUserNameEscapingForValidateUserLoginAndPassword()
		{
			AssertEquals(false, Controller.ValidateUserLoginAndPassword("Bob O'Shay", "blah").LoginValidated);

			GlbStaff staff = CreateStaff("Bob O'Shay", "blah", "BO", true, false);

			AssertEquals(true, Controller.ValidateUserLoginAndPassword("Bob O'Shay", "blah").LoginValidated);
		}

		[TestSemaphoreProvider]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLogInInfoAreLinkedToStaff()
		{
			var staff = CreateStaff("Sango", "testpassword", "tst", true, false);

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext("TestUser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			staff.GS_IsActive = true;
			Factory.Save();

			AssertEquals("User logged in.", true, Controller.Login("Sango", "testpassword", CurrentBranch, CurrentDepartment));

			var sqlText =
	"SELECT count(*) FROM dbo.StmALog" +
	" WHERE SL_Table = '" + GlbStaffSchema.Constants.TableName + "'" +
	" AND SL_Parent = '" + Env.CurrentUser.PK.ToString() + "'" +
	" AND SL_GS_NKUser = '" + Env.CurrentUser.Initials + "'" +
	" AND SL_SE_NKEvent = 'LGI'";

			var rowCount = (int)ExecuteScalar(sqlText);
			AssertEquals("Login event should have been logged and linked to GlbStaff table", 1, rowCount);
		}

		public void TestValidateUserLoginAndPassword_InactiveUser()
		{
			GlbStaff nonOperationalStaff = CreateStaff("NonOp", "PassNonOp", "NOP", true, false, isOperational: false);
			nonOperationalStaff.GS_IsActive = true;
			Factory.Save();

			GlbStaff staff = CreateStaff("Soon Leave", "blah", "BO", true, false);
			AssertEquals(true, Controller.ValidateUserLoginAndPassword("Soon Leave", "blah").LoginValidated);

			staff.GS_IsActive = false;
			Factory.Save();
			AssertEquals(false, Controller.ValidateUserLoginAndPassword("Soon Leave", "blah").LoginValidated);
		}

		public void TestValidateUserLoginAndPassword_LockedOut_ADUser()
		{
			var adUsermock = new Mock<IADUser>() { CallBase = true };

			adUsermock.Setup(u => u.HasExistingDirectoryEntry()).Returns(true);
			adUsermock.Setup(u => u.LockedOut).Returns(true);
			var staff = CreateStaffWithADEnabled("pooh", "Changeme1234", "xdd", adUserMock: adUsermock.Object);

			var controller = new UserLoginController();
			var loginInfo = controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "Changeme1234");

			AssertEquals("Should not login with locked out user", false, loginInfo.LoginValidated);
			AssertEquals("Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator.", loginInfo.FailureMessage);
			adUsermock.VerifyAll();
		}

		public void TestValidateUserLoginAndPassword_LockedOut_WindowsAuthenticationProvider()
		{
			var staff = CreateStaffWithADEnabled("pooh", "Changeme1234", "xdd");

			var controller = new UserLoginController();

			var mock = new Mock<IWindowsUserAuthenticationProvider>();

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(It.Is<Guid>(p => p == staff.PK), It.IsAny<string>())).Returns(ValidateCredentialsResult.LockedOut);
			var loginInfo = controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "Changeme1234");

			AssertEquals("Should not login with locked out user", false, loginInfo.LoginValidated);
			AssertEquals("Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator.", loginInfo.FailureMessage);
			mock.VerifyAll();
		}

		public void TestValidateUserLoginAndPassword_LockedOut_NonADUser()
		{
			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var staff = CreateStaff("testsheep", "baaa", "BO", true, false);

			// Locked out temporary
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);

			AssertEquals(false, Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah").LoginValidated);
			AssertEquals(false, Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah").LoginValidated);

			var loginInfo = Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			AssertEquals("Login Failed. Your login will be locked out temporarily.", loginInfo.FailureMessage);

			loginInfo = Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			AssertEquals("Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator.", loginInfo.FailureMessage);

			Controller.ClearLockout(staff);
			// Locked out until admin reset
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			AssertEquals(false, Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah").LoginValidated);
			AssertEquals(false, Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah").LoginValidated);

			loginInfo = Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			AssertEquals("Login Failed. Your login will be disabled.\r\nPlease see your System Administrator to have your login re-enabled.", loginInfo.FailureMessage);

			loginInfo = Controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			AssertEquals("Your login is currently locked out due to a previous failed login. Please try again later or see your system administrator.", loginInfo.FailureMessage);

			Controller.ClearLockout(staff);
		}

		public void TestValidateUserLoginAndPassword_LockedOut_NonADRuleShouldNotApplyToADLogin()
		{
			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3); // local lockout rule exists.

			var staff = CreateStaffWithADEnabled("testpigeon", "p000", "SHI");

			var controller = new UserLoginController();

			var mock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(It.Is<Guid>(p => p == staff.PK), It.IsAny<string>())).Returns(ValidateCredentialsResult.Invalid);
			controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");
			var loginInfo = controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "blah");

			AssertEquals(false, loginInfo.LoginValidated);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", loginInfo.FailureMessage);
			mock.VerifyAll();
		}

		public void TestValidateUserLoginAndPassword_HandleADException()
		{
			var staff = CreateStaffWithADEnabled("pooh", "bla", "xdd");

			var controller = new UserLoginController();

			var mock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(It.Is<Guid>(p => p == staff.PK), It.IsAny<string>())).Returns(ValidateCredentialsResult.ADError);

			var loginInfo = controller.ValidateUserLoginAndPassword(staff.GS_LoginName, "bla");
			AssertEquals("Should not login with AD Exception", false, loginInfo.LoginValidated);
			AssertEquals("The service is currently unavailable, please try again later.", loginInfo.FailureMessage);
			mock.VerifyAll();
		}

		public void TestValidateUserLoginAndPassword_IsDeviceOnly()
		{
			GlbStaff staff = CreateStaff("TestUser", "TEST", "TST", true, false);
			staff.GS_IsDevice = true;
			Factory.Save();

			AssertEquals("Device Only user login should not pass.", false, Controller.ValidateUserLoginAndPassword("TestUser", "TEST").LoginValidated);
			AssertEquals("Device Only user login from Device should pass.", true, Controller.ValidateUserLoginAndPassword("TestUser", "TEST", true).LoginValidated);

			staff.GS_IsDevice = false;
			Factory.Save();

			AssertEquals("Not Device Only user login should pass.", true, Controller.ValidateUserLoginAndPassword("TestUser", "TEST").LoginValidated);
			AssertEquals("Not Device Only user from Device login should pass.", true, Controller.ValidateUserLoginAndPassword("TestUser", "TEST", true).LoginValidated);
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestPromptPasswordChange()
		{
			GlbStaff staff = CreateStaff("TestUser", "", "TST", true, false);

			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext("TestUser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			try
			{
				Env.Registry.RawRegistry.PasswordChangeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);
				Env.Registry.RawRegistry.PromptPasswordChangeBeforeExpireDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);

				UserLoginController controller = new UserLoginController();

				bool mustChange;
				AssertEquals("No LastPasswordChangeDate should be treated as expired password", ZDialogResult.OK, controller.PromptPasswordChange(out mustChange));
				Assert("Password expired, password change compulsory", mustChange);

				staff.GS_ChangePasswordAtNextLogin = false;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate.AddDays(-30);
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertEquals("Password expired", ZDialogResult.OK, controller.PromptPasswordChange(out mustChange));

				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate.AddDays(-11);
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				controller.PromptPasswordChange(out mustChange);
				Assert("Password Change not compulsory for password expiry warning", !mustChange);

				staff.GS_ChangePasswordAtNextLogin = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertEquals("Change password at next login", ZDialogResult.OK, controller.PromptPasswordChange(out mustChange));

				staff.GS_ChangePasswordAtNextLogin = false;
				staff.GS_PasswordNeverChanges = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertEquals("Password never expires", ZDialogResult.No, controller.PromptPasswordChange(out mustChange));

				staff.GS_ChangePasswordAtNextLogin = true;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertEquals("Password never expires, but is interim. Must be changed once.", ZDialogResult.OK, controller.PromptPasswordChange(out mustChange));

				staff.GS_PasswordNeverChanges = false;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				Env.Registry.RawRegistry.PasswordChangeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				AssertEquals("No Password Expire Days, Change Password at Next logon is Y for user", ZDialogResult.OK, controller.PromptPasswordChange(out mustChange));

				staff.GS_ChangePasswordAtNextLogin = false;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertEquals("Password control options not set", ZDialogResult.No, controller.PromptPasswordChange(out mustChange));
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestPromptPasswordChange_WhenNoUserLoggedIn_NonAD()
		{
			var staff = CreateStaff("TestUser", "", "TST", true, false);
			staff.ChangePasswordAtNextLogin = true;
			Factory.Save();

			AssertEquals(false, staff.IsADLinked);
			AssertEquals(false, staff.IsADIntegrationEnabled);
			Assert(staff.ChangePasswordAtNextLogin);

			ActiveDirectoryRegistry.DisableADPasswordChange = false;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				AssertEquals("You are using an interim or expired password.\r\nYou are required to change your password.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(mustChange);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			ActiveDirectoryRegistry.DisableADPasswordChange = true;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				AssertEquals("You are using an interim or expired password.\r\nYou are required to change your password.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(mustChange);
			}
		}

		public void TestPromptPasswordChange_WhenNoUserLoggedIn_AD()
		{
			var adUserMock = new Mock<IADUser>() { CallBase = true };

			adUserMock.Setup(u => u.PasswordExpired).Returns(true);
			var staff = CreateStaffWithADEnabled("TestUser", "", "TST", true, false, adUserMock: adUserMock.Object);
			Assert(staff.IsADLinked);
			Assert(staff.IsADIntegrationEnabled);
			Assert(staff.ADPasswordExpired);

			ActiveDirectoryRegistry.DisableADPasswordChange = false;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				AssertEquals("You are using an interim or expired password.\r\nYou are required to change your password.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(mustChange);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			ActiveDirectoryRegistry.DisableADPasswordChange = true;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				var controller = new UserLoginController();
				AssertEquals(ZDialogResult.OK, controller.PromptPasswordChange(out bool mustChange, "TestUser"));
				AssertEquals("You are using an interim or expired password.\r\nPlease change your Active Directory account password before trying to login again.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(mustChange);
			}
			adUserMock.VerifyAll();
		}

		public void TestPassWordExpiredWhenUserIsNull()
		{
			var staff = CreateStaff("TestUser", "", "TST", true, false);

			staff.GS_ChangePasswordAtNextLogin = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				bool mustChange;
				var controller = new UserLoginController();
				Env.Registry.PasswordChangeDays = 30;

				AssertNoExceptionThrown(() => { controller.PromptPasswordChange(out mustChange, staff.GS_LoginName); });
			}
		}

		public void TestLoginAttempts()
		{
			GlbStaff nonOperationalStaff = CreateStaffWithValidTestData("NonOp", "PassNonOp", "NOP", true, false, isOperational: false);
			nonOperationalStaff.GS_IsActive = true;
			Factory.Save();

			GlbStaff staff = CreateStaffWithValidTestData("testuser", "password", "tst", true, false);
			staff.RunPreSaveValidation();
			staff.Validation.ValidateAll();
			AssertEquals(false, staff.HasErrors);

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			Controller.LoginUser("testuser", "badpassword");
			Controller.LoginUser("testuser", "anotherwrongpassword");
			AssertEquals("2 failed attempts. Login should not be locked out", ZDateTime.Empty, staff.Person.PER_LoginDisabledUntilUtc);
			AssertEquals("2 failed attempts. Login should not be disabled", true, staff.GS_IsActive);

			Controller.LoginUser("testuser", "lastattempt");
			AssertEquals("Login should be locked out indefinitely", ZDateTime.MaxSmallDateTimeUtc, staff.Person.PER_LoginDisabledUntilUtc);
			AssertEquals("Login should not be disabled", true, staff.GS_IsActive);
			AssertEquals("Login should be saved", false, staff.HasChanges);

			var factory2 = new BusinessObjectFactory();
			var staff2 = factory2.Load<GlbStaff>(staff.PK);
			AssertEquals(1, staff2.Groups.Count);
			AssertEquals(true, staff2.GS_IsActive);
			AssertEquals(ZDateTime.MaxSmallDateTimeUtc, staff2.Person.PER_LoginDisabledUntilUtc);
			staff2.RunPreSaveValidation();
			staff2.Validation.ValidateAll();
			AssertEquals("Setting PER_LoginDisabledUntilUtc that high doesn't cause an error preventing saving later", false, staff2.HasErrors);
		}

		public void TestLockoutLogin()
		{
			var staff = CreateStaff("testuser", "password", "tst", true, false);
			staff.GS_PER = Factory.NewWithValidTestData<GlbPerson>().PK;

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);

			UserLoginController controller = new UserLoginController();
			controller.LoginUser("testuser", "wrongpassword");
			controller.LoginUser("testuser", "wrongpassword");
			controller.LoginUser("testuser", "LastAttempt");

			AssertNotNull("Login should be locked out", staff.Person.PER_LoginDisabledUntilUtc);
		}

		public void TestGetSecurityForUser()
		{
			SecurityCore securityObject = Controller.GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertEquals("Login IsAllowed", Env.Security.Login.IsAllowed, securityObject.Login.IsAllowed);

			SecurityCore aSecurity = Controller.GetSecurityForUser(User.PostMasterUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertEquals("UserPK", new Guid("82592349-F1FB-4159-8E00-91AC33D8FAAA"), aSecurity.UserPK);
		}

		public void TestGetSecurityForUser_InvalidUserLogin()
		{
			var securityObject = Controller.GetSecurityForUser("LALA", Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertNotNull("Invalid user should not return null", securityObject);

			securityObject = Controller.GetSecurityForUser("LALA", Guid.Empty, Env.CurrentDepartment.PK);
			AssertNotNull("Invalid branch guid should not return null", securityObject);

			securityObject = Controller.GetSecurityForUser("LALA", Env.CurrentBranch.PK, Guid.Empty);
			AssertNotNull("Invalid department guid should not return null", securityObject);

			securityObject = Controller.GetSecurityForUser("LALA", Guid.Empty, Guid.Empty);
			AssertNotNull("Invalid both branch guid and department guid should not return null", securityObject);
		}

		[TestSemaphoreProvider]
		public void TestLicenceChecks()
		{
			GlbStaff nonOperationalUser = CreateStaff("NonOpLoginName", "NonOpPassword", "NOP", true, false, isOperational: false);
			ObjectFactory.Get<IProductRegistration>().KeyForTest.SystemExpiryDateForTest = ZDateTime.UtcNow.AddDays(-90).ToDateTime();
			LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
			var testLicence = new Licences();
			var core = testLicence.Core;
			core.Login(new TestLicensedComponent());
			Assert("Error text has something", core.LastReasonForNotAllowing.Length > 0);

			var controller = new UserLoginController();
			var info = controller.LoginLocationEx(controller.LoginUser(User.SupportUserName, CWSupportLoginToken.TokenForTest), Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertEquals(false, info.IsOK);
			AssertEquals("Error message", core.LastReasonForNotAllowing, info.FailureMessage);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Assert("Non-Operational user CAN login", controller.Login(nonOperationalUser.GS_LoginName, "NonOpPassword", CurrentBranch, CurrentDepartment));
		}

		[TestSemaphoreProvider]
		public void TestCoreLicenceComponentDoesNotLeak()
		{
			GlbStaff staff1 = CreateStaff("peter", "peter", "P", true, false);
			GlbStaff staff2 = CreateStaff("jane", "jane", "J", true, false);

			TestUserLoginController controller = new TestUserLoginController();
			AssertEquals("CoreLicenceComponent.LicensedComponentManager.CheckPointsCount", 0, ((LicensedComponentManager)controller.CoreLicenceComponent.LicensedComponentManager).CheckpointCount);

			AssertEquals("Login()", true, controller.Login("peter", "peter", CurrentBranch, CurrentDepartment));
			AssertEquals("CoreLicenceComponent.LicensedComponentManager.CheckPointsCount", 1, ((LicensedComponentManager)controller.CoreLicenceComponent.LicensedComponentManager).CheckpointCount);

			AssertEquals("Login()", true, controller.Login("jane", "jane", CurrentBranch, CurrentDepartment));
			AssertEquals("CoreLicenceComponent.LicensedComponentManager.CheckPointsCount", 1, ((LicensedComponentManager)controller.CoreLicenceComponent.LicensedComponentManager).CheckpointCount);
		}

		[TestSemaphoreProvider]
		public void TestWebUserLogin()
		{
			bool initialValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;

				bool result = Controller.Login(User.WebUserName, "webuser", CurrentBranch, CurrentDepartment);
				Assert("Webuser can't log in into Windows application", !result);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		[TestSemaphoreProvider]
		public void TestActiveUsersAfterLogin()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			Guid testUserGuid = staff.PK.ToGuid();
			Assert("Precondition: !ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));

			Assert("!Login", !Controller.Login("testuser", "wrongpassword", CurrentBranch, CurrentDepartment));
			Assert("!ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));

			using (Env.SetTemporaryUserContext("testuser", CurrentBranch, CurrentDepartment))
			{
				Assert("!ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));
			}
			Assert("!ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));

			Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment));
			Assert("ActiveUsersContainsTestUser", ActiveUsersContainsTestUser(testUserGuid));
		}

		[TestSemaphoreProvider]
		public void TestLoginAuthenticationInfoIsSetForCurrentUserAfterNotTwoFactorLogin()
		{
			var currentLoginAuthenticationInfo = Env.CurrentUserContext.LoginAuthenticationInfo;
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false, twoFactorEnabled: false);
			var testUserGuid = staff.PK.ToGuid();
			Assert("Precondition: !ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));
			AssertNotEquals("Precondition: test user not logged in", testUserGuid, Env.CurrentUser.PK);
			Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment));

			var result = Env.CurrentUserContext?.LoginAuthenticationInfo;

			AssertNotNull(result);
			AssertNotEquals(currentLoginAuthenticationInfo, result);
			AssertEquals(true, result.IsOK);
		}

		[TestSemaphoreProvider]
		public void TestLoginAuthenticationInfoIsSetForCurrentUserAfterTwoFactorLogin()
		{
			var currentLoginAuthenticationInfo = Env.CurrentUserContext.LoginAuthenticationInfo;
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false, twoFactorEnabled: true);
			var testUserGuid = staff.PK.ToGuid();
			Assert("Precondition: !ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));
			AssertNotEquals("Precondition: test user not logged in", testUserGuid, Env.CurrentUser.PK);
			Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, support2FA: true));

			var result = Env.CurrentUserContext?.LoginAuthenticationInfo;

			AssertNotNull(result);
			AssertNotEquals(currentLoginAuthenticationInfo, result);
			AssertEquals(false, result.IsOK);
		}

		[TestSemaphoreProvider]
		public void TestLoginAuthenticationInfoIsNotSetForCurrentUserOnLoginFail()
		{
			var currentLoginAuthenticationInfo = Env.CurrentUserContext.LoginAuthenticationInfo;
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			var testUserGuid = staff.PK.ToGuid();
			Assert("Precondition: !ActiveUsersContainsTestUser", !ActiveUsersContainsTestUser(testUserGuid));
			AssertNotEquals("Precondition: test user not logged in", testUserGuid, Env.CurrentUser.PK);
			Assert("Login", !Controller.Login("testuser", "wrongPassword", CurrentBranch, CurrentDepartment));

			var result = Env.CurrentUserContext?.LoginAuthenticationInfo;

			AssertEquals(currentLoginAuthenticationInfo, result);
		}

		[TestSemaphoreProvider]
		public void TestLoggingInDoesNotCreateAEdtEvent()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, false);

			var logsCountOld = staff.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.EditedARecordCode).ToList().Count;

			Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment, staff));

			var logCountNew = staff.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.EditedARecordCode).ToList().Count;
			AssertEquals(logsCountOld, logCountNew);
		}

		bool ActiveUsersContainsTestUser(Guid testUserGuid)
		{
			IActiveUserSession[] activeUsers = ActiveUserQuery.GetEnterpriseActiveUserSessions(true);

			foreach (IActiveUserSession user in activeUsers)
			{
				if (user.UserPk == testUserGuid)
				{
					return true;
				}
			}

			return false;
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);

			var semaphoreProvider = (SemaphoreProviderWithCurrentUserForTesting)TestSemaphoreProviderAttribute.TestProvider;
			semaphoreProvider.RemoteClientName = "machine1";
			Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment));

			semaphoreProvider.RemoteClientName = "machine2";
			var userInfo = Controller.LoginUser("testuser", "testpassword");
			var loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);
			Assert("Login fail", !loginResult.IsOK);
			AssertEquals(LoginAuthenticationInfo.Status.OneMachinePerUserLimitExceeded, loginResult.State);

			staff.GS_IsSystemAccount = true;
			Factory.Save();
			userInfo = Controller.LoginUser("testuser", "testpassword");
			loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
			Assert("System account can login on multiple machines", loginResult.IsOK);

			semaphoreProvider.RemoteClientName = "machine3";
			staff.GS_IsSystemAccount = false;
			Factory.Save();
			userInfo = Controller.LoginUser("testuser", "testpassword");
			loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
			Assert("Login fail", !loginResult.IsOK);
			Controller.ForceRemoteLogoff(loginResult.LoginOnAnotherMachine);

			loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
			Assert("Forced login success", loginResult.IsOK);
			Assert("is forced", Env.CurrentUser.LoginToken.ForcedRemoteLogoff);

			AssertContains("FORCED", Controller.GetLogInOutReference(true));
			var sqlText =
				"SELECT count(*) FROM dbo.StmALog" +
				" WHERE SL_Table = '" + GlbStaffSchema.Constants.TableName + "'" +
				" AND SL_Parent = '" + Env.CurrentUser.PK.ToString() + "'" +
				" AND SL_GS_NKUser = '" + Env.CurrentUser.Initials + "'" +
				" AND SL_SE_NKEvent = 'LGI'" +
				" AND SL_Reference = '" + Controller.GetLogInOutReference(true) + "'";
			var rowCount = (int)ExecuteScalar(sqlText);
			AssertEquals("Login event should have been logged", 1, rowCount);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_AllAreLocal()
		{
			AssertOneMachinePerUser_IgnoreTerminalServer(System.Environment.MachineName, System.Environment.MachineName, true);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_FirstIsLocalAndSecondIsRemoteClient()
		{
			AssertOneMachinePerUser_IgnoreTerminalServer(System.Environment.MachineName,
				"AU2CO-SRDH-039/" + System.Environment.MachineName,
				true);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_FirstIsRemoteClientAndSecondIsLocal()
		{
			AssertOneMachinePerUser_IgnoreTerminalServer("AU2CO-SRDH-039/" + System.Environment.MachineName,
				System.Environment.MachineName,
				true);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_AllAreRemoteClient() 
		{
			AssertOneMachinePerUser_IgnoreTerminalServer("AU2CO-SRDH-039/" + System.Environment.MachineName,
				"AU2CO-SRDH-044/" + System.Environment.MachineName,
				true);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_AllAreRemoteClient_DifferentMachines()
		{
			AssertOneMachinePerUser_IgnoreTerminalServer("AU2CO-SRDH-039/" + System.Environment.MachineName,
				$"AU2CO-SRDH-039/{System.Environment.MachineName}-X",
				false);
		}

		void AssertOneMachinePerUser_IgnoreTerminalServer(string machineName, string machineName2, bool loginResultIsOK)
		{
			var semaphoreProvider = (SemaphoreProviderWithCurrentUserForTesting)TestSemaphoreProviderAttribute.TestProvider;
			semaphoreProvider.RemoteClientName = machineName;

			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment);

			semaphoreProvider.RemoteClientName = machineName2;
			var userInfo = Controller.LoginUser("testuser", "testpassword");
			var loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);
			AssertEquals("Users should only be allowed to have multiple CW instances from the same PC.", loginResultIsOK, loginResult.IsOK);
			AssertEquals(loginResultIsOK ? LoginAuthenticationInfo.Status.OK : LoginAuthenticationInfo.Status.OneMachinePerUserLimitExceeded, loginResult.State);
		}

		[TestSemaphoreProvider]
		public void TestOneMachinePerUser_WinformsLoginIgnoresWinzorSessions()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			LoginAuthenticationInfo userInfo, loginResult;

			using (Globals.SetIsWinzorForTest(true))
			{
				userInfo = Controller.LoginUser("testuser", "testpassword");
				loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);
				Assert("Login", loginResult.IsOK);
			}

			var semaphoreProvider = (SemaphoreProviderWithCurrentUserForTesting)TestSemaphoreProviderAttribute.TestProvider;
			semaphoreProvider.RemoteClientName = "machine2";

			using (Globals.SetIsWinzorForTest(false))
			{
				userInfo = Controller.LoginUser("testuser", "testpassword");
				loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff); //now log in as winforms
				Assert("Login", loginResult.IsOK);
			}

			semaphoreProvider.RemoteClientName = "machine3";

			using (Globals.SetIsWinzorForTest(false))
			{
				userInfo = Controller.LoginUser("testuser", "testpassword");
				loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff); //now log in as winforms again
				Assert("Login fail", !loginResult.IsOK);
				AssertEquals(LoginAuthenticationInfo.Status.OneMachinePerUserLimitExceeded, loginResult.State);
			}

			staff.GS_IsSystemAccount = true;
			Factory.Save();
			userInfo = Controller.LoginUser("testuser", "testpassword");
			loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
			Assert("System account can login on multiple machines", loginResult.IsOK);
		}

		[TestSemaphoreProvider]
		public void TestOneClientIdentifierPerUser()
		{
			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);

			using (Globals.SetClientIdentifierForTest("client1"))
			{
				Assert("Login", Controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment));
			}

			using (Globals.SetClientIdentifierForTest("client2"))
			{
				var userInfo = Controller.LoginUser("testuser", "testpassword");
				var loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment, staff);
				Assert("Login fail", !loginResult.IsOK);
				AssertEquals(LoginAuthenticationInfo.Status.OneMachinePerUserLimitExceeded, loginResult.State);

				staff.GS_IsSystemAccount = true;
				Factory.Save();
				userInfo = Controller.LoginUser("testuser", "testpassword");
				loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
				Assert("System account can login on multiple machines", loginResult.IsOK);
			}

			using (Globals.SetClientIdentifierForTest("client3"))
			{
				staff.GS_IsSystemAccount = false;
				Factory.Save();
				var userInfo = Controller.LoginUser("testuser", "testpassword");
				var loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
				Assert("Login fail", !loginResult.IsOK);
				Controller.ForceRemoteLogoff(loginResult.LoginOnAnotherMachine);

				loginResult = Controller.LoginLocationEx(userInfo, CurrentBranch, CurrentDepartment);
				Assert("Forced login success", loginResult.IsOK);
				Assert("is forced", Env.CurrentUser.LoginToken.ForcedRemoteLogoff);
			}

			AssertContains("FORCED", Controller.GetLogInOutReference(true));
			var sqlText =
				"SELECT count(*) FROM dbo.StmALog" +
				" WHERE SL_Table = '" + GlbStaffSchema.Constants.TableName + "'" +
				" AND SL_Parent = '" + Env.CurrentUser.PK.ToString() + "'" +
				" AND SL_GS_NKUser = '" + Env.CurrentUser.Initials + "'" +
				" AND SL_SE_NKEvent = 'LGI'" +
				" AND SL_Reference = '" + Controller.GetLogInOutReference(true) + "'";
			var rowCount = (int)ExecuteScalar(sqlText);
			AssertEquals("Login event should have been logged", 1, rowCount);
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithSingleSignOnAndEmptyPassword_ShouldLoginWithActiveWindowsUserGuid()
		{
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.IsSingleSignOnEnabled = true;
			var staff = CreateStaffWithADEnabled("davey.boy", "Changeme1234", "DE", active: true, resource: false);

			var controller = new UserLoginController();

			var mock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(staff.PK.ToGuid(), string.Empty)).Returns(ValidateCredentialsResult.Invalid);

			mock.Setup(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid())).Returns(ValidateCredentialsResult.OK);

			AssertEquals("Should not login without ADObjectGuid", false, controller.Login("davey.boy", string.Empty, CurrentBranch, CurrentDepartment));
			AssertEquals("Should login with valid ADObjectGuid", true, controller.Login("davey.boy", string.Empty, CurrentBranch, CurrentDepartment, activeDirectoryObjectGuid: staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
			AssertEquals("Should login with valid ADObjectGuid", true, controller.Login(string.Empty, string.Empty, CurrentBranch, CurrentDepartment, activeDirectoryObjectGuid: staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
			AssertEquals("Should not login with invalid ADObjectGuid", false, controller.Login(string.Empty, string.Empty, CurrentBranch, CurrentDepartment, activeDirectoryObjectGuid: Guid.NewGuid()));

			mock.Verify(a => a.ValidateCredentials(staff.PK.ToGuid(), string.Empty), Times.Exactly(1));
			mock.Verify(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()), Times.Exactly(2));
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithSingleSignOnAnd2FA_ShouldTrigger2FA()
		{
			var currentRegistry = SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var wAuthProviderMock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };
			var staff = CreateStaffWithADEnabled("super.admin", "Changeme1234", "HI", active: true, resource: false);
			staff.GS_EmailAddress = "someone@somewhere.com";
			staff.IsTwoFactorAuthenticationEnabled = true;

			try
			{
				SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

				ActiveDirectoryRegistry.IsIntegrationEnabled = true;
				ActiveDirectoryRegistry.IsSingleSignOnEnabled = true;

				Factory.Save();

				var controller = new UserLoginController
				{
					WindowsAuthenticationProvider = wAuthProviderMock.Object
				};
				wAuthProviderMock.Setup(a => a.CurrentWindowsUserGuid).Returns(staff.GS_ActiveDirectoryObjectGuid.ToGuid());

				wAuthProviderMock.Setup(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid())).Returns(ValidateCredentialsResult.OK);
				AssertEquals("Should login with valid ADObjectGuid and DOES trigger 2FA", LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired, controller.LoginUserSingleSignOn(support2FA: true).State);
			}
			finally
			{
				Env.LoginController.Logout();
				SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentRegistry);
				wAuthProviderMock.Verify(a => a.CurrentWindowsUserGuid, Times.Exactly(1));
				wAuthProviderMock.Verify(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()), Times.Exactly(1));
			}
		}

		[TestSemaphoreProvider]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLogoutResetsLoginAuthenticationInfo()
		{
			// Arrange
			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartment.PK, loginAuthenticationInfo: LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser)));

			try
			{
				AssertEquals(true, Env.IsAuthenticated);

				// Act
				Env.LoginController.Logout();

				// Assert
				AssertEquals(false, Env.IsAuthenticated);
				AssertNull(Env.CurrentUserContext.LoginAuthenticationInfo);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		[ExpectNoExceptions]
		[TestSemaphoreProvider]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLogoutClearsGlowUserData()
		{
			// Arrange
			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartment.PK, loginAuthenticationInfo: LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser)));
			var glowServiceClientFactoryMock = new Mock<IGlowServiceClientFactory>(MockBehavior.Strict) { CallBase = true };

			using (new DisposableAction(() => Env.SetUserContext(initialUserContext)))
			using (ObjectFactory.Substitute(glowServiceClientFactoryMock.Object))
			{
				glowServiceClientFactoryMock.Setup(factory => factory.ClearUserData());
				// Act
				Env.LoginController.Logout();

				// Assert
				glowServiceClientFactoryMock.VerifyAll();
				glowServiceClientFactoryMock.Verify(factory => factory.ClearUserData(), Times.Exactly(1));
			}
		}

		[ExpectNoExceptions]
		[TestSemaphoreProvider]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLoginClearsGlowUserData()
		{
			// Arrange
			var initialUserContext = Env.CurrentUserContext;
			Env.SetUserContext(new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartment.PK, loginAuthenticationInfo: LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser)));
			var glowServiceClientFactoryMock = new Mock<IGlowServiceClientFactory>(MockBehavior.Strict) { CallBase = true };

			using (new DisposableAction(() => Env.SetUserContext(initialUserContext)))
			using (ObjectFactory.Substitute(glowServiceClientFactoryMock.Object))
			{
				try
				{
					glowServiceClientFactoryMock.Setup(factory => factory.ClearUserData());
					// Act
					Env.LoginController.LoginLocation(LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser), Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
					Env.LoginController.LoginLocationEx(LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser), Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
					Env.LoginController.LoginLocationAutomatically(LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser));
					Env.LoginController.LoginLocationExAutomatically(LoginAuthenticationInfo.NewSuccessfulLogin(Env.CurrentUser));

					// Assert
					glowServiceClientFactoryMock.VerifyAll();
					glowServiceClientFactoryMock.Verify(factory => factory.ClearUserData(), Times.Exactly(4));
				}
				finally
				{
					Env.LoginController.Logout();
				}
			}
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithSupportTokenAndSSO_ShouldLogin()
		{
			var masterPassword = CWSupportLoginToken.TokenForTest;
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.IsSingleSignOnEnabled = true;
			var staff = CreateStaffWithADEnabled("purple.monkey", "Changeme1234", "EB", active: true, resource: false);

			var controller = new UserLoginController();
			var wAuthMock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };
			controller.WindowsAuthenticationProvider = wAuthMock.Object;
			wAuthMock.Setup(m => m.ValidateCurrentUserSession(It.IsAny<Guid>()));
			AssertEquals("Should login with master password to a valid user", true, controller.Login("purple.monkey", masterPassword, CurrentBranch, CurrentDepartment));
			AssertEquals("Should not login with master password to an invalid user", false, controller.Login("dumb.monkey", masterPassword, CurrentBranch, CurrentDepartment));
			wAuthMock.Verify(m => m.ValidateCurrentUserSession(It.IsAny<Guid>()), Times.Never);
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithADUserNameAndPassword()
		{
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			var staff = CreateStaffWithADEnabled("evil.beaver", "Changeme1234", "EB", active: true, resource: false);

			var controller = new UserLoginController();
			var repository = new Mock<IWindowsUserAuthenticationProvider>(MockBehavior.Strict);
			controller.WindowsAuthenticationProvider = repository.Object;
			repository.Setup(a => a.ValidateCredentials(staff.PK.ToGuid(), "3v1l.b33v4h")).Returns(ValidateCredentialsResult.OK);
			repository.Setup(a => a.ValidateCredentials(staff.PK.ToGuid(), "Changeme1234")).Returns(ValidateCredentialsResult.Invalid);
			AssertEquals("Should login with AD password", true, controller.Login("evil.beaver", "3v1l.b33v4h", CurrentBranch, CurrentDepartment));
			AssertEquals("Should not login with wrong AD password", false, controller.Login("evil.beaver", "Changeme1234", CurrentBranch, CurrentDepartment));
			AssertEquals("Should not login with invalid user", false, controller.Login("devil.beaver", "3v1l.b33v4h", CurrentBranch, CurrentDepartment));

			repository.Verify(a => a.ValidateCredentials(staff.PK.ToGuid(), "3v1l.b33v4h"), Times.Once);
			repository.Verify(a => a.ValidateCredentials(staff.PK.ToGuid(), "Changeme1234"), Times.Once);
		}

		public void TestDoLoginWithADUserNameAndNullPassword()
		{
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			var staff = CreateStaffWithADEnabled("evil.beaver", "changeme", "EB", active: true, resource: false);

			var controller = new UserLoginController();
			var repository = new Mock<IWindowsUserAuthenticationProvider>(MockBehavior.Strict);
			controller.WindowsAuthenticationProvider = repository.Object;
			repository.Setup(a => a.ValidateCredentials(staff.PK.ToGuid(), null)).Returns(ValidateCredentialsResult.Invalid);

			AssertEquals(false, controller.Login("evil.beaver", null, CurrentBranch, CurrentDepartment));
			repository.Verify(a => a.ValidateCredentials(staff.PK.ToGuid(), null), Times.Once);
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithADObjectGuidAndNullUserNameAndPassword()
		{
			var repository = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			var staff = CreateStaffWithADEnabled("evil.beaver", "changeme", "EB", active: true, resource: false);

			var controller = new UserLoginController();
			controller.WindowsAuthenticationProvider = repository.Object;

			repository.Setup(a => a.ValidateCredentials(staff.PK.ToGuid(), null)).Returns(ValidateCredentialsResult.Invalid);
			repository.Setup(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid())).Returns(ValidateCredentialsResult.OK);

			ActiveDirectoryRegistry.IsSingleSignOnEnabled = false;
			AssertEquals("Should not login with ADGuid if not SSO enabled", false, controller.Login(null, null, CurrentBranch, CurrentDepartment, activeDirectoryObjectGuid: staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
			ActiveDirectoryRegistry.IsSingleSignOnEnabled = true;
			AssertEquals("Should login with valid ADGuid", true, controller.Login(null, null, CurrentBranch, CurrentDepartment, activeDirectoryObjectGuid: staff.GS_ActiveDirectoryObjectGuid.ToGuid()));
			AssertEquals("Should not login without valid ADGuid", false, controller.Login(null, null, CurrentBranch, CurrentDepartment));
			repository.Verify(a => a.ValidateCredentials(staff.PK.ToGuid(), null), Times.Exactly(1));
			repository.Verify(a => a.ValidateCurrentUserSession(staff.GS_ActiveDirectoryObjectGuid.ToGuid()), Times.Once);
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithADForSystemAccount_ShouldNotUseWindowsAuthenticationForLogin_WithValidPassword()
		{
			AssertDoLoginWithADForSystemAccount_ShouldNotUseWindowsAuthenticationForLogin(User.SupportUserName, CWSupportLoginToken.TokenForTest, true);
		}

		public void TestDoLoginWithADForSystemAccount_ShouldNotUseWindowsAuthenticationForLogin_WithInvalidPassword()
		{
			AssertDoLoginWithADForSystemAccount_ShouldNotUseWindowsAuthenticationForLogin(User.SupportUserName, "wrongPassword", false);
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithAD_LockedOut_ADUser()
		{
			var adUser = new Mock<IADUser>() { CallBase = true };

			adUser.Setup(u => u.HasExistingDirectoryEntry()).Returns(true);
			adUser.Setup(u => u.LockedOut).Returns(true);
			var staff = CreateStaffWithADEnabled("pooh", "Changeme1234", "xdd", adUserMock: adUser.Object);

			AssertEquals("Should not login with locked out user", false, new UserLoginController().Login(staff.GS_LoginName, "Changeme1234", CurrentBranch, CurrentDepartment));
			adUser.VerifyAll();
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithAD_LockedOut_WindowsAuthenticationProvider()
		{
			var staff = CreateStaffWithADEnabled("pooh", "Changeme1234", "xdd");

			var controller = new UserLoginController();
			var mock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(It.Is<Guid>(p => p == staff.PK), It.IsAny<string>())).Returns(ValidateCredentialsResult.LockedOut);

			AssertEquals("Should not login with locked out user", false, controller.Login(staff.GS_LoginName, "Changeme1234", CurrentBranch, CurrentDepartment));
			mock.VerifyAll();
		}

		[TestSemaphoreProvider]
		public void TestDoLoginWithAD_ExpiredPassword()
		{
			var staff = CreateStaffWithADEnabled("pooh", "Changeme1234", "xdd");

			var controller = new UserLoginController();
			var mock = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };

			controller.WindowsAuthenticationProvider = mock.Object;
			mock.Setup(a => a.ValidateCredentials(It.Is<Guid>(p => p == staff.PK), It.IsAny<string>())).Returns(ValidateCredentialsResult.PasswordExpired);
			AssertEquals("Should not login with expired password user", false, controller.Login(staff.GS_LoginName, "Changeme1234", CurrentBranch, CurrentDepartment));
			mock.VerifyAll();
		}

		void AssertDoLoginWithADForSystemAccount_ShouldNotUseWindowsAuthenticationForLogin(string user, string password, bool shouldLogin)
		{
			var mocks = new Mock<IWindowsUserAuthenticationProvider>() { CallBase = true };
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;

			var controller = new UserLoginController();
			controller.WindowsAuthenticationProvider = mocks.Object;
			bool isOK = controller.Login(user, password, CurrentBranch, CurrentDepartment);
			AssertEquals("Login", shouldLogin, isOK);
			Mock.Get(controller.WindowsAuthenticationProvider).Verify(p => p.ValidateCredentials(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
		}

		[TestSemaphoreProvider]
		public void TestLogin_Security()
		{
			GlbStaff staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			GlbSecurity loginSecurity = Factory.New<GlbSecurity>();
			loginSecurity.GU_GB = CurrentBranch;
			loginSecurity.GU_GE = CurrentDepartment;
			loginSecurity.GU_GS = staff.PK;
			loginSecurity.GU_SecurityRight = Env.Security.Login.Code;
			loginSecurity.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			TestUserLoginController controller = new TestUserLoginController();
			LicensedComponentManager licenceComponents = ((LicensedComponentManager)controller.CoreLicenceComponent.LicensedComponentManager);

			var info = controller.LoginLocationEx(controller.LoginUser("testuser", "testpassword"), CurrentBranch, CurrentDepartment);
			AssertEquals("Login", false, info.IsOK);
			AssertEquals("CoreLicenceComponent has no login - no licence usage if user has no security", 0, licenceComponents.CheckpointCount);
			AssertEquals("You do not have the security right to login to this branch or department.\r\nYou may ask your system administrator to grant you this security right.\r\n\r\nThis particular security right is called 'Login Branches and Departments', and is found at the bottom of the Security Tree View on the Staff or Group form.", info.FailureMessage);

			loginSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();
			AssertEquals("Login", true, controller.Login("testuser", "testpassword", CurrentBranch, CurrentDepartment));
			AssertEquals("CoreLicenceComponent has login", 1, licenceComponents.CheckpointCount);
		}

		public void TestLoginLocationEx_NullException()
		{
			var controller = new TestUserLoginController();
			CreateStaff("TestUser", "TestPassword", "TUS", true, false);
			Factory.Save();

			var info = controller.LoginLocationEx(controller.LoginUser("TestUser", "TestPassword"), Guid.NewGuid(), Guid.NewGuid());
			Assert(!info.IsOK);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUpdateUserDataHandlesConcurrencyConflicts()
		{
			var staff = CreateStaffWithValidTestData("user1", "password1", "usr", true, false);
			staff.Factory.Save();

			var staffInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbStaff>(staff.PK);

			var controller = new UserLoginController();
			controller.UpdateUserDataExposedForTest(staffInOtherFactory, localPerson =>
			{
				staff.GS_GB_HomeBranch = staff.Factory.NewWithValidTestData<GlbBranch>().PK;
				localPerson.PER_FullName = "friendly test";
				staff.Factory.Save();
				return true;
			});

			AssertNotEquals("friendly test", staff.Person.PER_FullName);
			staff.Person.Reload();
			AssertEquals("friendly test", staff.Person.PER_FullName);
		}

		[TestSemaphoreProvider]
		public void TestLoginToUnregisteredSystem()
		{
			CreateStaff("op.only", "testpassword", "st1", true, false, isOperational: true);

			var operationalSystemStaff = CreateStaff("op.sys", "testpassword", "st2", true, false, isOperational: true);
			operationalSystemStaff.GS_IsSystemAccount = true;

			var operationalControllerStaff = CreateStaff("op.controller", "testpassword", "st3", true, false, isOperational: true);
			operationalControllerStaff.GS_IsController = true;

			CreateStaff("non.op", "testpassword", "st4", true, false, isOperational: false);
			Factory.Save();

			AssertLogin("operational", false, "op.only", "testpassword", ProductRegistrationVerifyResult.Unregistered);
			AssertLogin("operational with support token", false, "op.only", CWSupportLoginToken.TokenForTest, ProductRegistrationVerifyResult.Unregistered, false);

			AssertLogin("operational and system", true, "op.sys", "testpassword", ProductRegistrationVerifyResult.Unregistered);
			AssertLogin("operational and system with support token", false, "op.sys", CWSupportLoginToken.TokenForTest, ProductRegistrationVerifyResult.Unregistered, false);

			AssertLogin("operational and controller", false, "op.controller", "testpassword", ProductRegistrationVerifyResult.Unregistered);
			AssertLogin("operational and controller with support token", false, "op.controller", CWSupportLoginToken.TokenForTest, ProductRegistrationVerifyResult.Unregistered, false);

			AssertLogin("non-operational", true, "non.op", "testpassword", ProductRegistrationVerifyResult.Unregistered);
			AssertLogin("non-operational with support token", false, "non.op", CWSupportLoginToken.TokenForTest, ProductRegistrationVerifyResult.Unregistered, false);

			AssertLogin("CWSupport with support token", true, User.SupportUserName, CWSupportLoginToken.TokenForTest, ProductRegistrationVerifyResult.Unregistered);
		}

		void AssertLogin(string msg, bool expected, string loginName, string password, ProductRegistrationVerifyResult registrationVerifyResult, bool withCorrectPwd = true)
		{
			var initialRego = ObjectFactory.Get<IProductRegistration>();
			var rego = new Mock<IProductRegistration>() { CallBase = true };

			using (ObjectFactory.Substitute(rego.Object))
			{
				rego.Setup(m => m.LocalVerify()).Returns(() => registrationVerifyResult);
				rego.Setup(m => m.Key).Returns(initialRego.Key);
				rego.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(initialRego.IsWiseTechGlobalInternalSystem());

				var controller = new UserLoginController();
				var result = controller.Login(loginName, password, CurrentBranch, CurrentDepartment);

				AssertEquals(msg, expected, result);
				rego.Verify(m => m.LocalVerify(), withCorrectPwd ? Times.Exactly(1) : Times.Never());
			}
		}

		public void TestSysAdminLogin()
		{
			bool previousForceValidRegistrationForTest = ProductRegister.ForceValidRegistrationForTest;

			try
			{
				var controller = new TestUserLoginController();

				var info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login", info.IsOK);

				var previousLoginAttempts = Env.Registry.LoginAttempts;
				Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				controller.LoginUser("sysadmin", "errorPassword");
				var passwordInvalidInfo = controller.LoginUser("sysadmin", "errorPassword");
				Assert("sysadmin should not be lock out", !info.User.IsLockedOut);
				AssertEquals("password is invalid", LoginAuthenticationInfo.Status.PasswordInvalid, passwordInvalidInfo.State);
				Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousLoginAttempts);

				//
				// Operational Staff
				//
				var operationalStaff = CreateStaff("operationalStaff", "testpassword", "st1", active: true, resource: false, isOperational: true);
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin With Active Operational Staff", info.IsOK);

				//
				// Non-Operational Staff
				//
				var nonOperationalStaff = CreateStaff("nonOperationalStaff", "testpassword", "st2", active: true, resource: false, isOperational: false);
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin With Active Non-Operational Staff", !info.IsOK);
				AssertEquals("sysadmin can only be used when there are no other controller or non-operational users.", info.FailureMessage);
				// In an unregistered system
				ProductRegister.ForceValidRegistrationForTest = false;
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin with no non-operational or controller staff", info.IsOK);
				// Force valid registration
				ProductRegister.ForceValidRegistrationForTest = true;
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin With Active Non-Operational Staff", !info.IsOK);
				// Delete non-operation staff and try again
				nonOperationalStaff.Delete();
				nonOperationalStaff.Factory.Save();
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin with no non-operational or controller staff", info.IsOK);

				//
				// Controller Staff
				//
				var controllerStaff = CreateStaff("controllerStaff", "testpassword", "st3", active: true, resource: false, isController: true);
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin With Active Controller Staff", !info.IsOK);
				AssertEquals("sysadmin can only be used when there are no other controller or non-operational users.", info.FailureMessage);
				// In an unregistered system
				ProductRegister.ForceValidRegistrationForTest = false;
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin with no non-operational or controller staff", info.IsOK);
				// Force valid registration
				ProductRegister.ForceValidRegistrationForTest = true;
				info = controller.LoginUser("sysadmin", "sysadmin");
				Assert("Login for SysAdmin With Active Non-Operational Staff", !info.IsOK);
			}
			finally
			{
				ProductRegister.ForceValidRegistrationForTest = previousForceValidRegistrationForTest;
			}
		}

		public void TestSysAdminLoginWithInactiveController()
		{
			var controller = new TestUserLoginController();

			var info = controller.LoginUser("sysadmin", "sysadmin");
			Assert("Login", info.IsOK);

			//Controller Staff
			var controllerStaff = CreateStaff("controllerStaff", "testpassword", "st3", false, resource: false, isController: true);
			info = controller.LoginUser("controllerStaff", "testpassword");
			Assert("Login for Controller Staff", !info.IsOK);
			AssertEquals("Your login has been disabled. Please see your system administrator to re-activate your login.", info.FailureMessage);

			info = controller.LoginUser("sysadmin", "sysadmin");
			Assert("Login for SysAdmin", info.IsOK);
		}

		public void TestSysAdminLoginWithControllerRequiredPasswordReset()
		{
			var controller = new TestUserLoginController();

			var info = controller.LoginUser("sysadmin", "sysadmin");
			Assert("Login", info.IsOK);

			//Controller Staff
			var controllerStaff = CreateStaff("controllerStaff", "testpassword", "st3", true, resource: false, isController: true);
			controllerStaff.LocalPasswordMustBeReset = true;
			Factory.Save();
			info = controller.LoginUser("controllerStaff", "testpassword");
			Assert("Login for Controller Staff", !info.IsOK);
			AssertEquals(LoginAuthenticationInfo.Status.TempPasswordRequired, info.State);

			info = controller.LoginUser("sysadmin", "sysadmin");
			Assert("Login for SysAdmin", info.IsOK);
		}

		public void TestLoginUser_IsDeviceOnly()
		{
			GlbStaff staff = CreateStaff("TestUser", "TEST", "TST", true, false);
			staff.GS_IsDevice = true;
			Factory.Save();

			Assert("Device Only user login should not pass.", !Controller.LoginUser("TestUser", "TEST").IsOK);
			Assert("Device Only user login from Device should pass.", Controller.LoginUser("TestUser", "TEST", true).IsOK);

			staff.GS_IsDevice = false;
			Factory.Save();

			Assert("Not Device Only user login should pass.", Controller.LoginUser("TestUser", "TEST").IsOK);
			Assert("Not Device Only user from Device login should pass.", Controller.LoginUser("TestUser", "TEST", true).IsOK);
		}

		public void TestClientIPAddressRestrictionWorks()
		{
			var wiseCloudSecurityClient = new Mock<IWiseCloudSecurityClient>() { CallBase = true };
			ObjectFactory.Substitute(wiseCloudSecurityClient.Object);
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = new string[] { EnterpriseChannelMessageTypes.GetClientIPAddress };

			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");

			var saff = CreateStaff("testuser", "testpassword", "tst", true, false);
			wiseCloudSecurityClient.Setup(o => o.GetClientIPAddress("EDIDAT", "testuser")).Returns("220.233.196.223");
			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			wiseCloudSecurityClient.Setup(o => o.GetClientIPAddress("EDIDAT", "testuser")).Returns("203.62.211.6");
			result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.OK, result.State);
		}

		public void TestClientIPAddressRestrictionReportWiseCloudClientError()
		{
			var wiseCloudSecurityClient = new Mock<IWiseCloudSecurityClient>() { CallBase = true };
			ObjectFactory.Substitute(wiseCloudSecurityClient.Object);
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = new string[] { EnterpriseChannelMessageTypes.GetClientIPAddress };
			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");
			var saff = CreateStaff("testuser", "testpassword", "tst", true, false);
			wiseCloudSecurityClient.Setup(o => o.GetClientIPAddress("EDIDAT", "testuser")).Throws(new IOException("Failure"));

			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			AssertEquals("Failed to find path: Failure", result.FailureMessage);
		}

		public void TestClientIPAddressRestrictionDoesNotApplyToNonOperationalUsers()
		{
			var wiseCloudSecurityClient = new Mock<IWiseCloudSecurityClient>() { CallBase = true };
			ObjectFactory.Substitute(wiseCloudSecurityClient.Object);

			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");

			var saff = CreateStaff("testuser", "testpassword", "tst", true, false, false);

			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.OK, result.State);

			result = Controller.LoginUser(User.SupportUserName, CWSupportLoginToken.TokenForTest);
			AssertEquals(LoginAuthenticationInfo.Status.OK, result.State);
		}

		public void TestClientIPAddressRestrictionMessageLocalAccess()
		{
			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");
			RawDataRegistry.Instance.ClientIPAddressRestrictedMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Stay out!!");

			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			AssertEquals("Access from the hosting server is not permitted. To enable add \"LOCALHOST\" in the IP Address Restriction list.", result.FailureMessage);

			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "LOCALHOST");
			result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.OK, result.State);
			AssertEquals(string.Empty, result.FailureMessage);
		}

		public void TestClientIPAddressRestrictionMessageWithoutRDSPlugin()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = Array.Empty<string>();
			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");
			RawDataRegistry.Instance.ClientIPAddressRestrictedMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Stay out!!");

			var staff = CreateStaff("testuser", "testpassword", "tst", true, false);
			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			AssertEquals("IP Address Restriction is not supported because Remote Desktop Service Plug-in is not installed.", result.FailureMessage);
		}

		public void TestClientIPAddressRestrictionMessage()
		{
			var wiseCloudSecurityClient = new Mock<IWiseCloudSecurityClient>() { CallBase = true };
			ObjectFactory.Substitute(wiseCloudSecurityClient.Object);
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = new string[] { EnterpriseChannelMessageTypes.GetClientIPAddress };

			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "203.62.211.4/30");
			RawDataRegistry.Instance.ClientIPAddressRestrictedMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Stay out!!");

			var saff = CreateStaff("testuser", "testpassword", "tst", true, false);

			wiseCloudSecurityClient.Setup(o => o.GetClientIPAddress("EDIDAT", "testuser")).Returns("220.233.196.223");
			var result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			AssertEquals("Stay out!! Accessing from IP: 220.233.196.223", result.FailureMessage);

			RawDataRegistry.Instance.ClientIPAddressRestriction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "LOCALHOST");
			result = Controller.LoginUser("testuser", "testpassword");
			AssertEquals(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, result.State);
			AssertEquals("Stay out!! Accessing from IP: 220.233.196.223", result.FailureMessage);
		}

		public void TestLoginUserWithDifferentLanguage()
		{
			CreateStaff("TestUser", "TEST", "TST", true, false);
			Factory.Save();
			var result = Controller.LoginUser("TestUser", "WrongPassword");
			Assert("Login should not pass.", !result.IsOK);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", result.FailureMessage);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			{
				result = Controller.LoginUser("TestUser", "WrongPassword");
				Assert("Login should not pass.", !result.IsOK);
				AssertContains("Message should be Chinese by Current Language", "用户名", result.FailureMessage);

				result = Controller.LoginUser("TestUser", "WrongPassword", language: Enterprise.Core.SharedConstants.Languages.Japanese);
				Assert("Login should not pass.", !result.IsOK);
				AssertContains("Message should be override to Japaness", "ユーザー名", result.FailureMessage);
			}
		}

		public void TestLoginLocationExAutomatically_WithSystemAccountAndHasCompanySpecificOverrides()
		{
			var staff = Factory.NewMoq<GlbStaff>();
			staff.Setup(m => m.GS_IsSystemAccount).Returns(true);

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff.Object);

			var testController = new TestUserLoginController();

			var result = testController.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: false, result.IsOK);
			AssertEquals(expected: "sysadmin can only be used when there are no other controller or non-operational users.", result.FailureMessage);
			AssertEquals(expected: LoginAuthenticationInfo.Status.DisallowedSysAdminLogon, result.State);
		}

		public void TestLoginLocationExAutomatically_WithLastLogonBranchAndDepartmentNotEmpty()
		{
			var staff = Factory.NewMoq<GlbStaff>();
			staff.Setup(m => m.GS_GB_LastLogonBranch).Returns(new ZGuid(Guid.NewGuid()));
			staff.Setup(m => m.GS_GE_LastLogonDepartment).Returns(new ZGuid(Guid.NewGuid()));

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff.Object);

			var mockController = new Mock<UserLoginController> { CallBase = true };

			mockController.Setup(m => m.LoginLocationEx(authenticatedUser, staff.Object.GS_GB_LastLogonBranch.ToGuid(),
				staff.Object.GS_GE_LastLogonDepartment.ToGuid())).Returns(authenticatedUser);

			var result = mockController.Object.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: true, result.IsOK);
			AssertEquals(expected: string.Empty, result.FailureMessage);
		}

		public void TestLoginLocationAutomaticallyEx_WithHomeBranchAndDepartmentNotEmpty()
		{
			var staff = Factory.NewMoq<GlbStaff>();
			staff.Setup(m => m.GS_GB_LastLogonBranch).Returns(new ZGuid(Guid.NewGuid()));
			staff.Setup(m => m.GS_GE_LastLogonDepartment).Returns(new ZGuid(Guid.NewGuid()));
			staff.Setup(m => m.GS_GB_HomeBranch).Returns(new ZGuid(Guid.NewGuid()));
			staff.Setup(m => m.GS_GE_HomeDepartment).Returns(new ZGuid(Guid.NewGuid()));

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff.Object);

			var mockController = new Mock<UserLoginController> { CallBase = true };

			mockController.Setup(m => m.LoginLocationEx(authenticatedUser, staff.Object.GS_GB_LastLogonBranch.ToGuid(),
				staff.Object.GS_GE_LastLogonDepartment.ToGuid())).Returns(LoginAuthenticationInfo.NewFailedLogin("failure login"));
			mockController.Setup(m => m.LoginLocationEx(authenticatedUser, staff.Object.GS_GB_HomeBranch.ToGuid(),
				staff.Object.GS_GE_HomeDepartment.ToGuid())).Returns(authenticatedUser);

			var result = mockController.Object.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: true, result.IsOK);
			AssertEquals(expected: string.Empty, result.FailureMessage);
		}

		public void TestLoginLocationAutomaticallyEx_WithSystemAccount()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsSystemAccount = true;
			Factory.Save();

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);

			var mockController = new Mock<UserLoginController> { CallBase = true };

			mockController.Setup(m => m.LoginLocationEx(authenticatedUser, It.IsAny<Guid>(),
				It.IsAny<Guid>())).Returns(authenticatedUser);

			var result = mockController.Object.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: true, result.IsOK);
			AssertEquals(expected: string.Empty, result.FailureMessage);
		}

		public void TestLoginLocationAutomaticallyEx_WithHomeBranchAndDepartmentNotEmptyButFailed()
		{
			var staff = Factory.NewMoq<GlbStaff>();
			staff.Setup(m => m.GS_GB_HomeBranch).Returns(new ZGuid(Guid.NewGuid()));
			staff.Setup(m => m.GS_GE_HomeDepartment).Returns(new ZGuid(Guid.NewGuid()));

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff.Object);

			var mockController = new Mock<UserLoginController> { CallBase = true };

			mockController.Setup(m => m.LoginLocationEx(authenticatedUser, staff.Object.GS_GB_HomeBranch.ToGuid(),
				staff.Object.GS_GE_HomeDepartment.ToGuid())).Returns(LoginAuthenticationInfo.NewFailedLogin("failure login for home branch"));

			var result = mockController.Object.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: false, result.IsOK);
			AssertEquals(expected: "failure login for home branch", result.FailureMessage);
		}

		public void TestLoginLocationExAutomatically_LoginForDeviceOnly()
		{
			var staff = Factory.NewMoq<GlbStaff>();

			var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff.Object);

			var mockController = new Mock<UserLoginController> { CallBase = true };

			var result = mockController.Object.LoginLocationExAutomatically(authenticatedUser);

			AssertEquals(expected: false, result.IsOK);
			AssertEquals(expected: "No user record could be found that matched the retrieved identity information. Please contact your system administrator.", result.FailureMessage);
			AssertEquals(expected: LoginAuthenticationInfo.Status.UserNotFound, result.State);
		}

		#region Test Classes

		class TestUserLoginController : UserLoginController
		{
			public TestUserLoginController()
			{
			}

			internal override ClientHook GetClientHook()
			{
				var mockClientHook = new Mock<ClientHook>();
				mockClientHook.Setup(m => m.HasCompanySpecificOverrides).Returns(true);
				return mockClientHook.Object;
			}

			public new void LogLoginTime(DateTime localTime)
			{
				base.LogLoginTime(localTime);
			}

			public new void LogLogoutTime(DateTime localTime)
			{
				base.LogLogoutTime(localTime);
			}

			public new string GetLogInOutReference(bool isLogin)
			{
				return base.GetLogInOutReference(isLogin);
			}

			public new LoginAuthenticationInfo LoginLocationExAutomatically(LoginAuthenticationInfo authenticatedUser)
			{
				return base.LoginLocationExAutomatically(authenticatedUser);
			}

			public new ILicensedComponent CoreLicenceComponent
			{
				get { return base.CoreLicenceComponent; }
			}

			public new TemporaryPassword TemporaryPasswordCache
			{
				get => temporaryPasswordCacheOverride ?? base.TemporaryPasswordCache;
				set => temporaryPasswordCacheOverride = value;
			}
			TemporaryPassword temporaryPasswordCacheOverride;
			public Dictionary<Guid, string> TwoFactorAuthenticationCodeTable => twoFactorAuthenticationCodeTable.Value;
		}

		class TestLicensedComponent : ILicensedComponent
		{
			#region ILicensedComponent Members

			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return new LicensedComponentManager(this); }
			}

			#endregion
		}

		#endregion

		#region Implementation

		GlbStaff CreateStaffWithValidTestData(string loginName, string password, string code, bool active, bool resource, bool isOperational = true, bool isController = false, bool canLogin = true, bool twoFactorEnabled = false)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			staff.GS_PER = person.PK;
			person.PER_City = "ABC";
			person.PER_FullName = "ABC DEF";
			person.PER_HomeAddress1 = "GHI";
			person.PER_RN_NKCountry = "AU";
			staff.GS_City = "ABC";
			staff.GS_FullName = "ABC DEF";
			staff.GS_UserAddress1 = "GHI";
			staff.GS_RN_NKCountryCode = "AU";
			return CreateStaffCore(staff, loginName, password, code, active, resource, isOperational, isController, canLogin, twoFactorEnabled);
		}

		GlbStaff CreateStaff(string loginName, string password, string code, bool active, bool resource, bool isOperational = true, bool isController = false, bool canLogin = true, bool twoFactorEnabled = false)
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			return CreateStaffCore(staff, loginName, password, code, active, resource, isOperational, isController, canLogin, twoFactorEnabled);
		}

		GlbStaff CreateStaffWithADEnabled(string loginName, string password, string code, bool active = true, bool resource = false, bool isOperational = true, bool isController = false, bool canLogin = true, IADEntityProvider adEntityProviderMock = null, IADUser adUserMock = null, bool twoFactorEnabled = false)
		{
			ActiveDirectoryRegistry.IsIntegrationEnabled = true;
			var iadrepository = new Mock<IADEntityProvider>() { CallBase = true };
			var adUserrepository = new Mock<IADUser>() { CallBase = true };

			var adEntityProvider = adEntityProviderMock ?? iadrepository.Object;
			var adUser = adUserMock ?? adUserrepository.Object;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Mock.Get(adEntityProvider).Setup(a => a.GetADUser(It.Is<IGlbStaff>(s => s.PK == staff.PK))).Returns(adUser);
			ObjectFactory.Substitute(adEntityProvider);

			return CreateStaffCore(staff, loginName, password, code, active, resource, isOperational, isController, canLogin, twoFactorEnabled);
		}

		GlbStaff CreateStaffCore(GlbStaff staff, string loginName, string password, string code, bool active, bool resource, bool isOperational, bool isController, bool canLogin, bool twoFactorEnabled)
		{
			staff.GS_IsResource = resource;
			staff.GS_CanLogin = canLogin;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = password;
			staff.GS_IsActive = active;
			staff.GS_IsOperational = isOperational;
			staff.GS_IsController = isController;
			staff.GS_IsTwoFactorAuthenticationEnabled = twoFactorEnabled;
			staff.GS_EmailAddress = "e@mail.com";

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();
			return staff;
		}

		TestUserLoginController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new TestUserLoginController();
				}
				return fController;
			}
		}
		TestUserLoginController fController;

		object ExecuteScalar(string sqlText)
		{
			return Db.Connection.ExecuteScalar(sqlText);
		}

		Guid CurrentBranch;
		Guid CurrentDepartment;
		IADRegistry ActiveDirectoryRegistry;
		IUserContext initialUserContext;

		protected override void SetUp()
		{
			CurrentBranch = Env.CurrentBranch.PK;
			CurrentDepartment = Env.CurrentDepartment.PK;
			ActiveDirectoryRegistry = ObjectFactory.Get<IADRegistry>();
			initialUserContext = Env.CurrentUserContext;

			base.SetUp();
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			base.TearDown();

			// Restores in-memory current user info (tests may have changed it)
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				Env.SetUserContext(initialUserContext);
			}

			LicenceCheckpoint.ShouldCheckForSystemExpiry = false;
		}

		#endregion
	}
}

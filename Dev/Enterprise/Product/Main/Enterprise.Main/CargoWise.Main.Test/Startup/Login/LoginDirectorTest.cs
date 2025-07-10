using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Login;

#if !WINZOR
using CargoWise.ActiveDirectory;
#endif

namespace Enterprise.Startup.Testing
{
	[GuiTest]
	sealed class LoginDirectorTest : AbstractApplicationStartupTaskTest<LoginDirector>
	{
		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestCheckStatusWhenDbLock()
		{
			using (LoginDirector.UseTestInstance())
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_LoginName = "Samuel.Cunard";
					staff.GS_ActiveDirectoryObjectGuid = new ZGuid("2d3c9768-561c-430f-81b7-b5263bc5bb2c");
					Factory.Save();

					ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(new FakeWindowsUserAuthenticationProvider());
					ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
					ObjectFactory.Get<IADRegistry>().IsSingleSignOnEnabled = true;

					try
					{
						((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
						LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-ShowLogin" }));

						DbLockout.AcquireLockout(adminConnection);
						LoginDirector.Instance.CheckUserStatusAndTerminateClient(null, null);
						Thread.Sleep(6000);
						Application.DoEvents();
						Thread.Sleep(6000);
						Application.DoEvents();
					}
					finally
					{
						DbLockout.ResetLockout(adminConnection);
					}

					Thread.Sleep(6000);
					Application.DoEvents();

					// !!! Any Assertion in this test should behind this line. !!!
					LoginDirector.Instance.AuthenticatedUser = null;

					AssertEquals(3, LoginDirector.Instance.CheckCount);
				}
			}
		}

		public void TestAdLoginSingleSignOn()
		{
			using (LoginDirector.UseTestInstance())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "Samuel.Cunard";
				staff.GS_ActiveDirectoryObjectGuid = new ZGuid("2d3c9768-561c-430f-81b7-b5263bc5bb2c");
				Factory.Save();

				ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(new FakeWindowsUserAuthenticationProvider());
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				ObjectFactory.Get<IADRegistry>().IsSingleSignOnEnabled = true;
				try
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
					LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-ShowLogin" }));
					AssertEquals("LoginValidated", true, LoginDirector.Instance.AuthenticatedUser.LoginValidated);
					AssertEquals("IsOK", true, LoginDirector.Instance.AuthenticatedUser.IsOK);
					AssertEquals(staff.PK, LoginDirector.Instance.AuthenticatedUser.User.PK);
				}
				finally
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
				}
			}
		}

		public void TestAdLoginSingleSignOn_2FA()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

			using (LoginDirector.UseTestInstance())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "Agent.Smith";
				staff.GS_EmailAddress = "agent.smith@th.ematr.ix";
				staff.GS_ActiveDirectoryObjectGuid = new ZGuid("2d3c9768-561c-430f-81b7-b5263bc5bb2c");
				staff.GS_IsTwoFactorAuthenticationEnabled = true;
				Factory.Save();

				ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(new FakeWindowsUserAuthenticationProvider());
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				ObjectFactory.Get<IADRegistry>().IsSingleSignOnEnabled = true;
				try
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
					LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-ShowLogin" }));
					AssertEquals("LoginValidated", true, LoginDirector.Instance.AuthenticatedUser.LoginValidated);
					AssertEquals("IsOK should be false as it require 2FA", false, LoginDirector.Instance.AuthenticatedUser.IsOK);
					AssertEquals("Expecting 2FA", LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired, LoginDirector.Instance.AuthenticatedUser.State);
					AssertEquals(staff.PK, LoginDirector.Instance.AuthenticatedUser.User.PK);
				}
				finally
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
				}
			}
		}

		public void TestAdLoginSingleSignOnFailsForInactiveUser()
		{
			using (LoginDirector.UseTestInstance())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "Samuel.Cunard";
				staff.GS_ActiveDirectoryObjectGuid = new ZGuid("2d3c9768-561c-430f-81b7-b5263bc5bb2c");
				staff.GS_IsActive = false;
				Factory.Save();

				ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(new FakeWindowsUserAuthenticationProvider());
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				ObjectFactory.Get<IADRegistry>().IsSingleSignOnEnabled = true;
				try
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
					LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-ShowLogin" }));
					Assert(!LoginDirector.Instance.AuthenticatedUser.LoginValidated);
				}
				finally
				{
					((UserLoginController)Env.LoginController).WindowsAuthenticationProvider = null;
				}
			}
		}

		public override int DefaultErrorExitCode => ExitCodes.LoginDirectorError;

		internal class FakeWindowsUserAuthenticationProvider : IWindowsUserAuthenticationProvider
		{
			public FakeWindowsUserAuthenticationProvider(Guid currentWindowsUserGuid = new Guid(), string currentWindowsUserName = "Samuel.Cunard")
			{
				this.CurrentWindowsUsername = currentWindowsUserName;
				this.CurrentWindowsUserGuid = currentWindowsUserGuid;
				if (currentWindowsUserGuid == Guid.Empty)
				{
					this.CurrentWindowsUserGuid = new Guid("2d3c9768-561c-430f-81b7-b5263bc5bb2c");
				}
			}

			public string CurrentWindowsUsername { get; set; }

			public Guid CurrentWindowsUserStaffPK { get; set; } // CW1's StaffPK need to be set

			public Guid CurrentWindowsUserGuid { get; set; }

			public ValidateCredentialsResult ValidateCredentials(Guid staffPK, string password)
			{
				return staffPK == CurrentWindowsUserStaffPK
					? ValidateCredentialsResult.OK
					: ValidateCredentialsResult.Invalid;
			}

			public ValidateCredentialsResult ValidateCurrentUserSession(Guid activeDirectoryObjectGuid)
			{
				return activeDirectoryObjectGuid == CurrentWindowsUserGuid
					? ValidateCredentialsResult.OK
					: ValidateCredentialsResult.Invalid;
			}
		}

		[GuiTest]
		public void TestVerboseGoesToEnv()
		{
			try
			{
				using (LoginDirector.UseTestInstance())
				using (var tempFile = TempFile.New())
				{
					Assert(string.IsNullOrEmpty(Env.LoginController.VerboseLoginFilename));
					CommandLineArguments args = new ApplicationArguments(new string[] { "-VerboseLoginFilename:" + tempFile.Filename });
					LoginDirector.Instance.Execute(args);
					Assert(!string.IsNullOrEmpty(Env.LoginController.VerboseLoginFilename));
				}
			}
			finally
			{
				Env.LoginController.VerboseLoginFilename = null;
			}
		}

		[GuiTest]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestDoExpiredPasswordCheck()
		{
			var staff = CreateSavedStaff();

			Env.SetUserContext(new UserContext("TestUser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			Env.Registry.PasswordChangeDays = 14;
			Env.Registry.PromptPasswordChangeBeforeExpireDays = 7;

			AssertEquals("DoExpiredPasswordCheck", true, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownDialogForTest);

			staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate.AddDays(-11);
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			AssertEquals("DoExpiredPasswordCheck", true, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertType("LastFormShownForTest", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			staff.GS_ChangePasswordAtNextLogin = true;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			AssertEquals("DoExpiredPasswordCheck", false, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertType("LastFormShownForTest", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			AssertEquals("DoExpiredPasswordCheck", false, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertType("LastFormShownForTest", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			AssertEquals("DoExpiredPasswordCheck", false, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertType("LastFormShownForTest", null, ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = false;
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			AssertEquals("DoExpiredPasswordCheck", false, LoginDirector.Instance.DoExpiredPasswordCheck());
			AssertType("LastFormShownForTest", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestDoExpiredPasswordCheck_WhenUserNotLoggedIn()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = false;

			var staff = CreateSavedStaff();
			staff.ChangePasswordAtNextLogin = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				LoginDirector.Instance.DoExpiredPasswordCheck(staff.GS_LoginName);
				AssertType("Should be showing ChangePasswordDialog", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);
			}

			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				LoginDirector.Instance.DoExpiredPasswordCheck(staff.GS_LoginName);
				AssertType("Should be showing ChangePasswordDialog", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[ExpectNoExceptions]
		public void TestDoExpiredPasswordCheck_AD_WhenUserNotLoggedIn()
		{
			var entityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			ObjectFactory.Substitute(entityProvider.Object);

			var staff = CreateSavedStaff();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = false;

			entityProvider.Setup(m => m.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == staff.GS_LoginName))).Returns(adUser.Object);
			adUser.Setup(m => m.PasswordExpired).Returns(true);

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				Assert(staff.IsADLinked);
				Assert(staff.IsADIntegrationEnabled);
				Assert(staff.ADPasswordExpired);
				Assert(staff.ShouldUseADPasswordPolicy);

				LoginDirector.Instance.DoExpiredPasswordCheck(staff.GS_LoginName);
				AssertType("Should be showing ChangePasswordDialog", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[ExpectNoExceptions]
		public void TestDoExpiredPasswordCheck_AD_WhenUserNotLoggedIn_DisableADPasswordChanged()
		{
			var entityProvider = new Mock<IADEntityProvider>();
			var adUser = new Mock<IADUser>();
			ObjectFactory.Substitute(entityProvider.Object);

			var staff = CreateSavedStaff();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().DisableADPasswordChange = true;

			entityProvider.Setup(m => m.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == staff.GS_LoginName))).Returns(adUser.Object);
			adUser.Setup(m => m.PasswordExpired).Returns(true);

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(Env.CurrentUser);

				Assert(staff.IsADLinked);
				Assert(staff.IsADIntegrationEnabled);
				Assert(staff.ADPasswordExpired);
				Assert(staff.ShouldUseADPasswordPolicy);

				LoginDirector.Instance.DoExpiredPasswordCheck(staff.GS_LoginName);
				AssertType("DisableADPasswordChange should not show ChangePasswordDialog", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[RequiresSTA]
		public void TestChangePasswordAtNextLogin()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var staff = CreateSavedStaff();

				var adUser = new Mock<IADUser>();
				adUser.Setup(m => m.HasExistingDirectoryEntry()).Returns(true);
				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(true);

				var entityProvider = new Mock<IADEntityProvider>();
				entityProvider.Setup(m => m.GetADUser(It.Is<IGlbStaff>(s => s.GS_LoginName == staff.GS_LoginName))).Returns(adUser.Object);

				var mockWindowsUserAuthenticationProvider = new Mock<IWindowsUserAuthenticationProvider>();
				mockWindowsUserAuthenticationProvider.Setup(m => m.ValidateCredentials(staff.PK.ToGuid(), staff.StaffPlainTextPassword)).Returns(ValidateCredentialsResult.OK);

				ObjectFactory.Substitute(entityProvider.Object);
				ObjectFactory.Substitute(mockWindowsUserAuthenticationProvider.Object);

				staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
				ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
				Factory.Save();

				Assert(staff.IsADLinked);
				Assert(staff.IsADIntegrationEnabled);
				Assert(staff.ChangePasswordAtNextLogin);
				AssertEquals(LoginAuthenticationInfo.Status.PasswordExpired, LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword).State);

				adUser.Setup(m => m.PasswordMustChangeAtNextLogon).Returns(false);
				AssertEquals(LoginAuthenticationInfo.Status.OK, LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword).State);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestDoExpiredPasswordCheck_Concurrency()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var staff = CreateSavedStaff();
				staff.GS_ChangePasswordAtNextLogin = true;
				Factory.Save();

				Env.SetUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				AssertNoExceptionThrown(() => LoginDirectorConcurrencyTest.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword));

				var newFactory = new BusinessObjectFactory();
				staff = newFactory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, staff.GS_LoginName);
				Assert("Do not need to change password again", !staff.GS_ChangePasswordAtNextLogin);
				LoginDirectorConcurrencyTest.Instance.LoginUserInteractive(staff.GS_LoginName, "firstChange");
				Assert("Login successful with the password change that occurred first", LoginDirectorConcurrencyTest.Instance.AuthenticatedUser.LoginValidated);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		[RequiresSTA]
		public void TestDoChangeUserPasswordWithoutCurrentUserContext()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var staff = CreateSavedStaff();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				staff.GS_ChangePasswordAtNextLogin = true;
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_GE_HomeDepartment = department.PK;
				Factory.Save();

				using (Env.SetTemporaryUserContext(null))
				{
					var correctContextToUpdatePassword = false;
					void AssertCorrectContextIsSet(BusinessObjectFactory factory)
					{
						if (factory.NameForDebugging == "Expired password")
						{
							correctContextToUpdatePassword = Env.CurrentUserPK == staff.PK
							&& Env.CurrentBranchPK == branch.PK
							&& Env.CurrentDepartmentPK == department.PK;
						}
					}
					BusinessObjectFactory.SetOnFactorySaveHookForTest(AssertCorrectContextIsSet);
					AssertNoExceptionThrown(() => LoginDirectorWithoutCurrentUserTest.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword));
					AssertEquals(staff.GS_Code, staff.GS_SystemLastEditUser);
					AssertEquals(true, correctContextToUpdatePassword);
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		[RequiresSTA]
		public void TestDoChangeUserPasswordWithoutCurrentUserContextOrStaffHomeBranch()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var staff = CreateSavedStaff();
				staff.GS_ChangePasswordAtNextLogin = true;
				Factory.Save();

				using (Env.SetTemporaryUserContext(null))
				{
					var correctContextToUpdatePassword = false;

					//current context is null, staff has no home branch or department so use first active branch/department
					var branch = EnvProxy.GetAnyBranch(Factory);
					var department = EnvProxy.GetAnyDepartment(Factory);
					void AssertCorrectContextIsSet(BusinessObjectFactory factory)
					{
						if (factory.NameForDebugging == "Expired password")
						{
							correctContextToUpdatePassword = Env.CurrentUserPK == staff.PK
							&& Env.CurrentBranchPK == branch
							&& Env.CurrentDepartmentPK == department;
						}
					}
					BusinessObjectFactory.SetOnFactorySaveHookForTest(AssertCorrectContextIsSet);
					AssertNoExceptionThrown(() => LoginDirectorWithoutCurrentUserTest.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword));
					AssertEquals(staff.GS_Code, staff.GS_SystemLastEditUser);
					AssertEquals(true, correctContextToUpdatePassword);
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		class LoginDirectorConcurrencyTest : LoginDirector
		{
			public new static LoginDirector Instance
			{
				get { return instance; }
				set { instance = value; }
			}
			static LoginDirector instance = new LoginDirectorConcurrencyTest();

			protected override DialogResult ChangePasswordCore(GlbStaff staff)
			{
				staff.Factory.Saving += concurrencyEvent => ChangePasswordConcurrencyEvent(staff.PK);
				staff.ChangeLocalPassword(null, "secondChange");
				staff.GS_ChangePasswordAtNextLogin = false;
				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDateTime;
				staff.GS_SystemLastEditTimeUtc = Env.Time.CurrentLocalDateTime;
				return DialogResult.OK;
			}

			void ChangePasswordConcurrencyEvent(ZGuid staffPK)
			{
				var anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				var staffReloaded = anotherFactory.Load<GlbStaff>(staffPK);
				staffReloaded.ChangeLocalPassword(null, "firstChange");
				staffReloaded.GS_ChangePasswordAtNextLogin = false;
				staffReloaded.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDateTime.AddMinutes(-5);
				staffReloaded.GS_SystemLastEditTimeUtc = Env.Time.CurrentLocalDateTime.AddMinutes(-5);
				anotherFactory.Save();
			}
		}

		class LoginDirectorWithoutCurrentUserTest : LoginDirector
		{
			public new static LoginDirector Instance
			{
				get { return instance; }
				set { instance = value; }
			}
			static LoginDirector instance = new LoginDirectorWithoutCurrentUserTest();

			protected override DialogResult ChangePasswordCore(GlbStaff staff)
			{
				staff.ChangeLocalPassword(null, "secondChange");
				staff.GS_ChangePasswordAtNextLogin = false;
				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDateTime;
				staff.GS_SystemLastEditTimeUtc = Env.Time.CurrentLocalDateTime;
				return DialogResult.OK;
			}
		}

		[RequiresSTA]
		public void TestLoginUserInteractivePromptsStaffWhenChangePasswordAtNextLogin()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var staff = CreateSavedStaff();
				Assert("Test precondition", !staff.GS_ChangePasswordAtNextLogin);

				ZFormModaliser.LastFormShownDialogForTest = null;
				LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword);
				AssertNull("Shouldn't have opened a modal after login.", ZFormModaliser.LastFormShownDialogForTest);

				staff.GS_ChangePasswordAtNextLogin = true;
				Factory.Save();

				LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword);
				AssertType<ChangePasswordDialog>("Should have opened ChangePasswordDialog", ZFormModaliser.LastFormShownDialogForTest);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		[RequiresSTA]
		public void TestLoginUserInteractivePromptsStaffWhenPasswordExpired()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();

				var staff = CreateSavedStaff();
				Env.SetUserContext(new UserContext("TestUser", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				Env.Registry.PasswordChangeDays = 14;

				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate.AddDays(-1);
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				ZFormModaliser.LastFormShownDialogForTest = null;
				LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword);
				AssertNull("Shouldn't have opened a modal after login.", ZFormModaliser.LastFormShownDialogForTest);

				staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate.AddDays(-30);
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				LoginDirector.Instance.LoginUserInteractive(staff.GS_LoginName, staff.StaffPlainTextPassword);
				AssertType<ChangePasswordDialog>("Should have opened ChangePasswordDialog", ZFormModaliser.LastFormShownDialogForTest);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

#if !WINZOR
		[TestSemaphoreProvider]
		[RequiresSTA]
		public void TestLoginUserInteractive_SSO_ShouldNotThrow_NoDomainPrivilegeException()
		{
			var mainForm = StartupOpenMainFormTask.MainFormInstance;

			var staff = CreateSavedStaff();
			staff.GS_EmailAddress = "someone@somewhere.com";
			staff.GS_CanLogin = true;
			staff.GS_IsOperational = true;
			staff.GS_IsTwoFactorAuthenticationEnabled = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.PasswordNeverChanges = false;

			Factory.Save();

			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			ObjectFactory.Get<IADRegistry>().IsSingleSignOnEnabled = true;

			var code = "1234";
			var mockCodeSender = new Mock<ITwoFactorAuthenticationEngine>();
			mockCodeSender.Setup(m => m.SendTwoFactorAuthenticationCode(It.Is<IUser>(s => s.PK == staff.PK))).Returns(code);

			var directorySearcherMock = new Mock<IDirectorySearcher>();
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Throws(new NoDomainPrivilegeException("domain user credential is incorrect"));
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;

			var windowsUserAuthenticationProvider = new FakeWindowsUserAuthenticationProvider(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), staff.GS_Code);

			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();

				using (SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email"))
				using (ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(windowsUserAuthenticationProvider))
				using (ObjectFactory.Substitute("TwoFactorAuthEmailSender", mockCodeSender.Object))
				{
					LoginDirector.Instance.LoginAutomatically();
					AssertNoExceptionThrown(() => LoginDirector.Instance.LoginUserInteractive(null, null, code, true));
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}
#endif

		[TestSemaphoreProvider]
		[RequiresSTA]
		public void TestDoOneMachinePerUserLimitExceededCheck()
		{
			var staff = CreateSavedStaff();
			staff.GS_GB_LastLogonBranch = Env.CurrentBranch.PK;
			staff.GS_GE_LastLogonDepartment = Env.CurrentDepartment.PK;
			Factory.Save();

			var mainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				var director = LoginDirector.Instance;
				director.AuthenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(staff);

				// In winzor, Globals.ClientIdentifier is used to differentiate between different local machines
				// In RDP, the local machine name is used
#if WINZOR
				using (Globals.SetClientIdentifierForTest("123"))
#endif
				{
					var loginInfo = director.LoginLocationInteractive(Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
					Assert("Login", loginInfo.LoginValidated);
					Assert("IsLoggedIn", staff.IsLoggedIn);
				}

#if WINZOR
				using (Globals.SetClientIdentifierForTest("321"))
#endif
				{
					var semaphoreProvider = (SemaphoreProviderWithCurrentUserForTesting)TestSemaphoreProviderAttribute.TestProvider;
					semaphoreProvider.RemoteClientName = "machine2";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					var loginResult = director.LoginLocationInteractive(Env.CurrentBranch.Code, Env.CurrentDepartment.Code);

					string expectedMessage = string.Format("This user is already logged in from machine {0} (since {1}).\r\nA user can be only logged in from one computer or terminal session at a time.\r\nYou may force the other login to exit. They will have {2} seconds to save their work.\r\n\r\nDo you want to force the other user to log off?",
						loginResult.LoginOnAnotherMachine.OwnerSession.HostName,
						loginResult.LoginOnAnotherMachine.CreateTimeUtc.ToLocalTime(),
						SelfLogoffForm.LogoffDelayInSeconds);

					AssertEquals(false, loginResult.LoginValidated);
					AssertEquals("Error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					loginResult = director.LoginUserInteractive(staff.GS_LoginName, "password");
					AssertEquals(false, director.LoggedInLocation);
					loginResult = director.LoginLocationInteractive(Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
					AssertEquals(true, loginResult.LoginValidated);
					AssertEquals(true, loginResult.User.LoginToken.ForcedRemoteLogoff);
				}

				Env.LoginController.Logout();
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = mainForm;
			}
		}

		public void TestLoginWithBranchOptionOnly()
		{
			using (LoginDirector.UseTestInstance())
			{
				((GlbStaff)Env.CurrentUser).Reload();
				((GlbStaff)Env.CurrentUser).GS_GE_LastLogonDepartment = Env.CurrentDepartment.PK;
				((GlbStaff)Env.CurrentUser).Factory.Save();
				AssertNotEquals("Precondition", "TES", Env.CurrentBranch.Code);
				LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-Branch:TES" }));
				AssertEquals(true, LoginDirector.Instance.LoggedInLocation);
				AssertEquals("TES", Env.CurrentBranch.Code);
			}
		}

		public void TestLoginWithDepartmentOptionOnly()
		{
			using (LoginDirector.UseTestInstance())
			{
				((GlbStaff)Env.CurrentUser).Reload();
				((GlbStaff)Env.CurrentUser).GS_GB_LastLogonBranch = Env.CurrentBranch.PK;
				((GlbStaff)Env.CurrentUser).Factory.Save();
				AssertNotEquals("Precondition", "FEA", Env.CurrentDepartment.Code);
				LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-Department:FEA" }));
				AssertEquals(true, LoginDirector.Instance.LoggedInLocation);
				AssertEquals("FEA", Env.CurrentDepartment.Code);
			}
		}

		public void TestLoginWithBranchAndDepartmentOptions()
		{
			using (LoginDirector.UseTestInstance())
			{
				AssertNotEquals("Precondition", "TES", Env.CurrentBranch.Code);
				AssertNotEquals("Precondition", "FEA", Env.CurrentDepartment.Code);
				LoginDirector.Instance.Execute(new ApplicationArguments(new string[] { "-Branch:TES", "-Department:FEA" }));
				AssertEquals(true, LoginDirector.Instance.LoggedInLocation);
				AssertEquals("TES", Env.CurrentBranch.Code);
				AssertEquals("FEA", Env.CurrentDepartment.Code);
			}
		}

		public void TestOidcLoginParameters()
		{
			var loginRequestMessages = new List<OIDCLoginRequestMessage>();
			var oidcConfig = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://something/",
				ClientIdentifier = "interactive.public",
			};
			var mockOIDCLoginServer = new Mock<IOIDCLoginServer>(MockBehavior.Strict);
			mockOIDCLoginServer.SetupGet(m => m.IsSupported).Returns(true);
			mockOIDCLoginServer
				.Setup(m => m.LoginLocal(Capture.In(loginRequestMessages), It.IsAny<OIDCWebLauncher>(),
					It.IsAny<CancellationToken>(), It.IsAny<OIDCLoginFactory>()))
				.Returns(OIDCLoginResponseMessage.CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.LoginFailed, "error - test", "description - test"));
			using (ObjectFactory.Substitute(mockOIDCLoginServer.Object))
			using (ObjectFactory.Substitute<IOIDCConfig>(oidcConfig))
			{
				var loginDirector = LoginDirector.Instance;
				loginDirector.Execute(new ApplicationArguments(new string[] { "-ShowLogin" }));
				var authenticatedUser = loginDirector.AuthenticatedUser;
				CombineAssertions(() =>
				{
					AssertEquals("Login Validated", false, authenticatedUser.LoginValidated);
					AssertEquals("State", LoginAuthenticationInfo.Status.Failure, authenticatedUser.State);
					AssertEquals("User", null, authenticatedUser.User);
					AssertEquals("Failure Message", "Authentication failed, please try again or contact your system administrator.", authenticatedUser.FailureMessage);
					AssertEquals("Extended Error Information", "description - test", authenticatedUser.ExtendedErrorInformation);
				});
			}
			mockOIDCLoginServer.Verify(m => m.LoginLocal(It.IsAny<OIDCLoginRequestMessage>(), WebUrlLauncher.Launch, It.Is<CancellationToken>(c => c.CanBeCanceled), null), Times.Once());
			AssertEquals(1, loginRequestMessages.Count);
			var loginRequestMessage = loginRequestMessages.First();
			CombineAssertions(() =>
			{
				AssertEquals("Authority", oidcConfig.AuthorityURL, loginRequestMessage.Authority);
				AssertEquals("ClientID", oidcConfig.ClientIdentifier, loginRequestMessage.ClientID);
				AssertEquals("DomainHint", "Azure", loginRequestMessage.DomainHint);
				AssertEquals("ServerType", OIDCLoginRequestMessage.OIDCServer.Generic, loginRequestMessage.ServerType);
				AssertEquals("Prompt", OIDCLoginRequestMessage.LoginPrompt.Default, loginRequestMessage.Prompt);
				AssertContainsExactElementsInAnyOrder("Scopes", new[] { "openid", "offline_access" }, loginRequestMessage.Scopes);
			});
			mockOIDCLoginServer.VerifyNoOtherCalls();
		}

		GlbStaff CreateSavedStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TestUser";
			staff.GS_Code = "TST";
			staff.StaffPlainTextPassword = "password";
			staff.GS_LastPasswordChangeDate = Env.Time.CurrentLocalDate;
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.GS_IsActive = true;

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();

			return staff;
		}

		protected override void TearDown()
		{
			Env.LoginController.Logout();
			Env.LoginController.LoginUserDeveloper();
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}
	}
}

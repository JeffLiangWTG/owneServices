using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Startup.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Login.Testing
{
	sealed class LoginControlTestCase : TestCaseWithLoginDirectorAndMainForm
	{
		[TestSemaphoreProvider]
		[RequiresSTA]
		public async Task TestDefaultUserName()
		{
			var control = ShowLoginUserControl();
			AssertEquals(Env.CurrentUser.LoginName, control.userNameTextbox.Text);
			MainForm.InitializeAfterLogin();

			using (Env.SetTemporaryUserContext(null))
			{
				ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(new LoginDirectorTest.FakeWindowsUserAuthenticationProvider());
				control = ShowLoginUserControl();
				AssertEquals("Samuel.Cunard", control.userNameTextbox.Text);
				await control.oidcLoginTask;
			}
		}

		[TestSemaphoreProvider]
		[RequiresSTA]
		public async Task TestoidcLoginVisibilityDefaultValue()
		{
			var control = ShowLoginUserControl();
			Assert(!control.oidcLabel.Visible);
			await control.oidcLoginTask;
		}

		[TestSemaphoreProvider]
		[RequiresSTA]
		public async Task TestDefaultUserName_TwoSimilarUsers()
		{
			try
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompany.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_GE_HomeDepartment = department.PK;
				staff.StaffPlainTextPassword = "password";
				staff.GS_ChangePasswordAtNextLogin = false;

				var security1 = Factory.NewWithValidTestData<GlbSecurity>();
				security1.GU_GS = staff.PK;
				security1.GU_SecurityItemIsAllowed = true;
				security1.GU_SecurityRight = "Login";

				Factory.Save();

				var auth = new LoginDirectorTest.FakeWindowsUserAuthenticationProvider();

				using (Env.SetTemporaryUserContext(null))
				{
					ObjectFactory.Substitute<IWindowsUserAuthenticationProvider>(auth);
					var control = ShowLoginUserControl();
					AssertEquals("Samuel.Cunard", control.userNameTextbox.Text);
					control.userNameTextbox.Text = staff.GS_LoginName;
					control.passwordTextBox.Text = "password";
					GetLoginButton(control).PerformClick();
					AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
					AssertNotNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
					AssertEquals(staff.PK, Env.CurrentUser.PK);
					AssertEquals(branch.PK, Env.CurrentBranch.PK);
					AssertEquals(department.PK, Env.CurrentDepartment.PK);
					await control.oidcLoginTask;
				}

				using (Env.SetTemporaryUserContext(null))
				{
					var control = ShowLoginUserControl();
					AssertEquals(staff.GS_LoginName, control.userNameTextbox.Text);
					control.userNameTextbox.Text = staff.GS_LoginName;
					control.passwordTextBox.Text = "password";
					GetLoginButton(control).PerformClick();
					await control.oidcLoginTask;
				}

				using (Env.SetTemporaryUserContext(null))
				{
					var secondGuid = Guid.NewGuid();
					auth.CurrentWindowsUserGuid = secondGuid;

					var control = ShowLoginUserControl();
					AssertEquals("Samuel.Cunard", control.userNameTextbox.Text);
					control.userNameTextbox.Text = staff.GS_LoginName;
					control.passwordTextBox.Text = "password";
					GetLoginButton(control).PerformClick();
					await control.oidcLoginTask;
				}

				using (Env.SetTemporaryUserContext(null))
				{
					var control = ShowLoginUserControl();
					AssertEquals(staff.GS_LoginName, control.userNameTextbox.Text);
					await control.oidcLoginTask;
				}
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		[RequiresSTA]
		public async Task TestSupportLoginButtonShouldNotShowInNoneHostedSystem()
		{
			var oidcConfig = new Mock<IOIDCConfig>();
			oidcConfig.Setup(m => m.IsOIDCEnabled).Returns(true);
			using (SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(oidcConfig.Object))
			{
				var control = ShowLoginUserControl();
				Assert("Should be visible as Oidc login is enabled.", control.oidcLabel.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.userNameTextbox.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.passwordTextBox.Visible);

				var supportLogin = FindButton(control, "SupportLoginButton");
				AssertNull("support login button should not show in none hosted system.", supportLogin);
				await control.oidcLoginTask;
			}
		}

		[RequiresSTA]
		public async Task TestSupportLoginButtonShouldShowInSelfHostedSystemIfEnableSupportUserLoginIsTrue()
		{
			Assert("It should be self hosted system", !EnvProxy.IsHostedWithCargowise);

			var oidcConfig = new Mock<IOIDCConfig>();
			oidcConfig.Setup(m => m.IsOIDCEnabled).Returns(true);
			using (SystemDataRegistry.Instance.EnableSupportUserLogin.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(oidcConfig.Object))
			{
				var control = ShowLoginUserControl();
				Assert("Should be visible as Oidc login is enabled.", control.oidcLabel.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.userNameTextbox.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.passwordTextBox.Visible);

				var supportLogin = FindButton(control, "SupportLoginButton");
				AssertNotNull("support login button should show.", supportLogin);

				supportLogin.PerformClick();
				Assert("Should not be visible as support login enabled.", !control.oidcLabel.Visible);
				Assert("Should be visible as support login enabled.", control.userNameTextbox.Visible);
				Assert("Should be visible as support login enabled.", control.passwordTextBox.Visible);
				var loginButton = FindButton(control, "LoginButton");
				AssertNotNull("Login button should show in support login.", loginButton);

				control.userNameTextbox.Text = "NonSupport";
				control.passwordTextBox.Text = "passowrd";

				loginButton.PerformClick();

				AssertStartsWith("only CWSupport is valid in this case.", "Please login as a support user.", (StartupOpenMainFormTask.MainFormInstance.Controls.Find("errorLabel", true).First() as ZLabel).Text);
				await control.oidcLoginTask;
			}
		}

		[RequiresSTA]
		public async Task TestSupportLoginButtonShouldShowInHostedSystem()
		{
			var oidcConfig = new Mock<IOIDCConfig>();
			oidcConfig.Setup(m => m.IsOIDCEnabled).Returns(true);
			using (ObjectFactory.Substitute(oidcConfig.Object))
			{
				EnvProxy.SetHostedLocationForTest("SYD");

				var control = ShowLoginUserControl();
				Assert("Should be visible as Oidc login is enabled.", control.oidcLabel.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.userNameTextbox.Visible);
				Assert("Should not be visible as Oidc login is enabled.", !control.passwordTextBox.Visible);

				var supportLogin = FindButton(control, "SupportLoginButton");
				AssertNotNull("support login button should show in hosted system.", supportLogin);

				supportLogin.PerformClick();
				Assert("Should not be visible as support login enabled.", !control.oidcLabel.Visible);
				Assert("Should be visible as support login enabled.", control.userNameTextbox.Visible);
				Assert("Should be visible as support login enabled.", control.passwordTextBox.Visible);
				var loginButton = FindButton(control, "LoginButton");
				AssertNotNull("Login button should show in support login.", loginButton);
				await control.oidcLoginTask;
			}
		}

		[RequiresSTA]
		public async Task TestCWSupportLoginIfTokenIsNeeded()
		{
			try
			{
				var control = ShowLoginUserControl();
				control.userNameTextbox.Text = "CWSupport";
				control.passwordTextBox.Text = "invalidtoken";
				var loginButton = GetLoginButton(control);
				loginButton.PerformClick();
				AssertStartsWith("The token is invalid.", "The CWSupport token provided is invalid.", (StartupOpenMainFormTask.MainFormInstance.Controls.Find("errorLabel", true).First() as ZLabel).Text);

				control.passwordTextBox.Text = CWSupportLoginToken.TokenForTest;
				loginButton.PerformClick();
				AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
				AssertNotNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
				await control.oidcLoginTask;
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		ZToolStripButton FindButton(LoginUserControl loginUserControl, string buttonName)
		{
			var toolStrip = (ZToolStrip)loginUserControl.Controls.Find("toolStrip", true)[0];
			var button = toolStrip.Items.Find(buttonName, true);
			return button.Length > 0 ? (ZToolStripButton)button[0] : null;
		}

		[RequiresSTA]
		public async Task TestLoginError()
		{
			var control = ShowLoginUserControl();
			control.userNameTextbox.Text = "noone";
			control.passwordTextBox.Text = "invalid";
			GetLoginButton(control).PerformClick();
			AssertEquals(control, FindLoginUserControl());
			AssertEquals("You must enter a correct user name and/or password. Please try again.", control.errorLabel.Text);
			control.userNameTextbox.Text = "someone";
			AssertEquals("", control.errorLabel.Text);
			GetLoginButton(control).PerformClick();
			AssertEquals(control, FindLoginUserControl());
			AssertEquals("You must enter a correct user name and/or password. Please try again.", control.errorLabel.Text);
			control.passwordTextBox.Text = "";
			GetLoginButton(control).PerformClick();
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestLoginErrorUserADRecordNotCreated()
		{
			IADRegistry a = ObjectFactory.Get<IADRegistry>();
			a.IsIntegrationEnabled = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var control = ShowLoginUserControl();
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "testPass";
			GetLoginButton(control).PerformClick();
			AssertEquals("This staff record does not have an AD User created or linked to it yet. Please try again later.", control.errorLabel.Text);
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestLoginWithoutBranchDepartment()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "password";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.ResetBranchAndDepartment();
			Factory.Save();

			var control = ShowLoginUserControl();
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "password";
			GetLoginButton(control).PerformClick();
			AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
			AssertEquals(1, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true).Length);
			AssertNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
			AssertEquals(true, LoginDirector.Instance.AuthenticatedUser.LoginValidated);
			AssertEquals(staff.PK, LoginDirector.Instance.AuthenticatedUser.User.PK);
			AssertEquals(false, LoginDirector.Instance.LoggedInLocation);
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestLoginWithUserRequiredPasswordReset()
		{
			((WinFormsEnvironment)Env.Instance).SetLoginControllerForTesting(null);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			staff.GS_PasswordHash = ZBlob.Empty;
			staff.GS_PasswordSalt = ZBlob.Empty;
			staff.GS_PasswordHashIterations = 0;
			Factory.Save();
			AssertEquals("Test precondition", true, staff.LocalPasswordMustBeReset);

			//AD Enabled - should not offer local password reset
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			var control = ShowLoginUserControl();

			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "password";
			GetLoginButton(control).PerformClick();

			AssertEquals("Email Temp. Password should not be shown", false, control.toolStrip.Items.Contains(control.TempPasswordButton));
			AssertEquals("Should be showing Login Button", GetLoginButton(control), control.toolStrip.Items[0]);
			AssertEquals("This staff record does not have an AD User created or linked to it yet. Please try again later.", control.errorLabel.Text);

			//AD Disaabled - should offer local password reset
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			control = ShowLoginUserControl();
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "password";
			GetLoginButton(control).PerformClick();

			AssertEquals("Email Temp. Password should be shown", true, control.toolStrip.Items.Contains(control.TempPasswordButton));
			AssertEquals("Email Temp. Password should be shown as the first one", control.TempPasswordButton, control.toolStrip.Items[0]);
			AssertEquals("Email Temp. Password should be shown as the first one", "Email Temp. Password", control.toolStrip.Items[0].Text);
			AssertEquals("Your password must be reset. Click the 'Email Temp. Password' button to send a temporary password to your email address.", control.errorLabel.Text);
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestLoginWithTemporaryPassword()
		{
			((WinFormsEnvironment)Env.Instance).SetLoginControllerForTesting(null);
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			staff.GS_EmailAddress = "larry@gmail.com";
			staff.GS_PasswordHash = ZBlob.Empty;
			staff.GS_PasswordSalt = ZBlob.Empty;
			staff.GS_PasswordHashIterations = 0;
			staff.GS_ChangePasswordAtNextLogin = true;
			Factory.Save();
			AssertEquals("Test precondition", true, staff.LocalPasswordMustBeReset);

			var control = ShowLoginUserControl();
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "password";
			Login(control);

			AssertEquals("Email Temp. Password should be shown", true, control.toolStrip.Items.Contains(control.TempPasswordButton));
			AssertEquals("Your password must be reset. Click the 'Email Temp. Password' button to send a temporary password to your email address.", control.errorLabel.Text);

			//Email Temp. Password
			control.TempPasswordButton.PerformClick();
			AssertEquals("A temporary password has been sent to your email address.", control.errorLabel.Text);

			//Login with incorrect Temp Password
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "wrong";
			Login(control);
			AssertEquals("The password is invalid or has expired. Verify that you have entered the password correctly or request a new temporary password.", control.errorLabel.Text);

			//Login with correct Temp Password - should offer local password reset
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var tempPasswordFromEmail = email.Body.Split(System.Environment.NewLine.ToCharArray())[0];
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = tempPasswordFromEmail;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			Login(control);
			AssertType("Temp password login sucessfully, should be showing Change Password Dialig", typeof(ChangePasswordDialog), ZFormModaliser.LastFormShownDialogForTest);
			var changePasswordDialog = ZFormModaliser.LastFormShownDialogForTest as ChangePasswordDialog;
			AssertNotNull("Temp password login sucessfully, should be showing Change Password Dialig", changePasswordDialog);
			AssertEquals("Should be offering password reset", true, changePasswordDialog.IsResetPassword);
			AssertEquals("Your password has been successfully reset. Please login with your new password.", control.errorLabel.Text);
			AssertEquals("Self Reset password does not require ChangePasswordAtNextLogin after password reset", false, staff.GS_ChangePasswordAtNextLogin);
			await control.oidcLoginTask;
		}

		void Login(Control control)
		{
			GetLoginButton(control).PerformClick();
		}

		[RequiresSTA]
		public async Task TestLogin_CanHandleTempPasswordLoginResult()
		{
			var user = new Mock<IUser>();
			var testLoginDirector = new Mock<LoginDirector>();
			using (LoginDirector.UseTestInstance(testLoginDirector.Object))
			{
				var control = ShowLoginUserControl();

				//user required to reset password
				testLoginDirector.Setup(m => m.LoginUserInteractive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(LoginAuthenticationInfo.NewTempPasswordRequired(user.Object));
				GetLoginButton(control).PerformClick();
				AssertEquals("Email Temp. Password should be shown", true, control.toolStrip.Items.Contains(control.TempPasswordButton));
				AssertEquals("Your password must be reset. Click the 'Email Temp. Password' button to send a temporary password to your email address.", control.errorLabel.Text);

				//user enter a wrong temp password
				testLoginDirector.Setup(m => m.LoginUserInteractive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.TempPasswordRequired, "You are awesome"));
				GetLoginButton(control).PerformClick();
				AssertEquals("Email Temp. Password should be shown", true, control.toolStrip.Items.Contains(control.TempPasswordButton));
				AssertEquals("You are awesome", control.errorLabel.Text);

				//user enter correct temp password but cancel password reset
				testLoginDirector.Setup(m => m.LoginUserInteractive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(LoginAuthenticationInfo.NewTempPasswordLoginSuccessfully(user.Object));
				testLoginDirector.Setup(m => m.ResetLocalPassword(It.IsAny<Guid>())).Returns(false);
				control.errorLabel.Text = "";
				GetLoginButton(control).PerformClick();
				AssertEquals("Email Temp. Password should not be shown", true, control.toolStrip.Items.Contains(control.TempPasswordButton));
				AssertEquals("", control.errorLabel.Text);

				//user enter correct temp password and reset password
				testLoginDirector.Setup(m => m.LoginUserInteractive(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(LoginAuthenticationInfo.NewTempPasswordLoginSuccessfully(user.Object));
				testLoginDirector.Setup(m => m.ResetLocalPassword(It.IsAny<Guid>())).Returns(true);
				control.errorLabel.Text = "";
				GetLoginButton(control).PerformClick();
				AssertEquals("Email Temp. Password should not be shown", false, control.toolStrip.Items.Contains(control.TempPasswordButton));
				AssertEquals("Your password has been successfully reset. Please login with your new password.", control.errorLabel.Text);
				await control.oidcLoginTask;
			}
		}

		[RequiresSTA]
		public async Task TestEmailTemporaryPassword()
		{
			var testLoginDirector = new Mock<LoginDirector>();
			using (LoginDirector.UseTestInstance(testLoginDirector.Object))
			{
				testLoginDirector.Setup(m => m.EmailTemporaryPassword(It.IsAny<string>())).Returns("whatever it needs to say");
				var control = ShowLoginUserControl();
				control.errorLabel.Text = "";
				control.TempPasswordButton.PerformClick();
				AssertEquals("whatever it needs to say", control.errorLabel.Text);
				await control.oidcLoginTask;
			}
		}

		[RequiresSTA]
		public async Task TestLogin()
		{
			try
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompany.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_GE_HomeDepartment = department.PK;
				staff.StaffPlainTextPassword = "password";
				staff.GS_ChangePasswordAtNextLogin = false;

				var security1 = Factory.NewWithValidTestData<GlbSecurity>();
				security1.GU_GS = staff.PK;
				security1.GU_SecurityItemIsAllowed = true;
				security1.GU_SecurityRight = "Login";

				Factory.Save();

				var control = ShowLoginUserControl();
				control.userNameTextbox.Text = staff.GS_LoginName;
				control.passwordTextBox.Text = "password";
				GetLoginButton(control).PerformClick();
				AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
				AssertNotNull(StartupOpenMainFormTask.MainFormInstance.NavigationBar);
				AssertEquals(staff.PK, Env.CurrentUser.PK);
				AssertEquals(branch.PK, Env.CurrentBranch.PK);
				AssertEquals(department.PK, Env.CurrentDepartment.PK);
				await control.oidcLoginTask;
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		[RequiresSTA]
		public async Task TestLogin_IsDeviceOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "password";
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.GS_IsDevice = true;
			staff.ResetBranchAndDepartment();
			Factory.Save();

			var control = ShowLoginUserControl();
			control.userNameTextbox.Text = staff.GS_LoginName;
			control.passwordTextBox.Text = "password";
			GetLoginButton(control).PerformClick();
			AssertEquals("You do not have the appropriate settings to login, this login is for device only.", control.errorLabel.Text);

			staff.GS_IsDevice = false;
			Factory.Save();
			GetLoginButton(control).PerformClick();
			AssertEquals(0, StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true).Length);
			AssertEquals(true, LoginDirector.Instance.AuthenticatedUser.LoginValidated);
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestLoginNameIsRemebered()
		{
			try
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompany.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_GE_HomeDepartment = department.PK;
				staff.StaffPlainTextPassword = "password";
				staff.GS_ChangePasswordAtNextLogin = false;
				Factory.Save();

				var control = ShowLoginUserControl();
				control.userNameTextbox.Text = staff.GS_LoginName;
				control.passwordTextBox.Text = "password";
				GetLoginButton(control).PerformClick();
				await control.oidcLoginTask;

				using (Env.SetTemporaryUserContext(null))
				{
					AssertNull(Env.CurrentUser);
					control = ShowLoginUserControl();
					AssertEquals(staff.GS_LoginName, control.userNameTextbox.Text);
					await control.oidcLoginTask;
				}
			}
			finally
			{
				Env.LoginController.Logout();
			}
		}

		[RequiresSTA]
		public async Task TestTwoFactorAuthenticationLogin()
		{
			var currentRegistry = SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			try
			{
				SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = Env.CurrentCompany.PK;
				var department = Factory.NewWithValidTestData<GlbDepartment>();
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_GE_HomeDepartment = department.PK;
				staff.StaffPlainTextPassword = "password";
				staff.GS_ChangePasswordAtNextLogin = false;
				staff.IsTwoFactorAuthenticationEnabled = true;
				staff.GS_EmailAddress = "someone@somewhere.com";

				Factory.Save();

				var code = "000000";
				var mockCodeSender = new Mock<ITwoFactorAuthenticationEngine>();
				mockCodeSender.Setup(m => m.SendTwoFactorAuthenticationCode(It.Is<IUser>(s => s.PK == staff.PK))).Returns(code);
				ObjectFactory.Substitute("TwoFactorAuthEmailSender", mockCodeSender.Object);

				var control = ShowLoginUserControl();
				AssertEquals(control.twoFactorAuthenticationTextBox.Enabled, false);
				Assert(!control.toolStrip.Items.Contains(control.BackButton));

				control.userNameTextbox.Text = staff.GS_LoginName;
				control.passwordTextBox.Text = "password";
				GetLoginButton(control).PerformClick();

				AssertEquals("Please enter the validation code that has been sent to you.", control.errorLabel.Text);
				Assert(control.toolStrip.Items.Contains(control.BackButton));

				Assert(control.twoFactorAuthenticationTextBox.Enabled);
				control.twoFactorAuthenticationTextBox.Text = "invalid";
				GetLoginButton(control).PerformClick();

				AssertEquals("The validation code you entered is incorrect. Please try again.", control.errorLabel.Text);
				Assert(control.toolStrip.Items.Contains(control.BackButton));

				Assert(!control.userNameTextbox.Enabled);
				Assert(!control.passwordTextBox.Enabled);
				control.BackButton.PerformClick();
				Assert(!control.toolStrip.Items.Contains(control.BackButton));
				Assert(control.userNameTextbox.Enabled);
				Assert(control.passwordTextBox.Enabled);
				AssertEquals("", control.passwordTextBox.Text);
				control.passwordTextBox.Text = "password";
				GetLoginButton(control).PerformClick();
				Assert(control.toolStrip.Items.Contains(control.BackButton));
				Assert(!control.userNameTextbox.Enabled);
				Assert(!control.passwordTextBox.Enabled);

				AssertNotEquals(staff.PK, Env.CurrentUser.PK);
				control.twoFactorAuthenticationTextBox.Text = code;
				GetLoginButton(control).PerformClick();
				AssertEquals(staff.PK, Env.CurrentUser.PK);
				await control.oidcLoginTask;
			}
			finally
			{
				Env.LoginController.Logout();
				SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentRegistry);
			}
		}

		[RequiresSTA]
		public async Task TestAutoCloseParentForm()
		{
			var control = ShowLoginUserControl();

			Assert("Auto close timer should have enabled (started).", control.autoCloseTimer.Enabled);

			AssertNoExceptionThrown("Parent (main) form should have closed with no exception.", () => control.AutoCloseParentForm());

			Assert("Auto close timer should have disabled (stopped).", control.autoCloseTimer == null);
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestCallingDisposeMultipleTimesDoesNotThrow()
		{
			var control = ShowLoginUserControl();
			control.Dispose();
			AssertNoExceptionThrown("Calling Dispose multiple times should be safe.", () => control.Dispose());
			await control.oidcLoginTask;
		}

		[RequiresSTA]
		public async Task TestChangingLastUserActivityTime()
		{
			var control = ShowLoginUserControl();
			DateTime baseTime = control.lastUserAcivityTime;
			DateTime newTime;

			Thread.Sleep(100);
			control.passwordTextBox.Text = "123";
			newTime = control.lastUserAcivityTime;
			Assert("lastUserAcivityTime should have changed.", baseTime != newTime);
			baseTime = newTime;

			Thread.Sleep(100);
			control.userNameTextbox.Text = "789";
			newTime = control.lastUserAcivityTime;
			Assert("lastUserAcivityTime should have changed.", baseTime != newTime);
			baseTime = newTime;

			Thread.Sleep(100);
			control.passwordTextBox.Focus();
			newTime = control.lastUserAcivityTime;
			Assert("lastUserAcivityTime should have changed.", baseTime != newTime);
			baseTime = newTime;

			Thread.Sleep(100);
			control.userNameTextbox.Focus();
			newTime = control.lastUserAcivityTime;
			Assert("lastUserAcivityTime should have changed.", baseTime != newTime);
			await control.oidcLoginTask;
		}

		#region Implementation

		LoginUserControl ShowLoginUserControl()
		{
			StartupOpenMainFormTask.MainFormInstance.ShowLoginUserControl();
			return FindLoginUserControl();
		}

		LoginUserControl FindLoginUserControl()
		{
			var controls = StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginUserControl", true);
			AssertEquals(1, controls.Length);
			AssertType(typeof(LoginUserControl), controls[0]);
			return (LoginUserControl)controls[0];
		}

		ZToolStripButton GetLoginButton(Control control)
		{
			var toolStrip = (ZToolStrip)control.Controls.Find("toolStrip", true)[0];
			return (ZToolStripButton)toolStrip.Items.Find("LoginButton", true)[0];
		}

		#endregion
	}
}

using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;
using Timer = System.Windows.Forms.Timer;

namespace Enterprise.Startup.Login
{
	partial class LoginUserControl : ZUserControl
	{
		public Task oidcLoginTask { get; private set; } = Task.CompletedTask;
		public LoginUserControl(bool isSSOWith2FA = false, bool isSupportLogin = false)
		{
			InitializeComponent();
			InitializeTimerForAutoClose();
			oidcLabel.Visible = false;
			LoadDefaultUsername();

			var oidcConfig = ObjectFactory.Get<IOIDCConfig>();
			if (oidcConfig.IsOIDCEnabled && !isSupportLogin)
			{
				oidcLabel.Visible = true;

				SetupLayoutForOIDCLogin();
				SetupCWSupportButtonIfRequired();
				SetupOIDCButtons();
				oidcLoginTask = oidcLogin();
				return;
			}
			else if (isSSOWith2FA)
			{
				passwordTextBox.Text = null; // so it authenticate as SSO
				SetupLayoutFor2FA();
			}
			else
			{
				SetupLayoutForLogin();
			}
			SetupLoginButton();
		}

		void SetupLayoutFor2FA()
		{
			userNameTextbox.Enabled = false;
			passwordTextBox.Enabled = false;
			errorLabel.Text = Res.GetString("CE5EB543-DA51-42A0-81F8-D49974A83508", "Please enter the validation code that has been sent to you.");
			Setup2FATextBox(true);
			toolStrip.Items.Insert(0, BackButton);
		}

		void SetupLayoutForLogin()
		{
			SetupBackColorAndPicture();
			Setup2FATextBox(false);
			SetupUserNameAndPasswordTextbox();
		}

		void SetupLayoutForOIDCLogin()
		{
			SetupBackColorAndPicture();
			Setup2FATextBox(false);

			oidcLabel.Show();
			userNameTextbox.Hide();
			passwordTextBox.Hide();
		}

		void SetupUserNameAndPasswordTextbox()
		{
			userNameTextbox.TextChanged += userNameTextbox_TextChanged;
			passwordTextBox.TextChanged += passwordTextBox_TextChanged;
			passwordTextBox.EnterPressedLeavingControl += passwordTextBox_NextOnReturn;
			userNameTextbox.Enter += userNameTextbox_Enter;
			passwordTextBox.Enter += passwordTextBox_Enter;
			passwordTextBox.TrackDisposedAccess = true;
		}

		void SetupBackColorAndPicture()
		{
			BackColor = SystemDataRegistry.Instance.ColorTheme.MainFormBackgroundColor;
			PictureBox.Image = new Bitmap(BrandingFactory.Instance.CompanyLogoButton);
		}

		void Setup2FATextBox(bool enable)
		{
			if (enable)
			{
				twoFactorAuthenticationTextBox.Enabled = true;
				twoFactorAuthenticationTextBox.Show();
			}
			else
			{
				twoFactorAuthenticationTextBox.Enabled = false;
				twoFactorAuthenticationTextBox.Hide();
			}
		}

		void SetupCWSupportButtonIfRequired()
		{
			if (EnvProxy.IsHostedWithCargowise || SystemDataRegistry.Instance.EnableSupportUserLogin.Value)
			{
				CWSupportLogin = new ZToolStripButton
				{
					Name = "SupportLoginButton",
					Image = Icons.GetImage(IconTypes.Login),
					ImageScaling = ToolStripItemImageScaling.SizeToFit,
					Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 100, 0),
					Text = Res.GetString("7FD91E3E-DE58-4818-A9CB-A21339CA93FC", "Support Login")
				};

				CWSupportLogin.Click += CWSupportLogin_Click;
				toolStrip.Items.Add(CWSupportLogin);
			}
		}

		void SetupOIDCButtons()
		{
			oidcCancelButton = new ZToolStripButton
			{
				Name = "CancelButton",
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				Text = Res.GetString("39BDE059-BC6A-4B22-B0B9-6F7B60000AE7", "Cancel")
			};

			oidcCancelButton.Click += oidcCancel_Click;

			oidcLoginButton = new ZToolStripButton
			{
				Name = "OidcLoginButton",
				Image = Icons.GetImage(IconTypes.Login),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				Text = Res.GetString("5b34bce1-4f5d-49ab-bc63-00b080c47190", "Login")
			};

			oidcLoginButton.Click += (_, _) => oidcLoginTask = oidcLogin();
			toolStrip.Items.Add(oidcCancelButton);
		}

		void ResetButtonsDuringOidcLoginEvents(ZToolStripButton addedButton)
		{
			toolStrip.Items.Clear();
			if (EnvProxy.IsHostedWithCargowise || SystemDataRegistry.Instance.EnableSupportUserLogin.Value)
			{
				toolStrip.Items.Add(CWSupportLogin);
			}
			toolStrip.Items.Add(addedButton);
		}

		void CWSupportLogin_Click(object sender, EventArgs e)
		{
			isInCWSupportLogin = true;
			oidcCancel_Click(null, null);
			SetupUserNameAndPasswordTextbox();
			oidcLabel.Hide();
			userNameTextbox.Show();
			passwordTextBox.Show();
			toolStrip.Items.Clear();
			SetupLoginButton();
		}

		ZToolStripButton oidcCancelButton;
		ZToolStripButton oidcLoginButton;
		ZToolStripButton CWSupportLogin;
		bool isInCWSupportLogin;

		void oidcCancel_Click(object sender, EventArgs e)
		{
			oidcCancellationTokenSource?.Cancel();
			oidcCancellationTokenSource = null;
		}

		async Task oidcLogin()
		{
			LogLastUserActivity();
			errorLabel.Text = string.Empty;
			oidcLabel.Text = Res.GetString("1B4CB8A6-4B14-4F6F-8BC3-0E6807E9ED3A", "Please login via the new tab that has opened in your web browser and return to the application when completed.");
			oidcCancellationTokenSource?.Cancel();
			oidcCancellationTokenSource = new CancellationTokenSource();
			ResetButtonsDuringOidcLoginEvents(oidcCancelButton);

			var oidcConfig = ObjectFactory.Get<IOIDCConfig>();
			var authenticationInfo = await Task.Run(() => OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, (url) => WebUrlLauncher.Launch(url), oidcCancellationTokenSource.Token));
			if (!authenticationInfo.LoginValidated)
			{
				oidcLabel.Text = authenticationInfo.FailureMessage;
				errorLabel.Text = authenticationInfo.ExtendedErrorInformation;

				if (!isInCWSupportLogin)
				{
					ResetButtonsDuringOidcLoginEvents(oidcLoginButton);
				}
			}
			else
			{
				bool loggedInLocation;

				authenticationInfo.TakeThreadOwnershipForUser();
				LoginDirector.Instance.AuthenticatedUser = authenticationInfo;
				if (!string.IsNullOrEmpty(LoginDirector.Instance.ForceBranch) || !string.IsNullOrEmpty(LoginDirector.Instance.ForceDepartment))
				{
					loggedInLocation = Env.LoginController.LoginLocation(authenticationInfo, LoginDirector.Instance.ForceBranch, LoginDirector.Instance.ForceDepartment);
				}
				else
				{
					loggedInLocation = Env.LoginController.LoginLocationAutomatically(authenticationInfo);
				}

				if (loggedInLocation)
				{
					MainForm.InitializeAfterLogin();
				}
				else
				{
					StartupOpenMainFormTask.MainFormInstance.ShowLoginLocationControl();
				}

				LoginNameRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authenticationInfo.User.LoginName);
				oidcLabel.Visible = false;
			}
		}
		CancellationTokenSource oidcCancellationTokenSource;

		void SetupLoginButton()
		{
			var loginButton = new ZToolStripButton
			{
				Name = "LoginButton",
				Image = Icons.GetImage(IconTypes.Login),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				Text = Res.GetString("5b34bce1-4f5d-49ab-bc63-00b080c47190", "Login")
			};

			loginButton.Click += loginButton_Click;
			toolStrip.Items.Add(loginButton);
		}

		internal ZToolStripButton BackButton
		{
			get
			{
				if (backButton == null)
				{
					backButton = new ZToolStripButton
					{
						Name = "BackButton",
						Image = Icons.GetImage(IconTypes.BackButtonActive),
						ImageScaling = ToolStripItemImageScaling.SizeToFit,
						Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
						Text = Res.GetString("c96c0fdc-733e-4197-9f0d-3f3653bd02e3", "Back")
					};
				}
				backButton.Click += BackButton_Click;
				return backButton;
			}
		}
		ZToolStripButton backButton;

		internal ZToolStripButton TempPasswordButton
		{
			get
			{
				if (tempPasswordButton == null)
				{
					tempPasswordButton = new ZToolStripButton
					{
						Name = "TempPasswordButton",
						Image = Icons.GetImage(IconTypes.EmailPassword16x16),
						ImageScaling = ToolStripItemImageScaling.SizeToFit,
						Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
						Text = Res.GetString("F875AF69-A865-417B-B85A-658D703CBD5C", "Email Temp. Password"),
					};
					tempPasswordButton.Click += TempPasswordButton_Click;
				}
				return tempPasswordButton;
			}
		}
		ZToolStripButton tempPasswordButton;

		void LoadDefaultUsername()
		{
			if (Env.CurrentUser != null)
			{
				userNameTextbox.Text = Env.CurrentUser.LoginName;
			}
			else
			{
				var preferredLoginName = LoginNameRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(preferredLoginName))
				{
					preferredLoginName = LoginNameRegistryItem_Old.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				if (string.IsNullOrEmpty(preferredLoginName))
				{
					preferredLoginName = CurrentWindowsUserName;
				}
				userNameTextbox.Text = preferredLoginName;
			}
		}

		void BackButton_Click(object sender, EventArgs e)
		{
			userNameTextbox.Enabled = true;
			passwordTextBox.Enabled = true;
			passwordTextBox.Text = "";
			errorLabel.Text = "";
			twoFactorAuthenticationTextBox.Enabled = false;
			twoFactorAuthenticationTextBox.ResetText();
			twoFactorAuthenticationTextBox.Hide();
			toolStrip.Items.Remove(BackButton);
		}

		void TempPasswordButton_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(userNameTextbox.Text))
			{
				errorLabel.Text = LoginDirector.Instance.EmailTemporaryPassword(userNameTextbox.Text);
			}
		}

		void loginButton_Click(object sender, EventArgs e)
		{
			LogLastUserActivity();

			if (!CanUserLoginIfOIDCEnabled(userNameTextbox.Text))
			{
				errorLabel.Text = Res.GetString("173A37E1-66FB-4844-BBFF-061B0DBD55AB", "Please login as a support user.");
				return;
			}

			var twoFactorAuthenticationCode = twoFactorAuthenticationTextBox.Enabled ? twoFactorAuthenticationTextBox.Text : null;

			var authenticatedUser = LoginDirector.Instance.LoginUserInteractive(userNameTextbox.Text, passwordTextBox.Text, twoFactorAuthenticationCode, true);
			if (authenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordRequired)
			{
				toolStrip.Items.Insert(0, TempPasswordButton);
				if (string.IsNullOrEmpty(authenticatedUser.FailureMessage))
				{
					errorLabel.Text = Res.GetString("75108A34-C38F-4816-86D1-F490BD30EE44", "Your password must be reset. Click the '{0}' button to send a temporary password to your email address.", tempPasswordButton.Text);
				}
				else
				{
					errorLabel.Text = authenticatedUser.FailureMessage;
				}
			}
			else if (authenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordLoginSuccessfully)
			{
				if (LoginDirector.Instance.ResetLocalPassword(authenticatedUser.User.PK))
				{
					toolStrip.Items.Remove(TempPasswordButton);
					passwordTextBox.ResetText();
					errorLabel.Text = Res.GetString("D67D141C-D9AB-4918-96A2-5D2330A89103", "Your password has been successfully reset. Please login with your new password.");
				}
			}
			else if (!authenticatedUser.LoginValidated)
			{
				errorLabel.Text = authenticatedUser.FailureMessage;
			}
			else
			{
				if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired)
				{
					SetupLayoutFor2FA();
				}
				else if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFailed)
				{
					errorLabel.Text = Res.GetString("F971E43C-8C9E-4766-AC75-D863AA49966A", "The validation code you entered is incorrect. Please try again.");
					twoFactorAuthenticationTextBox.ResetText();
				}
				else if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFieldMissing)
				{
					var twoFactorAuthType = SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					errorLabel.Text = Res.GetString("23996FCC-584A-4DED-80D1-91FC5B2F3F16", "The validation code could not be sent as your account has no {0} set up. Please contact your system administrator for assistance.", twoFactorAuthType.ToLower(CultureInfo.CurrentCulture));
				}
				else
				{
					LoginNameRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, authenticatedUser.User.LoginName);
				}
			}
		}

		bool CanUserLoginIfOIDCEnabled(string loginName)
		{
			var oidcConfig = ObjectFactory.Get<IOIDCConfig>();

			if (oidcConfig.IsOIDCEnabled && !string.Equals(loginName, User.SupportUserName, StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			return true;
		}

		StringRegistryItem LoginNameRegistryItem
		{
			get { return new StringRegistryItem("LoginName-" + CurrentWindowsUserGuidOrName, null, null, null, RegistryStorageFlags.System); }
		}

		StringRegistryItem LoginNameRegistryItem_Old
		{
			get { return new StringRegistryItem("LoginName-" + CurrentWindowsUserName, null, null, null, RegistryStorageFlags.System); }
		}

		string CurrentWindowsUserGuidOrName
		{
			get
			{
				var guid = ObjectFactory.Get<IWindowsUserAuthenticationProvider>().CurrentWindowsUserGuid;
				if (guid != Guid.Empty)
				{
					return guid.ToString();
				}
				return ObjectFactory.Get<IWindowsUserAuthenticationProvider>().CurrentWindowsUsername;
			}
		}

		string CurrentWindowsUserName
		{
			get { return ObjectFactory.Get<IWindowsUserAuthenticationProvider>().CurrentWindowsUsername; }
		}

		void userNameTextbox_TextChanged(object sender, EventArgs e)
		{
			LogLastUserActivity();
			ClearErrorMessage();
			toolStrip.Items.Remove(TempPasswordButton);
		}

		void passwordTextBox_TextChanged(object sender, EventArgs e)
		{
			LogLastUserActivity();
			ClearErrorMessage();
		}

		void ClearErrorMessage()
		{
			if (!string.IsNullOrEmpty(errorLabel.Text))
			{
				errorLabel.Text = string.Empty;
			}
		}

		void passwordTextBox_NextOnReturn(object sender, EventArgs e)
		{
			LogLastUserActivity();
			loginButton_Click(sender, e);
		}

		void userNameTextbox_Enter(object sender, EventArgs e)
		{
			LogLastUserActivity();
			userNameTextbox.SelectAll();
		}

		void passwordTextBox_Enter(object sender, EventArgs e)
		{
			LogLastUserActivity();
			passwordTextBox.SelectAll();
		}

		void ADLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LogLastUserActivity();
			WebUrlLauncher.Launch("http://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/ediEnterpriseupdatenote20130628.pdf");
		}

		#region Auto-Close of CargowiseOne

		internal Timer autoCloseTimer;
		internal DateTime lastUserAcivityTime;
		static readonly TimeSpan timeoutForAutoClose = new TimeSpan(0, 10, 0);

		void InitializeTimerForAutoClose()
		{
			lastUserAcivityTime = DateTime.Now;

			autoCloseTimer = new Timer();
			autoCloseTimer.Interval = 5000;
			autoCloseTimer.Tick += AutoCloseTimer_Tick;
			autoCloseTimer.Enabled = true;
			autoCloseTimer.Start();

			Disposed += LoginUserControl_Disposed;
		}

		void LogLastUserActivity()
		{
			lastUserAcivityTime = DateTime.Now;
		}

		internal void AutoCloseParentForm()
		{
			if ((autoCloseTimer != null) && (autoCloseTimer.Enabled))
			{
				autoCloseTimer.Enabled = false;
				autoCloseTimer.Stop();
			}

			var parentForm = ParentForm ?? StartupOpenMainFormTask.MainFormInstance;

			parentForm?.Close();
		}

		void AutoCloseTimer_Tick(object sender, EventArgs e)
		{
			if ((autoCloseTimer != null) && (autoCloseTimer.Enabled) && (DateTime.Now - lastUserAcivityTime) >= timeoutForAutoClose)
			{
				AutoCloseParentForm();
			}
		}

		void LoginUserControl_Disposed(object sender, EventArgs e)
		{
			if (autoCloseTimer != null)
			{
				if (autoCloseTimer.Enabled)
				{
					autoCloseTimer.Enabled = false;
					autoCloseTimer.Stop();
				}

				autoCloseTimer.Dispose();
				autoCloseTimer = null;
			}

			backButton?.Dispose();
			backButton = null;

			tempPasswordButton?.Dispose();
			tempPasswordButton = null;
		}

		#endregion // Auto-Close of CargowiseOne
	}
}

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.Login;

public class NextLoginViewModel : INextLoginViewModel
{
	public event PropertyChangedEventHandler PropertyChanged;

	readonly ILoginService loginService;

	public NextLoginViewModel(ILoginService loginService)
	{
		this.loginService = loginService;

		IsOIDCLogin = loginService.IsOIDCEnabled;

		if (IsOIDCLogin)
		{
			SetOidcLoginStartView();
		}
		else
		{
			SetDefaultLoginStartView();
		}
	}

	#region AutoCloseTimer
#if WINZOR
	public void AutoCloseTimerStart(TimeSpan interval)
	{
	}

	public void AutoCloseTimerRestart()
	{
	}

	public void AutoCloseTimerStop()
	{
	}
#else
	readonly System.Windows.Threading.DispatcherTimer timer = new ();

	public void AutoCloseTimerStart(TimeSpan interval)
	{
		timer.Stop();
		timer.Interval = interval;
		timer.Tick += AutoCloseTick;
		timer.Start();
	}

	public void AutoCloseTimerRestart()
	{
		if (timer.IsEnabled)
		{
			timer.Stop();
			timer.Start();
		}
	}
	public void AutoCloseTimerStop() => timer.Stop();

	void AutoCloseTick(object sender, EventArgs e)
	{
		timer.Stop();
		loginService.CloseApplication();
	}
#endif
	#endregion

	#region Labels
	public MultilingualString Title => ResString.GetMultilingualString("CWNext|Login|287cb29d-a726-4fc4-bc9f-d9a12e616628", "Login to CargoWise Next");

	public MultilingualString UsernameLabel => ResString.GetMultilingualString("CWNext|Login|b8d777f0-2d2d-416e-a036-78c34ac6f2f8", "Username");

	public MultilingualString PasswordLabel => ResString.GetMultilingualString("CWNext|Login|caf88de1-21a5-4fcf-9ea0-df9cf7f7f2f0", "Password");

	public MultilingualString VerificationCodeLabel => ResString.GetMultilingualString("CWNext|Login|195095cd-f699-485d-88df-f17b53262430", "Verification Code");

	public MultilingualString LoginButtonCaption => ResString.GetMultilingualString("CWNext|Login|2d8b7791-85fb-4496-9db6-04b00901c206", "Login");

	public MultilingualString SupportLoginButtonCaption => ResString.GetMultilingualString("CWNext|Login|4d8b7791-85fb-4496-9db6-04b00901c206", "Support Login");

	public MultilingualString TemporaryLoginButtonCaption => ResString.GetMultilingualString("CWNext|Login|3d8b7791-85fb-4496-9db6-04b00901c206", "Email Temporary Password");

	public MultilingualString BackButtonCaption => ResString.GetMultilingualString("CWNext|Login|3d8b7791-01fb-4496-9db6-04b00901c206", "Back");

	public MultilingualString CancelButtonCaption => ResString.GetMultilingualString("CWNext|Login|3d8b7791-02fb-4496-9db6-04b00901c206", "Cancel");

	public MultilingualString OIDCLoginButtonCaption => ResString.GetMultilingualString("CWNext|Login|3d8b7791-03fb-4496-9db6-04b00901c206", "Login with SSO");
	#endregion

	#region Properties
	string username;
	public string Username
	{
		get => username;
		set
		{
			if (username != value)
			{
				username = value;
				NotifyPropertyChanged();
			}
		}
	}

	SecureString password;
	public SecureString Password
	{
		get => password;
		set
		{
			password = value;
			NotifyPropertyChanged();
		}
	}

	string verificationCode;
	public string VerificationCode
	{
		get => verificationCode;
		set
		{
			if (verificationCode != value)
			{
				verificationCode = value;
				NotifyPropertyChanged();
			}
		}
	}

	string errorMessage = string.Empty;
	public string ErrorMessage
	{
		get => errorMessage;
		private set
		{
			errorMessage = value;
			NotifyPropertyChanged();
		}
	}

	string warningMessage = string.Empty;
	public string WarningMessage
	{
		get => warningMessage;
		private set
		{
			warningMessage = value;
			NotifyPropertyChanged();
		}
	}

	string infoMessage = string.Empty;
	public string InfoMessage
	{
		get => infoMessage;
		private set
		{
			infoMessage = value;
			NotifyPropertyChanged();
		}
	}

	bool isTwoFactorAuthentication;
	public bool IsTwoFactorAuthentication
	{
		get => isTwoFactorAuthentication;
		set
		{
			if (isTwoFactorAuthentication != value)
			{
				isTwoFactorAuthentication = value;
				NotifyPropertyChanged();
			}
		}
	}

	bool isTemporaryLoginRequired;
	public bool IsTemporaryLoginRequired
	{
		get => isTemporaryLoginRequired;
		set
		{
			if (isTemporaryLoginRequired != value)
			{
				isTemporaryLoginRequired = value;
				NotifyPropertyChanged();
			}
		}
	}

	bool isOIDCLogin;
	public bool IsOIDCLogin
	{
		get => isOIDCLogin;
		private set
		{
			isOIDCLogin = value;
			NotifyPropertyChanged();
		}
	}

	bool isUserPasswordLogin;
	public bool IsUserPasswordLogin
	{
		get => isUserPasswordLogin;
		private set
		{
			isUserPasswordLogin = value;
			NotifyPropertyChanged();
		}
	}

	public bool IsSupportLoginEnabled => IsOIDCLogin && loginService.IsSupportLoginEnabled;

	bool isCancelButtonVisible;
	public bool IsCancelButtonVisible
	{
		get => isCancelButtonVisible;
		set
		{
			isCancelButtonVisible = value;
			NotifyPropertyChanged();
		}
	}

	bool isOIDCButtonVisible;
	public bool IsOIDCButtonVisible
	{
		get => isOIDCButtonVisible;
		set
		{
			isOIDCButtonVisible = value;
			NotifyPropertyChanged();
		}
	}

	bool isSupportLoginButtonVisible;
	public bool IsSupportLoginButtonVisible
	{
		get => isSupportLoginButtonVisible;
		set
		{
			isSupportLoginButtonVisible = value;
			NotifyPropertyChanged();
		}
	}

	bool isBackButtonVisible;
	public bool IsBackButtonVisible
	{
		get => isBackButtonVisible;
		set
		{
			isBackButtonVisible = value;
			NotifyPropertyChanged();
		}
	}

	bool isLoginButtonVisible;
	public bool IsLoginButtonVisible
	{
		get => isLoginButtonVisible;
		set
		{
			isLoginButtonVisible = value;
			NotifyPropertyChanged();
		}
	}
	#endregion

	public void Login()
	{
		ClearMessages();

		if (IsOIDCLogin && !loginService.IsSupport(Username))
		{
			ErrorMessage = Res.GetString("CWNext|Login|173A37E1-66FB-4844-BBFF-061B0DBD55AB", "Please login as a support user.");
			return;
		}

		var twoFactorCode = !string.IsNullOrEmpty(VerificationCode) ? VerificationCode : null;
		var authenticatedUser = loginService.LoginUserInteractive(Username, Password.ToInsecureString(), twoFactorCode);
		if (authenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordRequired)
		{
			SetDefaultLoginWaitingTemporaryPasswordView();
			if (!string.IsNullOrEmpty(authenticatedUser.FailureMessage))
			{
				ErrorMessage = authenticatedUser.FailureMessage;
				return;
			}

			return;
		}

		if (authenticatedUser.State == LoginAuthenticationInfo.Status.TempPasswordLoginSuccessfully)
		{
			if (loginService.ResetLocalPassword(authenticatedUser.User.PK))
			{
				IsTemporaryLoginRequired = false;
				Password = new SecureString();
				ErrorMessage = Res.GetString("CWNext|Login|D67D141C-D9AB-4918-96A2-5D2330A89103", "Your password has been successfully reset. Please login with your new password.");
			}
			return;
		}

		if (!authenticatedUser.LoginValidated)
		{
			if (string.IsNullOrEmpty(authenticatedUser.FailureMessage))
			{
				ErrorMessage = Res.GetString("CWNext|Login|ABCD141C-D9AB-4918-96A2-5D2330A89103", "You must enter correct user name and/or password. Please try again.");
				return;
			}

			ErrorMessage = authenticatedUser.FailureMessage;
			return;
		}

		if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired)
		{
			SetDefaultLoginWaiting2FAView();
			return;
		}

		if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFailed)
		{
			ErrorMessage = Res.GetString("CWNext|Login|F971E43C-8C9E-4766-AC75-D863AA49966A", "The validation code you entered is incorrect. Please try again.");
			VerificationCode = string.Empty;
			return;
		}

		if (authenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationFieldMissing)
		{
			var twoFactorAuthType = loginService.TwoFactorAuthenticationType?.ToLower(CultureInfo.CurrentCulture);
			ErrorMessage = Res.GetString("CWNext|Login|23996FCC-584A-4DED-80D1-91FC5B2F3F16", "The validation code could not be sent as your account has no {0} set up. Please contact your system administrator for assistance.", twoFactorAuthType);
			return;
		}

		loginService.SetSession(authenticatedUser.User.LoginName);

		ClearControls();
	}

	CancellationTokenSource oidcTokenSource;
	public async void OIDCLogin()
	{
		SetOidcLoginWaitingView();

		oidcTokenSource?.Cancel();
		oidcTokenSource = new CancellationTokenSource();

		try
		{
			var authenticationInfo = await loginService.LoginOIDC(oidcTokenSource.Token);
			if (!authenticationInfo.LoginValidated)
			{
				SetOidcLoginStartView();
				ErrorMessage = string.IsNullOrEmpty(authenticationInfo.FailureMessage)
					? authenticationInfo.ExtendedErrorInformation
					: authenticationInfo.FailureMessage;
				return;
			}

			loginService.SetOIDCAuthenticatedUser(authenticationInfo);
			loginService.SetSession(authenticationInfo.User.LoginName);

			ClearControls();
		}
		catch(Exception ex)
		{
			SetOidcLoginStartView();
			ErrorMessage = ex.Message;
		}
	}

	public void CancelOIDCLogin()
	{
		oidcTokenSource?.Cancel();
		oidcTokenSource = null;

		SetOidcLoginStartView();
	}

	public void SupportLogin()
	{
		oidcTokenSource?.Cancel();
		oidcTokenSource = null;

		SetOidcLoginSupportView();
	}

	public void GoBack()
	{
		SetOidcLoginStartView();
	}

	public void TemporaryLogin()
	{
		ClearMessages();

		if (!string.IsNullOrEmpty(Username))
		{
			InfoMessage = loginService.EmailTemporaryPassword(Username);
		}
	}

	void ClearMessages()
	{
		ErrorMessage = string.Empty;
		WarningMessage = string.Empty;
		InfoMessage = string.Empty;
	}

	void ClearControls()
	{
		Username = string.Empty;
		Password = new SecureString();
		VerificationCode = string.Empty;
	}

	void HideButtons()
	{
		IsOIDCButtonVisible = false;
		IsLoginButtonVisible = false;
		IsCancelButtonVisible = false;
		IsBackButtonVisible = false;
		IsSupportLoginButtonVisible = false;
		IsTemporaryLoginRequired = false;
	}

	void NotifyPropertyChanged([CallerMemberName] string property = null)
	{
		if (!string.IsNullOrEmpty(property))
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}

	void SetOidcLoginStartView()
	{
		ClearControls();
		ClearMessages();
		HideButtons();

		IsUserPasswordLogin = false;
		IsOIDCButtonVisible = true;
		IsSupportLoginButtonVisible = loginService.IsSupportLoginEnabled;
		WarningMessage = Res.GetString("CWNext|Login|6e8b7799-85fb-4496-9db6-04b00901c206", "Authentication operation was canceled, please try again.");
	}

	void SetOidcLoginWaitingView()
	{
		ClearControls();
		ClearMessages();
		HideButtons();

		IsUserPasswordLogin = false;
		IsCancelButtonVisible = true;
		IsSupportLoginButtonVisible = loginService.IsSupportLoginEnabled;
		InfoMessage = Res.GetString("CWNext|Login|5d8b7799-85fb-4496-9db6-04b00901c206", "Please login via the new tab that has opened in your web browser and return to the application when completed.");
	}

	void SetOidcLoginSupportView()
	{
		ClearControls();
		ClearMessages();
		HideButtons();

		Username = loginService.SupportUsername;
		IsUserPasswordLogin = true;
		IsBackButtonVisible = true;
		IsLoginButtonVisible = true;
	}

	void SetDefaultLoginStartView()
	{
		ClearControls();
		ClearMessages();
		HideButtons();

		Username = loginService.CurrentUsername;
		IsUserPasswordLogin = true;
		IsLoginButtonVisible = true;
	}

	void SetDefaultLoginWaiting2FAView()
	{
		ClearMessages();
		HideButtons();

		IsUserPasswordLogin = true;
		IsLoginButtonVisible = true;
		IsTwoFactorAuthentication = true;
		VerificationCode = string.Empty;
		ErrorMessage = Res.GetString("CE5EB543-DA51-42A0-81F8-D49974A83508", "Please enter the validation code that has been sent to you.");
	}

	void SetDefaultLoginWaitingTemporaryPasswordView()
	{
		ClearMessages();
		HideButtons();

		IsUserPasswordLogin = true;
		IsLoginButtonVisible = true;
		IsTemporaryLoginRequired = true;
		IsTwoFactorAuthentication = false;
		ErrorMessage = Res.GetString("75108A34-C38F-4816-86D1-F490BD30EE44", "Your password must be reset. Click the '{0}' button to send a temporary password to your email address.", TemporaryLoginButtonCaption);
	}
}

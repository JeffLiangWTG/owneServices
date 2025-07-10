using System;
using System.ComponentModel;
using System.Security;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.Login;

public interface INextLoginViewModel : INotifyPropertyChanged
{
	MultilingualString Title { get; }
	MultilingualString UsernameLabel { get; }
	MultilingualString PasswordLabel { get; }
	MultilingualString VerificationCodeLabel { get; }
	MultilingualString BackButtonCaption { get; }
	MultilingualString CancelButtonCaption { get; }
	MultilingualString OIDCLoginButtonCaption { get; }
	MultilingualString LoginButtonCaption { get; }
	MultilingualString SupportLoginButtonCaption { get; }
	MultilingualString TemporaryLoginButtonCaption { get; }
	SecureString Password { get; set; }
	string Username { get; set; }
	string VerificationCode { get; set; }
	string ErrorMessage { get; }
	string WarningMessage { get; }
	string InfoMessage { get; }
	bool IsOIDCLogin { get; }
	bool IsSupportLoginEnabled { get; }
	bool IsTemporaryLoginRequired { get; }
	bool IsTwoFactorAuthentication { get; }
	bool IsUserPasswordLogin { get; }
	bool IsCancelButtonVisible { get; }
	bool IsOIDCButtonVisible { get; }
	bool IsSupportLoginButtonVisible { get; }
	bool IsBackButtonVisible { get; }
	bool IsLoginButtonVisible { get; }
	void Login();
	void SupportLogin();
	void OIDCLogin();
	void TemporaryLogin();
	void CancelOIDCLogin();
	void GoBack();

	void AutoCloseTimerStart(TimeSpan interval);
	void AutoCloseTimerRestart();
	void AutoCloseTimerStop();
}

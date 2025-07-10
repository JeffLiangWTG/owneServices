using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Login;

public interface ILoginService
{
	string CurrentUsername { get; }
	string TwoFactorAuthenticationType {  get; }
	string SupportUsername { get; }
	string EmailTemporaryPassword(string username);
	bool IsOIDCEnabled { get; }
	bool IsSupportLoginEnabled { get; }
	bool IsTwoFactorAuthenticationRequired { get; }

	bool IsSupport(string username);
	bool ResetLocalPassword(Guid staffPK);
	void SetSession(string username);
	void ShowLoginUserControl();
	LoginAuthenticationInfo LoginUserInteractive(string username, string password, string twoFactorAuthenticationCode = null);
	string LoginLocationInteractive();

	ICompany Company { get; set; }
	ObservableCollection<ICompany> Companies { get; }

	IBranch Branch { get; set; }
	ObservableCollection<IBranch> Branches { get; }

	IDepartment Department { get; set; }
	ObservableCollection<IDepartment> Departments { get; }
	Task<LoginAuthenticationInfo> LoginOIDC(CancellationToken token);
	void SetOIDCAuthenticatedUser(LoginAuthenticationInfo authenticationInfo);
	void CloseApplication();
}

class LoginService : ILoginService
{
	public string CurrentUsername
	{
		get
		{
			if (!string.IsNullOrEmpty(Env.CurrentUser?.LoginName))
			{
				return Env.CurrentUser.LoginName;
			}

			var username = GetRegistryItem(WindowsUserId).GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (!string.IsNullOrEmpty(username))
			{
				return username;
			}

			var authProvider = ObjectFactory.Get<IWindowsUserAuthenticationProvider>();
			username = GetRegistryItem(authProvider.CurrentWindowsUsername).GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (!string.IsNullOrEmpty(username))
			{
				return username;
			}

			return authProvider.CurrentWindowsUsername;
		}
	}

	public string TwoFactorAuthenticationType => SystemDataRegistry
		.Instance
		.TwoFactorAuthenticationTypes
		.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
	public string SupportUsername => User.SupportUserName;
	public bool IsOIDCEnabled => ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled;

	public bool IsSupportLoginEnabled => EnvProxy.IsHostedWithCargowise || SystemDataRegistry.Instance.EnableSupportUserLogin.Value;

	public bool IsTwoFactorAuthenticationRequired => LoginDirector.Instance.AuthenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired;

	public bool IsSupport(string username)
	{
		return string.Equals(SupportUsername, username, StringComparison.InvariantCultureIgnoreCase);
	}

	public void SetSession(string username)
	{
		var registryItem = GetRegistryItem(WindowsUserId);
		registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, username);
	}

	public void ShowLoginUserControl()
	{
		var mainForm = StartupOpenMainFormTask.MainFormInstance;
		if (mainForm != null)
		{
#if WINZOR
				if (ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
				{
					mainForm.ReLaunchWinzorAppServer(WTG.OpenIDConnect.Login.OIDCLoginRequestMessage.LoginPrompt.Login);
					return;
				}
#endif
			mainForm.UnloadLoginLocationControl();
			mainForm.ShowLoginUserControl();
		}
	}

	public LoginAuthenticationInfo LoginUserInteractive(string username, string password, string twoFactorAuthCode = null)
	{
		return LoginDirector.Instance.LoginUserInteractive(username, password, twoFactorAuthCode, support2FA: true);
	}

	public string LoginLocationInteractive()
	{
		LoginLocationBizo.Validation.ValidateAll();
		if (LoginLocationBizo.HasErrors)
		{
			return new ZNotificationCollector(LoginLocationBizo, false, true, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString();
		}
		var loginInfo = LoginDirector.Instance.LoginLocationInteractive(LoginLocationBizo.BranchCode, LoginLocationBizo.DepartmentCode);

		if (!loginInfo.IsOK)
		{
			return loginInfo.FailureMessage;
		}

		Enterprise.Startup.MainForm.InitializeAfterLogin();

		return string.Empty;
	}

	public bool ResetLocalPassword(Guid staffPK) => LoginDirector.Instance.ResetLocalPassword(staffPK);

	public string EmailTemporaryPassword(string username) => LoginDirector.Instance.EmailTemporaryPassword(username);

	public ICompany Company
	{
		get => LoginLocationBizo.Company;
		set
		{
			LoginLocationBizo.CompanyCode = value?.Code ?? string.Empty;
		}
	}
	public ObservableCollection<ICompany> Companies => new ObservableCollection<ICompany>(LoginLocationBizo.Companies);

	public IBranch Branch
	{
		get => LoginLocationBizo.Branch;
		set
		{
			LoginLocationBizo.BranchCode = value?.Code ?? string.Empty;
		}
	}
	public ObservableCollection<IBranch> Branches => new ObservableCollection<IBranch>(LoginLocationBizo.Branches);

	public IDepartment Department
	{
		get => LoginLocationBizo.Department;
		set
		{
			LoginLocationBizo.DepartmentCode = value?.Code ?? string.Empty;
		}
	}
	public ObservableCollection<IDepartment> Departments => new ObservableCollection<IDepartment>(LoginLocationBizo.Departments);

	LoginLocationBusinessObjectForMainForm loginLocationBizo;
	LoginLocationBusinessObjectForMainForm LoginLocationBizo => loginLocationBizo ??= new LoginLocationBusinessObjectForMainForm(new BusinessObjectFactory());

	static string WindowsUserId
	{
		get
		{
			var authProvider = ObjectFactory.Get<IWindowsUserAuthenticationProvider>();
			var winUserGuid = authProvider.CurrentWindowsUserGuid;

			return winUserGuid == Guid.Empty
				? authProvider.CurrentWindowsUsername
				: winUserGuid.ToString();
		}
	}

	static StringRegistryItem GetRegistryItem(string username) => new(
		$"LoginName-{username}",
		category: null,
		caption: null,
		hint: null,
		storage: RegistryStorageFlags.System);

	public Task<LoginAuthenticationInfo> LoginOIDC(CancellationToken token)
	{
		return Task.Factory.StartNew(() =>
		{
			var oidcConfig = ObjectFactory.Get<IOIDCConfig>();
			return OIDCUserLogin.PromptUserLoginAndClearRefreshToken(oidcConfig, WebUrlLauncher.Launch, token);
		});
	}

	public void SetOIDCAuthenticatedUser(LoginAuthenticationInfo authInfo)
	{
		authInfo.TakeThreadOwnershipForUser();
		LoginDirector.Instance.AuthenticatedUser = authInfo;

		bool loggedInLocation;
		if (!string.IsNullOrEmpty(LoginDirector.Instance.ForceBranch) || !string.IsNullOrEmpty(LoginDirector.Instance.ForceDepartment))
		{
			loggedInLocation = Env.LoginController.LoginLocation(authInfo, LoginDirector.Instance.ForceBranch, LoginDirector.Instance.ForceDepartment);
		}
		else
		{
			loggedInLocation = Env.LoginController.LoginLocationAutomatically(authInfo);
		}

		if (loggedInLocation)
		{
			Enterprise.Startup.MainForm.InitializeAfterLogin();
		}
		else
		{
			StartupOpenMainFormTask.MainFormInstance.ShowLoginLocationControl();
		}
	}

	public void CloseApplication() => StartupOpenMainFormTask.MainFormInstance?.Close();
}

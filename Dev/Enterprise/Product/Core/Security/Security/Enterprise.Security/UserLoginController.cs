using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public class UserLoginController : IUserLoginController
	{
		protected Lazy<Dictionary<Guid, string>> twoFactorAuthenticationCodeTable = new Lazy<Dictionary<Guid, string>>(() => new Dictionary<Guid, string>());

		protected TemporaryPassword TemporaryPasswordCache;

#if  DEBUG
		internal virtual
#endif
		ClientHook GetClientHook()
		{
			return ClientHookLoader.Instance.ClientHook;
		}

		public string GenerateAndSendTemporaryPassword(string loginName)
		{
			var staff = GlbStaff.LoadFromLoginName(new BusinessObjectFactory(), loginName);
			if (staff == null)
			{
				return string.Empty; // just play dumb - we don't have to tell the user if the login name can returns a valid staff or not
			}
			else
			{
				TemporaryPasswordCache = TemporaryPassword.CreateAndSendTemporaryPassword(staff);
				if (TemporaryPasswordCache != null)
				{
					return Res.GetString("955EC20B-F28E-4662-9F43-51664C95B76E", "A temporary password has been sent to your email address.");
				}
				else
				{
					return Res.GetString("D8E247E4-47D7-4F22-B42C-BE02C17A536F", "Your Staff record does not have a valid email address or your email address is used on multiple Staff records. Please contact your system admin to reset your password.");
				}
			}
		}

		public LoginAuthenticationInfo LoginUser(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false, string language = null)
		{
			using (Res.TemporarilySwitchLanguage(language ?? Res.CurrentLanguage))
			{
				isForcedLogin = false;
				var result = ValidateUserLoginAndPassword(loginName, password, isDeviceUser, twoFactorAuthenticationCode, activeDirectoryObjectGuid, support2FA);
				if (result.LoginValidated)
				{
					ClearLockout(result.User);
				}
				return result;
			}
		}

		public LoginAuthenticationInfo LoginUserSingleSignOn(string twoFactorAuthenticationCode = null, bool support2FA = false)
		{
			if (ADRegistry.IsIntegrationEnabled && ADRegistry.IsSingleSignOnEnabled)
			{
				return LoginUser(null, null, twoFactorAuthenticationCode: twoFactorAuthenticationCode, activeDirectoryObjectGuid: WindowsAuthenticationProvider.CurrentWindowsUserGuid, support2FA: support2FA);
			}
			else
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, null);
			}
		}

#if DEBUG
		public LoginAuthenticationInfo LoginUserDeveloper()
		{
			IUser staff = GlbStaff.LoadFromLoginName(new BusinessObjectFactory(), User.SupportUserName);
			if (staff == null)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, $"{User.SupportUserName} is not found.");
			}

			var loginToken = new LoginToken() { IsDeveloper = true, LoggedInWithSupportToken = true, SupportTokenUserCode = "abc", ForcedRemoteLogoff = false, SupportTokenUserName = "Developer" };
			staff.LoginToken = loginToken;
			var result = LoginAuthenticationInfo.NewSuccessfulLogin(staff);
			ClearLockout(result.User);
			return result;
		}
#endif

		public bool LoginLocation(LoginAuthenticationInfo authenticatedUser, string branchCode, string departmentCode)
		{
			var result = LoginLocationEx(authenticatedUser, branchCode, departmentCode);
			return result.IsOK;
		}

		public LoginAuthenticationInfo LoginLocationEx(LoginAuthenticationInfo authenticatedUser, string branchCode, string departmentCode)
		{
			LoginAuthenticationInfo result;
			if (authenticatedUser.LoginValidated)
			{
				var branchPk = ZGuid.Empty;
				var staff = (GlbStaff)authenticatedUser.User;
				if (string.IsNullOrEmpty(branchCode) && !staff.GS_GB_LastLogonBranch.IsEmpty)
				{
					branchPk = staff.GS_GB_LastLogonBranch;
				}
				else if (string.IsNullOrEmpty(branchCode) && !staff.GS_GB_HomeBranch.IsEmpty)
				{
					branchPk = staff.GS_GB_HomeBranch;
				}
				else
				{
					var branch = staff.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
					if (branch != null)
					{
						branchPk = branch.PK;
					}
				}
				if (branchPk.IsEmpty)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.BranchNotFound,
						Res.GetString("8f50c65f-0973-4d57-86b0-2f7b99b794fc", "There is no '{0}' branch in the system.", branchCode));
				}
				else
				{
					var departmentPk = ZGuid.Empty;
					if (string.IsNullOrEmpty(departmentCode) && !staff.GS_GE_LastLogonDepartment.IsEmpty)
					{
						departmentPk = staff.GS_GE_LastLogonDepartment;
					}
					else if (string.IsNullOrEmpty(departmentCode) && !staff.GS_GE_HomeDepartment.IsEmpty)
					{
						departmentPk = staff.GS_GE_HomeDepartment;
					}
					else
					{
						var department = staff.Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
						if (department != null)
						{
							departmentPk = department.PK;
						}
					}
					if (departmentPk.IsEmpty)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DepartmentNotFound,
							Res.GetString("8f50c65f-0973-4d57-86b0-2f7b99b794fc", "There is no '{0}' branch in the system.", branchCode));
					}
					else
					{
						result = LoginLocationEx(authenticatedUser, branchPk.ToGuid(), departmentPk.ToGuid());
					}
				}
			}
			else
			{
				result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotAuthenticated, LoginFailMsg);
			}

			return result;
		}

		public bool LoginLocation(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk)
		{
			var info = LoginLocationEx(authenticatedUser, branchPk, departmentPk);
			return info.IsOK;
		}

#if DEBUG
		public bool LoginLocation(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk, GlbStaff staff)
		{
			var info = LoginLocationEx(authenticatedUser, branchPk, departmentPk);
			staff?.Reload();
			return info.IsOK;
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Actual user login here")]
		public virtual LoginAuthenticationInfo LoginLocationEx(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk)
		{
			if (branchPk == Guid.Empty)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.BranchNotFound, Res.GetString("7ea4e7f3-c9c0-4b3c-b4d9-89b2f8a5568f", "No branch found for login."));
			}

			var wasForcedLogin = isForcedLogin;
			isForcedLogin = false;

			LoginAuthenticationInfo result;

			if (authenticatedUser.LoginValidated)
			{
				var user = authenticatedUser.User;
				var token = (LoginToken)user.LoginToken;

				if (token != null)
				{
					token.ForcedRemoteLogoff = wasForcedLogin;
				}

				coreLicenceComponent?.LicensedComponentManager.Dispose();

				var userContext = new UserContext(user, branchPk, departmentPk, loginAuthenticationInfo: authenticatedUser);
				if (userContext.Branch == null)
				{
					return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.BranchNotFound, Res.GetString("7ea4e7f3-c9c0-4b3c-b4d9-89b2f8a5568f", "No branch found for login."));
				}
				ValidateCountrySpecificDatabases(userContext);

				var resultInactive = ValidateInactive(authenticatedUser, branchPk, departmentPk);

				if (resultInactive != null)
				{
					result = resultInactive;
				}
				else if (ObjectFactory.Get<IProductRegistration>().LocalVerify() == ProductRegistrationVerifyResult.Unregistered && CannotLoginToUnregisteredSystem(user))
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.OperationalUserCannotLoginToUnregisteredSystem, OperationalAndUnregisteredMsg);
				}
				else if (!userContext.LoginSecurityCheckpoint.IsAllowed)
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.SecurityFailure, SecurityFailMsg);
				}
				else
				{
					var remoteLoginInfo = !user.IsSystemAccount ? UserLoginValidation.GetLoginOnAnotherMachine(user.PK) : null;
					if (remoteLoginInfo != null)
					{
						result = LoginAuthenticationInfo.NewFailedLogin(remoteLoginInfo, UserLoginValidation.OneMachinePerUserLimitExceededMsg);
					}
					else if (userContext.Company != null && userContext.Licence.Core.Login(CoreLicenceComponent, userContext.Company.IsDemoCompany) != LicenceLoginResponse.Denied)
					{
						Env.SetUserContext(userContext);
						GetNewActiveLoginSemaphore();
						LogLoginTime(Env.Time.CurrentLocalDateTime);

						if (!Env.Instance.IsWeb && !Env.CurrentUser.IsSystemAccount)
						{
							UpdateLastLoginBranchDepartment();
						}

						result = LoginAuthenticationInfo.NewSuccessfulLogin(user);
					}
					else
					{
						result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.LicenceFailure, userContext.Licence.Core.LastReasonForNotAllowing);
					}
				}
			}
			else
			{
				result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotAuthenticated, LoginFailMsg);
			}

			return result;
		}

#if DEBUG
		public LoginAuthenticationInfo LoginLocationEx(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk, GlbStaff staff)
		{
			var toReturn = LoginLocationEx(authenticatedUser, branchPk, departmentPk);
			staff?.Reload();
			return toReturn;
		}
#endif

		LoginAuthenticationInfo ValidateInactive(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk)
		{
			var staff = (GlbStaff)authenticatedUser.User;

			var branch = staff.Factory.Load<GlbBranch>(branchPk);
			if (branch != null)
			{
				if (!branch.GB_IsActive)
				{
					return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.BranchNotFound,
						Res.GetString("0839ECA6-BF5F-4891-960D-F3698739C286", "The branch '{0}' is inactive.", branch.GB_Code));
				}
				else if (!branch.Company.GC_IsActive)
				{
					return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.BranchNotFound,
						Res.GetString("F4FFB6E4-FD2C-4BC3-8449-07124E559E3B", "The company '{0}' is inactive.", branch.Company.GC_Code));
				}
			}

			var department = staff.Factory.Load<GlbDepartment>(departmentPk);
			if (department != null && !department.GE_IsActive)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DepartmentNotFound,
					Res.GetString("360D2BA0-E833-4741-825D-AE7AA9E2C9D5", "The department '{0}' is inactive.", department.GE_Code));
			}

			return null;
		}

		static bool CannotLoginToUnregisteredSystem(IUser user)
		{
#if DEBUG
			// For debug builds there is no restriction unless during unit tests
			if (!Globals.IsTest)
			{
				return false;
			}
#endif
			return user.IsOperational && !user.IsSystemAccount && !user.IsSupportUser;
		}

		public bool LoginLocationAutomatically(LoginAuthenticationInfo authenticatedUser)
		{
			var result = LoginLocationExAutomatically(authenticatedUser);
			return result.IsOK;
		}

		public LoginAuthenticationInfo LoginLocationExAutomatically(LoginAuthenticationInfo authenticatedUser)
		{
			LoginAuthenticationInfo loginExResult = null;
			var staff = (GlbStaff)authenticatedUser.User;
			var clientHook = GetClientHook();
			if (authenticatedUser.User.IsSystemAccount && clientHook is { HasCompanySpecificOverrides: true })
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DisallowedSysAdminLogon, UserLoginValidation.InvalidSysadminLogonMsg);
			}

			if (!staff.GS_GB_LastLogonBranch.IsEmpty && !staff.GS_GE_LastLogonDepartment.IsEmpty)
			{
				loginExResult = LoginLocationEx(authenticatedUser, staff.GS_GB_LastLogonBranch.ToGuid(), staff.GS_GE_LastLogonDepartment.ToGuid());
				if (loginExResult.IsOK)
				{
					return loginExResult;
				}
			}

			if (!staff.GS_GB_HomeBranch.IsEmpty && !staff.GS_GE_HomeDepartment.IsEmpty)
			{
				loginExResult = LoginLocationEx(authenticatedUser, staff.GS_GB_HomeBranch.ToGuid(), staff.GS_GE_HomeDepartment.ToGuid());
				if (loginExResult.IsOK)
				{
					return loginExResult;
				}
			}

			if (authenticatedUser.User.IsSystemAccount)
			{
				var branch = GetAnyBranch(staff.Factory);
				var department = GetAnyDepartment(staff.Factory);
				if (branch != Guid.Empty && department != Guid.Empty)
				{
					return LoginLocationEx(authenticatedUser, branch, department);
				}
			}

			if (loginExResult != null)
			{
				return loginExResult;
			}

			return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, ResString.GetMultilingualString(
				"D43CD1EB-B5B2-499F-B168-E676C616B5BC",
				"No user record could be found that matched the retrieved identity information. Please contact your system administrator."));
		}

#if DEBUG
		internal bool Login(string loginName, string password, Guid branchPk, Guid departmentPk, GlbStaff staff = null, bool isDeviceUser = false, Guid? activeDirectoryObjectGuid = null, bool support2FA = false)
		{
			var loginAuthenticationInfo = LoginUser(loginName, password, isDeviceUser, activeDirectoryObjectGuid: activeDirectoryObjectGuid, support2FA: support2FA);
			var toReturn = LoginLocation(loginAuthenticationInfo, branchPk, departmentPk);
			staff?.Reload();
			return toReturn;
		}
#endif

		protected ILicensedComponent CoreLicenceComponent
		{
			get { return coreLicenceComponent ?? (coreLicenceComponent = new CoreLicensedComponent()); }
		}
		CoreLicensedComponent coreLicenceComponent;

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "we're doing logout")]
		public void Logout()
		{
			LogLogoutTime(Env.Time.CurrentLocalDateTime);
			Env.Licence.Core.Logout(CoreLicenceComponent);
			var userContext = new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK, loginAuthenticationInfo: null);
			Env.SetUserContext(userContext);
			DisposeActiveLoginSemaphore();
		}

		/// <summary>
		/// Indicates if Change Password dialog needs to be displayed after logging in.
		/// </summary>
		/// <param name="mustChange">Indicates if password change is compulsory</param>
		/// <returns>OK: Show Change Password Dialog, No: Don't show dialog, Cancel: End application</returns>
		public ZDialogResult PromptPasswordChange(out bool mustChange, string userName = null)
		{
			mustChange = true;
			var message = "";

			var factory = NewFactory();
			var staff = userName != null
				? GlbStaff.LoadFromLoginName(factory, userName)
				: factory.Load<GlbStaff>(Env.CurrentUser.PK);
			if (staff != null)
			{
				message = CheckPasswordIsExpired(staff);

				if (!string.IsNullOrEmpty(message))
				{
					return Globals.Message.Show(message, BrandingFactory.Instance.ProductName, staff.AllowPasswordChange ? ZMessageBoxButtons.OKCancel : ZMessageBoxButtons.OK, ZDialogResult.OK);
				}
				else if (!staff.ShouldUseADPasswordPolicy && !staff.PasswordNeverChanges && Env.Registry.PasswordChangeDays > 0 && Env.Registry.PromptPasswordChangeBeforeExpireDays > 0)
				{
					var lastChange = staff.LastPasswordChangeDate.Date.ToDateTime();
					var expireDay = lastChange.AddDays(Env.Registry.PasswordChangeDays);
					var dueInDays = expireDay.Subtract(Env.Time.CurrentLocalDate).Days;

					if (dueInDays <= Env.Registry.PromptPasswordChangeBeforeExpireDays)
					{
						mustChange = false;
						return ShowPasswordExpireWarning(dueInDays);
					}
				}
			}

			return ZDialogResult.No;
		}

		public string CheckPasswordIsExpired(GlbStaff staff)
		{
			var message = string.Empty;
			if (staff != null)
			{
				if (PasswordExpired(staff))
				{
					if (staff.AllowPasswordChange)
					{
						message = Res.GetString("dbea97a3-b06e-4d90-9b02-a8bea299f06f", "You are using an interim or expired password.\r\nYou are required to change your password.");
					}
					else
					{
						message = Res.GetString("56005D9A-3474-414B-B8A5-0FB79C65EA95", "You are using an interim or expired password.\r\nPlease change your Active Directory account password before trying to login again.");
					}
				}
			}
			return message;
		}

		/// <summary>
		/// Authenticates a user login/password without actually logging in to the application.
		/// Used to run processes that require a valid user before the Login.
		/// Will lock out the user if the password is invalid for three times in  a row.
		/// </summary>
		public LoginAuthenticationInfo ValidateUserLoginAndPassword(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false, byte[] loginHash = null, string loginNonce = "")
		{
			var userLogin = CreateUserLoginValidation(loginName, password, twoFactorAuthenticationCode, twoFactorAuthenticationCodeTable.Value, activeDirectoryObjectGuid, support2FA);
			var result = userLogin.Validate(isDeviceUser, TemporaryPasswordCache);
			if (result.State == LoginAuthenticationInfo.Status.PasswordInvalid)
			{
				if (!CheckFailedLoginAttempts(userLogin.User, out var errorMessage))
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.PasswordInvalid, errorMessage);
				}
			}

			return result;
		}

		public LoginAuthenticationInfo ValidateUserLoginAndPasswordAndExpiredPassword(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, bool support2FA = false)
		{
			var result = ValidateUserLoginAndPassword(loginName, password, isDeviceUser, twoFactorAuthenticationCode, support2FA: support2FA);
			if (result.LoginValidated)
			{
				var staff = NewFactory().LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, loginName);
				var errorMessage = CheckPasswordIsExpired(staff);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					result = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.PasswordExpired, errorMessage);
				}
			}

			return result;
		}

		public SecurityCore GetSecurityForUser(string loginName, Guid branchPK, Guid departmentPK)
		{
			var factory = NewFactory();
			GlbStaff staff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, loginName));
			GlbBranch branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, branchPK));

			var branchGB_GC_Guid = branch == null ? Guid.Empty : branch.GB_GC.ToGuid();
			var staffPK_Guid = staff == null ? (object)null : staff.PK.ToGuid();

			IZGlbSecurityCollection glbSecurityCollection = null;
			if (staff != null)
			{
				glbSecurityCollection = Env.Security.SecurityInstance.UserPK == staff.PK ? Env.Security.SecurityInstance.GlbSecurityCollection : null;
			}

			return new SecurityCore(glbSecurityCollection, staffPK_Guid, branchPK, departmentPK, branchGB_GC_Guid);
		}

		/// <summary>
		/// Validates the login for upgrade purposes (without actually logging in to the application):
		///	  1. validates the credentials
		///	  2. checks if it is a controller
		///	  3. registers the login used for the upgrade
		///	 Called before the DB is upgraded. Cannot use BizObj or anything which references the latest schema.
		/// </summary>
		public bool ValidateAndRegisterUserForUpgrade(string loginName, string loginPwd, out string errorMessage)
		{
			bool result = false;
			errorMessage = string.Empty;
			var userLogin = CreateUserLoginValidation(loginName, loginPwd);
			var validateResult = userLogin.ValidateForUpgrade();
			if (validateResult == UserLoginValidation.ValidateForUpgradeResult.NotController)
			{
				errorMessage = Res.GetString("938189be-e4ca-4b48-a4cf-dc4ea2589f89", "System Controller permission is required.");
			}
			else if (validateResult == UserLoginValidation.ValidateForUpgradeResult.OK)
			{
				result = true;
				UpgradeLogonName = loginName;
				UpgradeLogonStaffCode = userLogin.StaffCodeForUpgrade;
			}
			else
			{
				errorMessage = LoginFailMsg;
			}

			return result;
		}

		public string UpgradeLogonName { get; private set; }
		public string UpgradeLogonStaffCode { get; private set; }

		#region Implementation

		UserLoginValidation CreateUserLoginValidation(string loginName, string password, string twoFactorAuthenticationCode = null, Dictionary<Guid, string> codeTable = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false)
		{
			var userLogin = new UserLoginValidation(loginName, password, twoFactorAuthenticationCode, codeTable, activeDirectoryObjectGuid, support2FA);
			if (windowsAuthenticationProvider != null)
			{
				userLogin.WindowsAuthenticationProvider = windowsAuthenticationProvider;
			}
			if (adIntegrationModeProvider != null)
			{
				userLogin.ADRegistry = adIntegrationModeProvider;
			}
			return userLogin;
		}

		string LoginFailMsg { get { return UserLoginValidation.LoginFailMsg; } }
		string SecurityFailMsg { get { return Res.GetString("349ae7b1-050b-4d16-9088-b52c7092df13", "You do not have the security right to login to this branch or department.\r\nYou may ask your system administrator to grant you this security right.\r\n\r\nThis particular security right is called 'Login Branches and Departments', and is found at the bottom of the Security Tree View on the Staff or Group form."); } }
		string OperationalAndUnregisteredMsg { get { return Res.GetString("F7BE6667-B1CB-41BC-A128-4D1911FACB6C", "This installation is not registered. Only non-operational users, such as sysadmin, can login."); } }

		Dictionary<string, int> failedLoginAttempts;

		#region Semaphore

		bool GetNewActiveLoginSemaphore()
		{
			DisposeActiveLoginSemaphore();
			ActiveLoginSemaphore = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(Globals.IsWinzor ? new EnterpriseWinzorActiveLoginSemaphore() : new EnterpriseActiveLoginSemaphore());
			return ActiveLoginSemaphore.Success;
		}

		void DisposeActiveLoginSemaphore()
		{
			if (ActiveLoginSemaphore != null)
			{
				ActiveLoginSemaphore.Dispose();
				ActiveLoginSemaphore = null;
			}
		}

		ISemaphoreHandle ActiveLoginSemaphore
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && Enterprise.Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider != null)
				{
					return testActiveLoginSemaphore;
				}
#endif

				return activeLoginSemaphore;
			}
			set
			{
#if DEBUG
				if (Globals.IsTest && Enterprise.Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider != null)
				{
					testActiveLoginSemaphore = value;
					return;
				}
#endif

				activeLoginSemaphore = value;
			}
		}

		ISemaphoreHandle activeLoginSemaphore;

#if DEBUG
		ISemaphoreHandle testActiveLoginSemaphore;
#endif

		#endregion

		#region Used by PromptPasswordChange

		bool PasswordExpired(GlbStaff staff)
		{
			if (staff.ChangePasswordAtNextLogin)
			{
				return true;
			}

			if (staff.PasswordNeverChanges)
			{
				return false;
			}

			if (staff.ShouldUseADPasswordPolicy)
			{
				return staff.ADPasswordExpired;
			}
			else
			{
				if (Env.Registry.PasswordChangeDays <= 0)
				{
					return false;
				}

				if (!staff.LastPasswordChangeDate.IsEmpty && staff.LastPasswordChangeDate.IsValid)
				{
					DateTime lastChange = staff.LastPasswordChangeDate.ToDateTime().Date;
					return lastChange.AddDays(Env.Registry.PasswordChangeDays) < Env.Time.CurrentLocalDate.Date;
				}
				return true;
			}
		}

		ZDialogResult ShowPasswordExpireWarning(int dueInDays)
		{
			//Cannot show OK with No button. Show YesNo buttons and Convert Yes result to OK.
			string message = Res.GetString("3b3bab1b-77e1-4312-8e6e-6ae20012aeb9", "Your password is due to expire in {0} day(s).\r\nDo you want to change your password now?", dueInDays);
			var result = Globals.Message.Show(message, BrandingFactory.Instance.ProductName, ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Information, ZDialogResult.Yes);
			return result == ZDialogResult.Yes ? ZDialogResult.OK : result;
		}

		#endregion

		class CoreLicensedComponent : ILicensedComponent
		{
			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return fLicensedComponentManager ?? (fLicensedComponentManager = new LicensedComponentManager(this)); }
			}

			LicensedComponentManager fLicensedComponentManager;
		}

		public void ValidateCountrySpecificDatabases(UserContext userContext)
		{
			string countryCode = userContext.Company?.Country?.Code;
			if (!string.IsNullOrEmpty(countryCode))
			{
				try
				{
					if (countryCode == Enterprise.Core.Constants.CountryCodes.Canada ||
						countryCode == Enterprise.Core.Constants.CountryCodes.UnitedStates)
					{
						ValidateReferenceDatabaseSynonymExists(RefDbTypeEnum.Enterprise, countryCode);
					}

					if (countryCode == Enterprise.Core.Constants.CountryCodes.Australia ||
						countryCode == Enterprise.Core.Constants.CountryCodes.Canada ||
						countryCode == Enterprise.Core.Constants.CountryCodes.NewZealand)
					{
						ValidateReferenceDatabaseSynonymExists(RefDbTypeEnum.Tariff, countryCode);
					}
				}
				catch (DatabaseMissingException dbException)
				{
					var currentUser = userContext.User;
					if (currentUser.IsController || currentUser.IsDeveloper || (currentUser is GlbStaff staff && staff.IsDatabaseDeveloper))
					{
						Globals.Message.Show(dbException.Message, BrandingFactory.Instance.ProductName, ZMessageBoxButtons.OK, ZMessageBoxIcon.Warning, ZDialogResult.OK);
					}
					else
					{
						throw;
					}
				}
			}
		}

		void ValidateReferenceDatabaseSynonymExists(RefDbTypeEnum refDbType, string countryCode)
		{
			var sqlText = string.Format(
				"IF exists(SELECT null FROM sys.synonyms WHERE name like '{0}%') SELECT 1 ELSE SELECT 0",
				RefDbTableNameResolver.GetRefDbSynonymPrefix(refDbType, countryCode));
			int synonymCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));

			if (synonymCount == 0)
			{
				throw new DatabaseMissingException(Res.GetString("C0A7B6A7-1167-4E75-A22D-5163821BB827", "Country/Region [{0}] requires the [{1}] reference database to be installed.\r\nTo correct this issue run Help > Database Administration > Recreate Database Synonyms.", countryCode, refDbType));
			}
		}

		internal void ClearLockout(IUser user)
		{
			if (failedLoginAttempts != null)
			{
				if (failedLoginAttempts.ContainsKey(user.LoginName))
				{
					failedLoginAttempts.Remove(user.LoginName);
				}
			}
			if (!Env.Instance.IsWeb)
			{
				UpdateUserData(user, (GlbPerson person) =>
				{
					if (person.PER_LoginDisabledUntilUtc.IsValid && !person.PER_LoginDisabledUntilUtc.IsEmpty)
					{
						person.PER_LoginDisabledUntilUtc = ZDateTime.Empty;
						return true;
					}
					return false;
				});
			}
		}

		public void ForceRemoteLogoff(ISemaphoreInfo remoteLoginSemaphoreInfo)
		{
			isForcedLogin = true;
			EnvProxy.Instance.SemaphoreProvider.RemoteLogoff(remoteLoginSemaphoreInfo.OwnerSession.UserPk, remoteLoginSemaphoreInfo.OwnerSession.HeartbeatType, Globals.ClientIdentifier);
		}

		protected bool isForcedLogin;

		bool CheckFailedLoginAttempts(IUser user, out string errorMessage)
		{
			bool result = true;
			errorMessage = "";

			if ((user is GlbStaff staff) && staff.ShouldUseADPasswordPolicy)
			{
				// For AD-integrated system, the lockout policy is managed by AD/domain.
				return true;
			}

			if (!user.IsController)
			{
				if (failedLoginAttempts == null)
				{
					failedLoginAttempts = new Dictionary<string, int>();
				}
				int attempts;
				failedLoginAttempts.TryGetValue(user.LoginName, out attempts);
				failedLoginAttempts[user.LoginName] = ++attempts;

				if (Env.Registry.LoginAttempts > 0 && attempts >= Env.Registry.LoginAttempts && !user.IsSysAdmin)
				{
					if (Env.Registry.LoginLockoutMinutes == 0)
					{
						UpdateUserData(user, (GlbPerson person) =>
						{
							person.PER_LoginDisabledUntilUtc = ZDateTime.MaxSmallDateTimeUtc;
							return true;
						});

						errorMessage = Res.GetString("11dcb16b-827d-44ca-954b-9302c40939dc", "Login Failed. Your login will be disabled.\r\nPlease see your System Administrator to have your login re-enabled.");
					}
					else
					{
						UpdateUserData(user, (GlbPerson person) =>
						{
							person.PER_LoginDisabledUntilUtc = ZDateTime.UtcNow.AddMinutes(Env.Registry.LoginLockoutMinutes);
							return true;
						});

						errorMessage = Res.GetString("19351a6e-ec7a-4a9a-8408-63cbf305f6cc", "Login Failed. Your login will be locked out temporarily.");
					}

					result = false;
				}
			}

			return result;
		}

#if DEBUG
		protected
#endif
 void LogLoginTime(DateTime localTime)
		{
			AuditLogin(localTime);
		}

#if DEBUG
		protected
#endif
 void LogLogoutTime(DateTime localTime)
		{
			AuditLogout(localTime);
		}

		void UpdateLastLoginBranchDepartment()
		{
			const string sql = @"
UPDATE dbo.GlbStaff SET
	GS_GB_LastLogonBranch = @GS_GB_LastLogonBranch,
	GS_GE_LastLogonDepartment = @GS_GE_LastLogonDepartment,
	GS_SystemLastEditTimeUtc = GetUtcDate(),
	GS_SystemLastEditUser = @GS_Code
WHERE
	GS_Code = @GS_Code";
			var command = Db.Connection.Command(sql);
			command.AddParameterBasedOnDbColumn("@GS_GB_LastLogonBranch", Env.CurrentBranch.PK, GlbStaffSchema.GS_GB_LastLogonBranch);
			command.AddParameterBasedOnDbColumn("@GS_GE_LastLogonDepartment", Env.CurrentDepartment.PK, GlbStaffSchema.GS_GE_LastLogonDepartment);
			command.AddParameterBasedOnDbColumn("@GS_Code", EnvProxy.Instance.CurrentUser.Initials, GlbStaffSchema.GS_Code);
			command.ExecuteNonQuery();
		}

		public IWindowsUserAuthenticationProvider WindowsAuthenticationProvider
		{
			get { return windowsAuthenticationProvider ?? (windowsAuthenticationProvider = ObjectFactory.Get<IWindowsUserAuthenticationProvider>()); }
			set { windowsAuthenticationProvider = value; }
		}
		IWindowsUserAuthenticationProvider windowsAuthenticationProvider;

		public IADRegistry ADRegistry
		{
			get { return adIntegrationModeProvider ?? (adIntegrationModeProvider = ObjectFactory.Get<IADRegistry>()); }
			set { adIntegrationModeProvider = value; }
		}
		IADRegistry adIntegrationModeProvider;

		#region Branch/Department Selection

#if DEBUG
		internal
#endif
		Guid GetAnyBranch(BusinessObjectFactory factory)
		{
			return EnvProxy.GetAnyBranch(factory);
		}

		Guid GetAnyDepartment(BusinessObjectFactory factory)
		{
			return EnvProxy.GetAnyDepartment(factory);
		}

		#endregion

		void AuditLogin(DateTime loginTime)
		{
			GetNewEventManager().AddAuditLogEvent("LGI", GlbStaffSchema.Constants.TableName, Env.CurrentUserPK, loginTime, GetLogInOutReference(true));
			UpdateLastActivityDate(loginTime);
		}

		void UpdateLastActivityDate(DateTime loginTime)
		{
			const string sql = @"
UPDATE dbo.GlbStaff	SET
	GS_LastActivityDate = @GS_LastActivityDate,
	GS_SystemLastEditTimeUtc = GetUtcDate(),
	GS_SystemLastEditUser = @GS_Code
WHERE
	GS_Code = @GS_Code";
			var command = Db.Connection.Command(sql);
			command.AddParameterBasedOnDbColumn("@GS_LastActivityDate", Env.Time.GetUtcFromLocalTime(loginTime), GlbStaffSchema.GS_LastActivityDate);
			command.AddParameterBasedOnDbColumn("@GS_Code", EnvProxy.Instance.CurrentUser.Initials, GlbStaffSchema.GS_Code);
			command.ExecuteNonQuery();
		}

		void AuditLogout(DateTime logoutTime)
		{
			GetNewEventManager().AddAuditLogEvent("LGO", GlbStaffSchema.Constants.TableName, Env.CurrentUserPK, logoutTime, GetLogInOutReference(false));
		}

		protected virtual EventManager GetNewEventManager()
		{
			return new EventManager();
		}

		protected string GetLogInOutReference(bool isLogin)
		{
			string supportTokenIncident = null;
			string supportTokenUserCode = null;
			string dummyTokenOriginalUserName = null;
			string dummyTokenOriginalUserEmail = null;
			bool wasForcedLogin = false;
			if (Env.CurrentUser != null)
			{
				var loginToken = Env.CurrentUser.LoginToken as LoginToken;
				if (loginToken != null)
				{
					supportTokenIncident = loginToken.SupportTokenIncident;
					supportTokenUserCode = loginToken.SupportTokenUserCode;
					wasForcedLogin = loginToken.ForcedRemoteLogoff;
					dummyTokenOriginalUserName = loginToken.DummyTokenOriginalUserName;
					dummyTokenOriginalUserEmail = loginToken.DummyTokenOriginalUserEmail;
				}
			}
			string result =
				(!string.IsNullOrEmpty(supportTokenIncident) ? supportTokenIncident + ", " : null) +
				(!string.IsNullOrEmpty(supportTokenUserCode) ? supportTokenUserCode + ", " : null) +
				(!string.IsNullOrEmpty(dummyTokenOriginalUserName) ? dummyTokenOriginalUserName + ", " : null) +
				(!string.IsNullOrEmpty(dummyTokenOriginalUserEmail) ? dummyTokenOriginalUserEmail + ", " : null) +
				(isLogin && wasForcedLogin ? "FORCED," : "") +
				System.Environment.MachineName +
				", " + Utilities.GetLocalIPAddress() +
				", " + System.Environment.UserName;

			var terminalService = ObjectFactory.Get<TerminalService>();
			if (terminalService.IsWTSSession)
			{
				result += ", " + terminalService.SessionClientName() + ", " + terminalService.SessionClientIP();
			}

			if (StmALogSchema.SL_Reference.MaxLength > 0 && result.Length > StmALogSchema.SL_Reference.MaxLength)
			{
				result = result.Substring(0, StmALogSchema.SL_Reference.MaxLength);
			}

			return result;
		}

		#endregion

		#region IUserLoginController Members

		ISecurityProxy IUserLoginController.GetSecurityForUser(string loginName, Guid branchPK, Guid departmentPK)
		{
			return GetSecurityForUser(loginName, branchPK, departmentPK);
		}

		public string VerboseLoginFilename { get; set; }

		#endregion

		#region Factory

		BusinessObjectFactory NewFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = "UserLoginController";
			return factory;
		}

		void UpdateUserData(IUser user, Func<GlbPerson, bool> action)
		{
			GlbStaff staff = (GlbStaff)user;
			staff.Reload();
			var person = staff.Person;
			if (person != null && action(person))
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(staff.Factory.Save, null, true);
			}
		}

		#region Test Stuff

		internal void UpdateUserDataExposedForTest(IUser user, Func<GlbPerson, bool> action)
		{
			UpdateUserData(user, action);
		}

		#endregion

		#endregion
	}
}

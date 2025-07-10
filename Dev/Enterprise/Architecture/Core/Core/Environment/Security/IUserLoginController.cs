using System;
using CargoWise.EntityFramework;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public interface IUserLoginController
	{
		LoginAuthenticationInfo LoginUser(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false, string language = null);
		LoginAuthenticationInfo LoginUserSingleSignOn(string twoFactorAuthenticationCode = null, bool support2FA = false);
#if DEBUG
		LoginAuthenticationInfo LoginUserDeveloper();
#endif

		LoginAuthenticationInfo LoginLocationEx(LoginAuthenticationInfo authenticatedUser, string branchCode, string departmentCode);
		LoginAuthenticationInfo LoginLocationEx(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk);

		bool LoginLocation(LoginAuthenticationInfo authenticatedUser, string branchCode, string departmentCode);
		bool LoginLocation(LoginAuthenticationInfo authenticatedUser, Guid branchPk, Guid departmentPk);
		bool LoginLocationAutomatically(LoginAuthenticationInfo authenticatedUser);
		LoginAuthenticationInfo LoginLocationExAutomatically(LoginAuthenticationInfo authenticatedUser);

		void Logout();
		ZDialogResult PromptPasswordChange(out bool mustChange, string userName = null);
		LoginAuthenticationInfo ValidateUserLoginAndPassword(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, Guid? activeDirectoryObjectGuid = null, bool support2FA = false, byte[] loginHash = null, string loginNonce = "");
		LoginAuthenticationInfo ValidateUserLoginAndPasswordAndExpiredPassword(string loginName, string password, bool isDeviceUser = false, string twoFactorAuthenticationCode = null, bool support2FA = false);
		ISecurityProxy GetSecurityForUser(string loginName, Guid branchPK, Guid departmentPK);
		bool ValidateAndRegisterUserForUpgrade(string loginName, string password, out string errorMessage);
		string UpgradeLogonName { get; }
		string UpgradeLogonStaffCode { get; }
		string VerboseLoginFilename { get; set; }
		void ForceRemoteLogoff(ISemaphoreInfo remoteLoginSemaphoreInfo);
		string GenerateAndSendTemporaryPassword(string loginName);
	}

	public class LoginAuthenticationInfo
	{
		public enum Status
		{
			Failure,
			OK,

			// User & password status
			UserNotFound,
			WebUserCannotLogin,
			UserInactive,
			UserLockedOut,
			PasswordInvalid,
			PasswordExpired,
			ADRecordNotFound,
			ADRecordInvalid,
			UserExistsInMultipleDomains,
			TempPasswordRequired,
			TempPasswordLoginSuccessfully,

			// IDP status
			UserNotInIdp,
			UserNotVerified,

			// Location status
			UserNotAuthenticated,
			BranchNotFound,
			DepartmentNotFound,
			SecurityFailure,
			OneMachinePerUserLimitExceeded,
			LicenceFailure,
			OperationalUserCannotLoginToUnregisteredSystem,
			DisallowedSysAdminLogon,

			ClientIPAddressRestricted,

			TwoFactorAuthenticationRequired,
			TwoFactorAuthenticationFailed,
			TwoFactorAuthenticationFieldMissing,
		}

		public static LoginAuthenticationInfo NewSuccessfulLogin(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.OK);
		}

		public static LoginAuthenticationInfo NewTempPasswordRequired(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.TempPasswordRequired);
		}

		public static LoginAuthenticationInfo NewTempPasswordLoginSuccessfully(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.TempPasswordLoginSuccessfully);
		}

		public static LoginAuthenticationInfo NewTwoFactorAuthenticationLogin(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.TwoFactorAuthenticationRequired);
		}

		public static LoginAuthenticationInfo NewTwoFactorAuthenticationFailedLogin(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.TwoFactorAuthenticationFailed);
		}

		public static LoginAuthenticationInfo NewTwoFactorAuthenticationFieldMissing(IUser user)
		{
			return new LoginAuthenticationInfo(user, Status.TwoFactorAuthenticationFieldMissing);
		}

		public static LoginAuthenticationInfo NewFailedLogin(Status status, string failureMessage, string extendedErrorInformation = null)
		{
			return new LoginAuthenticationInfo(null, status, failureMessage, null, extendedErrorInformation);
		}

		public static LoginAuthenticationInfo NewFailedLogin(string failureMessage, string extendedErrorInformation = null)
		{
			return new LoginAuthenticationInfo(null, Status.Failure, failureMessage, null, extendedErrorInformation);
		}

		public static LoginAuthenticationInfo NewFailedLogin(ISemaphoreInfo loginOnAnotherMachine, string failureMessage, string extendedErrorInformation = null)
		{
			return new LoginAuthenticationInfo(null, Status.OneMachinePerUserLimitExceeded, failureMessage, loginOnAnotherMachine, extendedErrorInformation);
		}

		public void TakeThreadOwnershipForUser()
		{
			if (User is IFactoryProvider factoryProvider)
			{
				factoryProvider.Factory.TakeThreadOwnership();
			}
		}

		LoginAuthenticationInfo(IUser user, Status status, string failureMessage = null, ISemaphoreInfo loginOnAnotherMachine = null, string extendedErrorInformation = null)
		{
			this.user = user;
			this.status = status;
			this.failureMessage = failureMessage;
			this.extendedErrorInformation = extendedErrorInformation;
			this.loginOnAnotherMachine = loginOnAnotherMachine;
		}

		public Status State { get { return status; } }
		public bool LoginValidated { get { return User != null; } }
		public IUser User { get { return user; } }
		public string FailureMessage { get { return failureMessage ?? string.Empty; } }
		public string ExtendedErrorInformation { get { return extendedErrorInformation ?? string.Empty; } }
		public bool IsOK { get { return status == Status.OK; } }
		public ISemaphoreInfo LoginOnAnotherMachine { get { return loginOnAnotherMachine; } }

		readonly IUser user;
		readonly Status status;
		readonly string failureMessage;
		readonly string extendedErrorInformation;
		readonly ISemaphoreInfo loginOnAnotherMachine;
	}
}

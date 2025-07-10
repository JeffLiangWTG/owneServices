
using System;

namespace Enterprise.Security.ActiveDirectory
{
	public enum ValidateCredentialsResult
	{
		OK,
		Invalid,
		PasswordExpired,
		ADRecordNotCreated,
		ADRecordNotFound,
		LockedOut,
		ADError
	}

	public interface IWindowsUserAuthenticationProvider
	{
		ValidateCredentialsResult ValidateCurrentUserSession(Guid activeDirectoryObjectGuid);
		ValidateCredentialsResult ValidateCredentials(Guid staffPK, string password);

		string CurrentWindowsUsername { get; }
		Guid CurrentWindowsUserGuid { get; }
	}
}

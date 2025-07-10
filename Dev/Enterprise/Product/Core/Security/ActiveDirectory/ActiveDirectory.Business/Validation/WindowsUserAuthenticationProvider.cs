using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Principal;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public class WindowsUserAuthenticationProvider : IWindowsUserAuthenticationProvider
	{
		public ValidateCredentialsResult ValidateCredentials(Guid staffPK, string password)
		{
			var staff = StaffForLogin.LoadByStaffPK(staffPK);
			try
			{
				if (staff == null)
				{
					return ValidateCredentialsResult.Invalid;
				}

				if (!staff.IsADLinked)
				{
					return ValidateCredentialsResult.ADRecordNotCreated;
				}

				try
				{
					var adUser = new ADUserForLogin(staff);
					var userDirectoryEntry = adUser.GetDirectoryEntry();
					return ValidateCredentials(adUser.DomainCredentials, userDirectoryEntry, password);
				}
				catch (NoDomainPrivilegeException)
				{
					// If the Domain User Credentials in the registry is incorrect/outdated, we use the login user's credentials as a fallback to avoid the system being locked out totally
					var domainCredentials = new DomainCredentials()
					{
						DomainName = staff.DomainName,
						DomainUserName = staff.LoginName,
						DomainUserPassword = password
					};
					return ValidateCredentials(domainCredentials, staff, password);
				}
			}
			catch (NoDomainPrivilegeException)
			{
				return ValidateCredentialsResult.Invalid;
			}
			catch (PasswordMustBeChangedException)
			{
				return ValidateCredentialsResult.PasswordExpired;
			}
			catch (UserLockedOutException)
			{
				return ValidateCredentialsResult.LockedOut;
			}
			catch (InvalidOperationException)
			{
				return ValidateCredentialsResult.Invalid;
			}
			catch (UnableToConnectToDomainException)
			{
				return ValidateCredentialsResult.ADError;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!DirectoryExceptionHandler.TryHandleDirectoryException(ex))
				{
					throw;
				}
				return ValidateCredentialsResult.Invalid;
			}
		}

		ValidateCredentialsResult ValidateCredentials(IDomainCredentials domainCredentials, IUserDirectoryEntry userDirectoryEntry, string password)
		{
			if (userDirectoryEntry == null)
			{
				// AD record does not exist or is located outside the OU
				return ValidateCredentialsResult.ADRecordNotFound;
			}

			var validator = GetValidationProvider(domainCredentials);
			return (validator != null && validator.ValidateCredentials(userDirectoryEntry.UserPrincipalName, password))
				? ValidateCredentialsResult.OK
				: ValidateCredentialsResult.Invalid;
		}

		ValidateCredentialsResult ValidateCredentials(IDomainCredentials domainCredentials, StaffForLogin staff, string password)
		{
			var directorySearcher = DirectorySearcherFactory.GetDirectorySearcher(domainCredentials);
			var userDirectoryEntry = directorySearcher.FindUser(staff.ActiveDirectoryObjectGuid.ToGuid());
			return ValidateCredentials(domainCredentials, userDirectoryEntry, password);
		}

		bool HasDirectoryEntryInTheRightOU(Guid activeDirectoryObjectGuid)
		{
			var staff = StaffForLogin.LoadByADObjectGuid(activeDirectoryObjectGuid);
			if (staff != null)
			{
				try
				{
					var adUser = new ADUserForLogin(staff);
					return adUser.GetDirectoryEntry() != null;
				}
				catch (NoDomainPrivilegeException)
				{
					// If the Domain User Credentials in the registry is incorrect/outdated, we skip this check to avoid the system being locked out totally.
					return true;
				}
			}
			return false;
		}

		public ValidateCredentialsResult ValidateCurrentUserSession(Guid activeDirectoryObjectGuid)
		{
			if (activeDirectoryObjectGuid == Guid.Empty)
			{
				return ValidateCredentialsResult.ADRecordNotCreated;
			}
			if (!HasDirectoryEntryInTheRightOU(activeDirectoryObjectGuid))
			{
				return ValidateCredentialsResult.ADRecordNotFound;
			}
			if (CurrentWindowsUserGuid != activeDirectoryObjectGuid)
			{
				// We deny users who attempt to log in with an account that is not the user who started the application.
				return ValidateCredentialsResult.Invalid;
			}
			if (IsUserLocalAccount && !IsOnDomainController)
			{
				// We deny users who are local accounts, except when they are on the domain controller since they
				// could have created a local user account with the same name as a valid domain user.
				return ValidateCredentialsResult.Invalid;
			}
			using (var identity = WindowsIdentity.GetCurrent())
			{
				return identity.IsAuthenticated ? ValidateCredentialsResult.OK : ValidateCredentialsResult.Invalid;
			}
		}

		public Guid CurrentWindowsUserGuid => GetCurrentWindowsUserGuid();

		protected virtual Guid GetCurrentWindowsUserGuid()
		{
			try
			{
				using (var principal = UserPrincipal.Current)
				{
					return principal.Guid.Value;
				}
			}
			catch (Exception ex) when (
				ex is InvalidOperationException || // The underlying store does not support this property.
				ex is PrincipalException ||
				ex is COMException || // Some unexpected issues occur from AD.
				!ex.IsCriticalException())
			{
				return Guid.Empty;
			}
		}

		public string CurrentWindowsUsername
		{
			get
			{
				try
				{
					using (var principal = UserPrincipal.Current) // Need to use UserPrincipal to ensure user principal name is used, not Win2K user name (which WindowsIdentity provides)
					{
						var upn = principal.UserPrincipalName;
						return upn != null ? SearcherFilter.UserNameComponent(upn) : "";
					}
				}
				catch (Exception ex) when (
					ex is InvalidOperationException || // The underlying store does not support this property.
					ex is PrincipalException ||
					ex is COMException || // Some unexpected issues occur from AD.
					!ex.IsCriticalException())
				{
					return System.Environment.UserName;
				}
			}
		}

		protected virtual bool IsUserLocalAccount => System.Environment.MachineName == System.Environment.UserDomainName;

		bool IsOnDomainController
		{
			get
			{
				try
				{
					return System.Environment.MachineName == Domain.GetCurrentDomain().Name;
				}
				catch (Exception ex) when (
					// This exception is only going to happen if the machine running the code is not actually logged into that domain
					ex is ActiveDirectoryOperationException ||
					ex is AuthenticationException)
				{
					return false;
				}
			}
		}

		protected virtual IValidationPrincipalContextProvider GetValidationProvider(IDomainCredentials domainCredentials)
		{
			if (validationProviderCaches.TryGetValue(domainCredentials?.DomainName, out var result))
			{
				return result;
			}
			else
			{
				result = new ValidationPrincipalContextProvider(DirectorySearcherFactory.GetDirectorySearcher(domainCredentials));
				validationProviderCaches.Add(domainCredentials?.DomainName, result);
				return result;
			}
		}
		readonly Dictionary<string, IValidationPrincipalContextProvider> validationProviderCaches = new Dictionary<string, IValidationPrincipalContextProvider>();
	}
}

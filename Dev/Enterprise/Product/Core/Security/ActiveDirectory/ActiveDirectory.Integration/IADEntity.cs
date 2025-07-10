using System;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADUser : IADEntity
	{
		bool PasswordExpired { get; }
		bool PasswordMustChangeAtNextLogon { get; set; }
		bool PasswordDoesntExpire { get; }
		bool PasswordDoesntExpireUserAttribute { get; set; }

		DateTime PasswordLastSet { get; }

		int GetNumberOfDaysTillPasswordExpiry();
		bool IsPasswordValid(string currentPassword);
		void SetPassword(string newPassword);
		void ChangePassword(string oldPassword, string newPassword);
		void UnlockAccount();
		string DomainNetBiosName { get; }
		string LoginName { get; }
		string SAMAccountName { get; }
		bool LockedOut { get; }
	}

	public interface IADEntity
	{
		Guid Guid { get; }
		ZPropertyInfo GuidPropertyInfo { get; }
		ZBool IsActive { get; set; }
		IADLinkedEntity EnterpriseEntity { get; }
		string EnterpriseIdentity { get; }
		bool IsIdentityInConflict();
		ZString DomainCredentialDomainName { get; }
		IDomainCredentials DomainCredentials { get; }
		IDirectoryEntry GetDirectoryEntry(bool requireDomainWritePrivilege = false);
		void SetDirectoryEntry(IDirectoryEntry directoryEntry);

		bool CommitChanges();
		bool Synchronise(SyncMode? preferredSyncMode = null);
		void DisconnectFromAD();

		event EntitySynchronisedEventHandler Synchronised;

		bool HasExistingDirectoryEntry();

		SyncDirection SyncDirection { get; }
		SyncMode SyncMode { get; }
	}
}

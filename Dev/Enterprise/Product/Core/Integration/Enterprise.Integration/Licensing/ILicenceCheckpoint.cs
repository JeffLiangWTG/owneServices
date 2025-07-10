using System;

namespace Enterprise.Integration.Licensing
{
	public interface ILicenceCheckpoint
	{
		string LastReasonForNotAllowing { get; }
		ModuleLicenceType LicenceType { get; }
		string Name { get; }
		string DisplayName { get; }
		ILicenceCheckpoint ParentCheckpoint { get; }
		LicenceLoginResponse Login(ILicensedComponent licensedComponent);
		void Logout(ILicensedComponent licensedComponent);

		ILicenceCheckpointUserContext ParentUserContext { get; }
	}

	public interface ILicenceCheckpointUserContext
	{
		Guid BranchPk { get; }
		string UserInitials { get; }
	}
}

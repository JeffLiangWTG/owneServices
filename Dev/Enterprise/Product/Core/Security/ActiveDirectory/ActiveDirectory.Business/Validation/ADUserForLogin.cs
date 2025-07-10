using System;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADUserForLogin : ADEntity<StaffForLogin, IUserDirectoryEntry>
	{
		public ADUserForLogin(StaffForLogin staff) : base(staff)
		{
		}

		public override string EnterpriseIdentity => EnterpriseEntity.LoginName;

		protected override DirectoryObjectType EntityType => DirectoryObjectType.User;

		public override ZPropertyInfo GuidPropertyInfo => EnterpriseEntity.ActiveDirectoryObjectGuidInfo;

		protected override IUserDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, Guid guid, string rootOU) => guid != Guid.Empty ? directorySearcher.FindUser(guid, rootOU) : null;

		protected override IUserDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, string enterpriseIdentity, string rootOU) => directorySearcher.FindUser(enterpriseIdentity, rootOU);

		protected override string OUPathForEntity(IDomainCredentials domainCredentials) => domainCredentials?.UserOrganisationalUnit ?? "";

		public override SyncDirection SyncDirection => throw new NotImplementedException();
		public override SyncMode SyncMode => throw new NotImplementedException();
	}
}

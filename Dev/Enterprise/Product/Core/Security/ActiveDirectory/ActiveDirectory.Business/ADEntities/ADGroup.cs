using System;
using System.Collections.Generic;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADGroup : ADEntity<GlbGroup, IGroupDirectoryEntry>
	{
		public ADGroup(GlbGroup group)
			: base(group)
		{
		}

		#region Properties

		[ResourceStringData("ADGroup.GroupName", Caption = "Group Name")]
		public string GroupName
		{
			get { return (string)RequireDirectoryEntry().GetValue(GlbGroupSchema.GG_Desc); }
			set { RequireDirectoryEntry(true).Rename(value); }
		}

		public IEnumerable<IDirectoryEntry> Members
		{
			get { return members ?? (members = RequireDirectoryEntry().GetMembers()); }
		}
		IEnumerable<IDirectoryEntry> members;

		public void AddMember(ADUser member)
		{
			var directoryEntry = member.GetDirectoryEntry();
			if (directoryEntry != null)
			{
				RequireDirectoryEntry(true).AddMember(directoryEntry);
			}
		}

		public void RemoveMember(IDirectoryEntry memberEntry)
		{
			if (memberEntry != null)
			{
				RequireDirectoryEntry(true).RemoveMember(memberEntry);
			}
		}

		#endregion

		#region ADEntity overrides

		public override SyncDirection SyncDirection => ActiveDirectoryRegistry.Instance.SyncDirectionGroup;

		protected override IGroupDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, Guid guid, string rootOU)
		{
			return guid != Guid.Empty ? directorySearcher.FindGroup(guid, rootOU) : null;
		}

		protected override IGroupDirectoryEntry FindDirectoryEntryCore(IDirectorySearcher directorySearcher, string enterpriseIdentity, string rootOU)
		{
			return directorySearcher.FindGroup(enterpriseIdentity, rootOU);
		}

		public override string EnterpriseIdentity
		{
			get { return (string)EnterpriseEntity.GG_Desc ?? string.Empty; }
		}

		public override ZPropertyInfo GuidPropertyInfo
		{
			get { return EnterpriseEntity.GG_ActiveDirectoryObjectGuidInfo; }
		}

		protected override string OUPathForEntity(IDomainCredentials domainCredentials) => domainCredentials?.GroupOrganisationalUnit ?? "";

		protected override DirectoryObjectType EntityType
		{
			get { return DirectoryObjectType.Group; }
		}

		#endregion
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	abstract class GroupSyncStrategyBase : SyncStrategyBase<ADGroup, GlbGroup>
	{
		protected GroupSyncStrategyBase(IADEntity adEntity)
			: base(adEntity)
		{
		}

		#region AD master

		protected void SyncWithADAsMaster(ADGroup adGroup)
		{
			Argument.NotNull(adGroup, "adGroup");

			SetValue(g => g.GroupName, EnterpriseEntity.GG_Desc, EnterpriseEntity.GG_DescInfo, g => EnterpriseEntity.GG_Desc = g.GroupName);
			SetDomainName();
			SetIsLinked();
			SyncActiveStatus();
			if (adGroup.GetDirectoryEntry() != null)
			{
				SyncADGroupMembershipWithEnterprise(adGroup);
			}
		}

		void SyncADGroupMembershipWithEnterprise(ADGroup adGroup)
		{
			var enterpriseMembers = GetLinkedEnterpriseMembers(adGroup.EnterpriseEntity);
			var membersToAddToEnterprise = GetMembersNotInEnterprise(adGroup);
			var membersToRemoveFromEnterprise = enterpriseMembers.Where(s => s.DomainName == adGroup.EnterpriseEntity.DomainName && !adGroup.Members.Any(d => d.Guid == s.GS_ActiveDirectoryObjectGuid)).ToArray();

			SyncGroupMembership(adGroup, membersToAddToEnterprise, membersToRemoveFromEnterprise, null, null);
		}

		#endregion

		#region Enterprise master

		protected void SyncWithEnterpriseAsMaster(ADGroup adGroup, bool deactivateGroup = true)
		{
			Argument.NotNull(adGroup, "adGroup");

			SetValue(g => g.GroupName, EnterpriseEntity.GG_Desc, EnterpriseEntity.GG_DescInfo, g => g.GroupName = EnterpriseEntity.GG_Desc);
			SetDomainName();
			SetIsLinked();
			SyncActiveStatus(deactivateGroup);
			if (adGroup.GetDirectoryEntry(true) != null)
			{
				SyncEnterpriseGroupMembershipWithAD(adGroup);
			}
		}

		void SyncEnterpriseGroupMembershipWithAD(ADGroup adGroup)
		{
			var enterpriseMembers = GetLinkedEnterpriseMembers(adGroup.EnterpriseEntity);
			var membersToAddToAD = GetMembersNotInAD(enterpriseMembers, adGroup);
			var adMembersNotCurrentlyInEnterpriseGroup = adGroup.Members.Where(d => !enterpriseMembers.Any(s => s.GS_ActiveDirectoryObjectGuid == d.Guid));

			var trackedADMembersNotCurrentlyInEnterpriseGroup = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, adMembersNotCurrentlyInEnterpriseGroup.Select(d => d.Guid)));
			var membersToRemoveFromAD = adMembersNotCurrentlyInEnterpriseGroup.Where(d => trackedADMembersNotCurrentlyInEnterpriseGroup.Any(s => s.GS_ActiveDirectoryObjectGuid == d.Guid));

			SyncGroupMembership(adGroup, null, null, membersToAddToAD, membersToRemoveFromAD);
		}

		#endregion

		#region Group membership

		protected void SyncGroupMembership(ADGroup adGroup,
			IEnumerable<IDirectoryEntry> membersToAddToEnterprise,
			IEnumerable<GlbStaff> membersToRemoveFromEnterprise,
			IEnumerable<GlbStaff> membersToAddToAD,
			IEnumerable<IDirectoryEntry> membersToRemoveFromAD)
		{
			SetValue(g => GetADGroupMembers(g), GetEnterpriseGroupMembers(), null, g =>
			{
				if (membersToAddToEnterprise != null)
				{
					foreach (var memberToAdd in membersToAddToEnterprise.ToArray())
					{
						var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SearcherFilter.UserNameComponent((string)memberToAdd.GetValue(GlbStaffSchema.GS_LoginName))));
						if (staff != null && !EnterpriseEntity.Staff.Contains(staff))
						{
							EnterpriseEntity.Staff.Add(staff);
						}
					}
				}
				if (membersToRemoveFromEnterprise != null)
				{
					foreach (var memberToRemove in membersToRemoveFromEnterprise.ToArray())
					{
						if (EnterpriseEntity.Staff.Contains(memberToRemove))
						{
							EnterpriseEntity.Staff.Remove(memberToRemove);
						}
					}
				}
				if (membersToAddToAD != null)
				{
					foreach (var memberToAdd in membersToAddToAD.ToArray())
					{
						adGroup.AddMember(new ADUser(memberToAdd));
					}
				}
				if (membersToRemoveFromAD != null)
				{
					foreach (var memberToRemove in membersToRemoveFromAD.ToArray())
					{
						adGroup.RemoveMember(memberToRemove);
					}
				}

				return GetEnterpriseGroupMembers();
			});
		}

		#endregion

		#region Implementation

		protected void SyncActiveStatus(bool deactivateGroup = true)
		{
			SetValue(g => ZBool.True, EnterpriseEntity.GG_IsActive, EnterpriseEntity.GG_IsActiveInfo, u => EnterpriseEntity.GG_IsActive = ZBool.True, g => deactivateGroup ? DeactivateGroup(g) : DoNothing());
		}

		protected static IEnumerable<IDirectoryEntry> GetMembersNotInEnterprise(ADGroup adGroup)
		{
			Argument.NotNull(adGroup, "adGroup");

			var memberGuids = adGroup.Members
				.Select(d => d.Guid)
				.Where(g => adGroup.EnterpriseEntity.Staff.Cast<GlbStaff>().All(s => s.GS_ActiveDirectoryObjectGuid != g))
				.ToArray();

			var allMembersInEnterprise = adGroup.EnterpriseEntity.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_ActiveDirectoryObjectGuid, memberGuids));

			return adGroup.Members.Where(d => allMembersInEnterprise.Any(s => s.GS_ActiveDirectoryObjectGuid == d.Guid));
		}

		protected static IEnumerable<GlbStaff> GetMembersNotInAD(IEnumerable<GlbStaff> enterpriseMembers, ADGroup adGroup)
		{
			return enterpriseMembers.Where(s => s.DomainName == adGroup.EnterpriseEntity.DomainName && !adGroup.Members.Any(d => d.Guid == s.GS_ActiveDirectoryObjectGuid));
		}

		protected static IEnumerable<GlbStaff> GetLinkedEnterpriseMembers(GlbGroup group)
		{
			return group.Staff.Cast<GlbStaff>().Where(s => s.GS_ActiveDirectoryObjectGuid.IsValid);
		}

		string GetEnterpriseGroupMembers()
		{
			return string.Join(", ", EnterpriseEntity.Staff.Cast<GlbStaff>().Select(s => s.GS_LoginName).OrderBy(s => s));
		}

		string GetADGroupMembers(ADGroup adGroup)
		{
			return string.Join(", ", adGroup.Members.Select(d => SearcherFilter.UserNameComponent((string)d.GetValue(GlbStaffSchema.GS_LoginName))).OrderBy(s => s));
		}

		protected static ZBool DeactivateGroup(ADGroup adGroup)
		{
			adGroup.DisconnectFromAD();
			return ZBool.False;
		}

		protected ZBool DoNothing()
		{
			return EnterpriseEntity.GG_IsActive;
		}
		#endregion
	}
}

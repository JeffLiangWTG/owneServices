namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class MembershipUnionGroupSyncStrategy : GroupSyncStrategyBase
	{
		internal MembershipUnionGroupSyncStrategy(IADEntity adEntity, bool deactivateGroup = true)
			: base(adEntity)
		{
			this.deactivateGroup = deactivateGroup;
		}

		readonly bool deactivateGroup;
		protected override void SynchroniseCore(ADGroup adGroup)
		{
			SetValue(g => g.GroupName, EnterpriseEntity.GG_Desc, EnterpriseEntity.GG_DescInfo, g => EnterpriseEntity.GG_Desc);
			SetDomainName();
			SetIsLinked();
			SyncActiveStatus(deactivateGroup);

			if (adGroup.GetDirectoryEntry(true) != null)
			{
				var enterpriseMembers = GetLinkedEnterpriseMembers(adGroup.EnterpriseEntity);

				var membersToAddToEnterprise = GetMembersNotInEnterprise(adGroup);
				var membersToAddToAD = GetMembersNotInAD(enterpriseMembers, adGroup);

				SyncGroupMembership(adGroup, membersToAddToEnterprise, null, membersToAddToAD, null);
			}
		}
	}
}

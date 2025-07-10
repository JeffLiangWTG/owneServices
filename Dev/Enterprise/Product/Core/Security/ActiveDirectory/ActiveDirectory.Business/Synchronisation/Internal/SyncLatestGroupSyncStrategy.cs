namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class SyncLatestGroupSyncStrategy : GroupSyncStrategyBase
	{
		internal SyncLatestGroupSyncStrategy(IADEntity adEntity)
			: base(adEntity)
		{
		}

		protected override void SynchroniseCore(ADGroup adGroup)
		{
			if (adGroup.IsADEntityModifiedAfterEnterprise)
			{
				SyncWithADAsMaster(adGroup);
			}
			else
			{
				SyncWithEnterpriseAsMaster(adGroup);
			}
		}
	}
}

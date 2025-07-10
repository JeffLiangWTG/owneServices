namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class SyncLatestUserSyncStrategy : UserSyncStrategyBase
	{
		internal SyncLatestUserSyncStrategy(IADEntity adEntity)
			: base(adEntity)
		{
		}

		protected override void SynchroniseCore(ADUser adUser)
		{
			if (adUser.IsADEntityModifiedAfterEnterprise)
			{
				SyncWithADAsMaster();
			}
			else
			{
				SyncWithEnterpriseAsMaster();
			}
		}
	}
}

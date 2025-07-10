namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class EnterpriseMasterUserSyncStrategy : UserSyncStrategyBase
	{
		internal EnterpriseMasterUserSyncStrategy(IADEntity adUser)
			: base(adUser)
		{
		}

		protected override void SynchroniseCore(ADUser adUser)
		{
			SyncWithEnterpriseAsMaster(false);
		}
	}
}

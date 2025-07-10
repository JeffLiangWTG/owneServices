namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class EnterpriseMasterGroupSyncStrategy : GroupSyncStrategyBase
	{
		internal EnterpriseMasterGroupSyncStrategy(IADEntity adGroup)
			: base(adGroup)
		{
		}

		protected override void SynchroniseCore(ADGroup adGroup)
		{
			SyncWithEnterpriseAsMaster(adGroup, false);
		}
	}
}

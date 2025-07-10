namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class ADMasterGroupSyncStrategy : GroupSyncStrategyBase
	{
		internal ADMasterGroupSyncStrategy(IADEntity adGroup)
			: base(adGroup)
		{
		}

		protected override void SynchroniseCore(ADGroup adGroup)
		{
			SyncWithADAsMaster(adGroup);
		}
	}
}

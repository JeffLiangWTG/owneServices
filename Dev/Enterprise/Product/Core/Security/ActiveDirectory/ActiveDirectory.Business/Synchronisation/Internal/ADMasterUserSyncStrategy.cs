namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	class ADMasterUserSyncStrategy : UserSyncStrategyBase
	{
		internal ADMasterUserSyncStrategy(IADEntity adEntity)
			: base(adEntity)
		{
		}

		protected override void SynchroniseCore(ADUser adUser)
		{
			SyncWithADAsMaster();
		}
	}
}

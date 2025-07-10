namespace CargoWise.Data.Utils
{
	/// <summary>
	/// The lock mode used for a sql AppLock
	/// To read more about the behaviour of each lock type see
	/// https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide?view=sql-server-ver16#lock_modes
	/// and
	/// https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16#----lockmode
	/// </summary>
	public enum SqlApplicationLockMode
	{
		// Locks are ordered weakest to strongest
		IntentShared,
		Shared,
		Update,
		IntentExclusive,
		Exclusive
	}
}

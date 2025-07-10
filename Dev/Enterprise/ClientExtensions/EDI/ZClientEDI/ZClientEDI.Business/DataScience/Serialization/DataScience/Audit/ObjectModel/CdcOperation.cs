namespace WTG.Serialization.DataScience.Audit.ObjectModel
{
	// https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/cdc-fn-cdc-get-all-changes-capture-instance-transact-sql?view=sql-server-ver16#table-returned
	public enum CdcOperation
	{
		Delete = 1,
		Insert = 2,
		PreUpdate = 3,
		PostUpdate = 4,
	}
}

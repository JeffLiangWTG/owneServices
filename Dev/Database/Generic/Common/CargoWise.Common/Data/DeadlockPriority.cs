namespace CargoWise.Data
{
	/// <summary>
	/// Values to use with SET DEADLOCK_PRIORITY 
	/// Numeric values come from http://msdn.microsoft.com/en-us/library/ms186736.aspx
	/// </summary>
	public enum DeadlockPriority
	{
		Min = -10,
		Low = -5,
		Medium = 0,
		High = 5,
		Max = 10
	}
}

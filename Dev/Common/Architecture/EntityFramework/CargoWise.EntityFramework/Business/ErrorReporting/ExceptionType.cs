namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Indicates whether an exception represents an unhandled error that should be reported to the user, or a business-level failure that should be caught and suppressed
	/// </summary>
	public enum ExceptionType
	{
		Unhandled,
		BusinessFailure,
	}
}

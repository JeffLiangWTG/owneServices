namespace CargoWise.Async
{
	public interface IManagedByThreadSentry
	{
		void NotifyThreadSentryOwnershipRelinquished();
		void NotifyThreadSentryOwnershipTaken();

		string DisplayName { get; }
	}
}

using CargoWise.Async;

namespace Enterprise.Security
{
	class ZSecurityThreadSentryHelper : IManagedByThreadSentry
	{
		string IManagedByThreadSentry.DisplayName => nameof(ZSecurityThreadSentryHelper);

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipRelinquished()
		{
		}

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipTaken()
		{
		}
	}
}

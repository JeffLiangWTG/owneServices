using CargoWise.Async;

namespace Enterprise.DocumentScanning
{
	class eDocsPluginThreadSentryHelper : IManagedByThreadSentry
	{
		string IManagedByThreadSentry.DisplayName => nameof(eDocsPluginThreadSentryHelper);

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipRelinquished()
		{
		}

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipTaken()
		{
		}
	}
}

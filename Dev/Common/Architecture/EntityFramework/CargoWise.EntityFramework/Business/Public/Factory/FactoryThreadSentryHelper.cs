using CargoWise.Async;

namespace CargoWise.EntityFramework
{
	class FactoryThreadSentryHelper : IManagedByThreadSentry
	{
		public FactoryThreadSentryHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		string IManagedByThreadSentry.DisplayName => factory.NameForDebugging;

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipRelinquished()
		{
			if (factory.IsReadOnlyFactoryCreated)
			{
				var readOnlyThreadSentry = factory.GetCachedReadOnlyFactory().ThreadSentry;

				if (readOnlyThreadSentry.IsOwner)
				{
					readOnlyThreadSentry.RelinquishThreadOwnership();
				}
			}
		}

		void IManagedByThreadSentry.NotifyThreadSentryOwnershipTaken()
		{
			if (factory.IsReadOnlyFactoryCreated)
			{
				var readOnlyThreadSentry = factory.GetCachedReadOnlyFactory().ThreadSentry;

				if (!readOnlyThreadSentry.IsOwner)
				{
					readOnlyThreadSentry.TakeThreadOwnership();
				}
			}
		}
	}
}

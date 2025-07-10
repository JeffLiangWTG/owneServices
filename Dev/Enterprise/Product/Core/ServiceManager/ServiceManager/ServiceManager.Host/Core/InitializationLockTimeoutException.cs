using System;
using CargoWise.Data;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	class InitializationLockTimeoutException : HostInternalException
	{
		public InitializationLockTimeoutException()
			: base("Unable to acquire Cross Host Service Task Schedules initialization due to an error obtaining the lock. Exiting to retry later.")
		{
		}

		public InitializationLockTimeoutException(TimeSpan timeSpan)
			: base(FormattableString.Invariant($"Unable to acquire Cross Host Service Task Schedules initialization lock after {timeSpan}. Exiting to retry later."))
		{
		}

		public InitializationLockTimeoutException(LockedProcessResult lockResult)
			: base(FormattableString.Invariant($"Unable to acquire Cross Host Service Task Schedules initialization, unexpected result {lockResult} from obtaining the lock. Exiting to retry later."))
		{
		}

#if NETFRAMEWORK
		protected InitializationLockTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

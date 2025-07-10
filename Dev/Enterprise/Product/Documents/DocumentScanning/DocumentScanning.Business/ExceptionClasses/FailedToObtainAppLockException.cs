using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class FailedToObtainAppLockException : DocDbManagerException
	{
		public FailedToObtainAppLockException()
			: base((NoResString)"Failed to get AppLock. Please try again after other process completes its task.")
		{ }
		public FailedToObtainAppLockException(string message)
			: base(message)
		{ }

		public FailedToObtainAppLockException(string database, LockedProcessResult failedReason)
			: base($"Failed to get AppLock for {database} with Lock result {failedReason}. Please try again after other process finishes its task.")
		{ }

		public FailedToObtainAppLockException(string message, Exception innerException)
			: base(message, innerException)
		{ }

#if NETFRAMEWORK
		protected FailedToObtainAppLockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}

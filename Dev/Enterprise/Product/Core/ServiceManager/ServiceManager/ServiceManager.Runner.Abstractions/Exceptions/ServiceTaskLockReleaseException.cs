using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class ServiceTaskLockReleaseException : LockReleaseException
	{
		public ServiceTaskLockReleaseException(Exception exception, string taskCode) : this($"Lock for single instance Service task [{taskCode}] could not be released.", exception)
		{
		}

		public ServiceTaskLockReleaseException() : this("Lock for single instance Service task could not be released.")
		{
		}

		public ServiceTaskLockReleaseException(string message) : base(message)
		{
		}

		public ServiceTaskLockReleaseException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public ServiceTaskLockReleaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

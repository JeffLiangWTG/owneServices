using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class MutualExclusiveLockReleaseException : LockReleaseException
	{
		public MutualExclusiveLockReleaseException(Exception exception, string taskCode, string groupCode) : this($"Lock for mutual exclusive group [{groupCode}] for Service task [{taskCode}] could not be released.", exception)
		{
		}

		public MutualExclusiveLockReleaseException() : this("Lock for mutual exclusive group for Service task could not be released.")
		{
		}

		public MutualExclusiveLockReleaseException(string message) : base(message)
		{
		}

		public MutualExclusiveLockReleaseException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public MutualExclusiveLockReleaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

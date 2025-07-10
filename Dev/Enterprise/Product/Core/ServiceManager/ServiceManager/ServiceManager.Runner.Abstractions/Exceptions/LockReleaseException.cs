using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public abstract class LockReleaseException : Exception
	{
		protected LockReleaseException(string message) : base(message)
		{
		}

		protected LockReleaseException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected LockReleaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

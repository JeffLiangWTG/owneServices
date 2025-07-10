using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public abstract class EnvironmentCorruptedException : Exception
	{
		protected EnvironmentCorruptedException(string message) : base(message)
		{
		}

		protected EnvironmentCorruptedException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected EnvironmentCorruptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

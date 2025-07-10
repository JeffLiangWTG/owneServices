using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class EnvironmentCheckerException : Exception
	{
		public EnvironmentCheckerException(Exception innerException) : base(nameof(EnvironmentCheckerException), innerException)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public EnvironmentCheckerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

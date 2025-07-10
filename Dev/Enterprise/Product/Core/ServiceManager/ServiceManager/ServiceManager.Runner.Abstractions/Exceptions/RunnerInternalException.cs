using System;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class RunnerInternalException : Exception
	{
		public RunnerInternalException()
		{
		}

		public RunnerInternalException(string message) : base(message)
		{
		}

		public RunnerInternalException(string message, Exception exception) : base(message, exception)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected RunnerInternalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

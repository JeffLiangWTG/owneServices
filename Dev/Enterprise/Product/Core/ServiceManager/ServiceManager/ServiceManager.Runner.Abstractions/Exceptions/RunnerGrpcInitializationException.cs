using System;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class RunnerGrpcInitializationException : GrpcInitializationException
	{
		public RunnerGrpcInitializationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected RunnerGrpcInitializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

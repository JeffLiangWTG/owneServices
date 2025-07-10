using System;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	public class HostGrpcInitializationException : GrpcInitializationException
	{
		public HostGrpcInitializationException()
			: base("Service Runner communication missing required grpcPort")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected HostGrpcInitializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

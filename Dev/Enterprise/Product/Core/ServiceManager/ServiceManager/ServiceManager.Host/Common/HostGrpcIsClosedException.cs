using System;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	public class HostGrpcIsClosedException : Exception
	{
		public HostGrpcIsClosedException(Exception exception)
			: base("Service Runner communication stream has been closed", exception)
		{
		}

#if NETFRAMEWORK
		protected HostGrpcIsClosedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

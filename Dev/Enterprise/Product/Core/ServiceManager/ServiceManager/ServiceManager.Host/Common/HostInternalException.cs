using System;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	abstract class HostInternalException : Exception
	{
		protected HostInternalException(string message)
			: base(message)
		{
		}

		protected HostInternalException(string message, Exception exception)
			: base(message, exception)
		{
		}

#if NETFRAMEWORK
		protected HostInternalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

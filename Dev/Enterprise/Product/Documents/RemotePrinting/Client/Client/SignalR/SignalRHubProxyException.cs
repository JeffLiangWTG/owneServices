using System;

namespace Enterprise.RemotePrinting.Client
{
	[Serializable]
	public class SignalRHubProxyException : Exception
	{
		public SignalRHubProxyException(string message) : base(message)
		{ }

		public SignalRHubProxyException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected SignalRHubProxyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}

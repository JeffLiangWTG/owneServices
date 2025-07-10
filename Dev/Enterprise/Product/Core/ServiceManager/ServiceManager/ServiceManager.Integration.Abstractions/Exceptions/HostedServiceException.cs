using System;

namespace ServiceManager.Integration.Abstractions
{
	[Serializable]
	public class HostedServiceException : Exception
	{
		public HostedServiceException()
			: base()
		{ }

		public HostedServiceException(string message)
			: base(message)
		{ }

		public HostedServiceException(string message, Exception innerException)
			: base(message, innerException)
		{ }

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public HostedServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public bool LogException { get; set; } = true;
	}
}

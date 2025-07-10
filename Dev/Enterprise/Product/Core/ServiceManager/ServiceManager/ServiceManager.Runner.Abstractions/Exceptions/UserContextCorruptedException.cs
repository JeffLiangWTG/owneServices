using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class UserContextCorruptedException : EnvironmentCorruptedException
	{
		public UserContextCorruptedException(string message) : base(message) { }

		public UserContextCorruptedException(IHostedServiceAttribute hostedServiceAttribute)
			: this($@"The service task code: '{hostedServiceAttribute.Code} - {hostedServiceAttribute.TypeName}, {hostedServiceAttribute.TypeAssemblyName}' has corrupted user context.")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public UserContextCorruptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

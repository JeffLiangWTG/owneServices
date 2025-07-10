using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class DbConnectionDisposerCorruptedException : EnvironmentCorruptedException
	{
		public DbConnectionDisposerCorruptedException(string message) : base(message)
		{
		}

		public DbConnectionDisposerCorruptedException(IHostedServiceAttribute hostedServiceAttribute)
			: this($@"The service task code: '{hostedServiceAttribute.Code} - {hostedServiceAttribute.TypeName}, {hostedServiceAttribute.TypeAssemblyName}' has corrupted the Db Connection Disposer at the end of the run.")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public DbConnectionDisposerCorruptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

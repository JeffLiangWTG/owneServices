using System;
using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class UndisposedSqlLockException : EnvironmentCorruptedException
	{
		public UndisposedSqlLockException(string message) : base(message)
		{
		}

		public UndisposedSqlLockException(IHostedServiceAttribute hostedServiceAttribute, List<string> lockKeys)
			: this(
				$@"The service task code: '{hostedServiceAttribute.Code} - {hostedServiceAttribute.TypeName}, {hostedServiceAttribute.TypeAssemblyName}' has one or more undisposed SQL locks at the end of the run.{System.Environment.NewLine}Undisposed lock keys:{Environment.NewLine}{string.Join(Environment.NewLine, lockKeys)}")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public UndisposedSqlLockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

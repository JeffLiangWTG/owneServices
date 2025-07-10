using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Runner.Abstractions
{
	[Serializable]
	public class LoggerConfigurationCorruptedException : EnvironmentCorruptedException
	{
		public LoggerConfigurationCorruptedException(string message) : base(message)
		{
		}

		public LoggerConfigurationCorruptedException(IHostedServiceAttribute hostedServiceAttribute, string message)
			: this($"The service task '{hostedServiceAttribute.Code} - {hostedServiceAttribute.TypeName}, {hostedServiceAttribute.TypeAssemblyName}' has corrupted the logging configuration. Ensure that the logging configuration is not altered during the run and that added rules and targets are removed at the end of the service task's run.{System.Environment.NewLine}{message}")
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public LoggerConfigurationCorruptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

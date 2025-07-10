using System;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	class ProcessControllerConfigurationException : HostInternalException
	{
		public ProcessControllerConfigurationException(string message)
			: base(message)
		{
		}

		public ProcessControllerConfigurationException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected ProcessControllerConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

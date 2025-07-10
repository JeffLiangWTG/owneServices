using System;

namespace AppDomainWrappers.Net
{
	[Serializable]
	public class MonitoredLockFileException : Exception
	{
		public MonitoredLockFileException()
			: base()
		{
		}

		public MonitoredLockFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MonitoredLockFileException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MonitoredLockFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

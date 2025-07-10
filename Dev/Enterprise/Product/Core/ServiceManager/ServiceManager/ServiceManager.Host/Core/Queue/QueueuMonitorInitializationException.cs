using System;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	class QueueuMonitorInitializationException : Exception
	{
		public QueueuMonitorInitializationException() : this("Queue monitor was not initialized.")
		{
		}

		public QueueuMonitorInitializationException(string message) : base(message)
		{
		}

		public QueueuMonitorInitializationException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected QueueuMonitorInitializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

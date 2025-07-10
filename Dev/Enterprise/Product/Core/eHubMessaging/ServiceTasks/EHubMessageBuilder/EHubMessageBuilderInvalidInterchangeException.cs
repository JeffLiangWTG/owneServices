using System;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	[Serializable]
	class EHubMessageBuilderInvalidInterchangeException : Exception
	{
		public EHubMessageBuilderInvalidInterchangeException() : base() { }

		public EHubMessageBuilderInvalidInterchangeException(string message) : base(message) { }

		public EHubMessageBuilderInvalidInterchangeException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		EHubMessageBuilderInvalidInterchangeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}

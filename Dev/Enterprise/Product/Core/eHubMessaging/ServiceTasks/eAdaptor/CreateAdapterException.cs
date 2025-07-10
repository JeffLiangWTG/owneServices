using System;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	[Serializable]
	class CreateAdapterException : Exception
	{
		public CreateAdapterException() : base() { }
		public CreateAdapterException(String message) : base(message) { }
		public CreateAdapterException(String message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		CreateAdapterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}

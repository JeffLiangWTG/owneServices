using System;

namespace Enterprise.xTMessaging.ServiceTasks
{
	[Serializable]
	public class InterchangeSavingException : Exception
	{
		public InterchangeSavingException() { }

		public InterchangeSavingException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		protected InterchangeSavingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}

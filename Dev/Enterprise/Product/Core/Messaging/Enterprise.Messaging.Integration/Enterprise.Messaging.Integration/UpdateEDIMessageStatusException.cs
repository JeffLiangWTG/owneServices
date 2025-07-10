using System;

namespace Enterprise.Messaging.Integration
{
	[Serializable]
	public class UpdateEDIMessageStatusException : Exception
	{
		public UpdateEDIMessageStatusException(string message) : base(message)
		{
		}

		public UpdateEDIMessageStatusException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected UpdateEDIMessageStatusException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

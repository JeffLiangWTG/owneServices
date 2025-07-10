using System;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	class InitialisationRequestException : Exception
	{
		public InitialisationRequestException() : this(DefaultMessage)
		{
		}

		public InitialisationRequestException(string message) : base(message)
		{
		}

		public InitialisationRequestException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected InitialisationRequestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public InitialisationRequestException(Exception exception) : base(DefaultMessage, exception)
		{
		}

		const string DefaultMessage = "Reinitialisation of the task is requested.";
	}
}

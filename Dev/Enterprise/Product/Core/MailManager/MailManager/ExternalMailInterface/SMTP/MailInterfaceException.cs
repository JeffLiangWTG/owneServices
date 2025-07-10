using System;

namespace Enterprise.MailManager.ExternalMailInterface
{
	[Serializable]
	abstract public class MailInterfaceException : ApplicationException
	{
		public MailInterfaceException(string message) : base(message) { }
		public MailInterfaceException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected MailInterfaceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}

	[Serializable]
	public class SmtpConfigurationException : MailInterfaceException
	{
		public SmtpConfigurationException(string message) : base(message) { }
		public SmtpConfigurationException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected SmtpConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}

	[Serializable]
	public class FailedToConnectException : MailInterfaceException
	{
		public FailedToConnectException(string message) : base(message) { }
		public FailedToConnectException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected FailedToConnectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}

	[Serializable]
	public class FailedToAuthenticateException : MailInterfaceException
	{
		public FailedToAuthenticateException(string message) : base(message) { }
		public FailedToAuthenticateException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected FailedToAuthenticateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}

	[Serializable]
	public class FailedToSendMessageException : MailInterfaceException
	{
		public FailedToSendMessageException(string message) : base(message) { }
		public FailedToSendMessageException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected FailedToSendMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}

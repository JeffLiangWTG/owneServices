using System;

namespace Enterprise.xTMessaging.Shared
{
	[Serializable]
	public class XtTransactionException : Exception
	{
		public XtTransactionException() { }

		public XtTransactionException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected XtTransactionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}

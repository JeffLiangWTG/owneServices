using System;
using CargoWise.Common;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	class HttpCriticalException : Exception, ICriticalException
	{
		public HttpCriticalException() : this("A critical http exception occurred.")
		{
		}

		public HttpCriticalException(string message) : base(message)
		{
		}

		public HttpCriticalException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected HttpCriticalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public bool IsCriticalException => true;
	}
}

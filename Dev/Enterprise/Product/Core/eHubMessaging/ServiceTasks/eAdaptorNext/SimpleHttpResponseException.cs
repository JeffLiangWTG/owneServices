using System;
using System.Net;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	[Serializable]
	public class SimpleHttpResponseException : Exception
	{
		public HttpStatusCode StatusCode { get; private set; }

#if NETFRAMEWORK
		public SimpleHttpResponseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
		{
			StatusCode = statusCode;
		}
	}
}

using System;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	[Serializable]
	public class OAuth2Exception : Exception
	{
		public IErrorResponse ErrorResponse { get; }

		public OAuth2ErrorTypes ErrorType { get; }

		public OAuth2Exception(string message) : base(message)
		{
		}

		public OAuth2Exception(string message, Exception exception) : base(message, exception)
		{
		}
#if NETFRAMEWORK
		public OAuth2Exception(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public OAuth2Exception(IErrorResponse errorResponse, Exception exception) : base(errorResponse.ErrorDescription, exception)
		{
			ErrorResponse = errorResponse;
			ErrorType = OAuth2Errors.StringToEnum(errorResponse.Error);
		}
	}
}

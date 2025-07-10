using System;

namespace Enterprise.xTMessaging.Shared
{
	[Serializable]
	public class MsgSessionTimeoutException : MsgServerConnectionException
	{
		public MsgSessionTimeoutException(string message, Exception exception, string errorDetail = "")
			: base(message, exception, errorDetail)
		{
			ErrorDetail = errorDetail;
		}

#if NETFRAMEWORK
		protected MsgSessionTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

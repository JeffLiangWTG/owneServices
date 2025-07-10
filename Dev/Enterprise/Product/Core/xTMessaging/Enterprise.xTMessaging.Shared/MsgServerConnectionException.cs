using System;

namespace Enterprise.xTMessaging.Shared
{
	[Serializable]
	public class MsgServerConnectionException : Exception
	{
		public MsgServerConnectionException(string additionalMessage, MsgServerConnectionException ex)
			: this(ex.Message, ex.InnerException, additionalMessage + " | " + ex.ErrorDetail)
		{
		}

		public MsgServerConnectionException(string message, Exception exception, string errorDetail = "")
			: base(message, exception)
		{
			ErrorDetail = errorDetail;
		}

#if NETFRAMEWORK
		protected MsgServerConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string ErrorDetail;
	}
}

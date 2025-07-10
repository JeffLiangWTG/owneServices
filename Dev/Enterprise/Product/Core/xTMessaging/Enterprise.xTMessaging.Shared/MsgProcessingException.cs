using System;

namespace Enterprise.xTMessaging.Shared
{
	[Serializable]
	public class MsgProcessingException : Exception
	{
		public MsgProcessingException(string message) : base(message) { }

#if NETFRAMEWORK
		protected MsgProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

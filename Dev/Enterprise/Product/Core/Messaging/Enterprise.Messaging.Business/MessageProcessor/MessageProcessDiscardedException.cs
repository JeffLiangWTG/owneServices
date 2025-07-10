using System;

namespace Enterprise.Messaging.Business
{
	[Serializable]
	public class MessageProcessDiscardedException : MessageProcessException
	{
		public MessageProcessDiscardedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MessageProcessDiscardedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	[Serializable]
	public class MessageProcessLockException : MessageProcessException
	{
		public MessageProcessLockException(string message, Exception innerException, BusinessObject originator)
			: base(message, innerException)
		{
		}

		public MessageProcessLockException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MessageProcessLockException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MessageProcessLockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	[Serializable]
	public class InvalidMessageContentException : MessageProcessException
	{
		public InvalidMessageContentException(string message, Exception innerException, BusinessObject originator)
			: base(message, innerException, originator)
		{
		}

		public InvalidMessageContentException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public InvalidMessageContentException()
			: base()
		{
		}
		public InvalidMessageContentException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected InvalidMessageContentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}

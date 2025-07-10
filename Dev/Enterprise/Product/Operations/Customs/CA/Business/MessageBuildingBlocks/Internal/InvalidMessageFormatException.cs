using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	[Serializable]
	class InvalidMessageFormatException : Enterprise.Messaging.Business.InvalidMessageContentException
	{
		public InvalidMessageFormatException(string message, Exception innerException, BusinessObject originator)
			: base(message, innerException, originator)
		{
		}

		public InvalidMessageFormatException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public InvalidMessageFormatException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected InvalidMessageFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

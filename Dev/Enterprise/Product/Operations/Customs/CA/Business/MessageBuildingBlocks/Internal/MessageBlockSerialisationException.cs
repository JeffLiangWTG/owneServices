using System;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	[Serializable]
	class MessageBlockSerialisationException : Exception
	{
		public MessageBlockSerialisationException(string message, string newInvalidFormat)
			: base(message)
		{
			NewInvalidFormat = newInvalidFormat;
		}

#if NETFRAMEWORK
		protected MessageBlockSerialisationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public readonly string NewInvalidFormat;
	}
}

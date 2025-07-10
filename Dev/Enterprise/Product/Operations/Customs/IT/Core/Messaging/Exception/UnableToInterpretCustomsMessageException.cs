using System;

namespace Enterprise.Customs.IT.Messaging;

[Serializable]
public class UnableToInterpretCustomsMessageException : Exception
{
	public UnableToInterpretCustomsMessageException(string message)
		: base(message)
	{
	}

#if NETFRAMEWORK
	protected UnableToInterpretCustomsMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

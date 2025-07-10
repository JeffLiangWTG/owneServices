using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class IncomingMessageDoesNotMatchWithStatusException : CustomsMessageProcessorException
{
	public IncomingMessageDoesNotMatchWithStatusException(string message)
		: base(message)
	{
	}

#if NETFRAMEWORK
	protected IncomingMessageDoesNotMatchWithStatusException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

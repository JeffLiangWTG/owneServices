using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class IncomingMessageDoesNotMatchWithIdocException : CustomsMessageProcessorException
{
	public IncomingMessageDoesNotMatchWithIdocException()
		: base(Res.GetString("8FFE16FF-A21B-46FC-8BCE-F8C049FBAD92", "Incoming message does not match with associated transmit IDOC interchange."))
	{
	}

	public IncomingMessageDoesNotMatchWithIdocException(string message)
		: base(message)
	{
	}

#if NETFRAMEWORK
	protected IncomingMessageDoesNotMatchWithIdocException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

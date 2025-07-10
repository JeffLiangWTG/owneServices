using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class UnableToInterpretInterchangeException : CustomsMessageProcessorException
{
	public UnableToInterpretInterchangeException(Exception innerException)
		: base(Res.GetString("573F04C7-0EF0-418B-AB3C-E98F85CB6D2D", "Unable to parse the message content."), innerException)
	{
	}

	public UnableToInterpretInterchangeException(string message)
		: base(message)
	{
	}

	public UnableToInterpretInterchangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected UnableToInterpretInterchangeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

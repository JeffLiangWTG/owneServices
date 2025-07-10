using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class CustomsMessageProcessorException : Exception
{
	public CustomsMessageProcessorException(string message)
		: base(message)
	{
	}

	public CustomsMessageProcessorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected CustomsMessageProcessorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

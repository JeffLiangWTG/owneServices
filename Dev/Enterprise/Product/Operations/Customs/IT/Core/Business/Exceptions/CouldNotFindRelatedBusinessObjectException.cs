using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class CouldNotFindRelatedBusinessObjectException : CustomsMessageProcessorException
{
	public CouldNotFindRelatedBusinessObjectException(string message) : base(message)
	{
	}

#if NETFRAMEWORK
	protected CouldNotFindRelatedBusinessObjectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

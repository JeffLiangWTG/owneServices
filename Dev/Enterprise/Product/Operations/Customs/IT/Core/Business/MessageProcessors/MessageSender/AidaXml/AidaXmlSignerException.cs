using System;

namespace Enterprise.Customs.IT.Business;

[Serializable]
public class AidaXmlSignerException : Exception
{
	public AidaXmlSignerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected AidaXmlSignerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

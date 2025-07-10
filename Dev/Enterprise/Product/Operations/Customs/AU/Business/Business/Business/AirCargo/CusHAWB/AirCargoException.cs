using System;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class AirCargoException : Exception
{
	public AirCargoException(string message)
		: base(message)
	{
	}

#if NETFRAMEWORK
	protected AirCargoException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

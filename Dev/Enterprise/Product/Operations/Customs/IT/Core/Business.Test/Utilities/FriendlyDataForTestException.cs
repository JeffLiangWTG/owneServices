using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Testing;

[Serializable]
public class FriendlyDataForTestException : ZDataException
{
	public FriendlyDataForTestException(string friendlyMessage, string debugMessage)
		: base(new Exception(), friendlyMessage, debugMessage, null, null)
	{
	}

#if NETFRAMEWORK
	protected FriendlyDataForTestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{
	}
#endif
}

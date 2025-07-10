using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business.Testing;

[Serializable]
public class ZDataExceptionForTesting : ZDataException
{
	public ZDataExceptionForTesting(string message) : base(new Exception(message), message, message, null, null)
	{
	}

#if NETFRAMEWORK
	protected ZDataExceptionForTesting(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
	{
	}
#endif
}

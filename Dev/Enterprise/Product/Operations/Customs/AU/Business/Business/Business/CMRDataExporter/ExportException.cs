using System;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class ExportException : ApplicationException
{
	public ExportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected ExportException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{ }
#endif
}

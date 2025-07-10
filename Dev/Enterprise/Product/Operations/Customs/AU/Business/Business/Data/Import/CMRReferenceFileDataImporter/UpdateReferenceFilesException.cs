using System;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class UpdateReferenceFilesException : Exception
{
	public UpdateReferenceFilesException(string message)
		: base(message)
	{ }

#if NETFRAMEWORK
	public UpdateReferenceFilesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{ }
#endif
}

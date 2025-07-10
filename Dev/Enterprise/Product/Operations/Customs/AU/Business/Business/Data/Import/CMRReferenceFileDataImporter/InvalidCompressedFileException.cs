using System;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class InvalidCompressedFileException : ApplicationException, Integration.Customs.AU.IInvalidCompressedFileExceptionProvider
{
	public InvalidCompressedFileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected InvalidCompressedFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
	{ }
#endif
}

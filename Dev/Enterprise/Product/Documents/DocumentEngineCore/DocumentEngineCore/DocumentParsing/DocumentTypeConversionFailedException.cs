using System;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	[Serializable]
	public class DocumentTypeConversionFailedException : FormatException
	{
		public DocumentTypeConversionFailedException(string msg)
			: base(msg)
		{
		}

		public DocumentTypeConversionFailedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentTypeConversionFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

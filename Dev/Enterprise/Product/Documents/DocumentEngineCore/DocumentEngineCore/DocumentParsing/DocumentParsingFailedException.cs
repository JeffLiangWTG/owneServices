using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	[Serializable]
	public class DocumentParsingFailedException : OdysseyException
	{
		public DocumentParsingFailedException(string msg)
			: base(msg)
		{
		}

		public DocumentParsingFailedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentParsingFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

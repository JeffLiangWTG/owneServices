using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class CorruptedDocumentException : UnreadableDocumentException
	{
		public CorruptedDocumentException(string message, Exception innerException)
			: base(message, innerException)
		{ }

#if NETFRAMEWORK
		protected CorruptedDocumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}

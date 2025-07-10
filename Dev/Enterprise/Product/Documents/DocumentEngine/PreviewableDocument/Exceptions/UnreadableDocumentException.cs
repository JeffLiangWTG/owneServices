using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class UnreadableDocumentException : UnsupportedDocumentException
	{
		static readonly ResourceString unreadableDocumentMessage = ResString.GetMultilingualString("8ad59d7b-8cdc-47c0-8a50-f2ba43aac028|CantImportBecause...", "it could not be read");
		public UnreadableDocumentException(Exception innerException)
			: base(unreadableDocumentMessage, ResString.GetMultilingualString("0f4dc83d-64a8-4e74-934e-7f6e4cd4366d", "The document could not be read"), innerException)
		{
		}
		public UnreadableDocumentException(string message, Exception innerException)
			: base(unreadableDocumentMessage, message, innerException)
		{
		}

#if NETFRAMEWORK
		protected UnreadableDocumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
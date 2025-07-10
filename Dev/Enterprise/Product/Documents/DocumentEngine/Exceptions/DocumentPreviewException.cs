using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentPreviewException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public DocumentPreviewException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentPreviewException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal DocumentPreviewException(DocumentPreviewExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion

		public object GetJsonData() => new DocumentPreviewExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

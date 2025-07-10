using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	// used in one place, should probably be removed.
	[Serializable]
	class DocumentConvertException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal DocumentConvertException(string message)
			: base(message)
		{
		}

		internal DocumentConvertException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentConvertException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal DocumentConvertException(DocumentConvertExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new DocumentConvertExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentMenuException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		protected internal DocumentMenuException(string message)
			: base(message)
		{
		}

		protected internal DocumentMenuException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentMenuException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal DocumentMenuException(DocumentMenuExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new DocumentMenuExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

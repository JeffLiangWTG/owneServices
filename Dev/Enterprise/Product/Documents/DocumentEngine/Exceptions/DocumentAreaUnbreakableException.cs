using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentAreaUnbreakableException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		protected internal DocumentAreaUnbreakableException(string message)
			: base(message)
		{
		}

		protected internal DocumentAreaUnbreakableException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentAreaUnbreakableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal DocumentAreaUnbreakableException(DocumentAreaUnbreakableExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new DocumentAreaUnbreakableExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

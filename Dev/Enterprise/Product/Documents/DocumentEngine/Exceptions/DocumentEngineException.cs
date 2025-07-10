using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		/// <summary>
		/// This exception should only be thrown within the DocumentEngine itself. Can be caught outside of the engine just not thrown. 
		/// If you really want to throw a generic Documents exception outside of DocumentEngine, create your own subclass.
		/// </summary>
		/// <param name="Message"></param>
		protected internal DocumentEngineException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// This exception should only be thrown within the DocumentEngine itself. Can be caught outside of the engine just not thrown. 
		/// If you really want to throw a generic Documents exception outside of DocumentEngine, create your own subclass.
		/// </summary>
		/// <param name="Message"></param>
		protected internal DocumentEngineException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal DocumentEngineException(DocumentEngineExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public virtual object GetJsonData() => new DocumentEngineExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

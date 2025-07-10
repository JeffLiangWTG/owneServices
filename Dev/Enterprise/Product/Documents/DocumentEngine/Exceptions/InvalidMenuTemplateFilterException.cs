using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class InvalidMenuTemplateFilterException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal InvalidMenuTemplateFilterException(string message)
			: base(message)
		{
		}

		internal InvalidMenuTemplateFilterException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected InvalidMenuTemplateFilterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		#region Constructor For IJsonSerializable

		internal InvalidMenuTemplateFilterException(InvalidMenuTemplateFilterExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new InvalidMenuTemplateFilterExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = base.InnerException?.Message,
		};
	}
}

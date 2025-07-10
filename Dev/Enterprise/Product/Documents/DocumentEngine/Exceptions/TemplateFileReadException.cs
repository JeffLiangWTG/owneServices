using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class TemplateFileReadException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal TemplateFileReadException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected TemplateFileReadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal TemplateFileReadException(TemplateFileReadExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion

		public object GetJsonData() => new TemplateFileReadExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = base.InnerException?.Message,
		};

#if DEBUG
		public static TemplateFileReadException NewForTesting(string message, Exception innerException)
		{
			return new TemplateFileReadException(message, innerException);
		}
#endif
	}
}

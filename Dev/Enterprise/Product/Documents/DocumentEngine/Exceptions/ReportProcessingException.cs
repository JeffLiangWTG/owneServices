using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ReportProcessingException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal ReportProcessingException(string errorTitle, string errorMessage)
			: base(errorTitle + "\r\n\r\n" + errorMessage)
		{
		}

		internal ReportProcessingException(string errorTitle, string errorMessage, Exception innerException)
			: base(errorTitle + "\r\n\r\n" + errorMessage, innerException)
		{
		}

#if NETFRAMEWORK
		protected ReportProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal ReportProcessingException(ReportProcessingExceptionJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new ReportProcessingExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
		};
	}
}

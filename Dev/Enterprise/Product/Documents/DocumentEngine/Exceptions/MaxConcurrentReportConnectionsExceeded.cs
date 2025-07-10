using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class MaxConcurrentReportConnectionsExceeded : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		readonly string reportInfo;

		public MaxConcurrentReportConnectionsExceeded(string message, string reportInfo)
			: base(message)
		{
			this.reportInfo = reportInfo;
		}

		public MaxConcurrentReportConnectionsExceeded(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected MaxConcurrentReportConnectionsExceeded(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal MaxConcurrentReportConnectionsExceeded(MaxConcurrentReportConnectionsExceededJsonData data)
			: base(data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new MaxConcurrentReportConnectionsExceededJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = InnerException?.Message,
			ReportInfo = reportInfo,
		};
	}
}

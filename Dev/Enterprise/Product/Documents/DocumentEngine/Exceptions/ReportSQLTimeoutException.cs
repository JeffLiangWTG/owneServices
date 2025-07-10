using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	class ReportSQLTimeoutException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		const string MessageText = "Report query timed out";

		internal ReportSQLTimeoutException()
			: base(MessageText)
		{
		}

#if NETFRAMEWORK
		protected ReportSQLTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal ReportSQLTimeoutException(ReportSQLTimeoutExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new ReportSQLTimeoutExceptionJsonData()
		{
			Message = base.Message
		};
	}
}

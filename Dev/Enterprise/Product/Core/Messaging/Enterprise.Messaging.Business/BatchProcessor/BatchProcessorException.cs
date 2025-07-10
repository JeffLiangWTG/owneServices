using System;

namespace Enterprise.BatchProcessor
{
	[Serializable]
	public class BatchProcessorOperationCancelledException : ApplicationException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		const string EXCEPTION_MESSAGE = "Operation Cancelled";

		public BatchProcessorOperationCancelledException()
			: base(EXCEPTION_MESSAGE)
		{ }

		public BatchProcessorOperationCancelledException(Exception innerException)
			: base(EXCEPTION_MESSAGE, innerException)
		{ }

#if NETFRAMEWORK
		protected BatchProcessorOperationCancelledException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}

using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineTooManyRowsForThisFileFormatException : ExcelLimitationForThisFileFormatException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string NessageText = "Too many rows/columns for this file format in resulting report.";

		internal DocumentEngineTooManyRowsForThisFileFormatException(Exception inner = null)
			: base(ExcelLimitationsHelper.LimitationType.Row, NessageText, inner)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineTooManyRowsForThisFileFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal DocumentEngineTooManyRowsForThisFileFormatException(DocumentEngineTooManyRowsForThisFileFormatExceptionJsonData data)
			: this(data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}
	}
}

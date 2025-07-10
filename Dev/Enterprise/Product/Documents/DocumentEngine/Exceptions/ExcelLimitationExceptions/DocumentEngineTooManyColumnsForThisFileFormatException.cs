using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineTooManyColumnsForThisFileFormatException : ExcelLimitationForThisFileFormatException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string MessageText = "Too many rows/columns for this file format in resulting report.";

		internal DocumentEngineTooManyColumnsForThisFileFormatException(Exception inner = null)
			: base(ExcelLimitationsHelper.LimitationType.Column, MessageText, inner)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineTooManyColumnsForThisFileFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

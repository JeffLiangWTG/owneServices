using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineTooLongFormulaForThisFileFormatException : ExcelLimitationForThisFileFormatException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string MessageText = "Formula with too many characters for this file format in resulting report.";

		internal DocumentEngineTooLongFormulaForThisFileFormatException(Exception inner = null)
			: base(ExcelLimitationsHelper.LimitationType.Formula, MessageText, inner)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineTooLongFormulaForThisFileFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

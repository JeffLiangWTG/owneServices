using System;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	public interface IReportProcessingError
	{
		Exception Exception { get; }
		string Message { get; }
		string TemplatePath { get; }
		string SheetName { get; }
		string CellName { get; }
		string ToString();
		ReportProcessingErrorSeverity Severity { get; set; }
		string OuterContent { get; }
		string InnerMacro { get; }
		int Occurrences { get; }
		void SetCurrentStatus(string outerMacro, string innerMacro, CellReference currentCell);
		void SetTemplatePath(string templatePath);
		void IncrementOccurrences();
	}
}

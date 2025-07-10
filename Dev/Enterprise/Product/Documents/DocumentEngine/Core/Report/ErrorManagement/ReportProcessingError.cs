using System;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	class ReportProcessingError : IReportProcessingError
	{
		internal ReportProcessingError(string message, ReportProcessingErrorSeverity severity, Exception cause = null)
			: this(message, CellReference.UnKnown, severity, cause)
		{
		}

		internal ReportProcessingError(string message, CellReference cell, ReportProcessingErrorSeverity severity, Exception cause = null)
		{
			Message = message;
			CellReference = cell;
			Severity = severity;
			Occurrences = 1;
			Exception = cause;
		}

		public string Message { get; private set; }
		public string TemplatePath { get; private set; }
		public ReportProcessingErrorSeverity Severity { get; set; }
		public Exception Exception { get; private set; }

		public string OuterContent { get; private set; }
		public string InnerMacro { get; private set; }
		public int Occurrences { get; private set; }

		CellReference CellReference { get; set; }

		void IReportProcessingError.IncrementOccurrences()
		{
			Occurrences++;
		}

		void IReportProcessingError.SetCurrentStatus(string outerContent, string innerMacro, CellReference currentCell)
		{
			this.OuterContent = outerContent;
			this.InnerMacro = innerMacro;

			if (this.CellReference.IsEmpty)
			{
				this.CellReference = currentCell.IsEmpty ? CellReference.UnKnown : currentCell;
			}
		}

		void IReportProcessingError.SetTemplatePath(string templatePath)
		{
			this.TemplatePath = templatePath;
		}

		public override string ToString()
		{
			return String.Format((NoResString)@"{0} on Sheet: [{1}] at Cell {2}: [{3}] in File: [{4}]", Severity == ReportProcessingErrorSeverity.Fatal ? (NoResString)"Fatal Error" : Severity.ToString(), SheetName, CellName, Message, TemplatePath);
		}

		public string SheetName
		{
			get { return CellReference.SheetName; }
		}

		public string CellName
		{
			get { return CellReference.Cell; }
		}
	}
}

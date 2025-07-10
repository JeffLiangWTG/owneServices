using System;
using Microsoft.CodeAnalysis;

namespace AnalyzersRunner
{
	static class DiagnosticSeverityExtensions
	{
		public static ReportDiagnostic MapToReportDiagnostic(this DiagnosticSeverity severity)
		{
			switch (severity)
			{
				case DiagnosticSeverity.Hidden:
					return ReportDiagnostic.Hidden;

				case DiagnosticSeverity.Info:
					return ReportDiagnostic.Info;

				case DiagnosticSeverity.Warning:
					return ReportDiagnostic.Warn;

				case DiagnosticSeverity.Error:
					return ReportDiagnostic.Error;

				default:
					throw new InvalidOperationException($"unknown severity. severity = {severity}");
			}
		}
	}
}

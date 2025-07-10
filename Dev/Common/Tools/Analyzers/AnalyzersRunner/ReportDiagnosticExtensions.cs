using System;
using Microsoft.CodeAnalysis;

namespace AnalyzersRunner
{
	static class ReportDiagnosticExtensions
	{
		public static bool IsMoreSevereThan(this ReportDiagnostic thisSeverity, ReportDiagnostic otherSeverity)
		{
			if (thisSeverity == otherSeverity)
			{
				return false;
			}

			switch (thisSeverity)
			{
				case ReportDiagnostic.Default:
					return false;

				case ReportDiagnostic.Error:
					return true;

				case ReportDiagnostic.Warn:
					return otherSeverity != ReportDiagnostic.Error;

				case ReportDiagnostic.Info:
					return otherSeverity != ReportDiagnostic.Warn && otherSeverity != ReportDiagnostic.Error;

				case ReportDiagnostic.Hidden:
					return otherSeverity == ReportDiagnostic.Default || otherSeverity == ReportDiagnostic.Suppress;

				case ReportDiagnostic.Suppress:
					return otherSeverity == ReportDiagnostic.Default;

				default:
					throw new InvalidOperationException("Unknown severity. severity = " + thisSeverity);
			}
		}
	}
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzersRunner
{
	//taken from: https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/DiagnosticAnalyzer/AnalyzerOptionsExtensions.cs#L30
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	static class AnalyzerOptionsExtensions
	{
		const string DotnetAnalyzerDiagnosticPrefix = "dotnet_analyzer_diagnostic";
		const string CategoryPrefix = "category";
		const string SeveritySuffix = "severity";

		const string DotnetAnalyzerDiagnosticSeverityKey = DotnetAnalyzerDiagnosticPrefix + "." + SeveritySuffix;

		static string GetCategoryBasedDotnetAnalyzerDiagnosticSeverityKey(string category)
			=> $"{DotnetAnalyzerDiagnosticPrefix}.{CategoryPrefix}-{category}.{SeveritySuffix}";

		/// <summary>
		/// Tries to get configured severity for the given <paramref name="descriptor"/>
		/// for the given <paramref name="tree"/> from bulk configuration analyzer config options, i.e.
		///     'dotnet_analyzer_diagnostic.category-%RuleCategory%.severity = %severity%'
		///         or
		///     'dotnet_analyzer_diagnostic.severity = %severity%'
		/// </summary>
		public static bool TryGetSeverityFromBulkConfiguration(
			this AnalyzerOptions analyzerOptions,
			SyntaxTree tree,
			Compilation compilation,
			DiagnosticDescriptor descriptor,
			/*CancellationToken cancellationToken,*/
			out ReportDiagnostic severity)
		{
			// Analyzer bulk configuration does not apply to:
			//  1. Disabled by default diagnostics
			//  2. Compiler diagnostics
			//  3. Non-configurable diagnostics
			if (analyzerOptions == null ||
				!descriptor.IsEnabledByDefault /*||
                descriptor.IsCompilerOrNotConfigurable()*/)
			{
				severity = default;
				return false;
			}

			/* commented out. we will have checked these locations already
             * 
            // If user has explicitly configured severity for this diagnostic ID, that should be respected and
            // bulk configuration should not be applied.
            // For example, 'dotnet_diagnostic.CA1000.severity = error'
            if (compilation.Options.SpecificDiagnosticOptions.ContainsKey(descriptor.Id) ||
                compilation.Options.SyntaxTreeOptionsProvider.TryGetDiagnosticValue(tree, descriptor.Id, cancellationToken, out _) == true ||
                compilation.Options.SyntaxTreeOptionsProvider.TryGetGlobalDiagnosticValue(descriptor.Id, cancellationToken, out _) == true)
            {
                severity = default;
                return false;
            }
            */

			var analyzerConfigOptions = analyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);

			// If user has explicitly configured default severity for the diagnostic category, that should be respected.
			// For example, 'dotnet_analyzer_diagnostic.category-security.severity = error'
			var categoryBasedKey = GetCategoryBasedDotnetAnalyzerDiagnosticSeverityKey(descriptor.Category);
			if (analyzerConfigOptions.TryGetValue(categoryBasedKey, out var value) &&
				TryParseSeverity(value, out severity))
			{
				// '/warnaserror' should bump Warning bulk configuration to Error.
				if (severity == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
				{
					severity = ReportDiagnostic.Error;
				}

				return true;
			}

			// Otherwise, if user has explicitly configured default severity for all analyzer diagnostics, that should be respected.
			// For example, 'dotnet_analyzer_diagnostic.severity = error'
			if (analyzerConfigOptions.TryGetValue(DotnetAnalyzerDiagnosticSeverityKey, out value) &&
				TryParseSeverity(value, out severity))
			{
				// '/warnaserror' should bump Warning bulk configuration to Error.
				if (severity == ReportDiagnostic.Warn && compilation.Options.GeneralDiagnosticOption == ReportDiagnostic.Error)
				{
					severity = ReportDiagnostic.Error;
				}

				return true;
			}

			severity = default;
			return false;
		}

		static bool TryParseSeverity(string editorconfigSeverityString, out ReportDiagnostic reportDiagnostic)
		{
			switch (editorconfigSeverityString)
			{
				case "none":
					reportDiagnostic = ReportDiagnostic.Suppress;
					return true;

				case "refactoring":
				case "silent":
					reportDiagnostic = ReportDiagnostic.Hidden;
					return true;

				case "suggestion":
					reportDiagnostic = ReportDiagnostic.Info;
					return true;

				case "warning":
					reportDiagnostic = ReportDiagnostic.Warn;
					return true;

				case "error":
					reportDiagnostic = ReportDiagnostic.Error;
					return true;

				default:
					reportDiagnostic = default;
					return false;
			}
		}
	}
}

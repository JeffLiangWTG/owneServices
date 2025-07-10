using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class HouseBillDocumentBuilder
	{
		public struct Parameters
		{
			public IHouseBillTemplate Template { get; set; }
			public IDocumentDescriptor Descriptor { get; set; }
			public IServiceContainer Services { get; set; }
			public IVisualizerDocumentData DocumentData { get; set; }
			public IMacroEvaluationContext MacroEvaluationContext { get; set; }
			public IMacroScope Scope { get; set; }
			public ILogger Logger { get; set; }
			public ICommand[] Commands { get; set; }

			public bool IsValid => Template != null
				&& Descriptor != null
				&& Services != null
				&& DocumentData != null
				&& MacroEvaluationContext != null
				&& Scope != null
				&& Logger != null
				&& Commands != null;
		}

		public HouseBillDocumentBuilder(Parameters parameters)
		{
			this.parameters = parameters;
		}

		readonly Parameters parameters;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public Either<string, IDocument> Build()
		{
			if (!parameters.IsValid)
			{
				return "Invalid document builder parameters.";
			}

			var verboseLogger = new VerboseLogger();
			var combinedLogger = new CombinedLogger(verboseLogger, parameters.Logger);

			try
			{
				var res = CreateDocument(parameters.Scope, combinedLogger)
					.Then(MergeOverrides);

				SendErrorReportIfApplicable(verboseLogger.ReportableLogs);

				return res;
			}
			catch (Exception exc) when (!exc.IsCriticalException())
			{
				SendErrorReportIfApplicable(verboseLogger.Logs, exc);
				return Res.GetString("bb4a2211-9b2b-41b5-9633-4a4eecf33e34", "An error has occurred while processing the template.");
			}
		}

		void SendErrorReportIfApplicable(IReadOnlyCollection<string> logs, Exception exc = null)
		{
			if (logs.Count == 0)
			{
				return;
			}

			var combinedLogs = string.Join(System.Environment.NewLine, logs);
			var errorMessage = $"An error has occurred when creating Document: {parameters.Descriptor.Name}, Menu Name: {parameters.Descriptor.MenuName}{System.Environment.NewLine}{combinedLogs}";
			ErrorReporter.ReportOnce("BillOfLadingTemplateBuildIssue", errorMessage, exc);
		}

		Either<string, IDocument> CreateDocument(IMacroScope scope, ILogger logger)
		{
			var res = parameters.Template.CreateDocument(
				parameters.Descriptor.Name,
				scope,
				parameters.MacroEvaluationContext,
				logger);

			return res;
		}

		Either<string, IDocument> MergeOverrides(IDocument document)
		{
			var merger = new DocumentOverrideMerger(
				parameters.Services,
				parameters.DocumentData);

			return merger.MergeOverride(document);
		}
	}
}

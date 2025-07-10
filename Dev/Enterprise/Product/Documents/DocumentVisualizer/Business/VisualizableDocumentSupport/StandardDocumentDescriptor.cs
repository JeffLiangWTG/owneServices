using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class StandardDocumentDescriptor : IDocumentDescriptor
	{
		public StandardDocumentDescriptor(IDocumentPivot pivot, IVisualizerDocumentData documentData, IStandardTemplate template, IMacroScope scope, IMacroEvaluationContext macroEvaluationContext)
		{
			Argument.NotNull(pivot, nameof(pivot));
			Argument.NotNull(documentData, nameof(documentData));
			Argument.NotNull(template, nameof(template));
			Argument.NotNull(scope, nameof(scope));
			Argument.NotNull(macroEvaluationContext, nameof(macroEvaluationContext));

			this.pivot = pivot;
			this.DocumentData = documentData;
			this.scope = scope;
			this.macroEvaluationContext = macroEvaluationContext;
			this.template = template;
		}

		readonly IDocumentPivot pivot;
		readonly IStandardTemplate template;
		readonly IMacroScope scope;
		readonly IMacroEvaluationContext macroEvaluationContext;

		public string Name => template.Name ?? pivot.DocumentTitle;

		public string DataContext => pivot.DataContext;

		public bool EnableTranslation => template.EnableTranslation;

		public string Purpose => pivot.Purpose;

		public string DocumentType => pivot.DocType ?? string.Empty;

		public string MenuName => pivot.MenuName;

		public bool IsSystemDefined => pivot.IsSystemDefined;

		public IVisualizerDocumentData DocumentData { get; }

		public IPrintInstructions PrintInstructions => printInstructions ?? (printInstructions = new StandardPrintInstructions(pivot, template, scope, macroEvaluationContext));
		StandardPrintInstructions printInstructions;

		public IMessageInstructions MessageInstructions => messageInstructions ?? (messageInstructions = new StandardMessageInstructions(template, scope, macroEvaluationContext, Name));
		IMessageInstructions messageInstructions;

		public IEDocsInstructions EDocsInstructions => eDocsInstructions ?? (eDocsInstructions = new EDocsInstructions(pivot.SaveCopyToEDocs, DocumentData.Parent));
		IEDocsInstructions eDocsInstructions;

		public IDisplayInstructions DisplayInstructions => displayInstructions ?? (displayInstructions = new StandardDisplayInstructions(template, scope, macroEvaluationContext));
		IDisplayInstructions displayInstructions;
	}
}

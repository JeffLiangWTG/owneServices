using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class HouseBillDocumentDescriptor : IDocumentDescriptor
	{
		public HouseBillDocumentDescriptor(BusinessObject parent, IDocumentPivot pivot, IVisualizerDocumentData documentData, IHouseBillTemplate template, IServiceContainer services, IReadOnlyCollection<ICommand> commands)
		{
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(pivot, nameof(pivot));
			Argument.NotNull(documentData, nameof(documentData));
			Argument.NotNull(template, nameof(template));

			this.parent = parent;
			this.pivot = pivot;
			this.documentData = documentData;
			this.template = template;
			this.services = services;
			this.commands = commands;
		}

		readonly BusinessObject parent;
		readonly IDocumentPivot pivot;
		readonly IVisualizerDocumentData documentData;
		readonly IHouseBillTemplate template;
		readonly IServiceContainer services;
		readonly IReadOnlyCollection<ICommand> commands;

		public string Name => pivot.DocumentTitle;

		public string DataContext => pivot.DataContext;

		public bool EnableTranslation => false;

		public string Purpose => pivot.Purpose;

		public string DocumentType => pivot.DocType;

		public string MenuName => pivot.MenuName;

		public bool IsSystemDefined => pivot.IsSystemDefined;

		public IVisualizerDocumentData DocumentData => documentData;

		public IPrintInstructions PrintInstructions => printInstructions ?? (printInstructions = new HouseBillPrintInstructions(
			pivot,
			template.Definition.IsRight
				? template.Definition.Right.Parameters
				: null,
			parent));

		HouseBillPrintInstructions printInstructions;

		public IMessageInstructions MessageInstructions => messageInstructions ?? (messageInstructions = ObjectFactory.Get<IHouseBillMessageInstructionsCreator>().GetMessageInstructions(parent, pivot, template));
		IMessageInstructions messageInstructions;

		public IEDocsInstructions EDocsInstructions => eDocsInstructions ?? (eDocsInstructions = new EDocsInstructions(pivot.SaveCopyToEDocs, parent));
		IEDocsInstructions eDocsInstructions;

		public IDisplayInstructions DisplayInstructions => displayInstructions ?? (displayInstructions = new HouseBillDisplayInstructions(services, MessageInstructions, commands));
		IDisplayInstructions displayInstructions;
	}
}

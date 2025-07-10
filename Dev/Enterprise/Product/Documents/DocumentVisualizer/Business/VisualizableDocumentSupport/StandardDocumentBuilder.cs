using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class StandardDocumentBuilder
	{
		public struct Parameters
		{
			public IStandardTemplate Template { get; set; }
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

		public StandardDocumentBuilder(Parameters parameters)
		{
			this.parameters = parameters;
		}

		readonly Parameters parameters;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public Either<string, IDocument> Build()
		{
			try
			{
				if (!parameters.IsValid)
				{
					return "Invalid document builder parameters.";
				}

				var broker = parameters.Services.Resolve<IEventBroker>();

				var res = CreateDocument(parameters.Scope, broker)
					.Then(MergeOverrides);

				return res;
			}
			catch (MacroOutOfMemoryException ex)
			{
				var error = FormattableString.Invariant($@"
Exception message: {ex.GetFullMessage()}
StackTrace: {ex.StackTrace}");
				ErrorReporter.ReportOnce("MacroOutOfMemoryException", error);
				throw;
			}
		}

		Either<string, IDocument> CreateDocument(IMacroScope scope, IEventBroker broker)
		{
			IDocument createdDocument = null;
			IEnumerable<IMenuItemDescriptor> menuItems = null;

			using (broker.GetEvent<CreateMenuEvent>().Subscribe(data => menuItems = data.MenuItems ?? Enumerable.Empty<IMenuItemDescriptor>()))
			{
				createdDocument = parameters.Template.CreateDocument(
					parameters.Descriptor.Name,
					parameters.Descriptor.DataContext,
					scope,
					parameters.MacroEvaluationContext,
					parameters.Logger,
					new CurrentUserLanguageProvider());
			}

			if (createdDocument != null && menuItems == null)
			{
				var res = parameters.Services.Resolve<IResourceAccessor>();
				var messageInstructions = parameters.Descriptor.MessageInstructions;
				var menuItemBuilder = new CommandMenuItemBuilder(res, parameters.Commands);
				menuItems = new DefaultMenuBuilder().Build(createdDocument.Data, menuItemBuilder, res, messageInstructions);
			}

			if (parameters.Descriptor.DisplayInstructions is StandardDisplayInstructions standardDisplayInstructions)
			{
				standardDisplayInstructions.MenuItems = menuItems;
			}

			var overridable = scope.Data as IOverridable;

			overridable?.AcceptChanges();

			return new Either<string, IDocument>(createdDocument);
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

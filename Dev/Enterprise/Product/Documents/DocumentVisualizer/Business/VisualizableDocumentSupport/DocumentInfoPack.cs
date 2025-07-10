using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business.Documents;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using IServiceContainer = Enterprise.DocumentVisualizer.Presentation.IServiceContainer;
using NotificationType = Enterprise.DocumentVisualizer.Core.NotificationType;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentInfoPack : IDocumentInfoPack
	{
		public DocumentInfoPack(IServiceContainer services, BusinessObject parent, object additionalData, string menuName, IReadOnlyCollection<IStmMenuTemplatePivot> pivots)
		{
			Argument.NotNull(services, nameof(services));
			Argument.NotNull(parent, nameof(parent));
			Argument.NotNull(pivots, nameof(pivots));

			this.parent = parent;
			this.additionalData = additionalData;
			this.pivots = pivots;

			this.templateCache = new TemplateCache(parent.Factory);
			this.documentDataCache = new DocumentDataCache(parent);

			this.Name = menuName;
			this.lazyDocumentInfos = new Lazy<IEnumerable<IDocumentInfo>>(() => CreateDocumentInfos(services).ToArray());
		}

		readonly BusinessObject parent;
		readonly IReadOnlyCollection<IStmMenuTemplatePivot> pivots;
		readonly object additionalData;
		readonly TemplateCache templateCache;
		readonly DocumentDataCache documentDataCache;
		readonly Lazy<IEnumerable<IDocumentInfo>> lazyDocumentInfos;

		#region IDocumentInfoPack members

		public string Name { get; }
		public IEnumerable<IDocumentInfo> DocumentInfos => lazyDocumentInfos.Value;

		#endregion

		#region Implementation

		IEnumerable<IDocumentInfo> CreateDocumentInfos(IServiceContainer commonServices)
		{
			var dynamicDataProvider = new DynamicDataProvider(parent);

			var infos = CreateDocumentInfos(
				parent as IDocumentSupportable,
				commonServices,
				dynamicDataProvider);

			commonServices
				.Resolve<IEventBroker>()
				.GetEvent<ResetEvent>()
				.Subscribe(OnReset);

			commonServices
				.Resolve<IEventBroker>()
				.GetEvent<SavedEvent>()
				.Subscribe(OnSaved);

			return infos;
		}

		void OnReset(ResetEvent eventData)
		{
			SynchWithAnotherInstancesOfDataStore(
				eventData.Document.Data,
				eventData.StoreName,
				doc => doc.Data.CancelChanges());
		}

		void OnSaved(SavedEvent eventData)
		{
			var changeSet = eventData.Document.Data.GetOverriddenValuesXml();

			if (changeSet == null)
			{
				SynchWithAnotherInstancesOfDataStore(
					eventData.Document.Data,
					eventData.StoreName,
					doc => doc.Data.CancelChanges());
			}
			else
			{
				SynchWithAnotherInstancesOfDataStore(
					eventData.Document.Data,
					eventData.StoreName,
					doc => doc.Data.MergeDataFromXml(changeSet));
			}
		}

		void SynchWithAnotherInstancesOfDataStore(IDynamicData data, string dataStoreName, Action<IDocument> action)
		{
			foreach (var info in DocumentInfos)
			{
				var isAnotherInstanceOfTheSameDataStore = string.CompareOrdinal(info.DocumentData.Name, dataStoreName) != 0
					|| info.Document.Data == data;

				if (!isAnotherInstanceOfTheSameDataStore)
				{
					action(info.Document);
				}
			}
		}

		IReadOnlyCollection<IDocumentInfo> CreateDocumentInfos(
			IDocumentSupportable documentSupportable,
			IServiceContainer commonServices,
			DynamicDataProvider dynamicDataProvider)
		{
			if (!documentSupportable.IsSupportedByDocumentVisualizer())
			{
				return Array.Empty<IDocumentInfo>();
			}

			var documentDelivery = documentSupportable != null
				? (ICommandProvider)new DocumentDelivery(documentSupportable)
				: new EmptyCommandProvider();

			var result = new List<IDocumentInfo>();

			foreach (var pivot in DocumentPivot.Create(pivots))
			{
				var macroEvaluationContext = GetMacroLibraries(pivot.DataContext).CreateContext();

				var lazyTemplate = new Lazy<ITemplate>(() => templateCache.Get(pivot.TemplatePK));
				var lazyDocumentData = new Lazy<IVisualizerDocumentData>(() => documentDataCache.Get(pivot.DataStoreName));
				var lazyServices = CreateLazyServices(commonServices, lazyTemplate);
				var lazyDataContext = new Lazy<string>(() => lazyTemplate.Value.DataContext);

				var lazyCommandProviders = new Lazy<ICommandProvider[]>(GetCommandProviders);

				ICommandProvider[] GetCommandProviders()
				{
					return new[]
					{
						documentDelivery,
						new DocumentVisualizer.Presentation.Messaging(),
						new DataEditing(),
						new Tools(macroEvaluationContext),
						new Exit()
					};
				}

				var customCommandsProvider = new Lazy<CustomCommandsProvider>(GetCustomCommandsProvider);
				CustomCommandsProvider GetCustomCommandsProvider() => new CustomCommandsProvider(documentSupportable.GetSupporter(), lazyDataContext.Value);

				var lazyCommands = new Lazy<ICommand[]>(GetCommands);

				ICommand[] GetCommands()
				{
					// system commands
					var commands = lazyCommandProviders.Value
						.SelectMany(m => m.Commands)
						.ToDictionary(c => c.Id, c => c);

					// custom commands
					var customCommands = customCommandsProvider
						.Value
						.Commands
						?.ToArray()
						?? Enumerable.Empty<ICommand>();

					// override system commands
					foreach (var customCommand in customCommands)
					{
						commands[customCommand.Id] = customCommand;
					}

					return commands.Values.ToArray();
				}

				var lazyScope = CreateLazyScope(pivot, dynamicDataProvider, lazyDataContext, lazyServices, lazyCommands, lazyDocumentData);

				var lazyDocumentDescriptor = CreateLazyDocumentDescriptor(
					pivot,
					lazyServices,
					lazyCommands,
					lazyDocumentData,
					lazyTemplate,
					lazyScope,
					macroEvaluationContext);

				var lazyDocument = CreateLazyDocument(
					lazyTemplate,
					lazyServices,
					lazyScope,
					lazyDocumentDescriptor,
					lazyDocumentData,
					lazyCommands,
					macroEvaluationContext);

				var documentInfo = new DocumentInfo(lazyTemplate,
					lazyDocumentData,
					lazyServices,
					lazyDocumentDescriptor,
					lazyDocument);

				if (!documentInfo.CanDeliver())
				{
					continue;
				}

				foreach (var module in lazyCommandProviders.Value)
				{
					module.OnDocumentInfosCreated(documentInfo);
				}

				customCommandsProvider.Value.OnDocumentInfosCreated(documentInfo);

				result.Add(documentInfo);
			}

			return result;
		}

		Lazy<IDocument> CreateLazyDocument(Lazy<ITemplate> lazyTemplate,
			Lazy<IServiceContainer> lazyServices,
			Lazy<IMacroScope> lazyScope,
			Lazy<IDocumentDescriptor> lazyDescriptor,
			Lazy<IVisualizerDocumentData> lazyDocumentData,
			Lazy<ICommand[]> lazyCommands,
			IMacroEvaluationContext macroEvaluationContext)
		{
			return new Lazy<IDocument>(() =>
			{
				var descriptor = lazyDescriptor.Value;

				var template = lazyTemplate.Value;
				var services = lazyServices.Value;

				var documentData = lazyDocumentData.Value;
				var commands = lazyCommands.Value;

				var scope = lazyScope.Value;

				var logger = new ProgressNotificationLogger(services.Resolve<IEventBroker>(), descriptor.Name);

				var res = CreateDocument(template,
					services,
					scope,
					descriptor,
					documentData,
					commands,
					macroEvaluationContext,
					logger);

				if (res.IsLeft)
				{
					var document = new EmptyDocument(descriptor.Name, descriptor.DataContext);

					var source = new NotificationSource(descriptor.Name);
					var notification = new Notification(source,
						NotificationType.Error,
						res.Left);

					document.Add(notification);

					return document;
				}

				return HookUpEvents(res.Right, descriptor, services);
			});
		}

		IDocument HookUpEvents(IDocument document, IDocumentDescriptor descriptor, IServiceContainer services)
		{
			var supporter = parent.GetSupporter();

			var broker = services.Resolve<IEventBroker>();

			if (supporter is IPrintEventsProcessorProvider printEventsProcessorProvider)
			{
				broker
					.GetEvent<PrintJobsCreatedEvent>()
					.Subscribe(args => printEventsProcessorProvider.GetPrintEventsProcessor(args.Document)?.OnPrintJobsCreated(args.PrintJobs));
			}

			if (descriptor.MessageInstructions.AllowSendMessage)
			{
				broker
					.GetEvent<MessageSentEvent>()
					.Subscribe(args => supporter.GetMessageEventsProcessor(args.Document)?.OnMessageSent());
			}

			if (descriptor.MessageInstructions.AllowSendMessageWithdrawal)
			{
				broker
					.GetEvent<MessageWithdrawalSentEvent>()
					.Subscribe(args => supporter.GetMessageEventsProcessor(args.Document)?.OnMessageWithdrawalSent());
			}

			if (descriptor.MessageInstructions.AllowResetToOriginal)
			{
				broker
					.GetEvent<ResetToOriginalEvent>()
					.Subscribe(args => supporter.GetMessageEventsProcessor(args.Document)?.OnResetToOriginal());
			}

			return document;
		}

		Either<string, IDocument> CreateDocument(
			ITemplate template,
			IServiceContainer services,
			IMacroScope scope,
			IDocumentDescriptor descriptor,
			IVisualizerDocumentData documentData,
			ICommand[] commands,
			IMacroEvaluationContext macroEvaluationContext,
			ILogger logger)
		{
			IDocument document = null;

			if (template is IStandardTemplate standardTemplate)
			{
				var res = CreateStandardDocument(
					standardTemplate,
					services,
					scope,
					descriptor,
					documentData,
					commands,
					macroEvaluationContext,
					logger);

				if (res.IsLeft)
				{
					return res.Left;
				}

				document = res.Right;
			}

			if (template is IHouseBillTemplate houseBillTemplate)
			{
				var res = CreateHouseBillDocument(
					houseBillTemplate,
					services,
					scope,
					descriptor,
					documentData,
					commands,
					macroEvaluationContext,
					logger);

				if (res.IsLeft)
				{
					return res.Left;
				}

				document = res.Right;
			}

			if (document == null)
			{
				return Res.GetString("3fece4bd-9a68-43f7-a7f8-3c4057eefbc9", "Unsupported template '{0}'", template?.GetType().Name);
			}

			scope.RemoveVariable(VariableNames.Document);

			var pages = document
				.Pages
				.Select((page, index) => new Page(page.Name, index + 1))
				.ToArray();

			var documentWrapper = new Document(descriptor, (IStmALogParent)documentData, pages);
			scope.SetVariable(VariableNames.Document, documentWrapper);

			return new Either<string, IDocument>(document);
		}

		Either<string, IDocument> CreateStandardDocument(IStandardTemplate template,
			IServiceContainer services,
			IMacroScope scope,
			IDocumentDescriptor descriptor,
			IVisualizerDocumentData documentData,
			ICommand[] commands,
			IMacroEvaluationContext macroEvaluationContext,
			ILogger logger)
		{
			var parameters = new StandardDocumentBuilder.Parameters
			{
				Template = template,
				Services = services,
				Scope = scope,
				Descriptor = descriptor,
				DocumentData = documentData,
				Commands = commands,
				MacroEvaluationContext = macroEvaluationContext,
				Logger = logger
			};

			var builder = new StandardDocumentBuilder(parameters);

			return builder.Build();
		}

		Either<string, IDocument> CreateHouseBillDocument(IHouseBillTemplate template,
			IServiceContainer services,
			IMacroScope scope,
			IDocumentDescriptor descriptor,
			IVisualizerDocumentData documentData,
			ICommand[] commands,
			IMacroEvaluationContext macroEvaluationContext,
			ILogger logger)
		{
			var parameters = new HouseBillDocumentBuilder.Parameters
			{
				Template = template,
				Services = services,
				Scope = scope,
				Descriptor = descriptor,
				DocumentData = documentData,
				Commands = commands,
				MacroEvaluationContext = macroEvaluationContext,
				Logger = logger
			};

			var builder = new HouseBillDocumentBuilder(parameters);

			return builder.Build();
		}

		Lazy<IDocumentDescriptor> CreateLazyDocumentDescriptor(
			IDocumentPivot pivot,
			Lazy<IServiceContainer> lazyServices,
			Lazy<ICommand[]> lazyCommands,
			Lazy<IVisualizerDocumentData> lazyDocumentData,
			Lazy<ITemplate> lazyTemplate,
			Lazy<IMacroScope> lazyScope,
			IMacroEvaluationContext macroEvaluationContext)
		{
			return new Lazy<IDocumentDescriptor>(() =>
			{
				var services = lazyServices.Value;
				var commands = lazyCommands.Value;
				var documentData = lazyDocumentData.Value;
				var template = lazyTemplate.Value;
				var scope = lazyScope.Value;

				IDocumentDescriptor descriptor;

				if (template is IStandardTemplate standardTemplate)
				{
					descriptor = new StandardDocumentDescriptor(pivot, documentData, standardTemplate, scope, macroEvaluationContext);
				}
				else if (template is IHouseBillTemplate houseBillTemplate)
				{
					descriptor = new HouseBillDocumentDescriptor(parent, pivot, documentData, houseBillTemplate, services, commands);
				}
				else
				{
					throw new NotSupportedException();
				}

				var documentWrapper = new Document(descriptor, (IStmALogParent)documentData);
				scope.SetVariable(VariableNames.Document, documentWrapper);

				return descriptor;
			});
		}

		Lazy<IMacroScope> CreateLazyScope(
			IDocumentPivot pivot,
			DynamicDataProvider dynamicDataProvider,
			Lazy<string> lazyDataContext,
			Lazy<IServiceContainer> lazyServices,
			Lazy<ICommand[]> lazyCommands,
			Lazy<IVisualizerDocumentData> lazyDocumentData)
		{
			return new Lazy<IMacroScope>(() =>
			{
				var dataContext = lazyDataContext.Value;
				var services = lazyServices.Value;
				var commands = lazyCommands.Value;
				var logProvider = lazyDocumentData.Value as IStmALogProvider;

				var docDataParameters = new DocDataObjectParameters(pivot.DocumentTitle, pivot.DataStoreName, additionalData, logProvider);

				var result = dataContext != null
					? dynamicDataProvider.TryCreate(dataContext, Guid.Empty, docDataParameters)
					: Try<IDynamicData>.Failure(Res.GetString("c453dcc7-c866-4660-99e5-8c4b2db35a97", "Invalid Data Context."));

				if (result.IsFaulted)
				{
					HandleErrors(services, pivot.DocumentTitle, result.Message);
					return new MacroScope(new object().MakeDynamic());
				}

				var scope = new MacroScope(result.Value);

				foreach (var variable in GetVariables(services, commands))
				{
					scope.SetVariable(variable.Name, variable.Value);
				}

				return scope;
			});
		}

		Lazy<IServiceContainer> CreateLazyServices(IServiceContainer services, Lazy<ITemplate> lazyTemplate)
		{
			return new Lazy<IServiceContainer>(() =>
			{
				var dependantContainer = new DependentServiceContainer(services);
				var template = lazyTemplate.Value;

				IResourceAccessor resourceAccessor;

				if (template is IResourceProvider resourceProvider)
				{
					resourceAccessor = new ResourceAccessor(resourceProvider, services.Resolve<IResourceProvider>());
				}
				else
				{
					resourceAccessor = new ResourceAccessor(services.Resolve<IResourceProvider>());
				}

				dependantContainer.Register<IResourceAccessor>(resourceAccessor);
				return dependantContainer;
			});
		}

		void HandleErrors(IServiceContainer services, string documentName, string message)
		{
			var notifications = new List<INotification>();

			var source = new NotificationSource(documentName);

			notifications.Add(new Notification(source, NotificationType.Error, message));
			services.Resolve<IEventBroker>().Publish(new DocumentBuildErrorEvent(notifications));
		}

		#endregion

		#region Variables

		IEnumerable<IVariable> GetVariables(IServiceContainer services, ICommand[] commands)
		{
			yield return new Variable(VariableNames.Environment,
				new Enterprise.MasterFiles.Business.Macros.Environment());

			yield return new Variable(VariableNames.Console,
				new Enterprise.DocumentVisualizer.Presentation.Console(services.Resolve<IConsoleService>()));

			yield return new Variable(VariableNames.UI,
				new UserInterface(services));

			yield return new Variable(VariableNames.Commands,
				new MacroMap(commands.ToDictionary<ICommand, string, object>(action => action.Id, action => action)));

			yield return new Variable(VariableNames.Resources,
				new Resources(services.Resolve<IResourceAccessor>()));
		}

		#endregion

		#region MacroLibraries

		IEnumerable<IMacroLibrary> GetMacroLibraries(string dataContext)
		{
			yield return new MetaDataLibrary();
			yield return new StandardLibrary();
			yield return new DocumentLibrary();
			yield return new TableTextGeneratorLibrary();
			yield return new DataLibrary(parent.Factory);
			yield return new MasterFilesLibrary(parent.Factory);

			var supporter = parent.GetSupporter();
			var libraries = supporter?.GetLibraries(dataContext);

			if (libraries != null)
			{
				foreach (var library in libraries)
				{
					yield return library;
				}
			}
		}

		#endregion
	}
}

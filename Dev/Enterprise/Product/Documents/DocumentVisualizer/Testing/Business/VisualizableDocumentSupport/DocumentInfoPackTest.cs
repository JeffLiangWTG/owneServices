using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using IForwardingConsol = Enterprise.Integration.Forwarding.IForwardingConsol;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentInfoPackTest : TestCaseWithFactory
	{
		#region TestSkipCreatingInfoWithNoCopies

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSkipCreatingInfoWithNoCopies()
		{
			var parent = Factory.New<IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "hello";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "NO_COPIES";
			pivot1.SI_DataStoreName = "TestDataStore";
			pivot1.SI_SO = template.PK;
			pivot1.SI_SU = menuItem.PK;

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_DocumentTitle = "doc";
			pivot2.SI_DataStoreName = "TestDataStore";
			pivot2.SI_SO = template.PK;
			pivot2.SI_SU = menuItem.PK;

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, new[] { pivot1, pivot2 });

			AssertContainsExactElementsInAnyOrder("only one document has been created",
				new[]
				{
					"Test Document"
				},
				pack.DocumentInfos.Select(d => d.Descriptor.Name));
		}

		#endregion

		#region TestGroupPivots

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupPivots()
		{
			var template1 = Factory.New<VisualizerTemplate>();
			template1.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "doc";
			pivot1.SI_DataStoreName = "TestDataStore";
			pivot1.SI_SO = template1.PK;
			pivot1.SI_PrintCopyType = nameof(PrintCopyType.PRN);

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_DocumentTitle = "doc";
			pivot2.SI_DataStoreName = "TestDataStore";
			pivot2.SI_SO = template1.PK;
			pivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);

			AssertGroupPivots("both pivots got merged as they only have different SI_PrintCopyType",
			new[]
			{
				pivot1,
				pivot2
			},
			new[]
			{
				"doc"
			});

			var pivot3 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot3.SI_DocumentTitle = "doc 2";
			pivot3.SI_DataStoreName = "TestDataStore";
			pivot3.SI_SO = template1.PK;
			pivot3.SI_PrintCopyType = nameof(PrintCopyType.EML);

			AssertGroupPivots("2 pivots got merged as they only have different SI_PrintCopyType",
			new[]
			{
				pivot1,
				pivot2,
				pivot3
			},
			new[]
			{
				"doc",
				"doc 2"
			});

			var template2 = Factory.New<VisualizerTemplate>();
			template2.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot4 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot4.SI_DocumentTitle = "doc";
			pivot4.SI_DataStoreName = "TestDataStore";
			pivot4.SI_SO = template2.PK;
			pivot4.SI_PrintCopyType = nameof(PrintCopyType.EML);

			AssertGroupPivots("pivots didn't get merged as they have different template",
			new[]
			{
				pivot2,
				pivot4
			},
			new[]
			{
				"doc",
				"doc"
			});
		}

		void AssertGroupPivots(string message, VisualizerMenuTemplatePivot[] pivots, string[] expectedDocumentInfos)
		{
			var parent = Factory.New<IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "menu name";

			foreach (var pivot in pivots)
			{
				pivot.SI_SU = menuItem.PK;
			}

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, pivots);

			AssertContainsExactElementsInAnyOrder(message,
				expectedDocumentInfos,
				pack.DocumentInfos.Select(d => d.Descriptor.PrintInstructions.Title));
		}

		#endregion

		#region TestLazyCreateDocumentInfos

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLazyCreateDocumentInfos()
		{
			var parent = Factory.New<IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "hakuna matata";

			var template1 = Factory.New<VisualizerTemplate>();
			template1.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "doc1";
			pivot1.SI_DataStoreName = "7-11";
			pivot1.SI_SO = template1.PK;
			pivot1.SI_SU = menuItem.PK;

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_DocumentTitle = "doc2";
			pivot2.SI_DataStoreName = "Coles";
			pivot2.SI_SO = template1.PK;
			pivot2.SI_SU = menuItem.PK;

			Factory.Save();

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, new[] { pivot1, pivot2 });

			IDocumentInfo[] infos = null;

			AssertNoExceptionThrown("bad template hasn't been parsed yet, so no exception is expected", () =>
			{
				infos = pack.DocumentInfos.ToArray();
			});

			AssertEquals("2 infos have been created.", 2, infos.Length);
		}

		#endregion

		#region TestCustomCommands

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomCommands()
		{
			var parent = Factory.New<IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "hakuna matata";

			var template1 = Factory.New<VisualizerTemplate>();
			template1.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "doc1";
			pivot1.SI_DataStoreName = "7-11";
			pivot1.SI_SO = template1.PK;
			pivot1.SI_SU = menuItem.PK;

			Factory.Save();

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var customCommand = new DummyCommand();
			var dummySupporter = new DummyConsolVisualizableDocumentSupporterForCommandsTest();
			DummyConsolVisualizableDocumentSupporterForCommandsTest.CustomCommands = new ICommand[]
			{
				customCommand
			};

			using (ObjectFactory.Substitute("ForwardingConsolVisualizableDocumentSupporter", dummySupporter))
			{
				var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, new[] { pivot1 });

				var infos = pack.DocumentInfos.ToArray();
				AssertEquals("prerequisite: 1 info have been created.", 1, infos.Length);

				var document = infos[0].Document;

				var menuItems = infos[0].Descriptor.DisplayInstructions.MenuItems;

				var sendMessageParentCommand = menuItems
					.Single(mi => mi.Caption == "Send Message");

				var sendMessageCommand = sendMessageParentCommand
					.MenuItems
					.Single(mi => mi.Caption == "Send Message");

				AssertEquals("custom command was not yet invoked", false, customCommand.WasInvoked);

				sendMessageCommand.Invoke();

				AssertEquals("custom command was invoked", true, customCommand.WasInvoked);
			}
		}

		sealed class DummyCommand : ICommand
		{
			public string Id => CommandIds.SendMessage;
			public string Caption => "Send Message";
			public object Image => null;
			public bool IsEnabled { get; }
			public bool IsVisible { get; }
			public bool WasInvoked { get; private set; }

			public bool Invoke()
			{
				WasInvoked = true;
				return true;
			}

			public bool Invoke(MacroMap parameters)
			{
				WasInvoked = true;
				return true;
			}
		}

		sealed class DummyConsolVisualizableDocumentSupporterForCommandsTest : IVisualizableDocumentSupporter
		{
			public ISecurityCheckpoint CustomizeFormCheckpoint => null;
			public Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => (object)null;

			public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj) => bizObj;

			public IEnumerable<ICommand> GetCustomCommands(string dataContext) => CustomCommands;
			public static ICommand[] CustomCommands { get; set; }

			public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters) => new object();
			public object GetEventParent(IXmlEventValueObject universalEvent) => null;
			public IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;
			public IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;
			public IMessageLogCreator GetMessageLogCreator(IDocument document) => null;
			public IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;
			public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified) => new Either<string, ITopLevelDataObject>((ITopLevelDataObject)null);
			public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions) => null;

			public string GetMessageBroker() => string.Empty;
			public bool ShouldUseDraftWatermark(IDocument document) => false;
		}

		#endregion

		#region TestOnSavedEventDoesNotCreateDocumentsAgain

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnSavedEventDoesNotCreateDocumentsAgain()
		{
			var parent = Factory.New<IForwardingConsol>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "hakuna matata";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			const string dataStoreName = "7-11";

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "doc1";
			pivot1.SI_DataStoreName = dataStoreName;
			pivot1.SI_SO = template.PK;
			pivot1.SI_SU = menuItem.PK;

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_DocumentTitle = "doc2";
			pivot2.SI_DataStoreName = dataStoreName;
			pivot2.SI_SO = template.PK;
			pivot2.SI_SU = menuItem.PK;

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);
			var eventBroker = new EventBroker();

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(eventBroker);
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pivots = new PivotCollecton(pivot1, pivot2);
			var pack = new DocumentInfoPack(services, (BusinessObject)parent, null, menuItem.SU_MenuName, pivots);

			AssertEquals("pivots have not yet been enumerated to create documents", 0, pivots.NumberOfEnumerations);

			var documents = pack
				.DocumentInfos
				.Select(i => i.Document)
				.ToArray();

			AssertEquals("two documents have been created", 2, documents.Length);
			AssertEquals("pivots have been enumerated once", 1, pivots.NumberOfEnumerations);

			eventBroker.Publish(new SavedEvent(documents[0], dataStoreName));

			AssertEquals("pivots have not been enumerated again to create documents", 1, pivots.NumberOfEnumerations);
		}

		#endregion

		#region TestResetSynchsDocumentDatas

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSavingAndResetingSynchsDocumentDatas()
		{
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol[JobConsolSchema.JK_TransportMode] = "SEA";
			consol[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
			consol[JobConsolSchema.JK_RL_NKDischargePort] = "SGSIN";

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "hakuna matata";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			const string dataStoreName = "7-11";

			var pivot1 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot1.SI_DocumentTitle = "doc1";
			pivot1.SI_DataStoreName = dataStoreName;
			pivot1.SI_SO = template.PK;
			pivot1.SI_SU = menuItem.PK;

			var pivot2 = Factory.New<VisualizerMenuTemplatePivot>();
			pivot2.SI_DocumentTitle = "doc2";
			pivot2.SI_DataStoreName = dataStoreName;
			pivot2.SI_SO = template.PK;
			pivot2.SI_SU = menuItem.PK;

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);
			var eventBroker = new EventBroker();

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(eventBroker);
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pivots = new PivotCollecton(pivot1, pivot2);
			var pack = new DocumentInfoPack(services, consol, null, menuItem.SU_MenuName, pivots);

			AssertEquals("pivots have not yet been enumerated to create documents", 0, pivots.NumberOfEnumerations);

			var infos = pack
				.DocumentInfos
				.ToArray();

			var documents = infos
				.Select(i => i.Document)
				.ToArray();

			AssertEquals("both documents have the same data store",
				infos[0].DocumentData.Name,
				infos[1].DocumentData.Name);

			var cellDoc1 = (IDocumentCell)documents[0].GetCell(1, 2);
			var cellDoc2 = (IDocumentCell)documents[1].GetCell(1, 2);

			AssertEquals("Prerequsite; cell macro", "\"Load: <Modifiable(PortOfLoading.Code)>\"", cellDoc1.MacroExpression?.Text);
			AssertEquals("Prerequsite; cell macro", "\"Load: <Modifiable(PortOfLoading.Code)>\"", cellDoc2.MacroExpression?.Text);

			AssertEquals("doc 1 cell value", "Load: AUSYD", cellDoc1.Value);
			AssertEquals("doc 2 cell value", "Load: AUSYD", cellDoc2.Value);

			cellDoc1.EditableData.Single().Value.SetValue("NZAKL");

			// document datas are unsynched
			AssertEquals("doc 1 cell value", "Load: NZAKL", cellDoc1.Value);
			AssertEquals("doc 2 cell value", "Load: AUSYD", cellDoc2.Value);

			eventBroker.Publish(new SavedEvent(documents[0], dataStoreName));

			// document datas are synched
			AssertEquals("doc 1 cell value", "Load: NZAKL", cellDoc1.Value);
			AssertEquals("doc 2 cell value", "Load: NZAKL", cellDoc2.Value);

			cellDoc1.EditableData.Single().Value.CancelChanges();

			// document datas are unsynched
			AssertEquals("doc 1 cell value", "Load: AUSYD", cellDoc1.Value);
			AssertEquals("doc 2 cell value", "Load: NZAKL", cellDoc2.Value);

			eventBroker.Publish(new ResetEvent(documents[0], dataStoreName));

			// document datas are synched
			AssertEquals("doc 1 cell value", "Load: AUSYD", cellDoc1.Value);
			AssertEquals("doc 2 cell value", "Load: AUSYD", cellDoc2.Value);
		}

		#endregion

		#region TestCreatingPackForBizObjectNotImplementingVisualizableDocumentsSupportableAttribute

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatingPackForBizObjectNotImplementingVisualizableDocumentsSupportableAttribute()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "test document";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(TestTemplateFilePath);

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_DocumentTitle = "doc";
			pivot.SI_DataStoreName = "TestDataStore";
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var pack = new DocumentInfoPack(services, parent, null, menuItem.SU_MenuName, new[] { pivot });

			AssertContainsExactElementsInAnyOrder("no document has been created",
				Array.Empty<string>(),
				pack.DocumentInfos.Select(d => d.Descriptor.Name));
		}

		#endregion

		#region Implementaion

		sealed class PivotCollecton : IReadOnlyCollection<IStmMenuTemplatePivot>
		{
			public PivotCollecton(params IStmMenuTemplatePivot[] pivots)
			{
				this.pivots = pivots ?? Array.Empty<IStmMenuTemplatePivot>();
			}

			readonly IStmMenuTemplatePivot[] pivots;

			public IEnumerator<IStmMenuTemplatePivot> GetEnumerator()
			{
				NumberOfEnumerations++;
				return pivots.AsEnumerable().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int Count => pivots.Length;

			public int NumberOfEnumerations { get; private set; }
		}

		string TestTemplateFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Business\VisualizableDocumentSupport\TestTemplate.xls");
	}

	#endregion
}

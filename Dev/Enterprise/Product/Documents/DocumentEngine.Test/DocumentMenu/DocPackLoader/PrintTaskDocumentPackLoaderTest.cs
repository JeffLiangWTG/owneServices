using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class PrintTaskDocumentPackLoaderTest : PrintTaskDocumentPackLoaderTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildFormCommandsShouldNotAddEDocs()
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			dummy.Z0_NVarChar = "Jerry Test";

			var template = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = Helper.CreateDocCommand("Parent");
			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			var commandDocType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			command.AddEDoc(commandDocType).SX_Filter = "\"<Z0_NVarChar>\" == \"Jerry Test\"";

			var childCommand = Helper.CreateDocCommand("Child");
			childCommand.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			Helper.CreateMenuMenuPivot(command, childCommand);

			Factory.Save();

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, dummy);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
			}
			documentFactory.Save();

			using (PrintTask printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);

				AssertEquals("Total Count", 0, printTask.Count);
				loader.LoadAll();
				AssertEquals("Parent Count", 2, printTask[0].Count);

				foreach (IDeliverable item in printTask[0])
				{
					AssertEquals("The eDocs menu item should be the parent", item.MenuItem, command);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadEDocs_Documents()
		{
			AssertLoadEDocs(Core.Constants.StmMenuItemTypes.Documents, 2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadEDocs_OperationalActions()
		{
			AssertLoadEDocs(Core.Constants.StmMenuItemTypes.OperationalActions, 2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadEDocs_WebReports()
		{
			AssertLoadEDocs(Core.Constants.StmMenuItemTypes.WebReports, 2);
		}

		void AssertLoadEDocs(string menuItemType, int documentPackCount)
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			dummy.Z0_NVarChar = "Test";

			var template = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = Helper.CreateDocCommand("Parent");
			command.SU_MenuType = menuItemType;
			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			var commandDocType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			command.AddEDoc(commandDocType);

			Factory.Save();

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, dummy);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
			}
			documentFactory.Save();

			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				loader.LoadAll();
				AssertEquals($"{menuItemType} Document Pack Count", documentPackCount, printTask[0].Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommandsWithEDocs()
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			dummy.Z0_NVarChar = "Jerry Test";

			var template = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = Helper.CreateDocCommand("Parent");
			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			var commandDocType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			command.AddEDoc(commandDocType).SX_Filter = "\"<Z0_NVarChar>\" == \"Jerry Test\"";

			var childCommand = Helper.CreateDocCommand("Child");
			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);
			var childDocType = EDocsTestHelper.CreateDocType(Factory, "BBB", "UNL", "A Test Document BBB");
			childCommand.AddEDoc(childDocType).SX_Filter = "\"<Z0_NVarChar>\" == \"Jerry Test\"";

			Factory.Save();

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, dummy);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
			}
			documentFactory.Save();

			using (PrintTask printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);

				AssertEquals("Total Count", 0, printTask.Count);
				loader.LoadAll();
				AssertEquals("Total Count", 2, printTask.Count);

				var parentIncludeCount = printTask[0].Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("Parent Count", 2, printTask[0].Count);
				AssertEquals("Parent IncludeInPrint Count", 2, parentIncludeCount);
				AssertEquals("Parent OtherEDocsToAttach Count", 1, printTask[0].OtherEDocsToAttach.Count);

				var childIncludeCount = printTask[1].Cast<IDeliverable>().Count(x => x.IncludeInPrint);

				AssertEquals("Child Count", 2, printTask[1].Count);
				AssertEquals("Child IncludeInPrint Count", 2, childIncludeCount);
				AssertEquals("Child OtherEDocsToAttach Count", 1, printTask[1].OtherEDocsToAttach.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommandsRespectsFilter()
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			dummy.Z0_NVarChar = "Blah";
			var template = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = Helper.CreateDocCommand("Parent");

			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			var childCommand = Helper.CreateDocCommand("Child");
			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);

			var hiddenChildCommand = Helper.CreateDocCommand("Hidden Child");
			Helper.CreateMenuMenuPivot(command, hiddenChildCommand);
			Helper.CreateMenuTemplatePivot("Hidden Child", template, hiddenChildCommand);
			hiddenChildCommand.SU_FilterList = "CO=XXX";

			//Should never show as the DocumentSupport does not support DocBuilderInvoiceAsChildCommand
			var hiddenChildCommandDocBuilderInvoice = Helper.CreateDocCommand("DocBuilder Invoice");
			Helper.CreateMenuMenuPivot(command, hiddenChildCommandDocBuilderInvoice);
			Helper.CreateMenuTemplatePivot("Hidden DocBuilder Invoice child", template, hiddenChildCommandDocBuilderInvoice);
			hiddenChildCommandDocBuilderInvoice.SU_FilterList = "CTY=HideThisMenuItem";

			var childCommandWithMatchingFilter = Helper.CreateDocCommand("ChildMatching");
			Helper.CreateMenuMenuPivotWithFilter(command, childCommandWithMatchingFilter, "\"<Z0_NVarChar>\" == \"Blah\"");
			Helper.CreateMenuTemplatePivot("ChildMatching", template, childCommandWithMatchingFilter);

			var childCommandWithNonMatchingFilter = Helper.CreateDocCommand("ChildNonMatching");
			Helper.CreateMenuMenuPivotWithFilter(command, childCommandWithNonMatchingFilter, "\"<Z0_NVarChar>\" != \"Blah\"");
			Helper.CreateMenuTemplatePivot("ChildNonMatching", template, childCommandWithNonMatchingFilter);

			Factory.Save();

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Code = value, GlbCompany.CurrentCompany.GC_Code, "EDI"))
			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				AssertEquals("Count", 0, printTask.Count);
				loader.LoadChildCommands();
				AssertEquals("Count", 2, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", childCommand.PK, printTask[0].StmMenuCommand.PK);
				AssertEquals("[1].StmMenuCommand.PK", childCommandWithMatchingFilter.PK, printTask[1].StmMenuCommand.PK);
			}

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Code = value, GlbCompany.CurrentCompany.GC_Code, "XXX"))
			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				AssertEquals("Count", 0, printTask.Count);
				loader.LoadChildCommands();
				AssertEquals("Count", 3, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", childCommand.PK, printTask[0].StmMenuCommand.PK);
				AssertEquals("[1].StmMenuCommand.PK", hiddenChildCommand.PK, printTask[1].StmMenuCommand.PK);
				AssertEquals("[2].StmMenuCommand.PK", childCommandWithMatchingFilter.PK, printTask[2].StmMenuCommand.PK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommandsRespectsFilter_WithSupporterThatSupportsDocBuilderInvoiceAsChildCommand()
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObjectWithSupporterThatSupportDocBuilderInvoiceAsChildCommand>();
			var template = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "Template");
			var command = Helper.CreateDocCommand("Parent");

			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			var childCommand = Helper.CreateDocCommand("Child");
			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);

			var hiddenChildCommandDocBuilderInvoice = Helper.CreateDocCommand("DocBuilder Invoice");
			Helper.CreateMenuMenuPivot(command, hiddenChildCommandDocBuilderInvoice);
			Helper.CreateMenuTemplatePivot("Hidden DocBuilder Invoice child", template, hiddenChildCommandDocBuilderInvoice);
			hiddenChildCommandDocBuilderInvoice.SU_FilterList = "CTY=HideThisMenuItem";

			var hiddenChildCommand = Helper.CreateDocCommand("Hidden Child");
			Helper.CreateMenuMenuPivot(command, hiddenChildCommand);
			Helper.CreateMenuTemplatePivot("Hidden NON DocBuilder Invoice child", template, hiddenChildCommand);
			hiddenChildCommand.SU_FilterList = "CO=XXX";

			Factory.Save();

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Code = value, GlbCompany.CurrentCompany.GC_Code, "EDI"))
			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				AssertEquals("Count", 0, printTask.Count);
				loader.LoadChildCommands();
				AssertEquals("Count", 2, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", childCommand.PK, printTask[0].StmMenuCommand.PK);
				AssertEquals("[1].StmMenuCommand.PK", hiddenChildCommandDocBuilderInvoice.PK, printTask[1].StmMenuCommand.PK);
			}

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Code = value, GlbCompany.CurrentCompany.GC_Code, "XXX"))
			using (var printTask = new PrintTask())
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				AssertEquals("Count", 0, printTask.Count);
				loader.LoadChildCommands();
				AssertEquals("Count", 3, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", childCommand.PK, printTask[0].StmMenuCommand.PK);
				AssertEquals("[1].StmMenuCommand.PK", hiddenChildCommandDocBuilderInvoice.PK, printTask[1].StmMenuCommand.PK);
				AssertEquals("[2].StmMenuCommand.PK", hiddenChildCommand.PK, printTask[2].StmMenuCommand.PK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommands()
		{
			StmTemplateBase template = Helper.CreateTemplate(".MockEDocsProvider", "Template");
			DocumentCommand command = Helper.CreateDocCommand("Parent");

			command.Parent = MockEDocsProvider.New(Factory, BusinessContext.Shipment);

			DocumentCommand childCommand = Helper.CreateDocCommand("Child");
			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Parent", template, command);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);

			Factory.Save();

			using (PrintTask printTask = new PrintTask())
			{
				PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				AssertEquals("Count", 0, printTask.Count);
				loader.LoadChildCommands();
				AssertEquals("Count", 1, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", childCommand.PK, printTask[0].StmMenuCommand.PK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrintTaskDocumentPackLoader_LoadAll_DBHits()
		{
			var dummy = Factory.NewWithValidTestData<DocumentCommandTest.DocDummyBusinessObject>();
			var template = Helper.CreateTemplate(".MockEDocsProvider", "Template");
			var command = Helper.CreateDocCommand("Parent");
			command.Parent = dummy;
			Helper.CreateMenuTemplatePivot("Parent", template, command);

			for (var i = 0; i < 10; i++)
			{
				var childCommand = Helper.CreateDocCommand("Child" + i);
				Helper.CreateMenuMenuPivot(command, childCommand);
				Helper.CreateMenuTemplatePivot("Child" + i, template, childCommand);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyFromAnotherFactory = newFactory.Load<DocumentCommandTest.DocDummyBusinessObject>(dummy.PK);
			var commandFromAnotherFactory = DocumentCommand.GetDocumentCommand(newFactory, dummyFromAnotherFactory, "Parent");
			commandFromAnotherFactory.Parent = dummyFromAnotherFactory;

			using (var printTask = new PrintTask(commandFromAnotherFactory))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, commandFromAnotherFactory, null);
				loader.LoadAll();

				AssertEquals(2, newFactory.GetTableHitCount("StmMenuItem"));
				AssertEquals(2, newFactory.GetTableHitCount("StmMenuTemplatePivot"));
			}
		}

		protected override PrintTask CreateLoadedPrintTask(DocumentCommand command, UserControlProviderList userFieldList)
		{
			PrintTask result = new PrintTask();
			PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(result, command, userFieldList);
			loader.LoadAll();
			return result;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommands_DocumentSupportableShouldNotOverride()
		{
			DummyShipmentBusinessObject masterShipment = Factory.New<DummyShipmentBusinessObject>();
			masterShipment.Z0_Code = "MST";
			DummyShipmentBusinessObject subShipment = Factory.New<DummyShipmentBusinessObject>();
			subShipment.Z0_Code = "SUB";
			masterShipment.CoLoadShipments.Add(subShipment);

			StmTemplateBase template = Helper.CreateTemplate(".MockEDocsProvider", "Template");
			DocumentCommand command = Helper.CreateDocCommand("Parent");

			DocumentCommand childCommand = Helper.CreateDocCommand("Child", BusinessContext.SubShipment, null, ContactType.Consignee, 0);
			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Parent", template, command);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);

			Factory.Save();

			DocumentCommandCollection commands = new DocumentCommandCollection(masterShipment);
			commands.Load();

			using (PrintTask printTask = new PrintTask())
			{
				PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				loader.LoadChildCommands();
				DummyShipmentBusinessObject assertMasterShipment = (DummyShipmentBusinessObject)command.Parent;
				AssertEquals("Command's Supportable is MasterShipment should not be override.", "MST", assertMasterShipment.Z0_Code);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadChildCommandsFromProviderPlaceholder()
		{
			StmTemplateBase template = Helper.CreateTemplate(".MockEDocsProvider", "Template");
			DocumentCommand command = Helper.CreateDocCommand("Parent");
			StmMenuTemplatePivotBase pivot = Helper.CreateMenuTemplatePivot("Pivot", template, command);
			DocumentCommand childCommand = Helper.CreateDocCommand("Child");

			Helper.CreateMenuMenuPivot(command, childCommand);
			Helper.CreateMenuTemplatePivot("Parent", template, command);
			Helper.CreateMenuTemplatePivot("Child", template, childCommand);

			MockEDocsProvider consumer = MockEDocsProvider.New(Factory, BusinessContext.Shipment);
			MockEDocsProvider provider1 = MockEDocsProvider.New(Factory, BusinessContext.Consol);
			MockEDocsProvider provider2 = MockEDocsProvider.New(Factory, BusinessContext.Shipment);
			DocumentCommand provider1Placeholder = provider1.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(command);
			DocumentCommand provider2Placeholder = provider2.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(command);

			command.Parent = consumer;
			consumer.AddEDocsProvider(provider1);
			consumer.AddEDocsProvider(provider2);

			DocumentCommand provider1ChildCommand = Helper.CreateDocCommand("Provider1Child");
			DocumentCommand provider2ChildCommand1 = Helper.CreateDocCommand("Provider2Child1");
			DocumentCommand provider2ChildCommand2 = Helper.CreateDocCommand("Provider2Child2");

			Helper.CreateMenuTemplatePivot("Provider1ChildPivot", template, provider1ChildCommand);
			Helper.CreateMenuTemplatePivot("Provider2ChildPivot1", template, provider2ChildCommand1);
			Helper.CreateMenuTemplatePivot("Provider2ChildPivot2", template, provider2ChildCommand2);

			Helper.CreateMenuMenuPivot(provider1Placeholder, provider1ChildCommand);
			Helper.CreateMenuMenuPivot(provider2Placeholder, provider2ChildCommand1);
			Helper.CreateMenuMenuPivot(provider2Placeholder, provider2ChildCommand2);

			Factory.Save();

			using (PrintTask printTask = new PrintTask())
			{
				PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(printTask, command, null);
				loader.LoadChildCommandsFromProviderPlaceholder();
				AssertEquals("Count", 3, printTask.Count);
				AssertEquals("[0].StmMenuCommand.PK", provider1ChildCommand.PK, printTask[0].StmMenuCommand.PK);
				AssertEquals("[1].StmMenuCommand.PK", provider2ChildCommand1.PK, printTask[1].StmMenuCommand.PK);
				AssertEquals("[2].StmMenuCommand.PK", provider2ChildCommand2.PK, printTask[2].StmMenuCommand.PK);
			}
		}
	}
}

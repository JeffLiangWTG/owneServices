using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	abstract class PrintTaskDocumentPackLoaderTestCase : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRestrictedAndCreditCheckDocument()
		{
			DocumentRunner documentRunner = null;

			documentRunner = CreditCheckDocumentHelper(false, false);
			AssertEquals(2, documentRunner.LastRunReportInfosForTesting.Count);
			AssertEquals(false, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child1")));
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child2")));

			documentRunner = CreditCheckDocumentHelper(false, true);
			AssertEquals(3, documentRunner.LastRunReportInfosForTesting.Count);
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child1")));
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child2")));

			documentRunner = CreditCheckDocumentHelper(true, false);
			AssertEquals(0, documentRunner.LastRunReportInfosForTesting.Count);

			documentRunner = CreditCheckDocumentHelper(true, true);
			AssertEquals(3, documentRunner.LastRunReportInfosForTesting.Count);
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Parent")));
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child1")));
			AssertEquals(true, documentRunner.LastRunReportInfosForTesting.Any(a => a.PivotTitle.StartsWith("Child2")));
		}

		DocumentRunner CreditCheckDocumentHelper(bool withProtectedParentMenu, bool withAcceptToOverCreditLimit)
		{
			var fTemplateParent = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Consol), GetRandomString("ParentTemplate"));
			var fTemplateChild1 = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), GetRandomString("TemplateChild1"));
			var fTemplateChild2 = Helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), GetRandomString("TemplateChild2"));

			var parentName = GetRandomString("Parent");
			var parentMenu = Helper.CreateDocCommand(parentName);
			Helper.CreateMenuTemplatePivot(parentName, fTemplateParent, parentMenu);
			parentMenu.SU_DeliveryRestrictionType = withProtectedParentMenu ? nameof(DeliveryRestrictionType.CNH) : nameof(DeliveryRestrictionType.NON);

			var childName1 = GetRandomString("Child1");
			var childCommand1 = Helper.CreateDocCommand(childName1);
			Helper.CreateMenuMenuPivot(parentMenu, childCommand1);
			Helper.CreateMenuTemplatePivot(childName1, fTemplateChild1, childCommand1);
			childCommand1.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

			var childName2 = GetRandomString("Child2");
			var childCommand2 = Helper.CreateDocCommand(childName2);
			Helper.CreateMenuMenuPivot(parentMenu, childCommand2);
			Helper.CreateMenuTemplatePivot(childName2, fTemplateChild2, childCommand2);
			childCommand2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);

			var consol = Helper.CreateDummyConsol("Dummy");
			var shipment = Helper.CreateDummyShipment("DS1", "DC1", null);
			consol.Shipments.Add(shipment);

			((IDocumentSupportable)consol).DocumentSupporter.Initialise(new IDocumentEventsMock());

			parentMenu.Parent = consol;

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(new AmountOrPercentageBasedThreeLevelAuthorisationRequirement() { Amount = 1, Percentage = 0, Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired });
			collection.Add(new AmountOrPercentageBasedThreeLevelAuthorisationRequirement() { Amount = 1, Percentage = 0, Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly });

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			((Enterprise.MasterFiles.CreditControl.Business.ICreditControlledDocumentDelivery)shipment).GetDocumentLogin += (object sender, Enterprise.MasterFiles.CreditControl.Business.SecurityLoginEventArgs e) =>
			{
				((Enterprise.MasterFiles.CreditControl.Business.ICreditControlledDocumentDelivery)consol).RaiseOnGetDocumentLogin(e);
			};

			Factory.Save();

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			var mockCreditControlManager = new Mock<IDocumentDeliveryCreditControlManager>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			using (ObjectFactory.Substitute<IDocumentDeliveryCreditControlManager>(mockCreditControlManager.Object))
			{
				mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(false);
				mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(false);
				mockCreditControlManager.Setup(m => m.GetDocumentDeliveryStatusForCreditManagement(It.IsAny<BusinessObject>(), It.IsAny<string>(), It.IsAny<ZGuid>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(withAcceptToOverCreditLimit ? ZString.Empty : new ZString("Printing Document canceled."));

				var runner = new DocumentRunner();
				runner.Run(parentMenu);
				return runner;
			}
		}

		string GetRandomString(string prefix)
		{
			return prefix + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestChildDocumentSupportablesForNullChildCollection()
		{
			StmTemplateBase template = Helper.CreateTemplate(Core.Constants.DataContext.Consol, "Template");
			DocumentCommand command = Factory.New<DocumentCommand>();
			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			StmMenuTemplatePivot menuTemplatePivot = Helper.CreateMenuTemplatePivot("MenuTemplatePivot", template, childCommand);
			StmMenuMenuPivot menuMenuPivot = Helper.CreateMenuMenuPivot(command, childCommand);
			command.Parent = Helper.CreateDummyConsol("Dummy");
			childCommand.SU_BusinessContext = nameof(BusinessContext.Consol);

			Factory.Save();

			PrintTask printTask = CreateLoadedPrintTask(command, null);
			AssertEquals("Count", 1, printTask.Count);
			AssertEquals("[0].StmMenuCommand.Parent.DocumentSupporter.BusinessContext", BusinessContext.Consol, ((DocumentCommand)printTask[0].StmMenuCommand).Parent.DocumentSupporter.BusinessContext);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRecursionBug()
		{
			DocumentCommand shipmentCommand1 = Helper.CreateDocCommand("ShipmentCommand1");
			DocumentCommand shipmentCommand2 = Helper.CreateDocCommand("ShipmentCommand2");
			DocumentCommand shipmentCommand3 = Helper.CreateDocCommand("ShipmentCommand3");
			StmTemplateBase template = Helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			StmMenuMenuPivot shipment1ToShipment2Pivot = Helper.CreateMenuMenuPivot(shipmentCommand1, shipmentCommand2);
			StmMenuMenuPivot shipment1ToShipment3Pivot = Helper.CreateMenuMenuPivot(shipmentCommand1, shipmentCommand3);
			StmMenuMenuPivot shipment2ToShipment1Pivot1 = Helper.CreateMenuMenuPivot(shipmentCommand2, shipmentCommand1);
			StmMenuTemplatePivot templatePivot1 = Helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template, shipmentCommand1, 1);
			StmMenuTemplatePivot templatePivot2 = Helper.CreateMenuTemplatePivot("Pub System Shipment Document 2", template, shipmentCommand1, 2);

			Factory.Save();

			DummyShipmentBusinessObject dummyShipment = Factory.New<DummyShipmentBusinessObject>();
			shipmentCommand1.Parent = dummyShipment;
			using (PrintTask printTask = CreateLoadedPrintTask(shipmentCommand1, null))
			{
				AssertEquals("Count", 1, printTask.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSingleDocumentPack()
		{
			DummyShipmentBusinessObject dummyShipment = Helper.CreateDummyShipment("DS1", "DC1", null);
			StmTemplateBase template = Helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			DocumentCommand command = Helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			StmMenuTemplatePivotBase pivot = Helper.CreateMenuTemplatePivot("Pub System Shipment Document", template, command);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyShipmentBusinessObject loadedDummyShipment = newFactory.Load<DummyShipmentBusinessObject>(dummyShipment.PK);
			AssertEquals("dummyShipment.DocumentCommands.Count", 1, loadedDummyShipment.DocumentCommands.Count);

			DocumentCommand testCommand = loadedDummyShipment.DocumentCommands[0];
			using (PrintTask printTask = CreateLoadedPrintTask(testCommand, null))
			{
				AssertEquals("Count", 1, printTask.Count);
				AssertPack(printTask, 0, command, dummyShipment, pivot, template, null);
				AssertEquals("[0].StmMenuCommand.Parent.DocumentSupporter.PK", dummyShipment.PK, ((DocumentCommand)printTask[0].StmMenuCommand).Parent.DocumentSupporter.PK);
			}
		}

		#region Document With Child Menus

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithChildMenus()
		{
			DocumentWithChildMenusTestHelper helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(true);

			DummyConsolBusinessObject loadedDummyConsol = new BusinessObjectFactory().Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);
			AssertEquals("loadedDummyConsol.DocumentCommands.Count", 1, loadedDummyConsol.DocumentCommands.Count);
			using (PrintTask printTask = CreateLoadedPrintTask(loadedDummyConsol.DocumentCommands[0], helper.CreateUserFieldList()))
			{
				AssertEquals("Count", 3, printTask.Count);
				AssertPack(printTask, 0, helper.PubSysShipmentCommand, helper.DummyShipment1, helper.PubSysShipmentPivot, helper.Template, "Shipment1");
				AssertPack(printTask, 1, helper.PubSysShipmentCommand, helper.DummyShipment2, helper.PubSysShipmentPivot, helper.Template, "Shipment2");
				AssertPack(printTask, 2, helper.PubSysShipmentCommand, helper.DummyShipment3, helper.PubSysShipmentPivot, helper.Template, "Shipment3");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithChildMenusThatHaveChildMenus()
		{
			DocumentWithChildMenusTestHelper helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(false);

			DocumentCommand pubConsolListCommand = Helper.CreateDocCommand("Consol List", BusinessContext.CFSLoadList, null, null, 2);
			StmMenuMenuPivot menuPivot2 = Helper.CreateMenuMenuPivot(pubConsolListCommand, helper.PubSysConsolCommand);
			DummyConsolListBusinessObject dummyConsolList = Factory.New<DummyConsolListBusinessObject>();
			dummyConsolList.Z0_Code = "DCL";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyConsolListBusinessObject loadedDummyConsolList = newFactory.Load<DummyConsolListBusinessObject>(dummyConsolList.PK);
			DummyConsolBusinessObject loadedDummyConsol = newFactory.Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);
			loadedDummyConsolList.ConsolToReturn = loadedDummyConsol;
			AssertEquals("loadedDummyConsolList.DocumentCommands.Count", 1, loadedDummyConsolList.DocumentCommands.Count);

			using (PrintTask printTask = CreateLoadedPrintTask(loadedDummyConsolList.DocumentCommands[0], helper.CreateUserFieldList()))
			{
				AssertEquals("Count", 3, printTask.Count);
				AssertEquals("[0].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", loadedDummyConsol, ((DummyShipmentBusinessObject)((DocumentCommand)printTask[0].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
				AssertPack(printTask, 0, helper.PubSysShipmentCommand, helper.DummyShipment1, helper.PubSysShipmentPivot, helper.Template, "Shipment1");
				AssertEquals("[1].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", loadedDummyConsol, ((DummyShipmentBusinessObject)((DocumentCommand)printTask[1].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
				AssertPack(printTask, 1, helper.PubSysShipmentCommand, helper.DummyShipment2, helper.PubSysShipmentPivot, helper.Template, "Shipment2");
				AssertEquals("[2].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", loadedDummyConsol, ((DummyShipmentBusinessObject)((DocumentCommand)printTask[2].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
				AssertPack(printTask, 2, helper.PubSysShipmentCommand, helper.DummyShipment3, helper.PubSysShipmentPivot, helper.Template, "Shipment3");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithChildMenusThatHaveChildMenusForEUCountries()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				DocumentWithChildMenusTestHelper helper = new DocumentWithChildMenusTestHelper(this);
				helper.CreateTestData(false);

				DocumentCommand pubConsolListCommand = Helper.CreateDocCommand("Consol List", BusinessContext.CFSLoadList, null, null, 2);
				StmMenuMenuPivot menuPivot2 = Helper.CreateMenuMenuPivot(pubConsolListCommand, helper.PubSysConsolCommand);
				DummyConsolListBusinessObject dummyConsolList = Factory.New<DummyConsolListBusinessObject>();
				dummyConsolList.Z0_Code = "DCL";

				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				DummyConsolListBusinessObject loadedDummyConsolList = newFactory.Load<DummyConsolListBusinessObject>(dummyConsolList.PK);
				DummyConsolBusinessObject loadedDummyConsol = newFactory.Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);
				loadedDummyConsolList.ConsolToReturn = loadedDummyConsol;
				AssertEquals("loadedDummyConsolList.DocumentCommands.Count", 1, loadedDummyConsolList.DocumentCommands.Count);

				using (PrintTask printTask = CreateLoadedPrintTask(loadedDummyConsolList.DocumentCommands[0], helper.CreateUserFieldList()))
				{
					AssertEquals("Count", 3, printTask.Count);
					AssertType<DummyShipmentBusinessObject>("[0].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", ((DummyShipmentBusinessObject)((DocumentCommand)printTask[0].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
					AssertPack(printTask, 0, helper.PubSysShipmentCommand, helper.DummyShipment1, helper.PubSysShipmentPivot, helper.Template, "Shipment1");
					AssertType<DummyShipmentBusinessObject>("[1].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", ((DummyShipmentBusinessObject)((DocumentCommand)printTask[1].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
					AssertPack(printTask, 1, helper.PubSysShipmentCommand, helper.DummyShipment2, helper.PubSysShipmentPivot, helper.Template, "Shipment2");
					AssertType<DummyShipmentBusinessObject>("[2].StmMenuCommand.Parent.ParentBusinessObjectCalledByGetDocumentTitlesForPivot", ((DummyShipmentBusinessObject)((DocumentCommand)printTask[2].StmMenuCommand).Parent).ParentBusinessObjectCalledByGetDocumentTitlesForPivot);
					AssertPack(printTask, 2, helper.PubSysShipmentCommand, helper.DummyShipment3, helper.PubSysShipmentPivot, helper.Template, "Shipment3");
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithChildMenusWithDocPackFlagOnMenuItemSet()
		{
			DocumentWithChildMenusTestHelper helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(true);

			DummyConsolBusinessObject loadedConsol = new BusinessObjectFactory().Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);
			AssertEquals("loadedConsol.DocumentCommands.Count", 1, loadedConsol.DocumentCommands.Count);

			DocumentCommand command = loadedConsol.DocumentCommands[0];
			command.SU_IsDocPack = true;
			command.SU_ContactType = ContactType.NoContactType.Code;

			StmMenuMenuPivotBase childCommandPivot = command.ChildMenus[0];
			DocumentCommand childCommand = command.Factory.Load<DocumentCommand>(childCommandPivot.SF_SU_Outward);
			childCommand.SU_ContactType = ContactType.Consignee.Code;

			using (PrintTask printTask = CreateLoadedPrintTask(command, null))
			{
				AssertEquals("Count", 1, printTask.Count);
				AssertNotEquals("Should set SourcePivotPK on IDeliverable", default(ZGuid), printTask[0][0].SourcePivotPK);
			}

			AssertEquals("ChildCommand SU_ContactType should not be altered", childCommand.SU_ContactType, ContactType.Consignee.Code);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithChildMenusWithMoreThanOneChildRunningTwice()
		{
			DocumentWithChildMenusTestHelper helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(true);

			DummyConsolBusinessObject loadedConsol = new BusinessObjectFactory().Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);
			AssertEquals("loadedConsol.DocumentCommands.Count", 1, loadedConsol.DocumentCommands.Count);

			using (PrintTask printTask = CreateLoadedPrintTask(loadedConsol.DocumentCommands[0], helper.CreateUserFieldList()))
			{
				AssertEquals("Count", 3, printTask.Count);
				AssertPack(printTask, 0, helper.PubSysShipmentCommand, helper.DummyShipment1, helper.PubSysShipmentPivot, helper.Template, "Shipment1");
				AssertPack(printTask, 1, helper.PubSysShipmentCommand, helper.DummyShipment2, helper.PubSysShipmentPivot, helper.Template, "Shipment2");
				AssertPack(printTask, 2, helper.PubSysShipmentCommand, helper.DummyShipment3, helper.PubSysShipmentPivot, helper.Template, "Shipment3");
			}

			using (PrintTask printTask = CreateLoadedPrintTask(loadedConsol.DocumentCommands[0], helper.CreateUserFieldList()))
			{
				AssertEquals("Count", 3, printTask.Count);
				AssertPack(printTask, 0, helper.PubSysShipmentCommand, helper.DummyShipment1, helper.PubSysShipmentPivot, helper.Template, "Shipment1");
				AssertPack(printTask, 1, helper.PubSysShipmentCommand, helper.DummyShipment2, helper.PubSysShipmentPivot, helper.Template, "Shipment2");
				AssertPack(printTask, 2, helper.PubSysShipmentCommand, helper.DummyShipment3, helper.PubSysShipmentPivot, helper.Template, "Shipment3");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidMenuDataContext()
		{
			var helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(save: false, menuMenuPivotsToCreate: 3);
			var loadedConsol = Factory.Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);

			var command = loadedConsol.DocumentCommands[0];
			command.SU_MenuName = "The Bad Document";
			command.SU_MenuDataContext = "Lorem ipsum dolor sit amet";

			using (var printTask = new PrintTask())
			{
				var expectedErrorMessage = "The Email Subject Data Context of \"Lorem ipsum dolor sit amet\" for document \"The Bad Document\" is not valid. Please check the Delivery Options of this document";
				AssertExceptionThrown("Exception for invalid MenuDataContext should be thrown", typeof(DocumentMenuException), expectedErrorMessage, () => new PrintTaskDocumentPackLoader(printTask, command, helper.CreateUserFieldList()));
			}
		}

		#region MenuMenuPivot filters

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentWithFilteredChildMenus_EmptyFiltersPass_PopulatedFiltersMustEvaluateToTrue()
		{
			var helper = new DocumentWithChildMenusTestHelper(this);
			helper.CreateTestData(save: false, menuMenuPivotsToCreate: 3);
			var loadedConsol = Factory.Load<DummyConsolBusinessObject>(helper.DummyConsol.PK);

			var command = loadedConsol.DocumentCommands[0];

			CreatePrintTaskWithMenuMenuFiltersAndAssertChildCommandsLoaded(command, helper, 1, "", "\"0\" == \"1\"", "\"1\" == \"0\"");
			CreatePrintTaskWithMenuMenuFiltersAndAssertChildCommandsLoaded(command, helper, 2, "", "\"1\" == \"1\"", "\"1\" == \"0\"");
			CreatePrintTaskWithMenuMenuFiltersAndAssertChildCommandsLoaded(command, helper, 3, "\"1\" == \"1\"", "\"1\" == \"1\"", "\"1\" == \"1\"");
		}

		void CreatePrintTaskWithMenuMenuFiltersAndAssertChildCommandsLoaded(DocumentCommand command, DocumentWithChildMenusTestHelper helper, int numberOfChildCommandsLoaded, params string[] filters)
		{
			command.SU_MenuDataContext = "BusinessObject";
			for (int i = 0; i < filters.Length; i++)
			{
				command.ChildMenus[i].SF_Filter = filters[i];
			}

			using (var printTask = new PrintTask())
			{
				PrintTaskDocumentPackLoader loader = new PrintTaskDocumentPackLoader(printTask, command, helper.CreateUserFieldList());
				loader.LoadAll();
				AssertEquals(numberOfChildCommandsLoaded, loader.ChildCommandMetFilterCount);
			}
		}

		#endregion

		#region DocumentWithChildMenusTestHelper

		class DocumentWithChildMenusTestHelper
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyConsolBusinessObject dummyConsol;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyShipmentBusinessObject dummyShipment1;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyShipmentBusinessObject dummyShipment2;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyShipmentBusinessObject dummyShipment3;
			readonly PrintTaskDocumentPackLoaderTestCase parent;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DocumentCommand pubSysConsolCommand;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DocumentCommand pubSysShipmentCommand;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			StmMenuTemplatePivotBase pubSysShipmentPivot;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			StmTemplateBase template;

			public DocumentWithChildMenusTestHelper(PrintTaskDocumentPackLoaderTestCase parent)
			{
				this.parent = parent;
			}

			public DummyConsolBusinessObject DummyConsol
			{
				get { return dummyConsol; }
			}

			public DummyShipmentBusinessObject DummyShipment1
			{
				get { return dummyShipment1; }
			}

			public DummyShipmentBusinessObject DummyShipment2
			{
				get { return dummyShipment2; }
			}

			public DummyShipmentBusinessObject DummyShipment3
			{
				get { return dummyShipment3; }
			}

			public DocumentCommand PubSysConsolCommand
			{
				get { return pubSysConsolCommand; }
			}

			public DocumentCommand PubSysShipmentCommand
			{
				get { return pubSysShipmentCommand; }
			}

			public StmMenuTemplatePivotBase PubSysShipmentPivot
			{
				get { return pubSysShipmentPivot; }
			}

			public StmTemplateBase Template
			{
				get { return template; }
			}

			public void CreateTestData(bool save, int menuMenuPivotsToCreate = 1)
			{
				template = parent.Helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
				pubSysConsolCommand = parent.Helper.CreateDocCommand("Pub System Consol Document", BusinessContext.Consol, null, null, 2);
				pubSysShipmentCommand = null;
				for (int i = 0; i < menuMenuPivotsToCreate; i++)
				{
					var shipmentCommand = parent.Helper.CreateDocCommand("Pub System Shipment Document" + i, BusinessContext.Shipment, null, ContactType.Consignee, 1);
					if (pubSysShipmentCommand == null)
					{
						pubSysShipmentCommand = shipmentCommand;
					}
					var menuPivot = parent.Helper.CreateMenuMenuPivot(pubSysConsolCommand, shipmentCommand);
				}
				pubSysShipmentPivot = parent.Helper.CreateMenuTemplatePivot("Pub System Shipment Document", template, pubSysShipmentCommand);

				dummyShipment1 = parent.Helper.CreateDummyShipment("DS1", "DC1", "Shipment1");
				dummyShipment2 = parent.Helper.CreateDummyShipment("DS2", "DC1", "Shipment2");
				dummyShipment3 = parent.Helper.CreateDummyShipment("DS3", "DC1", "Shipment3");

				dummyConsol = parent.Helper.CreateDummyConsol("DC1");

				if (save)
				{
					parent.Factory.Save();
				}
			}

			public UserControlProviderList CreateUserFieldList()
			{
				UserControlProviderList result = new UserControlProviderList();
				TextField field = new TextField(new BusinessObjectFactory());
				field.DisplayName = "Field1";
				result.Add(field);
				return result;
			}
		}

		#endregion

		#endregion

		#region Is Doc Pack

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsDocPack()
		{
			IsDocPackTestHelper helper = new IsDocPackTestHelper(this);
			helper.CreateTestData(false);
			helper.ConsolCommand.SU_IsDocPack = true;
			Factory.Save();
			using (PrintTask printTask = CreateLoadedPrintTask(helper.ConsolCommand, null))
			{
				AssertEquals("Count", 1, printTask.Count);
				AssertNotEquals("Should set SourcePivotPK on IDeliverable", default(ZGuid), printTask[0][0].SourcePivotPK);
				helper.AssertPack(printTask, 0, helper.ConsolCommand, helper.DummyConsol.DocumentSupporter);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsNotDocPack()
		{
			IsDocPackTestHelper helper = new IsDocPackTestHelper(this);
			helper.CreateTestData(true);
			using (PrintTask printTask = CreateLoadedPrintTask(helper.ConsolCommand, null))
			{
				AssertEquals("Count", 2, printTask.Count);
				helper.AssertPack(printTask, 0, helper.ConsolCommand, helper.DummyConsol.DocumentSupporter);
				helper.AssertPack(printTask, 1, helper.ShipmentCommand, helper.DummyShipment.DocumentSupporter);
			}
		}

		#region IsDocPackTestHelper

		internal class IsDocPackTestHelper
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DocumentCommand consolCommand;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyConsolBusinessObject dummyConsol;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DummyShipmentBusinessObject dummyShipment;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			DocumentCommand shipmentCommand;
			readonly PrintTaskDocumentPackLoaderTestCase parent;

			public IsDocPackTestHelper(PrintTaskDocumentPackLoaderTestCase parent)
			{
				this.parent = parent;
			}

			public DocumentCommand ConsolCommand
			{
				get { return consolCommand; }
			}

			public DummyConsolBusinessObject DummyConsol
			{
				get { return dummyConsol; }
			}

			public DummyShipmentBusinessObject DummyShipment
			{
				get { return dummyShipment; }
			}

			public DocumentCommand ShipmentCommand
			{
				get { return shipmentCommand; }
			}

			public void AssertPack(PrintTask printTask, byte index, DocumentCommand expectedCommand, DocumentSupporter expectedSupporter)
			{
				DocumentPack pack = printTask[index];
				string id = '[' + index.ToString() + ']';
				AssertEquals(id + ".StmMenuCommand", expectedCommand, pack.StmMenuCommand);
				AssertEquals(id + ".StmMenuCommand.SU_ContactType", expectedCommand.SU_ContactType, pack.StmMenuCommand.SU_ContactType);
				AssertEquals(id + ".DeliveryFilter", expectedSupporter, pack.DocumentSupporter);
			}

			public void CreateTestData(bool save)
			{
				StmTemplateBase consolTemplate = parent.Helper.CreateTemplate(Core.Constants.DataContext.Consol, "Consol Template");
				StmTemplateBase shipmentTemplate = parent.Helper.CreateTemplate(Core.Constants.DataContext.Shipment, "Shipment Template");
				consolCommand = parent.Helper.CreateDocCommand("Consol Document Command", BusinessContext.Consol, Core.Constants.DataContext.Consol, ContactType.ExportFreightAgent, 1);
				shipmentCommand = parent.Helper.CreateDocCommand("Shipment Document Command", BusinessContext.Shipment, Enterprise.Core.Constants.DataContext.Shipment, ContactType.Consignee, 2);
				StmMenuTemplatePivotBase consolTemplatePivot = parent.Helper.CreateMenuTemplatePivot("ConsolDocument", consolTemplate, consolCommand);
				StmMenuTemplatePivotBase shipmentTemplatePivot = parent.Helper.CreateMenuTemplatePivot("Shipment Document", shipmentTemplate, shipmentCommand);
				StmMenuMenuPivot consolMenuPivot = parent.Helper.CreateMenuMenuPivot(consolCommand, shipmentCommand);
				dummyConsol = parent.Helper.CreateDummyConsol("DC1");
				dummyShipment = parent.Helper.CreateDummyShipment("DS1", dummyConsol.Z0_Code, null);
				dummyConsol.DocumentSupporter = new DummyConsolBusinessObjectDocumentSupporter(dummyConsol);
				dummyShipment.DocumentSupporter = new DummyShipmentBusinessObjectDocumentSupporter(dummyShipment);
				consolCommand.Parent = dummyConsol;
				shipmentCommand.Parent = dummyShipment;

				if (save)
				{
					parent.Factory.Save();
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract PrintTask CreateLoadedPrintTask(DocumentCommand command, UserControlProviderList userFieldList);

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
			helper = new PrintTaskDocumentPackTestHelper(Factory);
		}

		protected PrintTaskDocumentPackTestHelper Helper
		{
			get { return helper; }
		}
		PrintTaskDocumentPackTestHelper helper;

		void AssertPack(PrintTask printTask, byte index, DocumentCommand expectedCommand, DummyBusinessObject expectedDocDataProvider, StmMenuTemplatePivot expectedPivot, StmTemplateBase expectedTemplate, string expectedFilterField)
		{
			string id = '[' + index.ToString() + ']';
			DocumentPack pack = printTask[index];
			AssertEquals(id + ".StmMenuCommand.SU_MenuName", expectedCommand.SU_MenuName, pack.StmMenuCommand.SU_MenuName);
			AssertEquals(id + ".Organisation.PK", DummyShipmentBusinessObject.TestOrganisationPK, pack.Organisation.PK);
			AssertNotNull(id + ".StmMenuCommand.Parent", ((DocumentCommand)pack.StmMenuCommand).Parent);

			Report report = (Report)pack[0];
			AssertEquals(id + "[0].BODocDataProvider.Code", expectedDocDataProvider.Z0_Code, ((DummyShipmentDocumentWrapper)report.BODocDataProvider).Code);
			AssertEquals(id + "[0].Name", expectedPivot.SI_DocumentTitle, report.Name);
			AssertEquals(id + "[0].Template.TemplateName", expectedTemplate.SO_Name, report.Template.TemplateName);

			if (expectedFilterField != null)
			{
				UserControlProviderList fieldList = report.UserDefinedFieldValueList;
				FilterField filterField = fieldList["I'm on alpha shipment"];
				AssertNotNull("Report UserDefinedFieldValueList['I'm on alpha shipment'] should not be null", filterField);
				AssertEquals("Report UserDefinedFieldValueList['I'm on alpha shipment']", expectedFilterField, ((FilterFieldValueSerialisable)filterField).ValueAsStringForSerialisation);
			}
		}

		#endregion

		#region DummyConsolListBusinessObject

		class DummyConsolListBusinessObject : DummyBusinessObject, IDocumentSupportable
		{
			DummyConsolBusinessObject consolToReturn;
			DocumentCommandCollection documentCommands;
			DocumentSupporter documentSupporter;

			public DummyConsolListBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyConsolBusinessObject ConsolToReturn
			{
				get { return consolToReturn; }
				set { consolToReturn = value; }
			}

			public DocumentCommandCollection DocumentCommands
			{
				get
				{
					if (documentCommands == null)
					{
						documentCommands = new DocumentCommandCollection(this);
						documentCommands.Load();
					}
					return documentCommands;
				}
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return documentSupporter ?? (documentSupporter = new DummyConsolListBusinessObjectDocumentSupporter(this)); }
			}
		}

		#endregion

		#region DummyConsolListBusinessObjectDocumentSupporter

		class DummyConsolListBusinessObjectDocumentSupporter : DocumentSupporter
		{
			public DummyConsolListBusinessObjectDocumentSupporter(DummyConsolListBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.CFSLoadList; }
			}

			public new DummyConsolListBusinessObject BusinessObject
			{
				get { return (DummyConsolListBusinessObject)base.BusinessObject; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.None; }
			}

			public override BusinessContext[] SupportedChildBusinessContexts
			{
				get { return new BusinessContext[] { BusinessContext.Consol }; }
			}

			public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
			{
				IDocumentSupportable[] result = null;
				if (businessContext == BusinessContext.Consol)
				{
					result = new IDocumentSupportable[] { BusinessObject.ConsolToReturn };
				}
				return result;
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return null;
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return null;
			}
		}

		#endregion
	}
}

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalModule))]
	public class GLJournalModuleTest : ZModuleBasherTest
	{
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			collection.Add(Factory.NewWithValidTestData<GLJournal>(TestBusinessObjectKind.MinimumRequiredToSave));
		}

		public void TestApprovalRequestStatusColumn_WithApprovalRequestEnabled()
		{
			using (ZForm form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(Module.EmbeddedControl);
				form.Show();
				var columnStyle = Module.DisplayGrid.GetColumnStyle(GLJournal.Schema.ApprovalRequestStatus);
				Assert("IsUnavailable", !columnStyle.IsUnavailable);
			}
		}

		public void TestExportJournalCSVEventHandler()
		{
			Module.ExportJournalCSVEventHandler_ForTestOnly(null, EventArgs.Empty);
			AssertEquals("Last shown from should be CSVExportGLTransactions Form", "CsvExportGLTransactionForm", ZFormModaliser.LastFormShownDialogForTest.Name);
		}

		public void TestImportJournalCSVEventHandler()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			Module.ImportJournalXMLEventHandler_ForTestOnly(null, EventArgs.Empty);
			AssertEquals(ZFormModaliser.ResultToReturnFromShowDialog, DialogResult.Cancel);
		}

		public void TestCSVFileBeingUsedbyAnotherProcess()
		{
			string fileName = Env.TempPath + "test.csv";

			try
			{
				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("-------------------");
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (Stream fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					Module.ImportJournalCSVEventHandler_ForTestOnly(null, null);
				}

				AssertEquals("The process cannot access the file '" + fileName + "' because it is being used by another process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		public void TestImportJournalXMLEventHandler()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			Module.ImportJournalCSVEventHandler_ForTestOnly(null, EventArgs.Empty);
			AssertEquals(ZFormModaliser.ResultToReturnFromShowDialog, DialogResult.Cancel);
		}

		public void TestDeleteDisabledOnToolBar()
		{
			AssertNull("Delete on Toolbar buttons should be removed", Module.DeleteMenuItem);
		}

		public virtual void TestDisallowDelete()
		{
			Assert("Should allow delete(reverse)", Module.AllowDelete);
		}

		public virtual void TestMenuStructure()
		{
			MenuItem[] menuItems = Module.ContextMenu_ForTestOnly;
			AssertNotNull(Module.ViewMenuItem);
			AssertNotNull(Module.NewMenuItem);
			AssertNotNull(Module.EditMenuItem);
			AssertNotNull(Module.CopyMenuItem);
			AssertNotNull(menuItems.FindByText("&Actions"));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText("D&ata Transfer"));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText(Module.ForeignCurrencyBalanceAdjustmentMenuItemText_ForTestOnly));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText(AccountingJournalPrintHelper.PrintAccountingJournalText));
			AssertNotNull(menuItems.FindByText("Actions").MenuItems.FindByText(Module.UploadGLJournalsMenuItemText_ForTestOnly));

			bool exportCSVExists = false;
			foreach (MenuItem item in menuItems.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems)
			{
				if (item.Text == "Export GL Transactions To CSV")
				{
					exportCSVExists = true;
				}
			}
			AssertEquals("Export to CSV menu item should exist", true, exportCSVExists);
		}

		public void TestAllowAdvancedDataAutomationWizard()
		{
			var allowADAW = Module.AllowAdvancedDataAutomationWizard_ForTestOnly;
			AssertEquals("ADAW default menu item is not displayed in GL Journal module", false, allowADAW);
		}

		public void TestADAWMenuItem()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			var adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
			AssertNull(adawMenu);

			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.UploadGLJournalsUsingADAW.IsAllowed = false;

			menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
			AssertNotNull(adawMenu);
			adawMenu.PerformClick();
			AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nManage -> General Ledger -> Journals -> Actions -> Upload GL Journals using ADAW", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();

			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UploadGLJournalsUsingADAW.IsAllowed = true;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);

			menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull(menuItems.FindByText("Upload GL Journals using ADAW"));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("CWSupport", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				menuItems = Module.GetNewActionMenuItems_ForTestOnly();
				adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW (Unavailable for CW1 Support)", adawMenu.Caption);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				menuItems = Module.GetNewActionMenuItems_ForTestOnly();
				adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW", adawMenu.Caption);
			}
		}

		public void TestDataImportWizard_GlowDataWizardIntegrationReturnsError()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UploadGLJournalsUsingADAW.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWManageMappings.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWImportFileUsingMapping.IsAllowed = true;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);

			var menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull(menuItems.FindByText("Upload GL Journals using ADAW"));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var moq = new Mock<GLJournalModule>();
			moq.CallBase = true;
			moq.Setup(x => x.ShowImportMappingWizardCore(typeof(GLJournalHeaderForADAW), null)).Returns("some error");

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var mockModule = moq.Object)
			{
				menuItems = mockModule.GetNewActionMenuItems_ForTestOnly();
				var adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW", adawMenu.Caption);

				var manageMappingMenu = adawMenu.MenuItems.FindByText("Manage Mappings");
				var importWithMenu = adawMenu.MenuItems.FindByText("Import File Using");
				AssertNotNull(manageMappingMenu);
				AssertNull(importWithMenu);

				var manageMappingMenuItems = manageMappingMenu.MenuItems;
				AssertEquals(manageMappingMenuItems.Count, 1);
				AssertEquals(manageMappingMenuItems[0].Text, "New Mapping");

				manageMappingMenuItems[0].PerformClick();

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd(), "some error");
			}
		}

		public void TestDataImportWizard_GlowDataWizardIntegration()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UploadGLJournalsUsingADAW.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWManageMappings.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWImportFileUsingMapping.IsAllowed = true;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);

			var menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull(menuItems.FindByText("Upload GL Journals using ADAW"));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var mapping = Factory.New<StmModuleFilter>();
			mapping.S9_ModuleID = "GLOWDataImportV2_IGLJournalHeader";
			mapping.S9_FilterName = "test mapping 1";
			mapping.S9_RelatedEntityID = Guid.Empty;
			Factory.Save();

			IBusinessObjectCollection gljournalCollection = null;

			var modMapping = new Mock<IDataTransferMapping>();

			var moq = new Mock<GLJournalModule>();
			moq.CallBase = true;
			moq.Setup(x => x.ShowImportWizardCore(It.IsAny<IBusinessObjectCollection>(), It.IsAny<IDataTransferMapping>()))
				.Callback((IBusinessObjectCollection c, IDataTransferMapping m) =>
					{
						c.AddNew();
						gljournalCollection = c;
					});

			var parserMock = new Mock<IDataTransferMappingParser>();
			parserMock.Setup(p => p.FromModuleFilterInfo(It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
			{
				return new DataTransferMapping() { Name = filterInfo.FilterName, PK = filterInfo.PK };
			});
			ObjectFactory.Substitute(parserMock.Object);

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var mockModule = moq.Object)
			{
				menuItems = mockModule.GetNewActionMenuItems_ForTestOnly();
				var adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW", adawMenu.Caption);

				var importWithMenu = adawMenu.MenuItems.FindByText("Import File Using");
				AssertNotNull(importWithMenu);
				var mappingMenu = importWithMenu.MenuItems.FindByText("test mapping 1");
				AssertNotNull(mappingMenu);

				moq.Verify(x => x.ShowImportWizardCore(It.IsAny<IBusinessObjectCollection>(), It.IsAny<IDataTransferMapping>()), Times.Never);

				mappingMenu.PerformClick();

				moq.Verify(x => x.ShowImportWizardCore(It.IsAny<IBusinessObjectCollection>(), It.IsAny<IDataTransferMapping>()), Times.Once);

				AssertNotNull(gljournalCollection);
				AssertEquals(1, gljournalCollection.Count);
			}
		}

		public void TestDataImportWizard_ManageMappings_NoSecurity()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UploadGLJournalsUsingADAW.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWManageMappings.IsAllowed = false;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);

			var menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull(menuItems.FindByText("Upload GL Journals using ADAW"));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var module = new GLJournalModule())
			{
				menuItems = module.GetNewActionMenuItems_ForTestOnly();
				var adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW", adawMenu.Caption);

				var manageMappingMenu = adawMenu.MenuItems.FindByText("Manage Mappings");
				AssertNotNull(manageMappingMenu);

				var manageMappingMenuItems = manageMappingMenu.MenuItems;
				AssertEquals(manageMappingMenuItems.Count, 1);
				AssertEquals(manageMappingMenuItems[0].Text, "New Mapping");

				manageMappingMenuItems[0].PerformClick();

				AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nManage -> General Ledger -> Journals -> Actions -> Upload GL Journals using ADAW -> Manage Mappings", UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());
			}
		}

		public void TestDataImportWizard_ImportFileUsingMapping_NoSecurity()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testSecurity = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			testSecurity.UploadGLJournalsUsingADAW.IsAllowed = true;
			testSecurity.UploadGLJournalsUsingADAWImportFileUsingMapping.IsAllowed = false;
			Env.SetTemporarySecurityInstanceForTest(testSecurity);

			var menuItems = Module.GetNewActionMenuItems_ForTestOnly();
			AssertNotNull(menuItems.FindByText("Upload GL Journals using ADAW"));

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var mapping = Factory.New<StmModuleFilter>();
			mapping.S9_ModuleID = "GLOWDataImportV2_IGLJournalHeader";
			mapping.S9_FilterName = "test mapping 1";
			mapping.S9_RelatedEntityID = Guid.Empty;
			Factory.Save();

			var parserMock = new Mock<IDataTransferMappingParser>();
			parserMock.Setup(p => p.FromModuleFilterInfo(It.IsAny<IModuleFilterInfo>())).Returns((IModuleFilterInfo filterInfo) =>
			{
				return new DataTransferMapping() { Name = filterInfo.FilterName, PK = filterInfo.PK };
			});
			ObjectFactory.Substitute(parserMock.Object);

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			using (var module = new GLJournalModule())
			{
				menuItems = module.GetNewActionMenuItems_ForTestOnly();
				var adawMenu = menuItems.FindByText("Upload GL Journals using ADAW") as ZMenuItem;
				AssertNotNull(adawMenu);
				adawMenu.OnPopup(null);
				AssertEquals("Upload GL Journals using ADAW", adawMenu.Caption);

				var importWithMenu = adawMenu.MenuItems.FindByText("Import File Using");
				AssertNotNull(importWithMenu);
				var mappingMenu = importWithMenu.MenuItems.FindByText("test mapping 1");
				AssertNotNull(mappingMenu);

				mappingMenu.PerformClick();

				AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\nManage -> General Ledger -> Journals -> Actions -> Upload GL Journals using ADAW -> Import File Using Mapping", UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd());
			}
		}

		public void TestDataImportWizard_ImportWithPreviewAndMapping()
		{
			var targetCollection = new GLJournalHeaderForADAWCollection(Factory, "H");

			var headerValues = new string[]
			{
				"JournalType",
				"CompanyCode",
				"BranchCode",
				"PostPeriod",
				"JournalDescription"
			};

			var lineValues = new string[]
			{
				"GLJournalLines.GLAccount",
				"GLJournalLines.DepartmentCode",
				"GLJournalLines.LocalAmount",
				"GLJournalLines.Currency",
				"GLJournalLines.Amount",
				"GLJournalLines.JournalLineDescription"
			};

			var subAccountValues = new string[]
			{
				"GLJournalLines.SubAccounts.SubAccountType",
				"GLJournalLines.SubAccounts.SubAccountValue"
			};

			var headers = new[] {
				new ImportPreviewHeader(string.Empty, headerValues),
				new ImportPreviewHeader("GLJournalLines", lineValues),
				new ImportPreviewHeader("GLJournalLines.SubAccounts", subAccountValues),
			};

			var headerLine = new ImportPreviewLine(string.Empty, 0, new[] {
				new ImportPreviewLineDetails("GJL", 1),
				new ImportPreviewLineDetails("DAU", 2),
				new ImportPreviewLineDetails("SYD", 3),
				new ImportPreviewLineDetails("202401", 4),
				new ImportPreviewLineDetails("Import Journal", 5)
			});

			var childLine1 = new ImportPreviewLine("GLJournalLines", 1, new[] {
				new ImportPreviewLineDetails("1020.10.20", 1),
				new ImportPreviewLineDetails("BRN", 2),
				new ImportPreviewLineDetails("100", 3),
				new ImportPreviewLineDetails("AUD", 4),
				new ImportPreviewLineDetails("100", 5),
				new ImportPreviewLineDetails("Line Description1", 6)
			});
			var childLine2 = new ImportPreviewLine("GLJournalLines", 3, new[] {
				new ImportPreviewLineDetails("1020.20.20", 1),
				new ImportPreviewLineDetails("BRN", 2),
				new ImportPreviewLineDetails("-100", 3),
				new ImportPreviewLineDetails("AUD", 4),
				new ImportPreviewLineDetails("-100", 5),
				new ImportPreviewLineDetails("Line Description2", 6)
			});

			var subAccountLine = new ImportPreviewLine("GLJournalLines.SubAccounts", 2, new[] {
				new ImportPreviewLineDetails("ORG", 1),
				new ImportPreviewLineDetails("121MAR", 2)
			});

			childLine1.ChildLines.Add(subAccountLine);
			headerLine.ChildLines.Add(childLine1);
			headerLine.ChildLines.Add(childLine2);

			var dataRows = new ImportPreview(headers, headerLine);

			var headerDefinition = new MappingDataDefinition(
				"IGLJournalHeader",
				"IAH",
				Array.Empty<MappingDataDefinitionRelation>(),
				new[] {
					new MappingDataDefinitionCollection("GLJournalLines", "IGLJournalLine")
				});
			var lineDefinition = new MappingDataDefinition(
				"IGLJournalLine",
				"IAL",
				Array.Empty<MappingDataDefinitionRelation>(),
				new[] {
					new MappingDataDefinitionCollection("SubAccounts", "IGLJournalLineSubAccount")
				});
			var mappingDataModel = new MappingDataModel(new []
			{
				headerDefinition,
				lineDefinition,
				new MappingDataDefinition("IGLJournalLineSubAccount", "IAL1"),
			});

			var log = new GlowLog();
			var progressReporter = new Mock<IProgressReporter>();
			progressReporter.Setup(s => s.ItemsProcessed).Returns(1);
			ObjectFactory.Substitute(progressReporter.Object);

			var result = ObjectFactory.Get<IGlowCollectionImporter>().PopulateFromDataRows(targetCollection, new[] { dataRows }, mappingDataModel, log, progressReporter.Object);

			if (!result)
			{
				Fail("Import failed, please find below logs:\r\n" + log.GetLogs());
			}

			AssertEquals("GL Journal imported count", 1, targetCollection.Count);

			var glJournal = targetCollection[0];
			CombineAssertions("Journal", () =>
			{
				AssertEquals("JournalType", "GJL", glJournal.JournalType);
				AssertEquals("CompanyCode", "DAU", glJournal.CompanyCode);
				AssertEquals("BranchCode", "SYD", glJournal.BranchCode);
				AssertEquals("PostPeriod", "202401", glJournal.PostPeriod);
				AssertEquals("JournalDescription", "Import Journal", glJournal.JournalDescription);
			});

			AssertEquals("GL Journal Line count", 2, glJournal.GLJournalLines.Count);

			var journalLine1 = glJournal.GLJournalLines[0];
			CombineAssertions("Journal Line 1", () =>
			{
				AssertEquals("GLAccount", "1020.10.20", journalLine1.GLAccount);
				AssertEquals("DepartmentCode", "BRN", journalLine1.DepartmentCode);
				AssertEquals("LocalAmount", "100", journalLine1.LocalAmount);
				AssertEquals("Currency", "AUD", journalLine1.Currency);
				AssertEquals("Amount", "100", journalLine1.Amount);
				AssertEquals("JournalLineDescription", "Line Description1", journalLine1.JournalLineDescription);
			});

			var journalLine2 = glJournal.GLJournalLines[1];
			CombineAssertions("Journal Line 2", () =>
			{
				AssertEquals("GLAccount", "1020.20.20", journalLine2.GLAccount);
				AssertEquals("DepartmentCode", "BRN", journalLine2.DepartmentCode);
				AssertEquals("LocalAmount", "-100", journalLine2.LocalAmount);
				AssertEquals("Currency", "AUD", journalLine2.Currency);
				AssertEquals("Amount", "-100", journalLine2.Amount);
				AssertEquals("JournalLineDescription", "Line Description2", journalLine2.JournalLineDescription);
			});

			AssertEquals("GL Journal Line1 SubAccount count", 1, journalLine1.SubAccounts.Count);
			AssertEquals("GL Journal Line2 SubAccount count", 0, journalLine2.SubAccounts.Count);

			var subAccount = journalLine1.SubAccounts[0];
			CombineAssertions("Sub Account", () =>
			{
				AssertEquals("SubAccountType", "ORG", subAccount.SubAccountType);
				AssertEquals("SubAccountValue", "121MAR", subAccount.SubAccountValue);
			});
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			GLJournal testGLJournal = Factory.NewWithValidTestData<GLJournal>();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Test Printer";
			Factory.Save();

			PrintTask testTask = new PrintTask();
			DocumentCommand docCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "General Ledger Journal"));
			testTask.DeliveryInstructionsDefaultPK = docCommand.PK;
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer.PK;
			instructions.PrinterDelivery.NumberOfCopies = 14;
			testTask.SavePrinterDeliveryDefaults(instructions);

			using (ZForm form = new ZForm())
			{
				form.Controls.Add(Module.EmbeddedControl);
				form.Show();

				Module.PerformSearch_ForTest();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)Module.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals("Precondition: at least one bizo must be in module grid.", 1, testCollection.Count);

				Module.HandlePrint_ForTestOnly(Module, EventArgs.Empty);
				DeliveryInstructions printDeliveryInstructions = ZFormModaliser.LastIBusinessShownOnDialogForTest as DeliveryInstructions;
				AssertNotNull("Precondition: ", printDeliveryInstructions);

				AssertEquals("Printer must be as stored before", printer.PK, printDeliveryInstructions.PrinterDelivery.PrintQueuePK);
				AssertEquals("Number of Copies must be as stored before", 14, printDeliveryInstructions.PrinterDelivery.NumberOfCopies);
			}
		}

		public void TestPrintAccountingJournalForGL()
		{
			using (var testModule = new GLJournalModule())
			{
				using (ZForm form = new ZForm())
				{
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertEquals("Please select Journal(s) to print.", UnitTestUserNotification.Instance.LastMessage.Text);

					var testGLJournal = Factory.NewWithValidTestData<GLJournal>();
					Factory.Save();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestPrintWhenNothingIsSelected()
		{
			GLJournal testGLJournal1 = Factory.NewWithValidTestData<GLJournal>();
			GLJournal testGLJournal2 = Factory.NewWithValidTestData<GLJournal>();
			Factory.Save();

			using (ZForm form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(Module.EmbeddedControl);
				form.Show();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)Module.GetNewGridCollection_ForTestOnly();

				AssertEquals("Precondition: no bizo must be in module grid.", 0, testCollection.Count);
				Module.HandlePrint_ForTestOnly(Module, EventArgs.Empty);
				AssertEquals("Please select a valid Journal to print.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Module.PerformSearch_ForTest();
				testCollection.Load();
				AssertEquals("Precondition: two bizos must be in module grid.", 2, testCollection.Count);
				Module.HandlePrint_ForTestOnly(Module, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				DeliveryInstructions printDeliveryInstructions = ZFormModaliser.LastIBusinessShownOnDialogForTest as DeliveryInstructions;
				AssertNotNull(printDeliveryInstructions);
			}
		}

		public void TestAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_SystemCreateUser = creator;
			var journal1 = Factory.NewWithValidTestData<GLJournal>();
			journal1.AH_SystemCreateUser = creator;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be empty.", ZString.Empty, journal1.AH_GS_NKAuditedBy);

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to audit.", true, testModule.AuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.AuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var jlj in testCollection)
					{
						AssertEquals(auditor, ((GLJournal)jlj).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestUndoAuditTransaction()
		{
			const string creator = "JYW";
			string auditor = GlbStaff.CurrentUser.GS_Code;

			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_SystemCreateUser = creator;
			journal.AH_GS_NKAuditedBy = auditor;
			var journal1 = Factory.NewWithValidTestData<GLJournal>();
			journal1.AH_SystemCreateUser = creator;
			journal1.AH_GS_NKAuditedBy = auditor;

			Factory.Save();

			AssertNotEquals("Precondition: The two users should not be identical.", creator, auditor);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal.AH_GS_NKAuditedBy);
			AssertEquals("Precondition: Auditer should be auditor.", auditor, journal1.AH_GS_NKAuditedBy);

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					var menu = testModule.GetNewActionMenuItems_ForTestOnly();
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Precondition: There should be 2 records.", 2, testCollection.Count);
					AssertEquals("Precondition: Allow to undo audit.", true, testModule.UndoAuditSecurityCheckpoint_ForTestOnly.IsAllowed);

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					menu.FindByText(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText).PerformClick();

					testCollection.Load();
					foreach (var jlj in testCollection)
					{
						AssertEquals(ZString.Empty, ((GLJournal)jlj).AH_GS_NKAuditedBy);
					}
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestUniversalCopyIsntAccessableInMenuForGLJ()
		{
			using (var testModule = new GLJournalModule())
			{
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(testModule.EmbeddedControl);
					form1.Show();
					testModule.PerformSearch_ForTest();
					AssertNull(testModule.DisplayGrid.ContextMenu.MenuItems.FindByName("UniversalCopy"));
				}
			}
		}

		public void TestCheckCanReverseAndRedoCurrencyAdjustment()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_SystemCreateUser = "JYW";
			journal.AH_GS_NKAuditedBy = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					Env.Security.GLJournalRevserseAndRedoAutomatedCurrencyAdjustment.IsAllowed = false;
					Assert(!testModule.CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly());
					AssertEquals(Env.Security.GLJournalRevserseAndRedoAutomatedCurrencyAdjustment.ErrorMessageForNotAllowed.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.GLJournalRevserseAndRedoAutomatedCurrencyAdjustment.IsAllowed = true;
					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					AssertEquals("Precondition: no bizo must be in module grid.", 0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(!testModule.CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly());
					AssertEquals("Please select a valid Journal.", UnitTestUserNotification.Instance.LastMessage.Text);

					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(!testModule.CurrentlySelectedJournal_ForTestOnly.IsPeriodExists);
					Assert(!testModule.CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly());
					AssertEquals("This journal is not generated by the system in relation to the automated A/R and A/P outstanding balances currency adjustment. Please review selection.", UnitTestUserNotification.Instance.LastMessage.Text);

					var generalLedgerClosedMessage = "The general ledger has been closed for the accounting period(s) related to the selected automated currency adjustment journal. Please re-open the general ledger first before running this action menu.";
					var period1 = Factory.NewWithValidTestData<AccPeriodManagement>();
					period1.AM_IsGeneralLedgerClosed = true;
					var journal1 = Factory.NewWithValidTestData<GLJournal>();
					journal1.PeriodPK = period1.PK;
					Factory.Save();

					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(1);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(!testModule.CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly());
					AssertEquals(generalLedgerClosedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					period1.AM_IsGeneralLedgerClosed = false;
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(testModule.CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly());
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		public void TestHandleDeleteClickCore()
		{
			var journal1 = Factory.NewWithValidTestData<GLJournal>();
			var journal2 = Factory.NewWithValidTestData<GLJournal>();
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.SelectAllElements();

					testModule.HandleDeleteClickCore_ForTestOnly(null, null);

					AssertEquals("Please select one journal to reverse.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestCheckCanReverse()
		{
			var gLAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			var gLAccount2 = Factory.NewWithValidTestData<AccGLHeader>();

			var journal = Factory.NewWithValidTestData<GLJournal>();
			var line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_LocalExTaxAmount = 10m;
			var line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -10m;
			line2.AL_LocalExTaxAmount = -10m;

			journal.AH_SystemCreateUser = "JYW";
			journal.AH_GS_NKAuditedBy = GlbStaff.CurrentUser.GS_Code;
			journal.AH_TransactionBelongsToGroup = Guid.NewGuid();
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.AH_IsCancelled = false;

			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_IsGeneralLedgerClosed = false;
			journal.PeriodPK = period.PK;
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					Env.Security.ReverseGeneralLedgerJournal.IsAllowed = false;
					Assert(!testModule.CheckCanReverse_ForTestOnly(testModule.CurrentlySelectedJournal_ForTestOnly));
					AssertEquals(Env.Security.ReverseGeneralLedgerJournal.ErrorMessageForNotAllowed.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					Env.Security.ReverseGeneralLedgerJournal.IsAllowed = true;
					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(0);

					Assert(journal.IsPeriodExists);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(!testModule.CheckCanReverse_ForTestOnly(testModule.CurrentlySelectedJournal_ForTestOnly));
					AssertEquals("This journal is generated by the system in relation to the automated A/R and A/P outstanding balances currency adjustment. Please use \"Reverse and re-do Automated Currency Adjustment\" to reverse.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					journal.PeriodPK = Guid.Empty;

					var approvalRequest = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
					approvalRequest.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Requested;
					approvalRequest.Initialize(journal);
					journal.AH_TransactionBelongsToGroup = approvalRequest.PK;
					Factory.Save();
					Assert(!testModule.CheckCanReverse_ForTestOnly(testModule.CurrentlySelectedJournal_ForTestOnly));
					AssertEquals($"A reversed journal approval request ({approvalRequest.XP_RequestID}) is already created, please cancel this approval request before reversing this journal.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					journal.AH_TransactionBelongsToGroup = Guid.Empty;

					Factory.Save();
					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(0);

					Assert(!journal.IsPeriodExists);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert(testModule.CheckCanReverse_ForTestOnly(testModule.CurrentlySelectedJournal_ForTestOnly));
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
		}

		[TestDate(2022, 12, 27, 15, 30, 30)]
		public void TestCheckCanReverseBeforeOpenEditFormAndAfterOpenEditForm()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var gLAccount1 = testObjectCreator.CreateAccruedRevenueControlAccount();
			var gLAccount2 = testObjectCreator.CreateAccruedCostControlAccount();

			var journal = testObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_LocalExTaxAmount = 10m;
			var line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -10m;
			line2.AL_LocalExTaxAmount = -10m;

			journal.AH_SystemCreateUser = "GL1";
			journal.AH_GS_NKAuditedBy = GlbStaff.CurrentUser.GS_Code;
			journal.AH_TransactionBelongsToGroup = Guid.NewGuid();
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.AH_IsCancelled = false;

			Factory.Save();

			using (var testModule = new GLJournalModule())
			using (var form = new GLJournalForm(journal))
			{
				AssertEquals(true, testModule.CheckCanReverse_ForTestOnly(journal));

				form.Show();
				Application.DoEvents();

				AssertEquals(false, testModule.CheckCanReverse_ForTestOnly(journal));
				AssertStartsWith("Expected error message start.", "The GL Journal is currently being edited by user 'CargoWise Support' since", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEndsWith("Expected error message end", "you cannot reverse it before the other user closes the form.\r\n\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestCheckCanReverseGJLLinkingWithDsbJobCloseBatch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateGLJournalWithDSBJobCloseBatch(out GLJournal journal, out DsbJobCloseBatch batch);
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(0);

					AssertEquals(false, testModule.CheckCanReverse_ForTestOnly(testModule.CurrentlySelectedJournal_ForTestOnly));
					AssertEquals("This journal is generated by the system in relation to the Bulk Disbursement Job Close function. Reversal is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestNotAllowEditGJLLinkingWithDsbJobCloseBatch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateGLJournalWithDSBJobCloseBatch(out GLJournal journal, out DsbJobCloseBatch batch);
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testModule.PerformSearch_ForTest();
					testCollection.Load();
					testModule.DisplayGrid.Select(0);

					testModule.HandleEditClick_ForTestOnly(this, new EventArgs());

					var moduleInternalsForTest = (IFilterModuleInternalsForTesting)testModule;
					AssertEquals(ODisplayMode.ReadOnly, moduleInternalsForTest.LastController.LastShownForm.DisplayMode);

					moduleInternalsForTest.LastController.LastShownForm.Dispose();
				}
			}
		}

		protected GLJournalModule Module;

		public void TestHandleUploadGLJournals_NoSecurity()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			Env.Security.GLJournalUpload.IsAllowed = false;
			Module.HandleUploadGLJournals_ForTestOnly(null, EventArgs.Empty);
			AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Journals -> Actions -> Upload GL Journals", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			Env.Security.GLJournalUpload.IsAllowed = true;
			Module.ImportJournalCSVEventHandler_ForTestOnly(null, EventArgs.Empty);
			AssertNull("No security error should be shown when user does not have edit permission to branch", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleUploadGLJournals_ShowUploadGLJournalsDataImportForm()
		{
			Module.HandleUploadGLJournals_ForTestOnly(null, EventArgs.Empty);
			AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is UploadGLJournalsDataImportForm);
		}

		public void TestHandleUploadGLJournals()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			Module.HandleUploadGLJournals_ForTestOnly(null, EventArgs.Empty);
			AssertEquals(ZFormModaliser.ResultToReturnFromShowDialog, DialogResult.Cancel);
		}

		[TestDate(2020, 6, 20)]
		public void TestHasError_WhenEditGLJournalWhichIncludedInGLDComplianceReport()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;

			var gldReportConfig = CreateConfig("TST", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData);
			gldReportConfig.TaxRegistrationType = "APC";
			var ahReportConfig = CreateConfig("STA", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.TransactionHeader);
			ahReportConfig.TaxRegistrationType = "APC";

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var gldComplianceReportGEN = TestObjectCreator.CreateComplianceReport(gldReportConfig.ReportCode, AccComplianceReport.Status.ReportGenerated);
			var ahComplianceReportFIN = TestObjectCreator.CreateComplianceReport(ahReportConfig.ReportCode, AccComplianceReport.Status.ReportFinalised);

			var gLAccount1 = TestObjectCreator.CreateAccruedRevenueControlAccount();
			var gLAccount2 = TestObjectCreator.CreateAccruedCostControlAccount();

			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = (GLJournalLine)journal.Lines.AddNew();
			line1.AL_AG = gLAccount1.PK;
			line1.AL_OSExTaxAmount = 10m;
			line1.AL_LocalExTaxAmount = 10m;
			var line2 = (GLJournalLine)journal.Lines.AddNew();
			line2.AL_AG = gLAccount2.PK;
			line2.AL_OSExTaxAmount = -10m;
			line2.AL_LocalExTaxAmount = -10m;

			journal.AH_SystemCreateUser = "GL1";
			journal.AH_GS_NKAuditedBy = GlbStaff.CurrentUser.GS_Code;
			journal.AH_TransactionBelongsToGroup = Guid.NewGuid();
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.AH_IsCancelled = false;

			var gldPk = CreateDummyGLD(journal);

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{gldPk}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{gldComplianceReportGEN.PK}', 1),
(NEWID(), NEWID(), 'AH', '{GlbCompany.CurrentCompany.PK}', '{ahComplianceReportFIN.PK}', 2)";

			TestConnection.ExecuteNonQuery(sql);

			using (var testModule = new GLJournalModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				testModule.PerformSearch_ForTest();
				testCollection.Load();
				testModule.DisplayGrid.Select(0);
				testModule.HandleEditClick_ForTestOnly(this, new EventArgs());

				((IFilterModuleInternalsForTesting)testModule).LastController.LastShownForm.Dispose();

				var expectedMessage = $"The journal entries of this GL Journal have been included in the finalized compliance report <{gldComplianceReportGEN.ACR_ReportType}> and cannot be edited.";
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				gldComplianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.HandleEditClick_ForTestOnly(this, new EventArgs());
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				((IFilterModuleInternalsForTesting)testModule).LastController.LastShownForm.Dispose();
			}

			ComplianceReportConfiguration CreateConfig(string reportCode, string tablePrefix)
			{
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = reportCode;
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportBaseTablePrefix = tablePrefix;
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "ABN";
				reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
				return reportConfig;
			}

			ZGuid CreateDummyGLD(GLJournal gLJournal)
			{
				var narrowGLD = Factory.NewWithValidTestData<AccGeneralLedgerData>();
				narrowGLD.GLD_PostDate = gLJournal.AH_PostDate;
				narrowGLD.GLD_GC_Company = Env.CurrentCompany.PK;
				narrowGLD.GLD_Type = "PST";
				narrowGLD.GLD_GLAccountType = "ARC";
				narrowGLD.GLD_AH_TransactionHeader = gLJournal.PK;
				narrowGLD.GLD_PostPeriod = 202301;
				narrowGLD.GLD_JournalEntriesNumber = "";
				narrowGLD.GLD_JournalEntriesNumberRuleCode = "";
				return narrowGLD.PK;
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestHasError_WhenEditGLJournalWhichIncludedInAllTransactionComplianceReport()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var allTransactionReportConfig = CreateConfig("TST", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var complianceReport = TestObjectCreator.CreateComplianceReport(allTransactionReportConfig.ReportCode, AccComplianceReport.Status.ReportGenerated);

			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 08, 13), new ZDateTime(2020, 08, 13));
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{line1.PK}', 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReport.PK}', 1),
(NEWID(), '{line2.PK}', 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReport.PK}', 2)";
			TestConnection.ExecuteNonQuery(sql);

			using (var testModule = new GLJournalModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				testModule.PerformSearch_ForTest();
				testCollection.Load();
				testModule.DisplayGrid.SelectSingleElementByPK(journal.PK);
				testModule.HandleEditClick_ForTestOnly(this, new EventArgs());

				((IFilterModuleInternalsForTesting)testModule).LastController.LastShownForm.Dispose();

				var expectedMessage = $"The journal entries of this GL Journal have been included in the finalized compliance report <{complianceReport.ACR_ReportType}> and cannot be edited.";
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				complianceReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testModule.HandleEditClick_ForTestOnly(this, new EventArgs());
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
			}

			ComplianceReportConfiguration CreateConfig(string reportCode, string tablePrefix)
			{
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = reportCode;
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportBaseTablePrefix = tablePrefix;
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
				return reportConfig;
			}
		}

		#region RegenerateJournalEntries

		public void TestRegenerateJournalEntries()
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glJournal.Factory.Save();

			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as ZFilterGridModule)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					var regenerateJournalEntriesForTest = module as GLJournalModule;
					AssertEquals(1, regenerateJournalEntriesForTest.SelectedBusinessObjects_ForTestOnly().Length);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					regenerateJournalEntriesForTest.HandleRegenerateJournalEntries_ForTestOnly(null, null);

					var recoverVerifyBizos = glJournal.Lines;

					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == recoverVerifyBizos.Count() && y.All(z => recoverVerifyBizos.Any(a => a.PK == z.PK)))), Times.Once);
				}
			}
		}

		public void TestRegenerateJournalEntriesActionMenuItem()
		{
			var moq = new Mock<GLJournalModule>();
			moq.CallBase = true;

			using (var testModule = moq.Object)
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertHasMenuItem(testModule);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertHasMenuItem(testModule, hasMenuItem: false);

				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
				using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertHasMenuItem(testModule, hasMenuItem: false);
				}
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestCheckGLJournalEntriesNumberHasBeenAssigned_Assigned()
		{
			CheckIsGLJournalEntriesNumberHasBeenAssigned(true);
		}

		[TestDate(2020, 6, 20)]
		public void TestCheckGLJournalEntriesNumberHasBeenAssigned_NoAssigned()
		{
			CheckIsGLJournalEntriesNumberHasBeenAssigned(false);
		}

		void CheckIsGLJournalEntriesNumberHasBeenAssigned(bool isAssigned)
		{
			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glJournal.Factory.Save();

			var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "TLG";
			generalLedgerData.GLD_PostDate = ZDateTime.Today;
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			generalLedgerData.GLD_AH_TransactionHeader = glJournal.PK;
			if (isAssigned)
			{
				generalLedgerData.GLD_JournalEntriesNumber = "test";
				generalLedgerData.GLD_JournalEntriesNumberRuleCode = "test";
			}
			else
			{
				generalLedgerData.GLD_JournalEntriesNumber = null;
				generalLedgerData.GLD_JournalEntriesNumberRuleCode = null;
			}
			Factory.Save();

			using (var testModule = new GLJournalModule())
			{
				using (var form = new ZForm())
				{
					using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						form.Controls.Add(testModule.EmbeddedControl);
						form.Show();

						var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
						testModule.PerformSearch_ForTest();
						testCollection.Load();
						testModule.DisplayGrid.SelectSingleElementByPK(glJournal.PK);

						var expectedMessage = $"Editing is not allowed when an unique reference number has been assigned to the journal entries of this GL Journal.\r\nPlease reverse this GL Journal and re-enter.";
						Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testModule.HandleEditClick_ForTestOnly(this, new EventArgs());

						var moduleInternalsForTest = (IFilterModuleInternalsForTesting)testModule;
						if (isAssigned)
						{
							AssertEquals(ODisplayMode.ReadOnly, moduleInternalsForTest.LastController.LastShownForm.DisplayMode);
							moduleInternalsForTest.LastController.LastShownForm.Dispose();
							Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
						}
						else
						{
							AssertEquals(ODisplayMode.Browse, moduleInternalsForTest.LastController.LastShownForm.DisplayMode);
							moduleInternalsForTest.LastController.LastShownForm.Dispose();
							Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));
						}
					}
				}
			}
		}

		static void AssertHasMenuItem(GLJournalModule testConsol, bool hasMenuItem = true)
		{
			var testMenu = testConsol.FindMenuItemByText_ForTestOnly("Regenerate Journal Entries (CWSupport Only)");

			if (hasMenuItem)
			{
				AssertNotNull(testMenu);
			}
			else
			{
				AssertNull(testMenu);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}

		TestObjectCreator testObjectCreator;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			Module = ZModuleFactory.Instance.Create(GetModuleID()) as GLJournalModule;

			var testObjectCreator = new TestObjectCreator(Factory);

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "TST"));

			if (company == null)
			{
				company = testObjectCreator.CreateNewCompany("TST");
				testObjectCreator.CreateNewBranch(company, "TST");
				company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;

				Factory.Save();
			}
		}

		protected override void TearDown()
		{
			Module?.Dispose();
			base.TearDown();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLJournal;
		}
	}
}

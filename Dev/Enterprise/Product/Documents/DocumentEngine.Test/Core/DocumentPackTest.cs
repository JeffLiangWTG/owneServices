using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.DocBuilder.Testing;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Scheduler.Business.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DeliveryMethods.FactoryStrategy;
using static Enterprise.ExcelTemplates.ExcelTemplateReadFromExcelTemplatesSolution;
using static Enterprise.Integration.Forwarding;
using BusinessObject = CargoWise.EntityFramework.BusinessObject;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(DocumentPack))]
	sealed class DocumentPackTest : NonPersistentBusinessObjectCollectionTestCase<DocumentPack>
	{
		public void TestShouldNotResetManuallyIncludedDocumentsAfterPreviewing()
		{
			var bizO = new MockDocSupportBizO();
			using (var templateStream = new MemoryStream())
			using (var documentPack = new TestableDocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[Blah]
{A}-[#EndOfReport]"))
			{
				report.PrintCopyType = PrintCopyType.EML;
				documentPack.Add(report);
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;

				AssertEquals("report should not be included", false, report.IncludedInPrint);

				report.IncludedInPrint = true;
				AssertEquals("Manually include report", true, report.IncludedInPrint);

				deliveryInstructions.Destination = DeliveryInstructionDestination.Preview;
				documentPack.DocumentSupporter = bizO.DocumentSupporter;
				documentPack.Run(deliveryInstructions);

				AssertEquals("report should still be included", true, report.IncludedInPrint);
			}
		}

		[GuiTest]
		public void TestAutoDeliveryOfMultiDocPackWithSystemDefaultContact()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var factory = new BusinessObjectFactory();
				var menu = factory.NewWithValidTestData<DocumentCommand>();
				menu.SU_PreventAutoDelivery = false;
				menu.SU_ContactType = ContactType.Consignee.Code;

				var org = factory.New<OrgHeader>();
				org.OH_Code = "TESTTMP";
				org.OH_FullName = "Jerrry Test Organisation Pty Limited";
				org.OH_RL_NKClosestPort = "AUSYD";
				var mainAddress = org.MainAddress;
				mainAddress.OA_Address1 = "Jerry Test Address";
				mainAddress.OA_City = "A city";
				org.Contacts.RemoveAndDeleteAll();
				org.SuppressedDocuments.AddNew();
				factory.Save();

				var readOnlyfactory = factory.GetCachedReadOnlyFactory();
				readOnlyfactory.RefreshEnabled = false;

				var bizO = new AutoDeliveryBizO(org);
				using (var templateStream = new MemoryStream())
				using (var documentPack = new DocumentPack(menu))
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
		@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[Blah]
{A}-[#EndOfReport]"))
				{
					documentPack.Add(report);
					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					deliveryInstructions.Destination = DeliveryInstructionDestination.Auto;
					deliveryInstructions.DocumentPackCount = 2;
					deliveryInstructions.DeliveryGroups[0].Factory.Save();
					documentPack.DocumentSupporter = bizO;
					documentPack.Run(deliveryInstructions);

					AssertEquals("Should no errors", string.Empty, ErrorReporter.LastMessageReported);
				}

				var newFactory = new BusinessObjectFactory();
				var consignee = newFactory.Load<OrgHeader>(org.PK);
				consignee.MainAddress.OA_Email = "jerry@test.com";
				newFactory.Save();

				AssertEquals("jerry@test.com", org.MainAddress.OA_Email);

				TestCaseHelper.ClearTable(AutoStmPrintJobCopyRecipient.Schema.TableName);
				TestCaseHelper.ClearTable(AutoStmPrintJob.Schema.TableName);
				AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

				using (var templateStream = new MemoryStream())
				using (var documentPack = new DocumentPack(menu))
				using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
		@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[Blah]
{A}-[#EndOfReport]"))
				{
					documentPack.Add(report);
					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					deliveryInstructions.Destination = DeliveryInstructionDestination.Auto;
					deliveryInstructions.DocumentPackCount = 2;
					deliveryInstructions.DeliveryGroups[0].Factory.Save();
					documentPack.DocumentSupporter = bizO;
					documentPack.Run(deliveryInstructions);

					AssertEquals("Should no errors", string.Empty, ErrorReporter.LastMessageReported);

					var printJobs = new StmPrintJobCollection(Factory);
					printJobs.Load();
					AssertEquals("Should be OrgHeader main address email", "jerry@test.com", printJobs[0].EmailToRecipients[0].SPR_EmailAddress);
				}
			}
		}

		public void TestEmailSubjectHasAngleBracket()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignor.Code;
			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack) { Destination = DeliveryInstructionDestination.TakenFromContact };
			instructions.Recipients.RemoveAll();
			var contact1 = instructions.Recipients.AddNew();
			contact1.Name = "<AAA>";
			contact1.EmailSubjectMacro = "Hello <Name>";
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.DeliveryAddress = "address1@test.com";

			var bizo = Factory.New<DummyBusinessObject>();
			var wrapper = new DummyDocumentWrapper(bizo, Factory);

			using (var report = new Report(pack, TestReport, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				pack.Run(instructions);

				Assert(instructions.DeliveryGroups.Exists(g => g.SB_EmailSubjectLine == "Hello <AAA>"));
			}
		}

		public void TestAddAndSaveDocumentDeliveredLogToDocumentBusinessObjectShouldShowErrorMsgWhenOccurConcurrencyError()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			Factory.Save();
			var exception = new ZSaveConcurrencyException(
				new ZSaveConcurrencyException(
					new ZDataConcurrencyException(new Exception("~ConcurrencyError~"),
						((IBusinessObjectInternals)shipment).Row, Db.Connection), Factory), true);

			var command = Factory.New<DocumentCommand>();
			var report = new MockReport(TestReport);
			IBODocDataProvider docDataProvider = new DummyDocumentWrapper((BusinessObject)shipment, Factory);
			((IReportForUnitTesting)report).SetBusinessObjectForTesting(docDataProvider);
			report.DocumentDeliveredEventCode = Events.CustomisableEvent00.Code;
			using (var pack = new TestableDocumentPack())
			{
				var deliveryInstructions = new DeliveryInstructions(pack,
					new SaveInChunksForConcurrencyError() { ConcurrencyExceptionForTest = exception });
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Email = "test@1.com";

				pack.AddAndSetMenuItem(report, command);

				AssertNoExceptionThrown(() => pack.Run(deliveryInstructions));
				EventManager logManager = new EventManager();
				string lastEdit = logManager.GetUserNameAndTimeOfLastEditOrDeleteOfARecord(JobShipmentSchema.Constants.TableName, shipment.PK.ToGuid());
				AssertContains($"The document is successfully delivered, but error occurring when generating the Z00 event: \r\n**CONCURRENCY Error Saving Record **\r\n\r\nServerName: {Db.ServerName}\r\nDatabaseName: {Db.DatabaseName}\r\nTablename: JobShipment\r\nPK: {shipment.PK}\r\nRowState: Unchanged\r\nFactory validation suspended: False\r\nFactory name for debugging: \r\nBusiness object around row = Enterprise.Freight.Forwarding.Business.ForwardingShipment\r\nBusiness object validation suspended: 0\r\nBusiness object is marking as needing validation suspended: False\r\nBusiness object light validation is enabled: True\r\nBusiness object additional info: \r\n\r\nInner Message = ~ConcurrencyError~\r\n\r\nAdditional Information = \r\n--- Save Aborted Due to Concurrency Check ---\r\n\r\nROW INFORMATION\r\nTable      = JobShipment\r\nPK         = {shipment.PK}\r\nRowState   = Unchanged\r\n\r\nCOLUMN INFORMATION\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddAndSaveDocumentPasswordInformationLogToDocumentBusinessObject()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "Z00001001";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TTT";
			staff.GS_FullName = "test staff";
			staff.ExcelPasswordForModifying = "ModifyPassword";
			staff.ExcelPasswordForOpening = "OpenPassword";
			Factory.Save();

			var command = Factory.New<DocumentCommand>();
			IBODocDataProvider docDataProvider = new DummyDocumentWrapper((BusinessObject)shipment, Factory);
			var report = new Report(new DocumentPack(), TestReport, new DataProviderList(docDataProvider), "Some Document", null, DocumentDirection.ANY, true, true, null);
			report.DocumentDeliveredEventCode = Events.CustomisableEvent00.Code;
			using (var pack = new TestableDocumentPack() { ActuallyDeliver = true })
			{
				var deliveryInstructions = new DeliveryInstructions(pack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.AttachmentType = "XLS";
				recipient.StaffCode = staff.GS_Code;
				recipient.Email = "test@1.com";

				pack.AddAndSetMenuItem(report, command);
				pack.Run(deliveryInstructions);

				var dpiEventQuery = new ZQuery();
				dpiEventQuery.AddToFilter(StmALogSchema.SL_Parent, shipment.PK);
				dpiEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DocumentPasswordInformation.Code);
				var dpiEvent = Factory.Load<StmALog>(dpiEventQuery);
				AssertNotNull(dpiEvent);
			}
		}

		class SaveInChunksForConcurrencyError : SaveInChunks
		{
			public ZSaveConcurrencyException ConcurrencyExceptionForTest { get; set; }

			protected override void SaveChunkCore(BusinessObjectFactory factory)
			{
				throw ConcurrencyExceptionForTest;
			}
		}

		public void TestContactEmailEmptyWhenDeliveryByEmail()
		{
			TestCaseHelper.ClearTable(AutoStmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(AutoStmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_MenuName = "Test Menu Name";
			menuItem.SU_MenuPath = "Test Menu Path";
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTCODE";
			Factory.Save();

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack(menuItem))
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
	@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[Blah]
{A}-[#EndOfReport]"))
			{
				documentPack.Add(report);

				var deliveryInstructions = new DeliveryInstructions(documentPack)
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Email = "";
				recipient.EmailCarbonCopyRecipients.Value = "test@test.com";
				recipient.Name = "Test Contact";
				recipient.OrgHeaderPK = org.PK;
				deliveryInstructions.DeliveryGroups[0].Factory.Save();

				documentPack.RunForRecipients(deliveryInstructions);
				AssertEquals($@"An Email was created without any recipients.
Email Subject:Eagle Datamation International - BN - AUBNE - TestIsDraft
Parent Table:
Parent PK:00000000-0000-0000-0000-000000000000
Delivery Method:Email
Contact Name:Test Contact
OrgHeader:TESTCODE
IsSystemDefaultContact:False
Document:Test Menu Name
Path:Test Menu Path
DocumentGroup:A/R
UpdateEmailAndFax:True
Contact PK:
Contact Email:
OrgAddress PK:{recipient.OrgAddressPK}
OrgAddress Email:", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				Assert(!printJobs[0].EmailToRecipients.Any());
			}
		}

		public void TestAutoDeliveryWithDocumentGroupHasNoDeliveryAndDeliveryContactHasNotifyMode()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignee.Code;

			var org = Factory.NewWithValidTestData<OrgHeaderThatSupportJobDocumentRecipient>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jerry";
			contact.OC_Email = "jerry@test.com";
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;

			var orgDocumentWithDocument = contact.Documents.AddNew();
			orgDocumentWithDocument.OD_SU_MenuItem = menuItem.PK;
			orgDocumentWithDocument.OD_DeliverBy = string.Empty;

			using (var pack = new DocumentPack(menuItem))
			{
				var instructions = new DeliveryInstructions(pack);
				instructions.Destination = DeliveryInstructionDestination.Auto;

				pack.DocumentSupporter = new AutoDeliveryBizO(org);

				AssertNoExceptionThrown(() => pack.Run(instructions));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSuspendAddEDocsToPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.SetupDocManagerObjects();
			var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizo);

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;
			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			AssertEquals("Precondition: no contents in the pack", 0, command.EDocs.Count);

			var eDoc = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			command.AddEDoc(eDoc);
			AssertEquals("Contents in the pack after adding", 1, command.EDocs.Count);
			Factory.Save();

			AssertEquals(0, DocumentPack.AutoAddeDocsSuspendedCount.Value);

			var pack = new DocumentPack() { DocumentSupporter = docSupporter };
			pack.ForceBusinessObjectToLogAgainst(bizo);
			AssertEquals("Pack is empty", 0, pack.Count);

			using (PrintTask.SuspendDocumentPackAutoAddEDocs())
			{
				AssertEquals(1, DocumentPack.AutoAddeDocsSuspendedCount.Value);
				pack.AddEDocsToPack(command);
				AssertEquals(0, pack.Count);
			}
			AssertEquals(0, DocumentPack.AutoAddeDocsSuspendedCount.Value);

			var thread = new Thread(() =>
			{
				DocumentPack.AutoAddeDocsSuspendedCount.Value++;
				AssertEquals(1, DocumentPack.AutoAddeDocsSuspendedCount.Value);
			});

			AssertEquals(0, DocumentPack.AutoAddeDocsSuspendedCount.Value);
			pack.AddEDocsToPack(command);
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("DocumentPack should have 1 eDocs", 1, pack.Count);
			AssertEquals("DocumentPack should have 1 eDoc included in print", 1, includeCount);
			AssertEquals("DocumentPack should have 2 other eDocs which is not print by default", 2, pack.OtherEDocsToAttach.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBusinessEntityFactoryIsUsedWhenAddingEDocsToPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.SetupDocManagerObjects();
			var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizo);

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;
			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			AssertEquals("Precondition: no contents in the pack", 0, command.EDocs.Count);

			var eDoc = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			command.AddEDoc(eDoc);
			AssertEquals("Contents in the pack after adding", 1, command.EDocs.Count);
			Factory.Save();

			var pack = new DocumentPack() { DocumentSupporter = docSupporter };
			pack.ForceBusinessObjectToLogAgainst(bizo);
			AssertEquals("Pack is empty", 0, pack.Count);

			pack.AddEDocsToPack(command);
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludeInPrint);

			AssertEquals("DocumentPack should have 1 eDocs", 1, pack.Count);
			AssertEquals("DocumentPack should have 1 eDoc included in print", 1, includeCount);
			AssertEquals("DocumentPack should have 2 other eDocs which is not print by default", 2, pack.OtherEDocsToAttach.Count);
			AssertEquals(Factory.NameForDebugging, ((IDocManagerSupport)command.Parent).DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.NameForDebugging);
		}

		public void TestJustRegenerateTemplateForReportWhenLanguageChanged()
		{
			var documentCommand1 = Factory.New<DocumentCommand>();
			documentCommand1.SU_MenuName = "Test Document 1";
			documentCommand1.SU_BusinessContext = "Test1";

			var documentCommand2 = Factory.New<DocumentCommand>();
			documentCommand2.SU_MenuName = "Test Document 2";
			documentCommand2.SU_BusinessContext = "Test2";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Test]
{A}-[#EndOfReport]");
			template.SO_Name = "Test Template";

			var pivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pivot1.SI_SO = template.PK;
			pivot1.SI_SU = documentCommand1.PK;

			var pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SO = template.PK;
			pivot2.SI_SU = documentCommand2.PK;

			var documentSupportable = Factory.New<DummyBODocSupportable>();

			using (var pack = new DocumentPack(documentCommand1, documentSupportable, null, null))
			{
				pack.AddReportsToPack(documentCommand1, null, documentSupportable, null);

				AssertEquals(2, pack.Count);

				var report1 = pack[0] as Report;
				var excelTemplate1 = report1.Template;
				var xlInterface1 = report1.XlInterface;
				var report2 = pack[1] as Report;
				var excelTemplate2 = report2.Template;
				var xlInterface2 = report2.XlInterface;

				try
				{
					var instructions = new DeliveryInstructions(pack);
					pack.LastTemplateGeneratorLanguage = SharedConstants.Languages.ChineseSimplified;
					pack.RebuildIfLanguageChanged(instructions);

					AssertEquals(2, pack.Count);
					AssertEquals(report1, pack[0]);
					AssertEquals(report2, pack[1]);
					AssertNotEquals(excelTemplate1, report1.Template);
					AssertNotEquals(excelTemplate2, report2.Template);
					AssertNotEquals(xlInterface1, report1.XlInterface);
					AssertNotEquals(xlInterface2, report2.XlInterface);
				}
				finally
				{
					xlInterface1.Dispose();
					xlInterface2.Dispose();
					report1.XlInterface.Dispose();
					report2.XlInterface.Dispose();
				}
			}
		}

		public void TestMultipleReportsHaveOneSameFactoryForVisualizerNoteInOneDocPack()
		{
			var excelTemplate = TestXls;
			var report1 = new Report(testPack, excelTemplate, "TestingOnly", null, false);
			var wrapper = new DummyDocWrapper();
			((IReportForUnitTesting)report1).SetBusinessObjectForTesting(wrapper);
			var report2 = new Report(testPack, excelTemplate, "TestingOnly", null, false);
			((IReportForUnitTesting)report2).SetBusinessObjectForTesting(wrapper);

			AssertEquals(report1.VisualizerContentNote.Factory, report2.VisualizerContentNote.Factory);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleReportVisualizerNotesCanBeSaved()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Code = "Code1";

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);

			using (Report report1 = new Report(testPack, excelTemplate, BODocDataProvider.Get(dummyBizO), "1Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false),
				report2 = new Report(testPack, excelTemplate, BODocDataProvider.Get(dummyBizO), "2Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				testPack.Add(report1);
				testPack.Add(report2);

				report1.PrepareForRender();
				report2.PrepareForRender();
				var dataSet = report1.OverridingDataSet;
				var components1 = new TemplateToVisualiserComponentsConverter(report1, dataSet).Components;
				var components2 = new TemplateToVisualiserComponentsConverter(report2, dataSet).Components;

				AssertEquals(report1.OverridingDataSet, report2.OverridingDataSet);

				var dataTable = dataSet.MainTable;
				dataTable.Rows[0]["<Z0_Code>"] = "Code2";
				testPack.SaveVisualizerContentNote();

				AssertContains("Code2", report1.VisualizerContentNote.DD_DocumentData.ToUTF8());
				AssertContains("Code2", report2.VisualizerContentNote.DD_DocumentData.ToUTF8());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleReportVisualizerNotesCanBeSavedWithItsOwnOverridingDataSet()
		{
			var dummyBizO1 = Factory.New<DummyBusinessObject>();
			dummyBizO1.Z0_Code = "Code1";

			var dummyBizO2 = Factory.New<DummyBusinessObject>();
			dummyBizO2.Z0_Code = "Code2";

			Factory.Save();

			var excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);

			using (Report report1 = new Report(testPack, excelTemplate, BODocDataProvider.Get(dummyBizO1), "1Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false),
				report2 = new Report(testPack, excelTemplate, BODocDataProvider.Get(dummyBizO2), "2Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				testPack.Add(report1);
				testPack.Add(report2);

				report1.PrepareForRender();
				report2.PrepareForRender();
				var components1 = new TemplateToVisualiserComponentsConverter(report1, report1.OverridingDataSet).Components;
				var components2 = new TemplateToVisualiserComponentsConverter(report2, report2.OverridingDataSet).Components;

				var dataTable = report2.OverridingDataSet.MainTable;
				dataTable.Rows[0]["<Z0_Code>"] = "Code2";
				testPack.SaveVisualizerContentNote();

				Assert("report2's visualizer content note should be saved to database", report2.VisualizerContentNote.IsInDatabase);
				AssertContains("Code2", report2.VisualizerContentNote.DD_DocumentData.ToUTF8());
			}
		}

		public void TestRunWithEmailSubjectOverridden()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignor.Code;
			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack) { Destination = DeliveryInstructionDestination.TakenFromContact };
			instructions.Recipients.RemoveAll();
			var contact1 = instructions.Recipients.AddNew();
			contact1.Name = "Justin";
			contact1.EmailSubjectMacro = "Hello <Name>";
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.DeliveryAddress = "address1@test.com";
			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.DeliveryAddress = "address2@test.com";

			var bizo = Factory.New<DummyBusinessObject>();
			var wrapper = new DummyDocumentWrapper(bizo, Factory);

			using (var report = new Report(pack, TestReport, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				pack.Run(instructions);

				Assert(!contact1.DeliveryGroupId.IsEmpty);
				Assert(contact2.DeliveryGroupId.IsEmpty);

				AssertEquals(2, instructions.DeliveryGroups.Count);

				AssertNullOrEmpty(instructions.DeliveryGroups[0].SB_EmailSubjectLine);
				AssertEquals(1, instructions.DeliveryGroups[0].ContainedPrintJobs.Count);

				AssertEquals("Hello Justin", instructions.DeliveryGroups[1].SB_EmailSubjectLine);
				AssertEquals(1, instructions.DeliveryGroups[1].ContainedPrintJobs.Count);
			}
		}

		public void TestEmailSubjectMacroTruncated()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var wrapper = new DummyDocumentWrapper(bizo, Factory);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignor.Code;

			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack) { Destination = DeliveryInstructionDestination.TakenFromContact };
			instructions.Recipients.RemoveAll();

			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.DeliveryAddress = "address@test.com";
			contact.Name = "Test";
			contact.EmailSubjectMacro = new string('a', StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength + 1);

			using (var report = new Report(pack, TestReport, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				AssertNoExceptionThrown(() => pack.Run(instructions));
			}
		}

		public void TestRunForRecipientsWithSpaceEndOfEmailSubject()
		{
			var bizo = Factory.New<DummyBusinessObject>();
			var wrapper = new DummyDocumentWrapper(bizo, Factory);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignor.Code;

			var pack = new DocumentPack(menuItem);
			var instructions = new DeliveryInstructions(pack) { Destination = DeliveryInstructionDestination.TakenFromContact };
			instructions.Recipients.RemoveAll();

			var emailSubjectMacro = "Test email subject <Name>";

			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.DeliveryAddress = "address@test.com";
			contact1.EmailSubjectMacro = emailSubjectMacro;

			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.DeliveryAddress = "address@test.com";
			contact2.EmailSubjectMacro = emailSubjectMacro;

			using (var report = new Report(pack, TestReport, wrapper, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report);
				pack.RunForRecipients(instructions);
				var sameEmailSubjectLineCount = instructions.DeliveryGroups.Count(g => g.SB_EmailSubjectLine == "Test email subject");
				AssertEquals("There should be one email group of Test email subject.", 1, sameEmailSubjectLineCount);
			}
		}

		public void TestNumberOfDocumentNeedToBeConsolidated()
		{
			AssertEquals(0, testPack.NumberOfDocumentNeedToBeConsolidated);
			testPack.NumberOfDocumentNeedToBeConsolidated = 1;
			AssertEquals(1, testPack.NumberOfDocumentNeedToBeConsolidated);
		}

		public void TestIgnoreDuplicatedDeliveryInfos_NotAutoDelivery()
		{
			AssertIgnoreDuplicatedDeliveryInfos(false, 2);
		}

		public void TestIgnoreDuplicatedDeliveryInfos_AutoDelivery()
		{
			AssertIgnoreDuplicatedDeliveryInfos(true, 1);
		}

		public void TestAutoDocumentDelivery_SameReportWithDifferentContacts()
		{
			var pack = testPack;
			var task = new PrintTask();
			task.IsAutoDocumentDelivery = true;
			task.Add(pack);

			using (var report = new MockReport(TestReport))
			{
				report.IncludedInPrint = true;

				DeliveryInstructions deliveryInstructions = new DeliveryInstructions();
				DeliveryMethod deliveryMethod = new DeliveryMethod();

				DocDeliveryContact contact = new DocDeliveryContact(Factory);
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.DeliveryAddress = "Bilbo@hobbits.nz";

				pack.RenderDeliverable_Exposed(contact, deliveryInstructions, deliveryMethod, report);
				AssertEquals("Report added to delivery method", 1, deliveryMethod.FileCount);

				DocDeliveryContact anotherContact = new DocDeliveryContact(Factory);
				anotherContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				anotherContact.DeliveryAddress = "Frodo@hobbits.nz";

				pack.RenderDeliverable_Exposed(anotherContact, deliveryInstructions, deliveryMethod, report);
				AssertEquals("Report added to delivery method", 2, deliveryMethod.FileCount);

				pack.RenderDeliverable_Exposed(anotherContact, deliveryInstructions, deliveryMethod, report);
				AssertEquals("Report NOT added to delivery method as the same report/contact report was already added", 2, deliveryMethod.FileCount);
			}
		}

		public void TestDocumentDelivery_UserDefinedFailingReportSubsequentRenderShouldResetInterface()
		{
			var pack = testPack;

			var task = new PrintTask();
			task.IsAutoDocumentDelivery = true;
			task.Add(pack);
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.SimpleTest.xls", "SimpleTest.xls");
			var simpleTestXls = new ExcelTemplateForUnitTesting("SimpleTest.xls", Path.GetFullPath(tempFileName));

			using (var report = new Report(pack, simpleTestXls))
			{
				var renderer = new Mock<IReportRenderer>();
				var renderException = new SQLExecutionException("ARandomTableName", "SomeCommand", new Exception());
				renderer.SetupSequence(m => m.Render()).Throws(renderException).Throws(renderException);

				report.IncludedInPrint = true;
				report.Renderer = renderer.Object;
				((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

				var deliveryInstructions = new DeliveryInstructions();
				var deliveryMethod = new DeliveryMethod();
				var contact = new DocDeliveryContact(Factory);
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

				AssertNoExceptionThrown(() => { pack.RenderDeliverable_Exposed(contact, deliveryInstructions, deliveryMethod, report); });

				AssertNoExceptionThrown(() => { pack.RenderDeliverable_Exposed(contact, deliveryInstructions, deliveryMethod, report); });

				renderer.VerifyAll();
			}
		}

		public void TestBusinessObjectToLogAgainstForNonPersistentBusinessObjectDefaultsToParent()
		{
			BusinessObject businessObject = (BusinessObject)Factory.New<Enterprise.Freight.Integration.Agency.IBillOfLading>();
			IDocumentSupportable documentSupportableBusinessObject = businessObject as IDocumentSupportable;

			AssertNotNull(documentSupportableBusinessObject);

			TestableDocumentPack documentPack = new TestableDocumentPack(Factory.New<DocumentCommand>(), documentSupportableBusinessObject, null);
			documentPack.RemoveAndDeleteAll();
			AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

			IBODocDataProvider docDataProvider = BODocDataProvider.Get(new NonPersistentBusinessObjectForTesting());
			Assert("Pre-condition: docDataProvider.BusinessObjectToLogAgainst is NonPersistentBusinessObject", docDataProvider.BusinessObjectToLogAgainst is NonPersistentBusinessObject);

			documentPack.Add(new Report(documentPack, null, docDataProvider, "Test", null, DocumentDirection.ANY, false));
			AssertEquals("Pre-condition: documentPack.Count", 1, documentPack.Count);

			AssertEquals(businessObject, documentPack.BusinessObjectToLogAgainst);
		}

		public void TestCulture_LocalDocument()
		{
			Culture.Set(Culture.Default);

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsLocalDocument = true;
			DocumentPack pack = new DocumentPack(menuItem);

			Culture.CultureChanged += new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
			try
			{
				AssertEquals(0, oldCulture.Count);
				AssertEquals(0, newCulture.Count);

				pack.Run(new DeliveryInstructions());

				AssertEquals(2, oldCulture.Count);
				AssertEquals(2, newCulture.Count);

				AssertEquals(Culture.Default, oldCulture[0]);
				AssertEquals(Culture.CurrentCompanyCountryCulture, newCulture[0]);

				AssertEquals(Culture.CurrentCompanyCountryCulture, oldCulture[0]);
				AssertEquals(Culture.Default, newCulture[0]);

				oldCulture.Clear();
				newCulture.Clear();
			}
			finally
			{
				Culture.CultureChanged -= new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
			}
		}

		public void TestCulture_InternationalDocument()
		{
			Culture.Set(Culture.Default);

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsLocalDocument = false;
			DocumentPack pack = new DocumentPack(menuItem);

			Culture.CultureChanged += new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
			try
			{
				AssertEquals(0, oldCulture.Count);
				AssertEquals(0, newCulture.Count);

				pack.Run(new DeliveryInstructions());

				AssertEquals(0, oldCulture.Count);
				AssertEquals(0, newCulture.Count);
			}
			finally
			{
				Culture.CultureChanged -= new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);
			}
		}

		public void TestRunDocPackWithChildDocumentsOnly()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Document";
			documentCommand.SU_BusinessContext = "Test";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Date]
{A}-[#EndOfReport]");
			template.SO_Name = "Customized Document Elements";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var documentConfig = pivot.DocConfigs.AddNew();
			documentConfig.S3_IsSystem = true;
			documentConfig.S3_Description = "testDocConfig";

			var parentCommand = Factory.New<DocumentCommand>();
			parentCommand.SU_MenuName = "Test Parent Document";
			parentCommand.SU_BusinessContext = "Test";
			var documentSupportable = Factory.New<DummyBODocSupportable>();
			parentCommand.Parent = documentSupportable;
			parentCommand.SU_IsDocPack = true;

			var menuMenuPivot = parentCommand.ChildMenus.AddNew();
			menuMenuPivot.SF_SU_Outward = documentCommand.PK;
			menuMenuPivot.SF_SU_Inward = parentCommand.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(parentCommand, documentSupportable, null, null))
			using (var printTask = new DocumentPrintSet(parentCommand))
			{
				var printLoader = new PrintTaskDocumentPackLoader(printTask, parentCommand, null);
				documentPack.Loader = printLoader;
				DocumentPackLoader.Load(printLoader, printTask, documentPack, parentCommand, parentCommand);

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.ChineseSimplified;

				using (var mockRes = Res.UseMockData())
				{
					const string date = "日期";
					mockRes.SetResourceGetter(key => new ResourceStringData(key, date));
					documentPack.Run(deliveryInstructions);
				}

				AssertMultilineASCIIEquals("Generated Template Results - Should have 1 and 4", @"
{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[日期]
{A}-[#EndOfReport]
".Trim(), documentPack.GetFirstReport().XlInterface.WorkSheets[0].ToString());
			}
		}

		[SnailTest]
		public void TestRunMultiDocPackWithDocumentAndForm()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var consol = Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.AddShipment(shipment);

			var consolDocumentSupportable = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(consolDocumentSupportable, null, Factory);

			var docPack = CreateDocPackWithDocumentAndForm(customization);

			Factory.Save();

			RunReportForMutiJobs(customization, docPack);

			var query = new ZQuery(StmPrintJobSchema.SP_ParentTableName, JobConsolSchema.Constants.TableName);
			query.AddToFilter(StmPrintJobSchema.SP_ParentGuid, consol.PK);

			var printJobs = Factory.Load<StmPrintJob>(query);

			AssertEquals("consolidated print job has been created", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
				{
					excelInterface.LoadExcelFile(stream);

					var contents = string.Join(
						System.Environment.NewLine,
						excelInterface.WorkSheets.Select(w => w.ToString()));

					AssertMultilineASCIIEquals("document contents",
@"{B}-[Hello From Document]
{B}-[Hello From Form]", contents);
				}
			}
		}

		public void TestRunInOtherLanguageAlsoSendsEDocs()
		{
			var documentSupportable = Factory.New<DummyBODocSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = documentSupportable;
			documentCommand.SU_MenuName = "Test";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "TST";
			docType.RT_ReferenceType = "ALL";
			documentCommand.AddEDoc(docType);

			using (var bitmap = new Bitmap(1, 1))
			{
				using var stream = new CargoWise.IO.Shim.SubStreamableStream(new MemoryStream());
				bitmap.Save(stream, ImageFormat.Bmp);
				documentSupportable.DocManagerInfo.AddFileOrDocument(stream, "Test.bmp", docType.RT_DocType);
			}

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, documentSupportable, null, null))
			{
				AssertEquals("There should be a document and an eDoc in the pack.", 2, documentPack.Count);

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Language = Enterprise.Core.Constants.Languages.ChineseTraditional;

				documentPack.Run(deliveryInstructions);

				AssertEquals("There should still be a document and an eDoc in the pack.", 2, documentPack.Count);
			}
		}

		public void TestDocumentPackWithFilterFromDocumentWrapperWorksFine()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=GenericFreightJob]
{A}-[#ConfigurableSection:GEN, Strip 1]
{B}-[Strip 1]
{A}-[#ConfigurableSection:GEN, Strip 2]
{B}-[Strip 2]
{A}-[#ConfigurableSection:GEN, Strip 3]
{B}-[Strip 3]
{A}-[#ConfigurableSection:GEN, Strip 4]
{B}-[Strip 4]
{A}-[#EndOfReport]");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Document";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_IsSystem = ZBool.True;

			var configItem1 = config.ConfigItems.AddNew();
			configItem1.S4_PrintOrder = 1;
			configItem1.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItem1.S4_SectionItemName = "Strip 1";
			configItem1.S4_FilterList = @"""<ExportAgentsReference>"" == ""PLUM""";

			var configItem2 = config.ConfigItems.AddNew();
			configItem2.S4_PrintOrder = 2;
			configItem2.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItem2.S4_SectionItemName = "Strip 2";
			configItem2.S4_FilterList = @"""<ExportAgentsReference>"" != ""PLUM""";

			var configItem3 = config.ConfigItems.AddNew();
			configItem3.S4_PrintOrder = 3;
			configItem3.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItem3.S4_SectionItemName = "Strip 3";
			configItem3.S4_FilterList = @"""<JE_DeclarationReference>"" != ""PLUM""";

			var configItem4 = config.ConfigItems.AddNew();
			configItem4.S4_PrintOrder = 4;
			configItem4.S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			configItem4.S4_SectionItemName = "Strip 4";
			configItem4.S4_FilterList = @"""<JE_DeclarationReference>"" == ""PLUM""";

			BusinessObject declaration = Factory.New(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "PLUM";

			documentCommand.Parent = (IDocumentSupportable)declaration;

			var declarationType = declaration.GetType();
			AssertNull("Precondition: Declaration should not have a property called 'ExportAgentsReference' on it. Need to use a field present on the Wrapper but not the Declaration.", declarationType.GetProperty("ExportAgentsReference"));
			// BTW: ExportAgentsReference must be mapped to JE_DeclarationReference on the wrapper returned. If not, this test will have to be refactored.

			using (var documentPack = new DocumentPack(documentCommand, documentCommand.Parent, null, null))
			{
				var report = documentPack.GetFirstReport();

				AssertEquals("Precondition: There should be no errors in the report.", ReportErrorManagement.ReportErrorManager.HasNoErrors, report.ErrorManager.ToString());

				AssertMultilineASCIIEquals("Generated Template Results - Should have 1 and 4", @"
{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=GenericFreightJob]
{A}-[#SectionBody]
{B}-[Strip 1]
{A}-[#SectionBody]
{B}-[Strip 4]
{A}-[#EndOfReport]
".Trim(), report.XlInterface.WorkSheets[0].ToString());
			}
		}

		public void TestIsDraftWorksAfterReportPrepareForRender()
		{
			ReportPrintSetEndToEndTestHelper helper = new ReportPrintSetEndToEndTestHelper(Factory);
			helper.SetTemplate(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#if ""<IsDraft>"" == ""Y""]
{B}-[This is a Draft]
{A}-[#else]
{B}-[This is a Final]
{A}-[#endif]
{A}-[#if ""<IsDraft>"" == ""N""]
{B}-[This is a Final, Don't Place Draft Image in Box]
{A}-[#else]
{B}-[This is a Draft, Place Draft Image in Box]
{A}-[#endif]
{A}-[#EndOfReport]");

			using (var printTask = new ReportPrintSet(helper.MenuItem))
			{
				var report = printTask[0][0] as Report;
				AssertNotNull("Pre-conditon: Report should not be null.", report);

				var deliveryInstructions = new DeliveryInstructions();
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				recipient.AttachmentType = "PDF";
				recipient.Email = "unit.test@cargowise.com";

				report.PrepareForRender();

				deliveryInstructions.IsDraft = true;

				helper.AssertRun(
@"{B}-[This is a Draft]
{B}-[This is a Draft, Place Draft Image in Box]", printTask, deliveryInstructions);
			}
		}

		public void TestSuppressDraftWatermarkEnabled()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
	@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[SuppressDraftWatermark]
{A}-[#SectionBody]
{A}-[#if ""<IsDraft>"" == ""Y""]
{B}-[This is a Draft]
{A}-[#else]
{B}-[This is a Final]
{A}-[#endif]
{A}-[#if ""<IsDraft>"" == ""N""]
{B}-[This is a Final, Don't Place Draft Image in Box]
{A}-[#else]
{B}-[This is a Draft, Place Draft Image in Box]
{A}-[#endif]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					documentPack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.IsDraft = true;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Email = "unit.test@cargowise.com";

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()",
	@"{B}-[This is a Draft]
{B}-[This is a Draft, Place Draft Image in Box]",
							excelInterface.WorkSheets[0].ToString());
						Assert("printJobs[0].SP_WatermarkText.IsEmpty", printJobs[0].SP_WatermarkText.IsEmpty);
					}
				}
			}
		}

		public void TestSuppressDraftWatermarkDisabled()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#if ""<IsDraft>"" == ""Y""]
{B}-[This is a Draft]
{A}-[#else]
{B}-[This is a Final]
{A}-[#endif]
{A}-[#if ""<IsDraft>"" == ""N""]
{B}-[This is a Final, Don't Place Draft Image in Box]
{A}-[#else]
{B}-[This is a Draft, Place Draft Image in Box]
{A}-[#endif]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					documentPack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.IsDraft = true;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Email = "unit.test@cargowise.com";

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()",
@"{B}-[This is a Draft]
{B}-[This is a Draft, Place Draft Image in Box]",
							excelInterface.WorkSheets[0].ToString());
						Assert("!printJobs[0].SP_WatermarkText.IsEmpty", !printJobs[0].SP_WatermarkText.IsEmpty);
					}
				}
			}
		}

		public void TestRunUsesLanguageSelectedInDeliveryInstructions()
		{
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_Name = SectionRepositoryTemplateNames.System;// +" [GRM]";
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			templateRecord.SO_Template = CustomisableSectionTest.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			using (var documentPack = new DocumentPack(command, docSupportedBO, null, null))
			{
				documentPack.Organisation = Factory.New<OrgHeader>();
				documentPack.Organisation.OH_Language = Core.Constants.Languages.English;
				var instructions = new DeliveryInstructions(documentPack);

				documentPack.Run(instructions);
				AssertEquals("documentPack.TemplateGenerator.Language", Core.Constants.Languages.EnglishAmerican, documentPack.LastTemplateGeneratorLanguage);

				instructions.Language = Core.Constants.Languages.German;
				documentPack.Run(instructions);
				AssertEquals("documentPack.TemplateGenerator.Language", Core.Constants.Languages.German, documentPack.LastTemplateGeneratorLanguage);

				instructions.Language = Core.Constants.Languages.EnglishBritish;
				documentPack.Run(instructions);
				AssertEquals("documentPack.TemplateGenerator.Language", Core.Constants.Languages.EnglishBritish, documentPack.LastTemplateGeneratorLanguage);
			}
		}

		public void TestIsDisposed()
		{
			DocumentPack documentPack = null;

			using (documentPack = new DocumentPack())
			{
				AssertEquals("documentPack.IsDisposed", false, documentPack.IsDisposed);
			}

			AssertEquals("documentPack.IsDisposed", true, documentPack.IsDisposed);
		}

		public void TestGetFirstReport()
		{
			using (DocumentPack documentPack = new DocumentPack())
			{
				AssertNull("Pre-condition: Document pack has nothing to get.", documentPack.GetFirstReport());

				DummyDeliverable deliverable = new DummyDeliverable();
				documentPack.Add(deliverable);
				AssertEquals("Document pack has 1 deliverable.", 1, documentPack.Count);
				AssertNull("Document pack should have no reports to get.", documentPack.GetFirstReport());

				Report firstReport = new Report(documentPack, null);
				documentPack.Add(firstReport);
				AssertEquals("Document pack has 2 deliverables.", 2, documentPack.Count);
				AssertEquals("Document pack should get the first report.", firstReport, documentPack.GetFirstReport());

				Report secondReport = new Report(documentPack, null);
				documentPack.Add(secondReport);
				AssertEquals("Document pack has 3 deliverables.", 3, documentPack.Count);
				AssertEquals("Document pack should get the first report.", firstReport, documentPack.GetFirstReport());
			}
		}

		public void TestGetFirstNonCoverSheetReport()
		{
			using (var documentPack = new DocumentPack())
			{
				AssertNull("Pre-condition: Document pack has nothing of NonCoverSheet to get.", documentPack.GetFirstNonCoverSheetReport());

				var coverSheet = new Report(documentPack, null) { IsCoverSheet = true };
				documentPack.Add(coverSheet);
				AssertEquals("Document pack has 1 deliverables.", 1, documentPack.Count);
				AssertNull("Document pack can't get a NonCoverSheet report as there's only 1 coversheet", documentPack.GetFirstNonCoverSheetReport());

				var firstReport = new Report(documentPack, null);
				documentPack.Add(firstReport);
				AssertEquals("Document pack has 2 deliverables.", 2, documentPack.Count);
				AssertEquals("Document pack should get the first NonCoverSheet report.", firstReport, documentPack.GetFirstNonCoverSheetReport());

				var secondReport = new Report(documentPack, null);
				documentPack.Add(secondReport);
				AssertEquals("Document pack has 3 deliverables.", 3, documentPack.Count);
				AssertEquals("Document pack should get the first NonCoverSheet report.", firstReport, documentPack.GetFirstNonCoverSheetReport());

				firstReport.IncludedInPrint = false;
				documentPack.Add(secondReport);
				AssertEquals("Document pack has 3 deliverables.", 3, documentPack.Count);
				AssertEquals("Document pack should get the second report.", secondReport, documentPack.GetFirstNonCoverSheetReport());
			}
		}

		public void TestDocumentPackConstructorWithReportCommand()
		{
			ReportCommand command = Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Profit and Loss Periods Analysis"));
			DocumentPack pack = new DocumentPack(command);
			AssertEquals("pack.Count", 1, pack.Count);
			AssertEquals("pack[0] is Report", true, pack[0] is Report);
			Report report = (Report)pack[0];
			AssertMultilineASCIIEquals("Report Information", string.Format(@"
Report Information:

MenuItem:-
   BusinessContext = [RepGLReports]
   Name with Path = [Profit and Loss Periods Analysis]
   Filter = []
   PK = [{0}]
   IsSystemDefined = [Y]
   IsClientSpecific = [N]

Template:-
   Name = [Profit Loss Period Analysis]
   DataContext = [None]
   ExcelFilePath = [{1}]
   PK = [{2}]
   IsSystemDefined = [Y]
   IsClientSpecific = [N]

Not Running from Scheduled Report.".Trim(), report.MenuItem.PK, report.StTemplate.SO_ExcelTemplatePath, report.StTemplate.PK), report.ToString());
		}

		public void TestDocumentDeliveredEvent_Standard()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var pack = new DocumentPack();
			pack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(Events.DocumentDelivered.Code, pack[0].DocumentDeliveredEventCode);

			Factory.Save();
			AssertDocumentDeliveredEvents(docSupportedBO, pack, Events.DocumentDelivered.Code);
		}

		public void TestAddReportsToPack_ForDocBuilderDocumentWithNoDocumentConfig_ShouldNotAddBlankReport()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			command.SU_MenuName = "Document1";
			var docBuilderTemplate = Factory.New<StmTemplateBase>();
			docBuilderTemplate.SO_DataContext = ".DummyBODocSupportable";
			docBuilderTemplate.SO_Name = SectionRepositoryTemplateNames.System;
			docBuilderTemplate.SO_Template = SectionRepositoryTestHelper.GetConfigurableTemplateBlob("No need for cellText here");
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = docBuilderTemplate.PK;
			pivot.SI_SU = command.PK;
			pivot.SI_DocumentTitle = "Template1";

			string expectedNoConfMessage = string.Format(@"No Customizable Document Configuration could be applied for DocBuilder template '{0}' of document '{1}'.
Please check if a non-client specific configuration exists for this template, or whether client-specific configurations match the Document Group of this document.", pivot.SI_DocumentTitle, command.SU_MenuName);

			string expectedNoConfTemplateMessage = string.Format(@"No Customizable Document Configuration could be applied for DocBuilder template '{0}' of document '{1}'.
The system configuration provided serves as an example and needs to be copied with a default recipient.", pivot.SI_DocumentTitle, command.SU_MenuName);

			var pack = new DocumentPack();
			Assert(docBuilderTemplate.IsDocBuilderStyle);
			AssertEquals(0, pack.Count);
			AssertEquals(0, pack.ReasonsForEmptyPacks.Count);
			pack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(0, pack.Count);
			AssertEquals(3, pack.ReasonsForEmptyPacks.Count);
			AssertEquals(expectedNoConfMessage, pack.ReasonsForEmptyPacks[2]);

			var anotherPack = new DocumentPack();
			StmMenuDocumentConfig docConfig = pivot.DocConfigs.AddNew();
			AssertEquals(0, anotherPack.Count);
			AssertEquals(0, anotherPack.ReasonsForEmptyPacks.Count);
			anotherPack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(1, anotherPack.Count);
			AssertEquals(0, anotherPack.ReasonsForEmptyPacks.Count);

			var yetAnotherPack = new DocumentPack();
			docConfig.S3_OH = Guid.NewGuid();
			AssertEquals(0, yetAnotherPack.Count);
			AssertEquals(0, yetAnotherPack.ReasonsForEmptyPacks.Count);
			yetAnotherPack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(0, yetAnotherPack.Count);
			AssertEquals(3, yetAnotherPack.ReasonsForEmptyPacks.Count);
			AssertEquals(expectedNoConfMessage, yetAnotherPack.ReasonsForEmptyPacks[2]);

			var yetAgainAnotherPack = new DocumentPack();
			docConfig.S3_OH = ZGuid.Empty;
			docConfig.S3_IsSystem = true;
			docConfig.S3_IsTemplate = true;
			AssertEquals(0, yetAgainAnotherPack.Count);
			AssertEquals(0, yetAgainAnotherPack.ReasonsForEmptyPacks.Count);
			yetAgainAnotherPack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(0, yetAgainAnotherPack.Count);
			AssertEquals(3, yetAgainAnotherPack.ReasonsForEmptyPacks.Count);
			AssertEquals(expectedNoConfTemplateMessage, yetAgainAnotherPack.ReasonsForEmptyPacks[2]);

			var command2 = Factory.NewWithValidTestData<DocumentCommand>();
			command2.SU_MenuName = "NULL";
			var pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SO = docBuilderTemplate.PK;
			pivot2.SI_SU = command2.PK;
			pivot2.SI_DocumentTitle = "Template2";
			pivot2.DocConfigs.AddNew();
			var testPack4 = new DocumentPack();
			docConfig = pivot.DocConfigs.AddNew();
			testPack4.AddReportsToPack(command2, null, docSupportedBO, null);
			AssertEquals(0, testPack4.Count);
			AssertEquals(3, testPack4.ReasonsForEmptyPacks.Count);
			AssertEquals("NOT FOUND", testPack4.ReasonsForEmptyPacks[2]);
		}

		public void TestDocPackBoForPrintJobShouldBeReportBoForPrintJob()
		{
			var biz = Factory.New<DummyBODocSupportable_DocDataProvider>();
			var shipment = Factory.New<IForwardingShipment>() as BusinessObject;
			biz.SetBusinessObjectForPrintJob(shipment);
			var consol = Factory.New<IForwardingConsol>() as BusinessObject;

			var pack = new DocumentPack();
			pack.Add(new Report(pack, null, new DataProviderList(biz), "abc", null, DocumentDirection.ANY, false));

			AssertEquals(1, pack.Count);
			AssertEquals(shipment, pack.BusinessObjectForPrintJob);

			pack = new DocumentPack();
			pack.Add(new Report(pack, null, new DataProviderList(biz), "abc", null, DocumentDirection.ANY, false));
			pack.ForceBusinessObjectToLogAgainst(consol);

			AssertEquals(consol, pack.BusinessObjectForPrintJob);
		}

		public void TestBusinessObjectForPrintJobIsPrimaryDocumentBizObject()
		{
			var biz1 = Factory.New<DummyBODocSupportable_DocDataProvider>();
			var biz2 = Factory.New<DummyBODocSupportable_DocDataProvider>();
			var biz3 = Factory.New<DummyBODocSupportable_DocDataProvider>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = biz3;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var pack = new DocumentPack(documentCommand, biz3, null, null);
			var report1 = new Report(pack, null, new DataProviderList(biz1), "abc", null, DocumentDirection.ANY, false);
			var report2 = new Report(pack, null, new DataProviderList(biz2), "abc", null, DocumentDirection.ANY, false);
			pack.Add(report1);
			pack.Add(report2);

			report2.SourcePivotPK = ZGuid.NewZGuid();
			documentCommand.SU_PrimaryDocPackItemId = report2.SourcePivotPK;

			AssertEquals(3, pack.Count);
			AssertEquals(biz3, pack.BusinessObjectToLogAgainst);
			AssertEquals(biz2.ParentBusinessObject, pack.BusinessObjectForPrintJob);
		}

		public void TestParentBizObjectOfPrintJobShouldBeTheBizObjectOfPrimaryDocument()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var mainBizO = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = mainBizO;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			helper.CreateMenuTemplatePivot("Main Document", template, documentCommand);

			var command2 = Factory.New<DocumentCommand>();
			var bizO2 = Factory.New<DummyBODocSupportable>();
			helper.CreateMenuTemplatePivot("Main Document", template, command2);

			using (var pack = new DocumentPack(documentCommand, mainBizO, null, null))
			{
				pack.AddReportsToPack(command2, null, bizO2, null);

				AssertEquals(2, pack.Count);

				var instructions = new DeliveryInstructions(pack)
				{
					Destination = DeliveryInstructionDestination.TakenFromContact
				};
				instructions.SetAndSaveDeliveryGroupSubjectLine(pack, new PrintTask.ReportSubjectLineMapping(null, "test"));
				var contact = instructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Email = "test@test.com";

				documentCommand.SU_PrimaryDocPackItemId = pack.OfType<Report>().Last().SourcePivotPK;

				pack.Run(instructions);
				AssertEquals(1, Factory.GetDatabaseCount(typeof(StmPrintJob)));

				var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
				AssertEquals(bizO2.PK, printJob.SP_ParentGuid);
			}
		}

		public void TestBusinessObjectToLogAgainst_IsJustBizObject()
		{
			//Setup: docSupportedBO is the document pack's bizO, docSupportedBO's random parent is returned by report 1, docSupportedBO is returned by report 2. We want logging to be against docSupportedBO instead of its random parent.
			var docSupportedBO = Factory.New<DummyBODocSupportable_DocDataProvider>();
			var docSupportedBO2 = Factory.New<DummyBODocSupportable_DocDataProvider>();
			docSupportedBO2.fParentBusinessObject = docSupportedBO;
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_Name = SectionRepositoryTemplateNames.System;// +" [GRM]";
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			templateRecord.SO_Template = CustomisableSectionTest.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var pack = new DocumentPack(command, docSupportedBO, null, null);
			var firstReport = new Report(pack, null, new DataProviderList(docSupportedBO), "abc", null, DocumentDirection.ANY, false);
			pack.Add(firstReport);
			AssertEquals(1, pack.Count);
			AssertNotEquals(docSupportedBO, pack.BusinessObjectToLogAgainst);

			pack = new DocumentPack(command, docSupportedBO, null, null);
			var secondReport = new Report(pack, null, new DataProviderList(docSupportedBO2), "def", null, DocumentDirection.ANY, false);
			pack.Add(secondReport);
			AssertEquals(1, pack.Count);
			AssertEquals(docSupportedBO, pack.BusinessObjectToLogAgainst);

			pack = new DocumentPack(command, docSupportedBO, null, null);
			pack.Add(firstReport);
			pack.Add(secondReport);
			AssertEquals(2, pack.Count);
			AssertEquals(docSupportedBO, pack.BusinessObjectToLogAgainst);

			pack = new DocumentPack(command, docSupportedBO, null, null);
			pack.Add(secondReport);
			pack.Add(firstReport);
			AssertEquals(2, pack.Count);
			AssertEquals(docSupportedBO, pack.BusinessObjectToLogAgainst);
		}

		public void TestBusinessObjectToLogAgainstForMutiJobs()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "DEFRA";

			var consol = Factory.New<IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.AddShipment(shipment);

			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);
			var documentCommand = customization.Menus.AddNew();

			documentCommand.SU_BusinessContext = nameof(BusinessContext.Consol);
			documentCommand.SU_MenuName = "Test Primary Doc";

			var shipmentDocumentMenu = customization.AvailableChildMenus.Cast<StmMenuItemBase>().FirstOrDefault(a => a.SU_BusinessContext == "Shipment" && a.SU_MenuName == "Combined Cartage Advice");
			var shipmentChildPivot = documentCommand.ChildMenus.AddNew();
			shipmentChildPivot.SF_SU_Inward = documentCommand.PK;
			shipmentChildPivot.SF_SU_Outward = shipmentDocumentMenu.PK;

			var consolDocumentMenu = customization.AvailableChildMenus.Cast<StmMenuItemBase>().FirstOrDefault(a => a.SU_BusinessContext == "Consol" && a.SU_MenuName == "Combined Cartage Advice");
			var consolChildPivot = documentCommand.ChildMenus.AddNew();
			consolChildPivot.SF_SU_Inward = documentCommand.PK;
			consolChildPivot.SF_SU_Outward = consolDocumentMenu.PK;

			documentCommand.SU_IsDocPack = true;
			documentCommand.SU_PrimaryDocPackItemId = consolChildPivot.PK;
			RunReportForMutiJobs(customization, documentCommand);
			AssertEdocParentBO(customization, documentCommand, consol as BusinessObject);
			AssertDDVEventExists(customization, documentCommand, consol as BusinessObject);
		}

		public void TestAddDocumentsToPack_WhenTopLevelBusinessObjectIsNull_ShouldNotThrow()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			AssertNoExceptionThrown(() => new DocumentPack().AddReportsToPack(command, null, null, null));
		}

		public void TestAddDocumentsToPack_WhenDocumentSupporterIsNull_ShouldNotThrow()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var docSupportedBO = Factory.New<DummyBODocSupportableWithNullDocumentSupporter>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			AssertNoExceptionThrown(() => new DocumentPack().AddReportsToPack(command, null, docSupportedBO, null));
		}

		public void TestDocumentDeliveredEvent_AggressivelyRecoverFromErrorsAndExceptions()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			Factory.RefreshEnabled = false;

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_RT_DocType = docType.PK;
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var pack = new DocumentPack();
			pack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(Events.ExportReceivalAdvisePrinted.Code, pack[0].DocumentDeliveredEventCode);

			// And now we set up the test for failure.
			docSupportedBO.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(32)); // Creating an event in the future.
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "YAmIEvenLookingAtThis";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			Factory.Save();
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DummyWithWorkflow.ShouldApplyWorkflowTemplateOnSave.Value = true;
			Globals.IsUserInteractive = true;

			AssertDocumentDeliveredEvents(docSupportedBO, pack, Events.ExportReceivalAdvisePrinted.Code);
		}

		public void TestDocumentDeliveredEvent_DocTypeEvent()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			docType.RT_DocType = "AAA";
			docType.RT_ReferenceType = "ALL";
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_RT_DocType = docType.PK;
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var pack = new DocumentPack();
			pack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals(Events.ExportReceivalAdvisePrinted.Code, pack[0].DocumentDeliveredEventCode);

			Factory.Save();
			AssertDocumentDeliveredEvents(docSupportedBO, pack, Events.ExportReceivalAdvisePrinted.Code);
		}

		void AssertDocumentDeliveredEvents(DummyBODocSupportable docSupportedBO, DocumentPack pack, string expectedEventCode)
		{
			var contact = new DocDeliveryContact(Factory);
			contact.AttachmentType = "PDF";
			contact.DeliveryMethod = "EML";
			contact.Email = "zappoo@zip.com.au";
			contact.Fax = "12345";

			DeliveryInstructions instructions = new DeliveryInstructions();

			pack.Deliver(new DeliveryMethods.Email(contact) { Instructions = instructions }, instructions);
			var latestLogsQuery = new ZQuery(StmALogSchema.SL_Parent, docSupportedBO.PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, expectedEventCode)
				.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			latestLogsQuery.ReLoadExistingRows = true;

			AssertNotNull("Event created for Email, Final", Factory.Load<StmALog>(latestLogsQuery).Single());
			latestLogsQuery.AddToFilter(StmALogSchema.PK, SQLComparisonOperator.NotEqual, Factory.Load<StmALog>(latestLogsQuery).Single().PK);

			pack.Deliver(new Disk(), instructions);
			AssertNull("No event for Disk", Factory.Load<StmALog>(latestLogsQuery).SingleOrDefault(s => !s.IsDeleted));

			pack.Deliver(new Disk(), instructions);
			AssertNull("No event for Disk", Factory.Load<StmALog>(latestLogsQuery).SingleOrDefault(s => !s.IsDeleted));

			instructions.IsDraft = true;
			pack.Deliver(new Fax(contact), instructions);
			AssertNull("No event for Draft Fax", Factory.Load<StmALog>(latestLogsQuery).SingleOrDefault(s => !s.IsDeleted));

			instructions.IsDraft = false;
			pack.Deliver(new Fax(contact), instructions);
			AssertNotNull("Event created for Fax, not draft", Factory.Load<StmALog>(latestLogsQuery).SingleOrDefault(s => !s.IsDeleted));
		}

		public void TestReportNameAppliesOverrideDocumentNameForDocBuilder()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;
			pivot.SI_DocumentTitle = "New Document";

			var config = pivot.DocConfigs.AddNew();
			config.S3_GC = ZGuid.Empty;
			config.S3_OverrideEmailSubject = "<Z0_Code> Override Document Name";
			config.S3_OH = client.PK;
			config.S3_IsSystem = false;

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			docSupportedBO.Z0_Guid = client.PK;
			docSupportedBO.Z0_Code = "Test";

			Factory.Save();

			using (var pack = new DocumentPack(command, docSupportedBO, null, null))
			{
				var report = pack[0] as Report;
				AssertEquals("Test Override Document Name", report.Name);
			}
		}

		public void TestCustomisableDocumentTemplateWorksWhenMatchedUpByAnOrg()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_GC = ZGuid.Empty;
			config.S3_OH = client.PK;
			config.S3_IsSystem = false;

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			docSupportedBO.Z0_Guid = client.PK;
			docSupportedBO.Z0_Code = "Test";
			var child = docSupportedBO.Collection.AddNew();
			child.Z0_VarCharMax = "CRAPOLA!!";

			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			templateRecord.SO_Template = CustomisableSectionTest.GetAsByteArray();

			var item1 = config.ConfigItems.AddNew();
			item1.S4_SectionItemName = "My Head Hurts";
			item1.S4_SectionType = ConfigurableSectionTypeList.Codes.DocumentHeader;
			item1.S4_PrintOrder = 0;
			Factory.Save();

			using (var pack = new DocumentPack(command, docSupportedBO, null, null))
			using (var outputStream = new MemoryStream())
			{
				var report = pack[0] as Report;
				report.Save(outputStream);
				using (var xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(outputStream);
					AssertMultilineASCIIEquals(""
						, "{B}-[Test]"
						, xlInterface.WorkSheets[0].ToString());
				}
			}

			var item2 = config.ConfigItems.AddNew();
			item2.S4_SectionItemName = "Brett's Farts STILL Stink";
			item2.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			item2.S4_PrintOrder = 0;
			Factory.Save();

			using (var pack = new DocumentPack(command, docSupportedBO, null, null))
			using (var outputStream = new MemoryStream())
			{
				var report = pack[0] as Report;
				report.Save(outputStream);
				using (var xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(outputStream);
					AssertMultilineASCIIEquals(""
						, "{B}-[Test]\r\n\r\n{B}-[CRAPOLA!!]"
						, xlInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestDeliveryInstructions()
		{
			DocumentPack pack = new DocumentPack();
			AssertNotNull(pack.DeliveryInstructions);

			DeliveryInstructions instructions = pack.DeliveryInstructions;
			AssertEquals(instructions, pack.DeliveryInstructions);
		}

		public void TestMultiDocPackDeliveryFindsContacts()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			DeliveryInstructions instructions = new DeliveryInstructions();
			Assert("SINGLE document packs", !instructions.MultipleDocumentPacks);

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.Name = "Zubin";

			DocDeliveryContact contact2 = instructions.Recipients.AddNew();
			contact2.Name = "Harry";

			using (DocumentPack pack = new DocumentPack())
			{
				MockDocSupportBizO bizO = new MockDocSupportBizO();
				pack.DocumentSupporter = bizO.DocumentSupporter;

				AssertEquals("2 contacts to be delivered to - taken from Recipients as this is a single doc pack", 2, pack.GetDeliveryContacts(instructions).Count);
				pack.Run(instructions);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			instructions.DocumentPackCount = 2;
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.Destination = DeliveryInstructionDestination.Auto;
			Assert("Multiple document packs", instructions.MultipleDocumentPacks);

			using (DocumentPack pack = new DocumentPack())
			{
				pack.Organisation = OrgHeader.New(Factory);
				MockDocSupportBizO bizO = new MockDocSupportBizO();
				pack.DocumentSupporter = bizO.DocumentSupporter;

				DocDeliveryContactCollection results = pack.GetDeliveryContacts(instructions);
				AssertEquals("One System contacts to be delivered to - taken from AutoDelivery as multi doc pack", 1, results.Count);
				AssertEquals("Contact Delivery Mode", Core.Constants.ContactNotifyModes.Email, results[0].DeliveryMethod);

				instructions.PrintMultiDocPack = true;
				results = pack.GetDeliveryContacts(instructions);
				AssertEquals("One System contacts to be delivered to - taken from AutoDelivery as multi doc pack", 1, results.Count);
				AssertEquals("Contact Delivery Mode is changed to Print as instructions are print", Core.Constants.ContactNotifyModes.Print, results[0].DeliveryMethod);
			}

			instructions.AutoDeliverMultiDocPack = true;

			using (DocumentPack pack = new DocumentPack())
			{
				OrgHeader org = OrgHeader.New(Factory);
				org.OH_FullName = "Zubins Org";
				pack.Organisation = org;

				AssertEquals("No contacts to be delivered to - DeliveryFilter is null", 0, pack.GetDeliveryContacts(instructions).Count);
				pack.Run(instructions);
				AssertEquals("Error regarding no auto delivery contacts", "There are no contacts found that can be used for Delivery for 'Zubins Org'", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestMultiDocPackDelivery_WithPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var recipientName = @"{B}-[THE ACCOUNTS RECEIVABLE MANAGER|>MIAMI METRO POLICE]";
			AssertMultiDocPackDelivery_RespectPrintSystemCreatedContactWhenNoRealContactFound(recipientName);
		}

		public void TestMultiDocPackDelivery_WithoutPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var recipientName = @"{B}-[MIAMI METRO POLICE]";
			AssertMultiDocPackDelivery_RespectPrintSystemCreatedContactWhenNoRealContactFound(recipientName);
		}

		void AssertMultiDocPackDelivery_RespectPrintSystemCreatedContactWhenNoRealContactFound(string recipientName)
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MMP";
			org.OH_FullName = "Miami Metro Police";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_QueueName = "TestPrinter";
			printQueue.SQ_DisplayName = "TestPrinter";

			Factory.Save();

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Payables.Code;

			var instructions = new DeliveryInstructions();
			instructions.DocumentPackCount = 2;
			instructions.Destination = DeliveryInstructionDestination.Print;

			using (var printTask = new PrintTask())
			using (var templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack(menuItem))
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<RecipientNameAndAddress>]
{A}-[#EndOfReport]"))
			{
				instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
				documentPack.Organisation = org;
				printTask.Add(documentPack);
				documentPack.Add(report);
				printTask.Run(instructions);
			}

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();

			AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

			using (var excelInterface = new ExcelInterface())
			using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
			{
				excelInterface.LoadExcelFile(stream);
				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
				AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", recipientName, excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestAutoDeliveryMultiDocPacksUsesOrganisationFilter()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			MockDocSupportBizO bizO = new MockDocSupportBizO(org);
			testPack.DocumentSupporter = bizO.DocumentSupporter;
			testPack.OrgHeaderContact = (OrgHeaderContact)bizO.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY);

			instructions.Destination = DeliveryInstructionDestination.Auto;
			instructions.DocumentPackCount = 2;
			testPack.Run(instructions);
			AssertEquals("Should call the DeliveryContactFinder to load contacts for autodelivery of multiple doc packs", 1, contactFinder.CallCount);
		}

		public void TestOneDocumentOneContact()
		{
			using (var report1 = new MockReport(TestReport))
			{
				var testPack = new DocumentPack();
				testPack.Add(report1);
				instructions = new DeliveryInstructions(testPack);
				testPack.Run(instructions);

				AssertEquals("Run count", 1, report1.RunCount);
			}
		}

		public void TestOneEDocOneContact()
		{
			DummyDocManagerTestBizO dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();
			IDeliverable eDoc = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
			eDoc.IncludedInPrint = true;

			DocumentPack testPack = new DocumentPack();
			testPack.Add(eDoc);
			testPack.Run(new DeliveryInstructions(testPack));
			AssertEquals("Run count", 1, eDoc.RunCountForTesting);
			eDoc.DeleteTempFilesForTesting();
		}

		public void TestThreeDocumentsOneContact()
		{
			using (var report1 = new MockReport(TestReport))
			using (var report2 = new MockReport(TestReport))
			using (var report3 = new MockReport(TestReport))
			{
				var testPack = new DocumentPack();
				testPack.Add(report1);
				testPack.Add(report2);
				testPack.Add(report3);
				testPack.Run(new DeliveryInstructions(testPack));

				AssertEquals("Run count", 1, report1.RunCount);
				AssertEquals("Run count", 1, report2.RunCount);
				AssertEquals("Run count", 1, report3.RunCount);
			}
		}

		public void TestThreeDocumentsIncludingEDocOneContact()
		{
			using (var report1 = new MockReport(TestReport))
			{
				DummyDocManagerTestBizO dummy = Factory.New<DummyDocManagerTestBizO>();
				dummy.SetupDocManagerObjects();
				IDeliverable eDoc1 = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
				IDeliverable eDoc2 = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[1];
				eDoc1.IncludedInPrint = true;
				eDoc2.IncludedInPrint = true;
				string[] beforeFiles = Directory.GetFiles(Env.TempPath);

				DocumentPack testPack = new DocumentPack();
				testPack.Add(report1);

				testPack.Add(eDoc1);
				testPack.Add(eDoc2);
				testPack.Run(new DeliveryInstructions(testPack));

				string[] afterFiles = Directory.GetFiles(Env.TempPath);

				AssertEquals("Run count", 1, report1.RunCount);
				AssertEquals("Run count on eDoc", 1, eDoc1.RunCountForTesting);
				AssertEquals("Run count on eDoc", 1, eDoc2.RunCountForTesting);

				eDoc1.DeleteTempFilesForTesting();
				eDoc2.DeleteTempFilesForTesting();
			}
		}

		public void TestAllPrintJobsHaveSameRunDateTime()
		{
			using (var templateStream = new MemoryStream())
			using (var testPack = new DocumentPack())
			using (var report1 = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(testPack, templateStream,
				@"{A}-[#Config]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					var instructions = new DeliveryInstructions();
					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					instructions.Recipients.RemoveAndDeleteAll();

					var docContact = instructions.Recipients.AddNew();
					docContact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					docContact.AttachmentType = AttachmentTypeList.Codes.Pdf;
					docContact.Email = "unit.test@cargowise.com";
					docContact.Name = "Test";

					var dummy = Factory.New<DummyDocManagerTestBizO>();
					dummy.SetupDocManagerObjects();
					var eDoc1 = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
					var eDoc2 = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[1];
					eDoc1.IncludedInPrint = true;
					eDoc2.IncludedInPrint = true;
					testPack.Add(report1);
					testPack.Add(eDoc1);
					testPack.Add(eDoc2);

					printTask.Add(testPack);
					printTask.Run(instructions);

					var printJobs = new StmPrintJobCollection(Factory);
					printJobs.Load();

					AssertEquals("Pre-condition: printJobs.Count", 3, printJobs.Count);

					foreach (StmPrintJob printJob in printJobs)
					{
						AssertEquals(instructions.RunDateTime, printJob.SP_RunDateTime);
					}

					eDoc1.DeleteTempFilesForTesting();
					eDoc2.DeleteTempFilesForTesting();
				}
			}
		}

		public void TestOneDocumentThreeContacts()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var bizO = new MockDocSupportBizO(org1, org2);
			testPack.DocumentSupporter = bizO.DocumentSupporter;
			testPack.OrgHeaderContact = (OrgHeaderContact)testPack.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY);
			instructions = new DeliveryInstructions(testPack);

			instructions.Recipients.AddNew().DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			instructions.Recipients.AddNew().DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			instructions.Recipients.AddNew().DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			using (var report1 = new MockReport(TestReport))
			{
				testPack.AddAndSetMenuItem(report1, testMenuItem);
				testPack.Run(instructions);

				AssertEquals("Run count", 3, report1.RunCount);
			}
		}

		public void TestOneEDocThreeContacts()
		{
			ZQuery filter1 = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			filter1.AddToFilter(OrgHeaderSchema.OH_IsConsignor, ZBool.True);

			ZQuery filter2 = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			filter2.AddToFilter(OrgHeaderSchema.OH_IsConsignor, ZBool.True);

			OrgHeader org1 = Factory.LoadTop1<OrgHeader>(filter1);
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(filter2);
			MockDocSupportBizO bizO = new MockDocSupportBizO(org1, org2);
			testPack.DocumentSupporter = bizO.DocumentSupporter;
			testPack.OrgHeaderContact = (OrgHeaderContact)testPack.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY);
			instructions = new DeliveryInstructions(testPack);

			DocDeliveryContact contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "XLS";
			contact1.Email = "test@test.com";

			DocDeliveryContact contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.AttachmentType = "XLS";
			contact2.Email = "test@test.com";

			DocDeliveryContact contact3 = instructions.Recipients.AddNew();
			contact3.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact3.AttachmentType = "XLS";
			contact3.Email = "test@test.com";

			DummyDocManagerTestBizO dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();
			IDeliverable eDoc = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
			eDoc.IncludedInPrint = true;
			string[] beforeFiles = Directory.GetFiles(Env.TempPath);

			testPack.AddAndSetMenuItem(eDoc, testMenuItem);
			testPack.Run(instructions);
			string[] afterFiles = Directory.GetFiles(Env.TempPath);
			AssertEquals("The eDoc should be run three times", 3, eDoc.RunCountForTesting);
			eDoc.DeleteTempFilesForTesting();
		}

		public void TestTwoDocumentsTwoContacts()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var bizO = new MockDocSupportBizO(org1, org2);
			testPack.DocumentSupporter = bizO.DocumentSupporter;
			testPack.OrgHeaderContact = (OrgHeaderContact)testPack.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY);
			instructions = new DeliveryInstructions(testPack);

			var contact1 = instructions.Recipients.AddNew();
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.AttachmentType = "XLS";
			contact1.Email = "test@test.com";

			var contact2 = instructions.Recipients.AddNew();
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.AttachmentType = "XLS";
			contact2.Email = "test@test.com";

			using (var report1 = new MockReport(TestReport))
			using (var report2 = new MockReport(TestReport))
			{
				testPack.AddAndSetMenuItem(report1, testMenuItem);
				testPack.AddAndSetMenuItem(report2, testMenuItem);
				testPack.Run(instructions);

				AssertEquals("Run count", 2, report1.RunCount);
				AssertEquals("Run count", 2, report2.RunCount);
			}
		}

		public void TestTwoReportsWithDifferentDeliveryContacts()
		{
			testMenuItem.SU_ContactType = ContactType.Consignor.Code;
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			MockDocSupportBizO mockBiz = new MockDocSupportBizO(org1, org2);

			DocumentPack pack = new DocumentPack(testMenuItem);
			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.None;
			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			pack.OrgHeaderContact = new OrgHeaderContact(contact1);

			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			var template = TestReport;
			var wrapper1 = new DummyDocumentWrapper(dummy1, Factory);
			var wrapper2 = new DummyDocumentWrapperWithDeliveryContact(dummy2, Factory);

			using (var report1 = new Report(pack, template, wrapper1, "testReport", null, DocumentDirection.ARV, false))
			using (var report2 = new Report(pack, template, wrapper2, "testReport", null, DocumentDirection.ARV, false))
			{
				pack.Add(report1);
				pack.Add(report2);

				pack.Run(instructions);
				AssertEquals("report one using the contact of the document pack.", instructions.Recipients[0].PK, report1.DeliveryContact.PK);
				AssertEquals("report two using the contact of the its wrapper.", wrapper2.DeliveryContact.PK, report2.DeliveryContact.PK);
			}
		}

		public void TestRunUsesRelatedOrgHeaderForConsignorAndConsigneeDocuments()
		{
			testMenuItem.SU_ContactType = ContactType.Consignor.Code;
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			MockDocSupportBizO mockBiz = new MockDocSupportBizO(org1, org2);

			TestableDocumentPack pack = new TestableDocumentPack(testMenuItem, mockBiz, null);

			instructions = new DeliveryInstructions(pack);
			pack.Run(instructions);

			AssertEquals("OrganisationGiven", org1.PK, ((MockDeliveryContactFinder)pack.AutoDocumentDelivery).OrganisationGiven.PK);
			AssertEquals("RelatedOrganisationGiven", org2.PK, ((MockDeliveryContactFinder)pack.AutoDocumentDelivery).RelatedOrganisationGiven.PK);
		}

		public void TestUsingGetDocumentTitlesForPivot()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();

			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);
				AssertEquals("Title", "TestDoc1", ((Report)pack[0]).Name);
				AssertEquals("Copies", 42, ((Report)pack[0]).PrinterDetails.NumberOfCopies);
			}
			pivot.SI_DocumentTitle = "TestDocUsingDefaultTitle";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);
				AssertEquals("Title0", "TestDocUsingDefaultTitle", ((Report)pack[0]).Name);
			}
		}

		public void TestGetDocumentTitlesForPivotDoesNothingWhenCopyCountIsZero()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			mockBiz.CopyCount = 0;

			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null);
			AssertEquals("PackCount", 0, pack.Count);
		}

		public void TestDocumentPackWithDocTypeOnMenu()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			mockBiz.Description = "Dummy1";
			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			ZQuery query = new ZQuery(RefDocTypeSchema.RT_DocType, "CIV");
			RefDocType loadedDocType = Factory.LoadTop1<RefDocType>(query);

			StmMenuTemplatePivotBase pivotWithDocType = Factory.New<StmMenuTemplatePivotBase>();
			pivotWithDocType.SI_RT_DocType = loadedDocType.PK;
			pivotWithDocType.SI_SO = template.PK;
			pivotWithDocType.SI_SU = menu.PK;
			pivotWithDocType.SI_DocumentTitle = "TestDoc With DocType";

			AssertEquals("Precondition: Documents Count for Menu With DocType", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("Precondition: 1 doc in the pack", 1, pack.Count);
				AssertEquals("DocType code should be CIV on the docwrapper", loadedDocType.RT_DocType, ((Report)pack[0]).DocTypeCode);
			}
		}

		public void TestDocumentPackWithoutDocTypeOnMenu()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			mockBiz.Description = "Dummy1";
			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivotWithoutDocType = Factory.New<StmMenuTemplatePivotBase>();
			pivotWithoutDocType.SI_SO = template.PK;
			pivotWithoutDocType.SI_SU = menu.PK;
			pivotWithoutDocType.SI_DocumentTitle = "TestDoc Without DocType";

			AssertEquals("Precondition: Documents Count for Menu Without DocType", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("Precondition: 1 doc in the pack", 1, pack.Count);
				AssertEquals("DocType code should be empty", ZString.Empty, ((Report)pack[0]).DocTypeCode);
			}
		}

		public void TestRunForPreviewOfMultiDocPack()
		{
			AssertRunForPreviewOfMultiDocPack(3, 3);
			AssertRunForPreviewOfMultiDocPack(0, 1);
		}

		void AssertRunForPreviewOfMultiDocPack(int contactToReturn, int expectDeliveredCount)
		{
			var org = Factory.New<OrgHeader>();
			var mockBiz = new MockDocSupportBizO(org);

			var menu = Factory.New<DocumentCommand>();
			menu.SU_MenuName = "test";
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_Name = SectionRepositoryTemplateNames.System;
			templateRecord.SO_Template = CustomisableSectionTest.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = menu.PK;

			using (var pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				((MockDeliveryContactFinder)pack.AutoDocumentDelivery).ContactsToReturn = contactToReturn;
				instructions.DocumentPackCount = 2;
				instructions.Destination = DeliveryInstructionDestination.Preview;
				var deliverdCount = 0;
				pack.Delivered += delegate { deliverdCount++; };
				 pack.Run(instructions);

				AssertEquals(expectDeliveredCount, deliverdCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowDeliveryForm_FiteredDocumentsWithOther()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = helper.CreateDummyShipment("DS1", "DC1", null);
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			template.IsDocBuilderStyleForTest = true;

			var command = helper.CreateDocCommand("TestMenu1", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;
			var pivot1 = helper.CreateMenuTemplatePivot("TestDocForFilter1", template, command);

			var childCommand = helper.CreateDocCommand("TestMenu2", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand.Parent = dummy;
			var template2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2");
			template2.IsDocBuilderStyleForTest = true;
			helper.CreateMenuTemplatePivot("TestDocForFilter2", template2, childCommand);
			var menupivot = helper.CreateMenuMenuPivot(command, childCommand);
			menupivot.SF_IsSystemDefined = true;

			using (var printTask = helper.CreateLoadedPrintTask(command, null))
			{
				var documentPacks = printTask.GetDocumentPacks().ToList();
				AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
				AssertEquals("Document Pack Should Contain 2 documents", 2, documentPacks[0].Count);
			}
			pivot1.SI_MenuTemplateFilter = "\"<SI_DocumentTitle>\" == \"TestDocForFilter1\"";
			menupivot.SF_Filter = "\"<SF_IsSystemDefined>\" == \"Y\"";
			AssertDocumentPacksCount(helper, command, 2);

			pivot1.SI_MenuTemplateFilter = "\"<SI_DocumentTitle>\" == \"\"";
			AssertDocumentPacksCount(helper, command, 1);

			menupivot.SF_Filter = "\"<SF_IsSystemDefined>\" == \"N\"";
			AssertDocumentPacksCount(helper, command, 0);

			pivot1.SI_MenuTemplateFilter = "\"<Z0_Code>\" == \"DS1\"";
			menupivot.SF_Filter = "\"<Z0_Code>\" == \"DS1\"";
			AssertDocumentPacksCount(helper, command, 2);

			pivot1.SI_MenuTemplateFilter = "\"<Z0_Code>\" == \"\"";
			AssertDocumentPacksCount(helper, command, 1);

			menupivot.SF_Filter = "\"<Z0_Code>\" == \"\"";
			AssertDocumentPacksCount(helper, command, 0);
		}

		void AssertDocumentPacksCount(PrintTaskDocumentPackTestHelper helper, DocumentCommand command, int count)
		{
			using (var printTask = helper.CreateLoadedPrintTask(command, null))
			{
				var documentPacks = printTask.GetDocumentPacks().ToList();
				AssertEquals("PackCount", count, documentPacks[0].Count);
			}
		}

		public void TestMenuTemplateFilterValue()
		{
			var mockBiz = new MockDocSupportBizO();
			var menu = Factory.New<DocumentCommand>();
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDocForFilter";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);
			}

			((MockDocSupportBizODocumentSupporter)mockBiz.DocumentSupporter).UseSeaForMenuTemplateFilterValue = true;
			menu.Documents[0].SI_MenuTemplateFilter = "HBL=AIR";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=AIR PackCount", 0, pack.Count);
				AssertEquals(1, pack.ReasonsForEmptyPacks.Count);
				AssertEquals("Cannot produce this Document because the data required to do so is not present", pack.ReasonsForEmptyPacks[0]);
			}

			((MockDocSupportBizODocumentSupporter)mockBiz.DocumentSupporter).ShowReasonForNotPrintingForTesting = true;
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=AIR PackCount", 0, pack.Count);
				AssertEquals(1, pack.ReasonsForEmptyPacks.Count);
				AssertEquals("Cannot produce this Document because the data required to do so is not present", pack.ReasonsForEmptyPacks[0]);
			}

			menu.Documents[0].SI_MenuTemplateFilter = "HBL=SEA";
			((MockDocSupportBizODocumentSupporter)mockBiz.DocumentSupporter).ShowReasonForNotPrintingForTesting = false;
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=SEA PackCount", 1, pack.Count);
				AssertEquals(0, pack.ReasonsForEmptyPacks.Count);
			}
		}

		public void TestMenuTemplateComplexFilter()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();

			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDocForFilter";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);
			}

			((MockDocSupportBizODocumentSupporter)mockBiz.DocumentSupporter).UseSeaForMenuTemplateFilterValue = true;
			menu.Documents[0].SI_MenuTemplateFilter = "\"<HBL>\"==\"AIR\"";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=AIR PackCount", 0, pack.Count);
			}

			menu.Documents[0].SI_MenuTemplateFilter = "\"<HBL>\"==\"SEA\"";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=SEA PackCount", 1, pack.Count);
			}
		}

		public void TestUsingChildDataContextAndFilteringOnChildPivot()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();

			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDocForChildPivot";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);
			}

			MockDocSupportBizO dummy1 = new MockDocSupportBizO();
			dummy1.Description = "Dummy1";

			MockDocSupportBizO dummy2 = new MockDocSupportBizO();
			dummy2.Description = "Dummy2";

			DocumentWrapper[] wrappers = new DocumentWrapper[] { new DummyDocumentWrapper(dummy1, Factory), new DummyDocumentWrapper(dummy2, Factory) };
			mockBiz.Wrappers = wrappers;
			((MockDocSupportBizODocumentSupporter)mockBiz.DocumentSupporter).UseWrappedObjectFilterValue = true;

			menu.Documents[0].SI_MenuTemplateFilter = "HBL=AIR";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=AIR PackCount", 0, pack.Count);
			}

			menu.Documents[0].SI_MenuTemplateFilter = "HBL=Dummy1";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After HBL=Dummy1 PackCount", 1, pack.Count);
			}

			dummy2.Description = "Dummy1";
			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("After setting Dummy2's Desc to Dummy1 PackCount", 2, pack.Count);
			}
		}

		public void TestDocumentPackWithEDocsClientSupressed()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();

			DocumentCommand docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			docCommand.SU_MenuName = "Test";

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_IsSystemDefined = true;
			eDoc.SX_IsClientSupressed = true;

			TestableDocumentPack docPack = new TestableDocumentPack(docCommand, mockBiz, new UserControlProviderList());
			AssertEquals("DocPack should have NO elements in it! because edocs that are client suppressed shouldn't be included", 0, docPack.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentPackWithEDocsFromProviders()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var consumer = MockEDocsProvider.New(Factory, BusinessContext.Shipment);
			var provider1 = MockEDocsProvider.New(Factory, BusinessContext.Consol);
			var provider2 = MockEDocsProvider.New(Factory, BusinessContext.Customs);
			var provider3 = MockEDocsProvider.New(Factory, BusinessContext.DailyWorkSheet);

			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command.SU_MenuName = "-=WEAPONS OF MASS DESTRUCTION=-";

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			var provider1Placeholder = provider1.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(command);
			var provider2Placeholder = provider2.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(command);

			command.Parent = consumer;
			consumer.AddEDocsProvider(provider1);
			consumer.AddEDocsProvider(provider2);

			var docTypes = new RefDocTypeCollection(Factory);

			var mainEDocPivot1 = command.EDocs.AddNew();
			var mainEDocPivot2 = command.EDocs.AddNew();

			var provider1EDocPivot1 = provider1Placeholder.EDocs.AddNew();
			var provider1EDocPivot2 = provider1Placeholder.EDocs.AddNew();

			var provider2EDocPivot1 = provider2Placeholder.EDocs.AddNew();
			var provider2EDocPivot2 = provider2Placeholder.EDocs.AddNew();

			mainEDocPivot1.SX_IsClientSupressed = true;
			provider1EDocPivot1.SX_IsClientSupressed = true;

			mainEDocPivot1.SX_RT_DocType = docTypes[0].PK;
			mainEDocPivot2.SX_RT_DocType = docTypes[1].PK;

			provider1EDocPivot1.SX_RT_DocType = docTypes[2].PK;
			provider1EDocPivot2.SX_RT_DocType = docTypes[3].PK;

			provider2EDocPivot1.SX_RT_DocType = docTypes[0].PK;
			provider2EDocPivot2.SX_RT_DocType = docTypes[1].PK;

			Factory.Save();

			var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = provider.GetFactory(Factory);

			byte[] imageBytes;
			using (var stream = new MemoryStream())
			using (var image = new Bitmap(1, 1))
			{
				image.Save(stream, ImageFormat.Bmp);
				imageBytes = stream.ToArray();
			}

			var mainEDoc1 = documentFactory.AddFileOrDocument(consumer.PK, null, imageBytes, null, docTypes[0].RT_DocType, "DEF", false);
			var mainEDoc2 = documentFactory.AddFileOrDocument(consumer.PK, null, new byte[] { 1, 2, 3 }, "file.txt", docTypes[1].RT_DocType, "DEF", false);

			var provider1EDoc1 = documentFactory.AddFileOrDocument(provider1.PK, null, imageBytes, null, docTypes[2].RT_DocType, "DEF", false);
			var provider1EDoc2 = documentFactory.AddFileOrDocument(provider1.PK, null, imageBytes, null, docTypes[3].RT_DocType, "DEF", false);

			var provider2EDoc1 = documentFactory.AddFileOrDocument(provider2.PK, null, imageBytes, null, docTypes[0].RT_DocType, "DEF", false);
			var provider2EDoc2 = documentFactory.AddFileOrDocument(provider2.PK, null, imageBytes, null, docTypes[1].RT_DocType, "DEF", false);

			documentFactory.Save();

			var pack = new DocumentPack(command, consumer, null, null);
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("The DocumentPack should have 4 eDocs.", 4, pack.Count);
			AssertEquals("The DocumentPack should have 4 eDocs included in print.", 4, includeCount);
			AssertEquals("The DocumentPack should have 1 other eDoc which is not print by default", 1, pack.OtherEDocsToAttach.Count);
			AssertEquals("[0]", mainEDoc2.PK, ((BusinessObject)pack[0]).PK);
			AssertEquals("[1]", provider1EDoc2.PK, ((BusinessObject)pack[1]).PK);
			AssertEquals("[2]", provider2EDoc1.PK, ((BusinessObject)pack[2]).PK);
			AssertEquals("[3]", provider2EDoc2.PK, ((BusinessObject)pack[3]).PK);
		}

		public void TestDocumentDirection()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();

			DocumentCommand menu = Factory.New<DocumentCommand>();
			menu.SU_DocumentDirection = nameof(DocumentDirection.ARV);

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "Test Doc";

			AssertEquals("Documents.Count", 1, menu.Documents.Count);

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 1, pack.Count);

				AssertEquals("Direction", DocumentDirection.ARV, ((Report)pack[0]).Direction);
			}
		}

		public void TestTypeOfContact()
		{
			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			DocumentCommand menu = Factory.New<DocumentCommand>();
			menu.SU_ContactType = ContactType.Consignee.ToString();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "Test Doc";

			using (DocumentPack pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("ContactType", ContactType.Consignee.ToString(), ((Report)pack[0]).TypeOfContact.ToString());
			}
		}

		public void TestUsingRecipientsModeToSpecifyWhichPivotToPrintOrFaxOrEmail()
		{
			var mockBiz = new MockDocSupportBizO();

			var menu = Factory.New<DocumentCommand>();
			menu.SU_SupportsVisualisation = false;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			template.SO_Template = TestReport.GetAsByteArray();

			var printPivot = Factory.New<StmMenuTemplatePivotBase>();
			printPivot.SI_SO = template.PK;
			printPivot.SI_SU = menu.PK;
			printPivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			printPivot.SI_DocumentTitle = "Document For Print";

			var emailPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			emailPivot1.SI_SO = template.PK;
			emailPivot1.SI_SU = menu.PK;
			emailPivot1.SI_PrintCopyType = nameof(PrintCopyType.EML);
			emailPivot1.SI_DocumentTitle = "Document For Email 1";

			var emailPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			emailPivot2.SI_SO = template.PK;
			emailPivot2.SI_SU = menu.PK;
			emailPivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);
			emailPivot2.SI_DocumentTitle = "Document For Email 2";

			var faxPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot1.SI_SO = template.PK;
			faxPivot1.SI_SU = menu.PK;
			faxPivot1.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot1.SI_DocumentTitle = "Document For Fax 1";

			var faxPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot2.SI_SO = template.PK;
			faxPivot2.SI_SU = menu.PK;
			faxPivot2.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot2.SI_DocumentTitle = "Document For Fax 2";

			var faxPivot3 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot3.SI_SO = template.PK;
			faxPivot3.SI_SU = menu.PK;
			faxPivot3.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot3.SI_DocumentTitle = "Document For Fax 3";

			var allPivot = Factory.New<StmMenuTemplatePivotBase>();
			allPivot.SI_SO = template.PK;
			allPivot.SI_SU = menu.PK;
			allPivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			allPivot.SI_DocumentTitle = "Document For All 1";

			AssertEquals("Documents.Count", 7, menu.Documents.Count);

			using (var pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 7, pack.Count);

				var instructions = new DeliveryInstructions(pack);
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				var contact = new DocDeliveryContact(Factory);
				contact.Email = "daph@daph.com";
				contact.Fax = "234234";
				contact.AttachmentType = "PDF";

				instructions.Recipients.RemoveAndDeleteAll();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				instructions.Recipients.Add(contact);
				pack.Run(instructions);
				AssertEquals("LastRunFileCount for Email", 3, pack.LastRunFileCount);

				instructions.Recipients.RemoveAll();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
				instructions.Recipients.Add(contact);
				pack.Run(instructions);
				AssertEquals("LastRunFileCount for Fax", 4, pack.LastRunFileCount);

				instructions.Recipients.RemoveAll();
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				instructions.Recipients.Add(contact);
				pack.Run(instructions);
				AssertEquals("LastRunFileCount for Print", 2, pack.LastRunFileCount);

				instructions.Destination = DeliveryInstructionDestination.Preview;
				pack.Run(instructions);
				AssertEquals("LastRunFileCount for Preview", 2, pack.LastRunFileCount);
			}
		}

		public void TestDestinationPrintOnlyPrintDocumentsDelivered()
		{
			var mockBiz = new MockDocSupportBizO();

			var menu = Factory.New<DocumentCommand>();
			menu.SU_SupportsVisualisation = false;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			template.SO_Template = TestReport.GetAsByteArray();

			var printPivot = Factory.New<StmMenuTemplatePivotBase>();
			printPivot.SI_SO = template.PK;
			printPivot.SI_SU = menu.PK;
			printPivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			printPivot.SI_DocumentTitle = "Document For Print";

			var emailPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			emailPivot1.SI_SO = template.PK;
			emailPivot1.SI_SU = menu.PK;
			emailPivot1.SI_PrintCopyType = nameof(PrintCopyType.EML);
			emailPivot1.SI_DocumentTitle = "Document For Email 1";

			var emailPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			emailPivot2.SI_SO = template.PK;
			emailPivot2.SI_SU = menu.PK;
			emailPivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);
			emailPivot2.SI_DocumentTitle = "Document For Email 2";

			var faxPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot1.SI_SO = template.PK;
			faxPivot1.SI_SU = menu.PK;
			faxPivot1.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot1.SI_DocumentTitle = "Document For Fax 1";

			var faxPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot2.SI_SO = template.PK;
			faxPivot2.SI_SU = menu.PK;
			faxPivot2.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot2.SI_DocumentTitle = "Document For Fax 2";

			var faxPivot3 = Factory.New<StmMenuTemplatePivotBase>();
			faxPivot3.SI_SO = template.PK;
			faxPivot3.SI_SU = menu.PK;
			faxPivot3.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			faxPivot3.SI_DocumentTitle = "Document For Fax 3";

			var allPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			allPivot1.SI_SO = template.PK;
			allPivot1.SI_SU = menu.PK;
			allPivot1.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			allPivot1.SI_DocumentTitle = "Document For All 1";

			AssertEquals("Documents.Count", 7, menu.Documents.Count);

			using (var pack = new TestableDocumentPack(menu, mockBiz, null))
			{
				AssertEquals("PackCount", 7, pack.Count);

				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Print;
				pack.Run(instructions);
				AssertEquals("LastRunFileCount for Print", 2, pack.LastRunFileCount);

				AssertEquals("EML item is not unticked by system automatically if not on GUI", true, pack[1].CanIncludeInPrint);
				pack[1].CanIncludeInPrint = false;
				pack[1].IncludedInPrint = true;
				pack.Run(instructions);
				AssertEquals("File should still be included if user choose it manually", 3, pack.LastRunFileCount);
			}
		}

		public void TestSetDeliveryDetailsFromDocumentPrintSet()
		{
			MockDocSupportBizO parentBizO = new MockDocSupportBizO();
			DocumentCommand parentCommand = Factory.New<DocumentCommand>();
			parentCommand.Parent = parentBizO;
			parentCommand.SU_ContactType = ContactType.Consignee.Code;

			MockDocSupportBizO childBizO = new MockDocSupportBizO();
			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.Parent = childBizO;
			childCommand.SU_ContactType = ContactType.AirWholesaler.Code;

			TestableDocumentPack pack = new TestableDocumentPack(childCommand, childBizO, null);
			AssertEquals("ContactType", childCommand.SU_ContactType, pack.ContactTypeString);
			AssertEquals("Pack.StmMenuCommand", childCommand, pack.StmMenuCommand);
			AssertEquals("Pack.StmMenuCommand.SU_ContactType", childCommand.SU_ContactType, pack.StmMenuCommand.SU_ContactType);
			AssertEquals("Pack.DeliveryFilter", childBizO.DocumentSupporter, pack.DocumentSupporter);

			pack.SetDeliveryDetailsFromDocumentPrintSet(parentCommand);
			AssertEquals("ContactType", parentCommand.SU_ContactType, pack.ContactTypeString);
			AssertEquals("Pack.StmMenuCommand", childCommand, pack.StmMenuCommand);
			AssertEquals("Pack.DocumentGroup", parentCommand.SU_ContactType, pack.DocumentGroup);
			AssertEquals("Pack.DeliveryFilter", parentBizO.DocumentSupporter, pack.DocumentSupporter);
		}

		public void TestOneDocument()
		{
			var report1 = new MockReport(TestReport);
			testPack.Add(report1);
			testPack.Run(instructions);

			AssertEquals("Run count", 1, report1.RunCount);
		}

		public void TestThreeDocuments()
		{
			var report1 = new MockReport(TestReport);
			var report2 = new MockReport(TestReport);
			var report3 = new MockReport(TestReport);
			testPack.Add(report1);
			testPack.Add(report2);
			testPack.Add(report3);
			testPack.Run(instructions);

			AssertEquals("Run count", 1, report1.RunCount);
			AssertEquals("Run count", 1, report2.RunCount);
			AssertEquals("Run count", 1, report3.RunCount);
		}

		public void TestThreeDocumentsWithEDocs()
		{
			using (var report1 = new MockReport(TestReport))
			using (var report2 = new MockReport(TestReport))
			{
				var dummy = Factory.New<DummyDocManagerTestBizO>();
				dummy.SetupDocManagerObjects();
				var eDoc = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
				eDoc.IncludedInPrint = true;

				testPack.Add(report1);
				testPack.Add(report2);
				testPack.Add(eDoc);
				testPack.Run(instructions);

				AssertEquals("Run Count", 1, report1.RunCount);
				AssertEquals("Run Count", 1, report2.RunCount);
				AssertEquals("Run count", 1, eDoc.RunCountForTesting);
				eDoc.DeleteTempFilesForTesting();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesNotAddEDocsToPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.SetupDocManagerObjects();
			var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizo);

			//Load these DocTypes - created by the DummyDocManagerTestBizO
			var docTypeForRefUNLOCO = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			var docTypeForRefUNLOCOSuppressed = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BBB"));

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;
			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			AssertEquals("Precondition: no contents in the pack", 0, command.EDocs.Count);
			var eDoc1 = command.AddEDoc(docTypeForRefUNLOCO);
			var eDoc2 = command.AddEDoc(docTypeForRefUNLOCOSuppressed);
			eDoc2.SX_IsClientSupressed = true;

			AssertEquals("Contents in the pack after adding", 2, command.EDocs.Count);
			AssertEquals("Contents in the pack after adding - Supressed document not included in the view", 1, command.EDocsView.Count);

			Factory.Save();

			var pack = new DocumentPack(command, bizo, null, null, true);
			var includeEDocs = pack.Cast<IDeliverable>().Count(x => x is IeDoc && x.IncludeInPrint);
			var includeDocument = pack.Cast<IDeliverable>().Count(x => !(x is IeDoc) && x.IncludeInPrint);

			AssertEquals("Pack should now have 2 other eDocs", 2, pack.OtherEDocsToAttach.Count);
			AssertEquals("Pack should now have 1 eDoc included in print", 1, includeEDocs);
			AssertEquals("Pack should now have 1 document included in print", 1, includeDocument);
			AssertEquals("Pack should now have 2 eDocs", 2, pack.Count);

			pack = new DocumentPack(command, bizo, null, null, false);
			includeEDocs = pack.Cast<IDeliverable>().Count(x => x is IeDoc && x.IncludeInPrint);
			includeDocument = pack.Cast<IDeliverable>().Count(x => !(x is IeDoc) && x.IncludeInPrint);

			AssertEquals("Pack should now have no other eDoc", 0, pack.OtherEDocsToAttach.Count);
			AssertEquals("Pack should now have no eDoc included in print", 0, includeEDocs);
			AssertEquals("Pack should now have 1 document included in print", 1, includeDocument);
			AssertEquals("Pack should now have 1 eDoc", 1, pack.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddEDocsToPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			bizo.SetupDocManagerObjects();
			var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizo);

			//Load these DocTypes - created by the DummyDocManagerTestBizO
			var docTypeForRefUNLOCO = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "AAA"));
			var docTypeForRefUNLOCOSuppressed = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BBB"));

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			AssertEquals("Precondition: no contents in the pack", 0, command.EDocs.Count);
			var eDoc1 = command.AddEDoc(docTypeForRefUNLOCO);
			var eDoc2 = command.AddEDoc(docTypeForRefUNLOCOSuppressed);
			eDoc2.SX_IsClientSupressed = true;

			AssertEquals("Contents in the pack after adding", 2, command.EDocs.Count);
			AssertEquals("Contents in the pack after adding - Supressed document not included in the view", 1, command.EDocsView.Count);

			Factory.Save();

			var pack = new DocumentPack();
			pack.DocumentSupporter = docSupporter;
			pack.ForceBusinessObjectToLogAgainst(bizo);

			AssertEquals("Pack is empty", 0, pack.Count);

			// need to add storagedocs
			pack.AddEDocsToPack(command);
			var includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("Pack should now have 1 eDoc included in print", 1, includeCount);
			AssertEquals("Pack should now have 2 other eDocs ", 2, pack.OtherEDocsToAttach.Count);
			AssertEquals("Pack should now have 1 eDoc", 1, pack.Count);

			eDoc2.SX_IsClientSupressed = false;
			Factory.Save();
			pack.AddEDocsToPack(command);
			includeCount = pack.Cast<IDeliverable>().Count(x => x.IncludedInPrint);

			AssertEquals("Pack should now have 2 eDoc included in print", 2, includeCount);
			AssertEquals("Pack should now have 1 other eDoc", 1, pack.OtherEDocsToAttach.Count);
			AssertEquals("Pack should now have 3 eDocs - doesn't duplicate existing eDocs", 2, pack.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddEDocsToAttach()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var bizo = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));

			var docType1 = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document AAA");
			var docType2 = EDocsTestHelper.CreateDocType(Factory, "BBB", "UNL", "A Test Document BBB");
			var docType3 = EDocsTestHelper.CreateDocType(Factory, "CCC", "UNL", "A Test Document CCC");
			var docType4 = EDocsTestHelper.CreateDocType(Factory, "DDD", "UNL", "A Test Document DDD");

			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, bizo);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				storageMain.AddFileOrDocument(contents, string.Empty, "AAA", false).Description = "A Test Document AAA";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "BBB", false).Description = "A Test Document BBB";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "CCC", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Description = "A Test Document CCC";
				contents.Position = 0;
				storageMain.AddFileOrDocument(contents, string.Empty, "DDD", false).Description = "A Test Document DDD";
			}

			documentFactory.Save();

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			var eDocUsed1 = command.AddEDoc(docType1);
			eDocUsed1.SX_Filter = "\"<RL_Code>\" == \"AUSYD\"";

			var eDocUsed2 = command.AddEDoc(docType2);
			eDocUsed2.SX_Filter = "\"<FileName>\" == \"A Test Document BBB.tif\"";

			command.AddEDoc(docType3);

			var eDocUsed4 = command.AddEDoc(docType4);
			eDocUsed4.SX_Filter = $"\"<CurrentCompany.Country.Code>\" != \"{GlbCompany.CurrentCompany.Country.Code}\"";

			AssertEquals("Contents in the pack after adding", 4, command.EDocs.Count);

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendMostRecentOnly);
			using (var pack = new DocumentPack())
			{
				pack.AddEDocsToPack(command);
				var includeCount = pack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 2 eDocs included in print", 2, includeCount);
				AssertEquals("Pack should now have 2 eDocs", 2, pack.Count);
				AssertEquals("Pack should now have 1 other eDoc", 1, pack.OtherEDocsToAttach.Count);
			}

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendAll);
			using (var pack = new DocumentPack())
			{
				pack.AddEDocsToPack(command);
				var includeCount = pack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 2 eDocs included in print", 2, includeCount);
				AssertEquals("Pack should now have 2 eDocs", 2, pack.Count);
				AssertEquals("Pack should now have 1 other eDoc", 1, pack.OtherEDocsToAttach.Count);
			}

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendMostRecentSystemGeneratedAndAllManuallyAdded);
			using (var pack = new DocumentPack())
			{
				pack.AddEDocsToPack(command);
				var includeCount = pack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 2 eDocs included in print", 2, includeCount);
				AssertEquals("Pack should now have 2 eDocs", 2, pack.Count);
				AssertEquals("Pack should now have 1 other eDoc", 1, pack.OtherEDocsToAttach.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRelatedEdocsParentNotInheritsFromIDocManagerSupportToAttach()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var bizo = Factory.NewWithValidTestData<DummyDocManagerTestBizO>();

			bizo.AddRelatedObjectsForTest = true;
			bizo.AddNonIDocumentSupportableObjectForTest = true;

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo;

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, command);

			using (var pack = new DocumentPack())
			{
				AssertNoExceptionThrown(() => pack.AddEDocsToPack(command));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRelatedEDocsShouldUseBusinessEntityFactoryAsInternal()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);
			var bizo = new Mock<DummyDocManagerTestBizO>(Factory, null);

			var factory = new BusinessObjectFactory();
			var dummy = factory.New(typeof(DummyDocManagerTestBizO));
			EDocsTestHelper.CreateDocType(factory, "FFF", "UNL", "F Test Document FFF");
			factory.Save();

			var mockDocManagerInfo = new Mock<DocManagerInfo>(bizo.Object, Core.Constants.DocManagerCodes.UNLOCO) { CallBase = true };
			mockDocManagerInfo.Protected().Setup<BusinessObject[]>("GetRelatedObjects").Returns(() =>
			{
				var relatedBusinessObjects = new List<BusinessObject>() { dummy };
				return relatedBusinessObjects.ToArray();
			});

			bizo.Setup(m => m.GetDocManagerInfoForTest()).Returns(mockDocManagerInfo.Object);

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = bizo.Object;

			var template = helper.CreateTemplate(nameof(Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Test", template, command);

			using (var pack = new DocumentPack())
			{
				pack.AddEDocsToPack(command);
				var eDocsDocumentParent = command.Parent as IDocManagerSupport;
				AssertEquals(true, eDocsDocumentParent.DocManagerInfo.UseBusinessEntityFactoryAsInternal);

				var relatedObjects = eDocsDocumentParent.DocManagerInfo.RelatedObjects.Where(x => x is IDocManagerSupport && !(x is OrgHeader)).ToArray();
				AssertEquals(1, relatedObjects.Length);
				CombineAssertions(() =>
				{
					foreach (IDocManagerSupport relatedObject in relatedObjects)
					{
						AssertEquals(true, relatedObject.DocManagerInfo.UseBusinessEntityFactoryAsInternal);
					}
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 1, 1, 00, 00, 00)]
		public void TestAddMultipleEDocsOfTheSameDocType()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendMostRecentOnly);
			var docType = EDocsTestHelper.CreateDocType(Factory, "XYZ", "UNL", "My Document Type");
			var bizO = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, bizO);
			using (var contents = EDocsTestHelper.CreateSimpleBitmapContents())
			{
				var eDoc1 = storageMain.AddFileOrDocument(contents, string.Empty, "XYZ", false);
				eDoc1.Description = "XYZ - System 1";
				eDoc1.SetIsSystemGenerated(ZBool.True);
				documentFactory.Save();

				TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
				contents.Position = 0;
				var eDoc2 = storageMain.AddFileOrDocument(contents, string.Empty, "XYZ", false);
				eDoc2.Description = "XYZ - System 2";
				eDoc2.SetIsSystemGenerated(ZBool.True);
				documentFactory.Save();

				TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
				contents.Position = 0;
				var eDoc3 = storageMain.AddFileOrDocument(contents, string.Empty, "XYZ", false);
				eDoc3.Description = "XYZ - User 1";
				documentFactory.Save();

				TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
				contents.Position = 0;
				var eDoc4 = storageMain.AddFileOrDocument(contents, string.Empty, "XYZ", false);
				eDoc4.Description = "XYZ - User 2";
				documentFactory.Save();
			}

			Factory.Save();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "My Document";
			documentCommand.Parent = bizO;
			documentCommand.AddEDoc(docType);

			var template = helper.CreateTemplate(nameof(Enterprise.Core.Constants.DataContext.Shipment), "DocumentTest");
			helper.CreateMenuTemplatePivot("Document For Print", template, documentCommand);

			Factory.Save();

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendMostRecentOnly);
			using (var documentPack = new DocumentPack())
			{
				var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizO);
				documentPack.DocumentSupporter = docSupporter;
				documentPack.ForceBusinessObjectToLogAgainst(bizO);

				AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 3 other eDocs", 3, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have 1 eDoc included in print", 1, includeCount);
				AssertEquals("Pack should now have 1 eDocs", 1, documentPack.Count);
				AssertEquals("documentPack[0].Name", "XYZ - User 2", documentPack[0].Name);
			}

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendAll);
			using (var documentPack = new DocumentPack())
			{
				var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizO);
				documentPack.DocumentSupporter = docSupporter;
				documentPack.ForceBusinessObjectToLogAgainst(bizO);

				AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have no other eDoc", 0, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have 4 eDocs included in print", 4, includeCount);
				AssertEquals("Pack should now have 4 eDocs", 4, documentPack.Count);
				AssertEquals("documentPack[0].Name", "XYZ - System 1", documentPack[0].Name);
				AssertEquals("documentPack[1].Name", "XYZ - System 2", documentPack[1].Name);
				AssertEquals("documentPack[2].Name", "XYZ - User 1", documentPack[2].Name);
				AssertEquals("documentPack[3].Name", "XYZ - User 2", documentPack[3].Name);
			}

			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendMostRecentSystemGeneratedAndAllManuallyAdded);
			using (var documentPack = new DocumentPack())
			{
				var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizO);
				documentPack.DocumentSupporter = docSupporter;
				documentPack.ForceBusinessObjectToLogAgainst(bizO);

				AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 1 other eDoc", 1, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have 3 eDoc included in print", 3, includeCount);
				AssertEquals("Pack should now have 3 eDocs", 3, documentPack.Count);
				AssertEquals("documentPack[0].Name", "XYZ - User 1", documentPack[0].Name);
				AssertEquals("documentPack[1].Name", "XYZ - User 2", documentPack[1].Name);
				AssertEquals("documentPack[2].Name", "XYZ - System 2", documentPack[2].Name);
			}

			var eDocUsed2 = documentCommand.AddEDoc(docType);
			eDocUsed2.SX_Filter = "\"<FileName>\" == \"XYZ - System 1.tif\"";
			using (var documentPack = new DocumentPack())
			{
				var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizO);
				documentPack.DocumentSupporter = docSupporter;
				documentPack.ForceBusinessObjectToLogAgainst(bizO);

				AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 3 other eDocs", 3, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have 1 eDoc included in print", 1, includeCount);
				AssertEquals("Pack should now have 1 eDocs", 1, documentPack.Count);
				AssertEquals("documentPack[2].Name", "XYZ - System 1", documentPack[0].Name);
			}

			eDocUsed2.SX_Filter = "\"<RL_Code>\" == \"AUSYD\"";
			using (var documentPack = new DocumentPack())
			{
				var docSupporter = new DummyDocManagerTestBizODocumentSupporter(bizO);
				documentPack.DocumentSupporter = docSupporter;
				documentPack.ForceBusinessObjectToLogAgainst(bizO);

				AssertEquals("Pre-condition: documentPack.Count", 0, documentPack.Count);

				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 1 other eDoc", 1, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have 3 eDocs included in print", 3, includeCount);
				AssertEquals("Pack should now have 3 eDocs", 3, documentPack.Count);
				AssertEquals("documentPack[0].Name", "XYZ - User 1", documentPack[0].Name);
				AssertEquals("documentPack[1].Name", "XYZ - User 2", documentPack[1].Name);
				AssertEquals("documentPack[2].Name", "XYZ - System 2", documentPack[2].Name);
			}

			eDocUsed2.SX_Filter = "\"<RL_Code>\" != \"AUSYD\"";
			using (var documentPack = new DocumentPack())
			{
				documentPack.AddEDocsToPack(documentCommand);
				var includeCount = documentPack.Cast<IDeliverable>().Count(doc => doc.IncludedInPrint);

				AssertEquals("Pack should now have 4 other eDocs", 4, documentPack.OtherEDocsToAttach.Count);
				AssertEquals("Pack should now have no eDocs", 0, documentPack.Count);
				AssertEquals("Pack should now have no eDocs included in print", 0, includeCount);
			}
		}

		public void TestInsertFaxCoverSheet()
		{
			using (var task = new PrintTask())
			{
				var instructions = new DeliveryInstructions();
				instructions.IncludeCoverNote = true;

				var contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
				contact.Fax = "90251199";
				instructions.Recipients.Add(contact);

				var contact2 = new DocDeliveryContact(Factory);
				contact2.Name = "maryjane";
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
				contact2.Fax = "11112222";
				instructions.Recipients.Add(contact2);

				using (var pack = new DocumentPackForDeliveredTest())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					pack.Delivered += (sender, e) =>
					{
						AssertEquals("DocumentPack contains one report", 1, pack.Count);
						AssertEquals("Report is cover sheet", "Fax Cover Sheet", pack[0].Name);
					};
					task.Run(instructions);

					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("Report was deleted", 0, pack.Count);
				}
			}
		}

		public void TestInsertPrintCoverSheet()
		{
			using (var task = new PrintTask())
			{
				var instructions = new DeliveryInstructions();
				instructions.IncludeCoverNote = true;

				var contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				instructions.Recipients.Add(contact);

				var contact2 = new DocDeliveryContact(Factory);
				contact2.Name = "maryjane";
				contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				instructions.Recipients.Add(contact2);

				using (var pack = new DocumentPackForDeliveredTest())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					pack.Delivered += (sender, e) =>
					{
						AssertEquals("DocumentPack contains one report", 1, pack.Count);
						AssertEquals("Report is cover sheet", "Cover Sheet", pack[0].Name);
					};
					task.Run(instructions);

					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("Report was deleted", 0, pack.Count);
				}
			}
		}

		public void TestInsertFaxCoverSheetIncludeCoverNotOff()
		{
			using (PrintTask task = new PrintTask())
			{
				DeliveryInstructions instructions = new DeliveryInstructions();

				DocDeliveryContact contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
				contact.Fax = "90251199";
				instructions.Recipients.Add(contact);

				using (DocumentPack pack = new DocumentPack())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					task.Run(instructions);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports as cover note excluded", 0, pack.Count);
				}
			}
		}

		public void TestInsertEmailCoverSheet()
		{
			AssertInsertEmailCoverSheet(Core.Constants.ContactNotifyModes.Email);
			AssertInsertEmailCoverSheet(Core.Constants.ContactNotifyModes.EPrint);
		}

		void AssertInsertEmailCoverSheet(string deliveryMethod)
		{
			using (var task = new PrintTask())
			{
				var instructions = new DeliveryInstructions();
				instructions.IncludeCoverNote = true;

				var contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = deliveryMethod;
				contact.Fax = "zappoo@example.com";
				instructions.Recipients.Add(contact);

				var contact2 = new DocDeliveryContact(Factory);
				contact2.Name = "Janet2";
				contact2.DeliveryMethod = deliveryMethod;
				contact2.Fax = "zappoo@example.com";
				instructions.Recipients.Add(contact2);

				using (var pack = new DocumentPackForDeliveredTest())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					pack.Delivered += (sender, e) =>
					{
						AssertEquals("DocumentPack contains one report", 1, pack.Count);
						AssertEquals("Report is cover sheet", "Email Cover Sheet", pack[0].Name);
					};
					task.Run(instructions);

					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("Report was deleted", 0, pack.Count);
				}
			}
		}

		public void TestGetTranslatedLabelsInCoverSheet()
		{
			AssertGetTranslatedLabelsInCoverSheet(Core.Constants.ContactNotifyModes.Fax, TemplateNames.FaxCoverSheet);
			AssertGetTranslatedLabelsInCoverSheet(Core.Constants.ContactNotifyModes.Print, TemplateNames.PrintCoverSheet);
			AssertGetTranslatedLabelsInCoverSheet(Core.Constants.ContactNotifyModes.Email, TemplateNames.EmailCoverSheet);
			AssertGetTranslatedLabelsInCoverSheet(Core.Constants.ContactNotifyModes.EPrint, TemplateNames.EmailCoverSheet);
		}

		void AssertGetTranslatedLabelsInCoverSheet(string deliveryMethod, string templateName)
		{
			using (var task = new PrintTask())
			{
				var instructions = new DeliveryInstructions();
				instructions.IncludeCoverNote = true;
				instructions.CoverNote = "Test cover note";

				var contact = new DocDeliveryContact(Factory);
				contact.DeliveryMethod = deliveryMethod;
				instructions.Recipients.Add(contact);

				using (var pack = new DocumentPackForDeliveredTest())
				using (var resourceStrings = Res.UseMockData())
				{
					task.Add(pack);

					var key = DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.CoverSheetLabelKeyPrefix, "ATTENTION");
					resourceStrings.Put(key, new ResourceStringData(key, "Translation for ATTENTION"));

					pack.Delivered += (sender, e) =>
					{
						using (var stream = new MemoryStream())
						{
							var report = pack[0] as Report;
							report.Save(stream);
							using (var excelInterface = new ExcelInterface())
							{
								excelInterface.LoadExcelFile(stream);
								AssertEquals(true, excelInterface.WorkSheets.First().ToString().Contains("{C}-[Translation for ATTENTION]"));
							}
						}
					};
					task.Run(instructions);
				}
			}
		}

		public void TestInsertEmailCoverSheet_WhenDeliveringAsXLS_GeneratesAnXLSDocument()
		{
			AssertInsertEmailCoverSheet_PreserveRequestedDocumentFormat(AttachmentTypeList.Codes.Xls, "XLS", "Test_TemplateFromStream.XLS");
		}

		public void TestInsertEmailCoverSheet_WhenDeliveringAsXLSX_GeneratesAnXLSXDocument()
		{
			AssertInsertEmailCoverSheet_PreserveRequestedDocumentFormat(AttachmentTypeList.Codes.Xlsx, "XLSX", "Test_TemplateFromStream.XLSX");
		}

		public void TestInsertEmailCoverSheet_WhenDeliveringAsPDF_GeneratesAPDFDocument()
		{
			AssertInsertEmailCoverSheet_PreserveRequestedDocumentFormat(AttachmentTypeList.Codes.Pdf, "PDF", "Test_TemplateFromStream.XLSX");
		}

		public void TestInsertEmailCoverSheet_WhenDeliveringAsTIF_GeneratesATIFDocument()
		{
			AssertInsertEmailCoverSheet_PreserveRequestedDocumentFormat(AttachmentTypeList.Codes.Tif, "TIF", "Test_TemplateFromStream.XLSX");
		}

		void AssertInsertEmailCoverSheet_PreserveRequestedDocumentFormat(string attachementType, string emailAttachmentFormat, string emailAttachments)
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			var contact = new DocDeliveryContact(new BusinessObjectFactory());
			contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			contact.Email = "unit.test@cargowise.com";
			contact.AttachmentType = attachementType;

			using (var templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "Blah";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream, attachementType);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				var stmMenuItem = Factory.New<DocumentCommand>();
				using (var pack = new DocumentPack(stmMenuItem))
				using (var report = new Report(pack, excelTemplate, null, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				using (var printTask = new PrintTask())
				{
					printTask.Add(pack);
					pack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(pack);
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.IncludeCoverNote = true;
					deliveryInstructions.CoverNote = "I am an undercover note...";
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					deliveryInstructions.Recipients.Add(contact);

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);
				AssertEquals(emailAttachmentFormat, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(emailAttachments, printJobs[0].SP_EmailAttachments);
			}
		}

		public void TestInsertEmailCoverSheetIncludeCoverNoteOff()
		{
			AssertInsertEmailCoverSheetIncludeCoverNoteOff(Core.Constants.ContactNotifyModes.Email);
			AssertInsertEmailCoverSheetIncludeCoverNoteOff(Core.Constants.ContactNotifyModes.EPrint);
		}

		void AssertInsertEmailCoverSheetIncludeCoverNoteOff(string deliveryMethod)
		{
			using (PrintTask task = new PrintTask())
			{
				DeliveryInstructions instructions = new DeliveryInstructions();
				DocDeliveryContact contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = deliveryMethod;
				contact.Fax = "zappoo@example.com";
				instructions.Recipients.Add(contact);

				using (DocumentPack pack = new DocumentPack())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					task.Run(instructions);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports as cover note excluded", 0, pack.Count);
				}
			}
		}

		public void TestCoverSheetForPrint()
		{
			using (var task = new PrintTask())
			{
				var instructions = new DeliveryInstructions();
				var contact = new DocDeliveryContact(Factory);
				contact.Name = "Janet";
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
				contact.Fax = "96712065";
				instructions.Recipients.Add(contact);

				instructions.IncludeCoverNote = true;

				using (var pack = new DocumentPackForDeliveredTest())
				{
					task.Add(pack);
					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("DocumentPack contains no reports", 0, pack.Count);

					instructions.CoverNote = "Test cover note";
					pack.Delivered += (sender, e) =>
					{
						AssertEquals("DocumentPack contains one report", 1, pack.Count);
						AssertEquals("Report is cover sheet", "Cover Sheet", pack[0].Name);
					};
					task.Run(instructions);

					AssertEquals("Task has one DocumentPack", 1, task.Count);
					AssertEquals("Report was deleted", 0, pack.Count);
				}
			}
		}

		public void TestPrintJobHasBusinessObjectInfo()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			PrintTask task = new PrintTask();
			DocumentPack pack = new DocumentPack();

			DeliveryInstructions instructions = new DeliveryInstructions(pack);
			instructions.IncludeCoverNote = true;
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			instructions.Recipients.RemoveAndDeleteAll();
			DocDeliveryContact contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = "PDF";
			contact.Email = "test@example.com";

			task.Add(pack);

			DummyDocManagerTestBizO dummy = Factory.New<DummyDocManagerTestBizO>();
			dummy.SetupDocManagerObjects();

			using (var report = new Report(pack, TestReport))
			{
				IBODocDataProvider docDataProvider = new DummyDocumentWrapper(dummy, Factory);
				report.DocTypeCode = "ABC";
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(docDataProvider);
				pack.Add(report);
				task.Run(instructions);
			}

			StmPrintJobCollection printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("Should have made a print job", 1, printJobs.Count);
			AssertEquals("Should have logged table name", dummy.TableName, printJobs[0].SP_ParentTableName);
			AssertEquals("Should have logged pk", dummy.PK, printJobs[0].SP_ParentGuid);
			AssertEquals("Should have logged doc manager code", ((IDocManagerSupport)dummy).DocManagerInfo.DocManagerCode, printJobs[0].SP_RelatedBusinessContext);
			AssertEquals("Should have logged Document Type", "ABC", printJobs[0].SP_DocumentType);
		}

		public void TestRunForContactWithoutBusinessObjectToLogAgainst()
		{
			DocumentPack pack = new TestableDocumentPack();
			var instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.None;
			AssertNull("BusinessObjectToLogAgainst should be null before pack is run", pack.BusinessObjectToLogAgainst);
			var dummy = Factory.New<DummyBusinessObject>();

			using (var report = new MockReport(TestReport))
			{
				var wrapper = new DummyDocumentWrapper(dummy, Factory);
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				pack.Add(report);
				pack.Run(instructions);
				AssertEquals("MockBiz should be filled in BusinessObjectToLogAgainst", wrapper.WrappedObject, pack.BusinessObjectToLogAgainst);
			}
		}

		public void TestRunForContactWithBusinessObjectWithChangedLanguage()
		{
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			templateRecord.SO_Template = CustomisableSectionTest.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			using (var pack = new DocumentPack(command, docSupportedBO, null, null))
			{
				var instructions = new DeliveryInstructions(pack);
				var orgNumberOfReports = pack.Count;
				pack.Language = Core.Constants.Languages.German;
				pack.Run(instructions);
				AssertEquals("Number of Documents differ after language was changed.", orgNumberOfReports, pack.Count);
			}
		}

		public void TestSetDocPackWithOldLanguageCodes()
		{
			using (var pack = new DocumentPack())
			{
				pack.Language = "CHS";
				AssertEquals("Language of docPack should be ISO language", Core.SharedConstants.Languages.ChineseSimplified, pack.Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReRunningForContactWithBusinessObject_ChangingLanguageShouldNotAffectNumberOfDocumentsInPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = helper.CreateDummyShipment("DS1", "DC1", null);
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			template.IsDocBuilderStyleForTest = true;

			var command = helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;
			var pivot = helper.CreateMenuTemplatePivot("Pub System Shipment Document", template, command);

			var childCommand = helper.CreateDocCommand("Child Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand.Parent = dummy;
			var template2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2");
			template2.IsDocBuilderStyleForTest = true;
			helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", template2, childCommand);
			helper.CreateMenuMenuPivot(command, childCommand);

			using (var printTask = helper.CreateLoadedPrintTask(command, null))
			{
				AssertEquals("Should be just one pack", 1, printTask.Count);
				var pack = printTask.GetDocumentPacks().First();

				AssertEquals("Should be two documents in pack", 2, pack.Count);
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, pack.Language);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, ((Report)pack[0]).Language);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, ((Report)pack[1]).Language);

				instructions.Language = Core.Constants.Languages.German;
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);

				AssertEquals("Should still be two documents in pack after language change", 2, pack.Count);
				AssertEquals(Core.Constants.Languages.German, pack.Language);
				AssertEquals(Core.Constants.Languages.German, ((Report)pack[0]).Language);
				AssertEquals(Core.Constants.Languages.German, ((Report)pack[1]).Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReRunningForContactWithBusinessObject_MenuItemWithOnlyOtherDocuments_ChangingLanguageShouldNotAffectNumberOfDocumentsInPack()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = helper.CreateDummyShipment("DS1", "DC1", null);
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			template.IsDocBuilderStyleForTest = true;
			var command = helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;

			var childCommand1 = helper.CreateDocCommand("Child Pub System Shipment Document 1", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand1.Parent = dummy;
			var pivot = helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", template, childCommand1);
			helper.CreateMenuMenuPivot(command, childCommand1);

			var childCommand2 = helper.CreateDocCommand("Child Pub System Shipment Document 2", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand2.Parent = dummy;
			helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", template, childCommand2);
			helper.CreateMenuMenuPivot(command, childCommand2);

			using (var printTask = helper.CreateLoadedPrintTask(command, null))
			{
				AssertEquals("Should be just one pack", 1, printTask.Count);
				var pack = printTask.GetDocumentPacks().First();

				AssertEquals("Should be two documents in pack", 2, pack.Count);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, pack.Language);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, ((Report)pack[0]).Language);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, ((Report)pack[1]).Language);
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);

				instructions.Language = Core.Constants.Languages.German;
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);

				AssertEquals("Should still be two documents in pack after language change", 2, pack.Count);
				AssertEquals(Core.Constants.Languages.German, pack.Language);
				AssertEquals(Core.Constants.Languages.German, ((Report)pack[0]).Language);
				AssertEquals(Core.Constants.Languages.German, ((Report)pack[1]).Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRebuildIfLanguageChangedPreservesDeliveryGroupId()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var dummy = helper.CreateDummyShipment("DS1", "DC1", null);
			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template");
			template.IsDocBuilderStyleForTest = true;
			var command = helper.CreateDocCommand("Pub System Shipment Document", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			command.Parent = dummy;
			command.SU_IsDocPack = true;

			var childCommand1 = helper.CreateDocCommand("Child Pub System Shipment Document 1", BusinessContext.Shipment, null, ContactType.Consignee, 1);
			childCommand1.Parent = dummy;
			var pivot = helper.CreateMenuTemplatePivot("Child Pub System Shipment Document", template, childCommand1);
			helper.CreateMenuMenuPivot(command, childCommand1);

			using (var printTask = helper.CreateLoadedPrintTask(command, null))
			{
				AssertEquals("Should be just one pack", 1, printTask.Count);
				var pack = printTask.GetDocumentPacks().First();

				var deliveryGroupId = ZGuid.NewZGuid();
				pack.OfType<IDeliverable>().ToList().ForEach(d => d.DeliveryGroupID = deliveryGroupId);

				AssertEquals("Should be one document in pack", 1, pack.Count);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, pack.Language);
				AssertEquals(Core.Constants.Languages.EnglishAmerican, ((Report)pack[0]).Language);
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);

				pack.OfType<IDeliverable>().ToList().ForEach(d =>
				{
					AssertEquals(deliveryGroupId, d.DeliveryGroupID);
				});

				instructions.Language = Core.Constants.Languages.German;
				pack.GetTemplateGenerator(pivot);
				printTask.Run(instructions);

				AssertEquals("Should still be one document in pack after language change", 1, pack.Count);
				AssertEquals(Core.Constants.Languages.German, pack.Language);
				AssertEquals(Core.Constants.Languages.German, ((Report)pack[0]).Language);

				pack.OfType<IDeliverable>().ToList().ForEach(d =>
				{
					AssertEquals("DeliveryGroupId should be the same after language change", deliveryGroupId, d.DeliveryGroupID);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRebuildIfLanguageChanged_WithCopyReport()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var command = Factory.New<DocumentCommand>();
			command.SU_IsPublished = true;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = "Command";
			command.SU_SupportsVisualisation = false;

			var template1 = helper.CreateTemplate(Core.Constants.DataContext.None, "System Shipment Template 1");
			template1.IsDocBuilderStyleForTest = true;
			helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template1, command);

			var docSupportedBO = new MockDocSupportBizO();
			var info = new DocWrapperCopyInfoForTesting();
			info.DeliveryMethod = PrintCopyType.PRN;
			info.Name = (NoResString)"Copy Document";

			var docWrapperOriginal = new DummyDocumentWrapper(docSupportedBO, Factory);
			var docWrapperCopy = new DummyDocumentWrapper(docSupportedBO, Factory);
			docWrapperCopy.SetAdditionalCopyInfoForTesting(info);

			var docWrappers = new[] { docWrapperOriginal, docWrapperCopy };
			docSupportedBO.Wrappers = docWrappers;
			command.Parent = docSupportedBO;

			using (var printSet = new DocumentPrintSet(command, new UserControlProviderList()))
			{
				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Print;
				printSet.Run(instructions);

				var pack = printSet.GetFirstDocumentPack();
				AssertEquals("Precondition: DocumentPack should have 2 documents", 2, pack.Count);

				pack[1].IncludedInPrint = false;
				Assert("Precondition: Document 1 should be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 1 COPY should be excluded in print", !pack[1].IncludedInPrint);

				// Change language and rerun the report
				instructions.Language = Core.Constants.Languages.German;
				printSet.Run(instructions);
				Assert("Precondition: Document 1 should still be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 1 COPY should still be excluded in print", !pack[1].IncludedInPrint);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestRebuildIfLanguageChanged_WithMultipleReports()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command.SU_IsPublished = true;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = "Command";
			command.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			command.SU_EmailSubjectLine = string.Empty;

			var template1 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 1");
			template1.IsDocBuilderStyleForTest = true;
			var template2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2");
			template2.IsDocBuilderStyleForTest = true;
			var pivot1 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template1, command);
			var pivot2 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 2", template2, command);
			pivot2.SI_PrintByDefault = false;

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			command.Parent = dummy;

			using (var printSet = new DocumentPrintSet(command, new UserControlProviderList()))
			{
				var org1 = OrgHeader.New(Factory);
				var contact1 = org1.Contacts.AddNew();
				contact1.OC_Email = "dexter@morgan.com";

				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Print;
				instructions.DocumentPackCount = 2;

				var docSupportedBO = (IDocumentSupportable)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

				printSet.GetFirstDocumentPack().AddReportsToPack(command, null, docSupportedBO, null);
				printSet.Run(instructions);

				var pack = printSet.GetFirstDocumentPack();
				AssertEquals("Precondition: DocumentPack should have 4 documents", 4, pack.Count);
				AssertEquals("Precondition: DocumentPack language should be American English", Core.Constants.Languages.EnglishAmerican, pack.Language);
				AssertEquals("Precondition: Document 1 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[0]).Language);
				AssertEquals("Precondition: Document 2 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[1]).Language);
				AssertEquals("Precondition: Document 3 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[2]).Language);
				AssertEquals("Precondition: Document 4 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[3]).Language);
				Assert("Precondition: Document 1 should be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 2 should be excluded from print", !pack[1].IncludedInPrint);
				Assert("Precondition: Document 3 should be included in print", pack[2].IncludedInPrint);
				Assert("Precondition: Document 4 should be excluded from print", !pack[3].IncludedInPrint);

				pack[1].IncludedInPrint = true;
				pack[2].IncludedInPrint = false;
				Assert("Precondition: Document 1 should be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 2 should be included in print", pack[1].IncludedInPrint);
				Assert("Precondition: Document 3 should be excluded from print", !pack[2].IncludedInPrint);
				Assert("Precondition: Document 4 should be excluded from print", !pack[3].IncludedInPrint);

				// Change language and rerun the report
				instructions.Language = Core.Constants.Languages.German;
				printSet.Run(instructions);
				AssertEquals("Post condition: should still be 4 documents in pack after language change", 4, pack.Count);
				AssertEquals("Post condition: DocumentPack language should be German", Core.Constants.Languages.German, pack.Language);
				AssertEquals("Post condition: Document 1 language should be German", Core.Constants.Languages.German, ((Report)pack[0]).Language);
				AssertEquals("Post condition: Document 2 language should be German", Core.Constants.Languages.German, ((Report)pack[1]).Language);
				AssertEquals("Post condition: Document 3 language should be German", Core.Constants.Languages.German, ((Report)pack[2]).Language);
				AssertEquals("Post condition: Document 4 language should be German", Core.Constants.Languages.German, ((Report)pack[3]).Language);
				Assert("Post condition: Document 1 should still be included in print", pack[0].IncludedInPrint);
				Assert("Post condition: Document 2 should still be included in print", pack[1].IncludedInPrint);
				Assert("Post condition: Document 3 should still be excluded from print", !pack[2].IncludedInPrint);
				Assert("Post condition: Document 4 should still be excluded from print", !pack[3].IncludedInPrint);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRebuildIfLanguageChanged_DocPackWithChildMenu()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command.SU_IsPublished = true;
			command.SU_IsSystemDefined = true;
			command.SU_MenuName = "Command";
			command.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			command.SU_EmailSubjectLine = string.Empty;
			command.SU_IsDocPack = true;

			var template1 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 1");
			template1.IsDocBuilderStyleForTest = true;
			var template2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 2");
			template2.IsDocBuilderStyleForTest = true;
			var template3 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 3");
			template3.IsDocBuilderStyleForTest = true;
			var template4 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 4");
			template4.IsDocBuilderStyleForTest = true;
			var pivot1 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template1, command);
			var pivot2 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 2", template2, command);
			var pivot3 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 3", template3, command);
			var pivot4 = helper.CreateMenuTemplatePivot("Pub System Shipment Document 4", template4, command);
			pivot2.SI_PrintByDefault = false;
			pivot4.SI_PrintByDefault = false;

			var childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			childCommand.SU_IsPublished = true;
			childCommand.SU_IsSystemDefined = true;
			childCommand.SU_MenuName = "ChildCommand";
			childCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			childCommand.SU_EmailSubjectLine = string.Empty;

			var childTemplate1 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Child Template 1");
			childTemplate1.IsDocBuilderStyleForTest = true;
			var childTemplate2 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Child Template 2");
			childTemplate2.IsDocBuilderStyleForTest = true;
			var childTemplate3 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Child Template 3");
			childTemplate3.IsDocBuilderStyleForTest = true;
			var childTemplate4 = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Child Template 4");
			childTemplate4.IsDocBuilderStyleForTest = true;
			var childPivot1 = helper.CreateMenuTemplatePivot("Pub System Shipment Child Document 1", childTemplate1, childCommand);
			var childPivot2 = helper.CreateMenuTemplatePivot("Pub System Shipment Child Document 2", childTemplate2, childCommand);
			var childPivot3 = helper.CreateMenuTemplatePivot("Pub System Shipment Child Document 3", childTemplate3, childCommand);
			var childPivot4 = helper.CreateMenuTemplatePivot("Pub System Shipment Child Document 4", childTemplate4, childCommand);
			childPivot2.SI_PrintByDefault = false;
			childPivot4.SI_PrintByDefault = false;

			helper.CreateMenuMenuPivot(command, childCommand);

			var dummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			command.Parent = dummy;

			using (var printSet = new DocumentPrintSet(command, new UserControlProviderList()))
			{
				var org1 = OrgHeader.New(Factory);
				var contact1 = org1.Contacts.AddNew();
				contact1.OC_Email = "dexter@morgan.com";

				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Print;

				printSet.Run(instructions);

				var pack = printSet.GetFirstDocumentPack();
				AssertEquals("Precondition: DocumentPack should have 8 documents", 8, pack.Count);
				AssertEquals("Precondition: DocumentPack language should be American English", Core.Constants.Languages.EnglishAmerican, pack.Language);
				AssertEquals("Precondition: Document 1 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[0]).Language);
				AssertEquals("Precondition: Document 2 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[1]).Language);
				AssertEquals("Precondition: Document 3 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[2]).Language);
				AssertEquals("Precondition: Document 4 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[3]).Language);
				AssertEquals("Precondition: Child Document 1 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[4]).Language);
				AssertEquals("Precondition: Child Document 2 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[5]).Language);
				AssertEquals("Precondition: Child Document 3 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[6]).Language);
				AssertEquals("Precondition: Child Document 4 language should be American English", Core.Constants.Languages.EnglishAmerican, ((Report)pack[7]).Language);
				Assert("Precondition: Document 1 should be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 2 should be excluded from print", !pack[1].IncludedInPrint);
				Assert("Precondition: Document 3 should be included in print", pack[2].IncludedInPrint);
				Assert("Precondition: Document 4 should be excluded from print", !pack[3].IncludedInPrint);
				Assert("Precondition: Child Document 1 should be included in print", pack[4].IncludedInPrint);
				Assert("Precondition: Child Document 2 should be excluded from print", !pack[5].IncludedInPrint);
				Assert("Precondition: Child Document 3 should be included in print", pack[6].IncludedInPrint);
				Assert("Precondition: Child Document 4 should be excluded from print", !pack[7].IncludedInPrint);

				pack[1].IncludedInPrint = true;
				pack[2].IncludedInPrint = false;
				pack[5].IncludedInPrint = true;
				pack[6].IncludedInPrint = false;
				Assert("Precondition: Document 1 should be included in print", pack[0].IncludedInPrint);
				Assert("Precondition: Document 2 should be included in print", pack[1].IncludedInPrint);
				Assert("Precondition: Document 3 should be excluded from print", !pack[2].IncludedInPrint);
				Assert("Precondition: Document 4 should be excluded from print", !pack[3].IncludedInPrint);
				Assert("Precondition: Child Document 1 should be included in print", pack[4].IncludedInPrint);
				Assert("Precondition: Child Document 2 should be included in print", pack[5].IncludedInPrint);
				Assert("Precondition: Child Document 3 should be excluded from print", !pack[6].IncludedInPrint);
				Assert("Precondition: Child Document 4 should be excluded from print", !pack[7].IncludedInPrint);

				// Change language and rerun the report
				instructions.Language = Core.Constants.Languages.German;
				printSet.Run(instructions);
				AssertEquals("Post condition: should still be 8 documents in pack after language change", 8, pack.Count);
				AssertEquals("Post condition: DocumentPack language should be German", Core.Constants.Languages.German, pack.Language);
				AssertEquals("Post condition: Document 1 language should be German", Core.Constants.Languages.German, ((Report)pack[0]).Language);
				AssertEquals("Post condition: Document 2 language should be German", Core.Constants.Languages.German, ((Report)pack[1]).Language);
				AssertEquals("Post condition: Document 3 language should be German", Core.Constants.Languages.German, ((Report)pack[2]).Language);
				AssertEquals("Post condition: Document 4 language should be German", Core.Constants.Languages.German, ((Report)pack[3]).Language);
				AssertEquals("Post condition: Child Document 1 language should be German", Core.Constants.Languages.German, ((Report)pack[4]).Language);
				AssertEquals("Post condition: Child Document 2 language should be German", Core.Constants.Languages.German, ((Report)pack[5]).Language);
				AssertEquals("Post condition: Child Document 3 language should be German", Core.Constants.Languages.German, ((Report)pack[6]).Language);
				AssertEquals("Post condition: Child Document 4 language should be German", Core.Constants.Languages.German, ((Report)pack[7]).Language);
				Assert("Post condition: Document 1 should still be included in print", pack[0].IncludedInPrint);
				Assert("Post condition: Document 2 should still be included in print", pack[1].IncludedInPrint);
				Assert("Post condition: Document 3 should still be excluded from print", !pack[2].IncludedInPrint);
				Assert("Post condition: Document 4 should still be excluded from print", !pack[3].IncludedInPrint);
				Assert("Post condition: Child Document 1 should still be included in print", pack[4].IncludedInPrint);
				Assert("Post condition: Child Document 2 should still be included in print", pack[5].IncludedInPrint);
				Assert("Post condition: Child Document 3 should still be excluded from print", !pack[6].IncludedInPrint);
				Assert("Post condition: Child Document 4 should still be excluded from print", !pack[7].IncludedInPrint);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRebuildIfLanguageChanged_ShouldOnlyContainRelatedReports()
		{
			var helper = new PrintTaskDocumentPackTestHelper(Factory);

			var command1 = Factory.New<DocumentCommand>();
			command1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command1.SU_IsPublished = true;
			command1.SU_IsSystemDefined = true;
			command1.SU_MenuName = "Command1";
			command1.SU_MenuDataContext = nameof(Core.Constants.DataContext.Shipment);
			command1.SU_EmailSubjectLine = string.Empty;

			var command2 = Factory.New<DocumentCommand>();
			command2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command2.SU_IsPublished = true;
			command2.SU_IsSystemDefined = true;
			command2.SU_MenuName = "Command2";
			command2.SU_MenuDataContext = nameof(Core.Constants.DataContext.Shipment);
			command2.SU_EmailSubjectLine = string.Empty;

			var template = helper.CreateTemplate(Core.Constants.DataContext.Shipment, "System Shipment Template 1");
			template.IsDocBuilderStyleForTest = true;
			helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template, command1);
			helper.CreateMenuTemplatePivot("Pub System Shipment Document 1", template, command2);

			var dummy1 = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			var dummy2 = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();

			AssertEquals(0, testPack.Count);

			testPack.AddReportsToPack(command1, null, dummy1, null);
			testPack.AddReportsToPack(command2, null, dummy2, null);

			AssertEquals(2, testPack.Count);

			testPack.LastTemplateGeneratorLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			testPack.Language = Core.SharedConstants.Languages.EnglishAmerican;
			testPack.RebuildIfLanguageChangedCore_Exposed();

			AssertEquals(2, testPack.Count);
		}

		public void TestResetHeadingTextForSelectedLanguage()
		{
			using (var docPack = new DocumentPack())
			using (var report = new Report(docPack, TestReport))
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				docPack.Add(report);
				var templateName = Path.GetFileNameWithoutExtension(report.Template.TemplateSourceLocation);
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), "测试1"));

				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_ExcelTemplatePath = report.Template.TemplateSourceLocation;
				report.StTemplate = template;
				report.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
				report.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\", HeadingText = \"TestHeadingText\"";
				report.WorkSheetCurrentlyBeingProcessed[2, 2] = "DisplayLabel = \"Test2\", HeadingText = \"<CodePairValue(StorageClass, Code, 1)>\"";
				report.ColumnHeadingsProcessedByReportAnalyser = false;

				AreaFactory.InstantiateArea(1, 2, report, "#Config");
				var columnHeading1 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];
				var columnHeading2 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[1];

				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "TestHeadingText", 1, 0, 63, false);
				ColumnHeadingTest.AssertPropertyValues(columnHeading2, "Test2", "Test2", "20F", 2, 1, 63, false);

				docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "测试1", 1, 0, 63, false);
				ColumnHeadingTest.AssertPropertyValues(columnHeading2, "Test2", "Test2", "20F", 2, 1, 63, false);
			}
		}

		public void TestSuspendResetingReportHeadingText()
		{
			using (var docPack = new DocumentPack())
			using (var report = new Report(docPack, TestReport))
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				docPack.Add(report);
				var templateName = Path.GetFileNameWithoutExtension(report.Template.TemplateSourceLocation);
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), "测试1"));

				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_ExcelTemplatePath = report.Template.TemplateSourceLocation;
				report.StTemplate = template;
				report.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
				report.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\", HeadingText = \"TestHeadingText\"";
				report.ColumnHeadingsProcessedByReportAnalyser = false;

				AreaFactory.InstantiateArea(1, 2, report, "#Config");
				var columnHeading1 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];

				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "TestHeadingText", 1, 0, 63, false);

				using (docPack.SuspendResetingReportHeadingText())
				{
					docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
					ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "TestHeadingText", 1, 0, 63, false);
				}
			}
		}

		public void TestLanguageSwitchWithConfigSetting()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ReportColumnSettings xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Worksheets>
    <Worksheet>
      <Name>Sheet1</Name>
      <Title>Sheet1</Title>
      <ColumnHeadings>
        <ColumnHeading>
          <DisplayLabel>Test1</DisplayLabel>
          <HeadingText>测试T</HeadingText>
          <Hidden>false</Hidden>
          <HideIfDescriptionEmpty>false</HideIfDescriptionEmpty>
        </ColumnHeading>
      </ColumnHeadings>
    </Worksheet>
  </Worksheets>
  <Version>0</Version>
  <SelectedLanguage>ZH-CN</SelectedLanguage>
</ReportColumnSettings>";

			using (var docPack = new DocumentPack())
			using (var report = new Report(docPack, TestReport))
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				docPack.Add(report);
				var templateName = Path.GetFileNameWithoutExtension(report.Template.TemplateSourceLocation);
				mockChs.Put(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), new ResourceStringData(DocBuilderResourceStrings.GetKey(templateName, DocBuilderResourceStrings.ReportLabelKeyPrefix, "TestHeadingText"), "测试1"));

				var template = Factory.New<StmTemplate>();
				template.SO_IsSystemDefined = true;
				template.SO_ExcelTemplatePath = report.Template.TemplateSourceLocation;
				report.StTemplate = template;
				report.WorkSheetCurrentlyBeingProcessed[2, 0] = "ColumnHeadings";
				report.WorkSheetCurrentlyBeingProcessed[2, 1] = "DisplayLabel = \"Test1\", HeadingText = \"TestHeadingText\"";
				report.ColumnHeadingsProcessedByReportAnalyser = false;

				AreaFactory.InstantiateArea(1, 2, report, "#Config");
				var setting = new CombinedConfigurationManager(report.ColumnHeadingManager, "Test setting");
				setting.Deserialise(xml);

				var columnHeading1 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];
				docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "测试T", 1, 0, 63, false);

				docPack.Language = Core.SharedConstants.Languages.EnglishAmerican;
				columnHeading1 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];
				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "TestHeadingText", 1, 0, 63, false);

				docPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				columnHeading1 = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];
				ColumnHeadingTest.AssertPropertyValues(columnHeading1, "Test1", "Test1", "测试T", 1, 0, 63, false);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHeadingTextIsNotResetAfterCloning()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var excelTemplate = new ExcelTemplateForUnitTesting("TestColumnHeading.xls", TestFilesSubFolder.ReportTestFiles);
			templateRecord.SO_Template = excelTemplate.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			using (var original = new DocumentPack(command))
			{
				original.Language = SharedConstants.Languages.EnglishAmerican;
				AreaFactory.InstantiateArea(1, 8, ((Report)original[0]), "#Config");
				var columnHeading = ((Report)original[0]).ColumnHeadingManager.CurrentConfiguration.Worksheets[((Report)original[0]).WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0];
				columnHeading.EnglishHeadingText = "Something";
				columnHeading.HeadingText = "SomethingElse";
				var clone = original.Clone();

				AssertNotEquals(original, clone);
				AssertEquals(columnHeading.HeadingText,
					((Report)clone[0]).ColumnHeadingManager.CurrentConfiguration
					.Worksheets[((Report)original[0]).WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0].HeadingText);
			}
		}

		public void TestRunForContactWithOveriddenBusinessObjectToLogAgainst()
		{
			DocumentPack pack = new TestableDocumentPack();
			var instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.None;
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			pack.ForceBusinessObjectToLogAgainst(dummy);

			AssertEquals("BusinessObjectToLogAgainst should be set as it was set manually", dummy, pack.BusinessObjectToLogAgainst);

			using (var report = new MockReport(TestReport))
			{
				var wrapper = new DummyDocumentWrapper(dummy, factory);
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				pack.Add(report);
				pack.Run(instructions);
				AssertEquals("BusinessObjectToLogAgainst should not be changed - should still be the bizo that was set manually", dummy, pack.BusinessObjectToLogAgainst);
			}
		}

		public void TestPrintJobForEDocsHasBusinessObjectInfo()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			var task = new PrintTask();
			var pack = new DocumentPack();

			var instructions = new DeliveryInstructions(pack);
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			instructions.Recipients.RemoveAndDeleteAll();
			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact.AttachmentType = "PDF";
			contact.Email = "test@example.com";

			task.Add(pack);

			var dummy = Factory.LoadTop1<DummyDocManagerTestBizO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			dummy.SetupDocManagerObjects();

			var eDoc = (IDeliverable)((IDocManagerSupport)dummy).DocManagerInfo.Documents[0];
			eDoc.IncludedInPrint = true;
			pack.Add(eDoc);
			task.Run(instructions);

			Factory.Save();

			var branchCode = GlbBranch.CurrentBranch.GB_BranchName.ToString();
			var companyCode = GlbCompany.CurrentCompany.GC_Name.ToString();

			var printJobs = new StmPrintJobCollection(Factory);
			printJobs.Load();
			AssertEquals("Should have made a print job", 1, printJobs.Count);
			AssertEquals("Should have logged a table name", "RefUNLOCO", printJobs[0].SP_ParentTableName);
			AssertEquals("Should have logged a PK", dummy.PK, printJobs[0].SP_ParentGuid);
			AssertEquals("Should have logged an email attachment type of TIF", "TIF", printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals("Should have logged blob type of TIF", "TIF", printJobs[0].BlobType);
			AssertEquals("Should have logged a subject line ", companyCode + " - " + branchCode + " - A Test Document AAA - AUSYD", printJobs[0].SP_EmailSubjectLine);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageHeaderGetsDataFromCorrectSource()
		{
			// Setup test report
			var template = new ExcelTemplateForUnitTesting("PageHeaderTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				// Setup test data
				Db.Connection.ExecuteNonQuery(
					@"
					create table #MyHeader
					(
					Alpha int,
					Beta int
					)

					create table #MyLines
					(
					Alpha int,
					Gamma int
					)
				");

				var insert = new StringBuilder();
				for (var header = 1; header <= 10; header++)
				{
					insert.AppendFormat("INSERT #MyHeader VALUES ({0}, {0})\n", header);
				}
				for (var line = 1; line <= 200; line++)
				{
					insert.AppendFormat("INSERT #MyLines VALUES ({0}, {0})\n", line);
				}
				Db.Connection.ExecuteNonQuery(insert.ToString());

				using (var outputStream = new MemoryStream())
				{
					// Run the report against the test data
					report.Save(outputStream);

					// Read the output
					using (var @interface = new ExcelInterface())
					{
						@interface.LoadExcelFile(outputStream);
						int row;
						for (row = 0; !string.IsNullOrEmpty(@interface.WorkSheets[0][row, 1].ToString()); row++)
						{
							var error = string.Format("Error in row {0} (\"{1}\t{2}\t{3}\") - columns 2 and 3 should be equal", row, @interface.WorkSheets[0][row, 1], @interface.WorkSheets[0][row, 2], @interface.WorkSheets[0][row, 3]);
							AssertEquals(error, @interface.WorkSheets[0][row, 3], @interface.WorkSheets[0][row, 2]);
						}
						Assert("Should have had at least 201 rows, actual count was " + row, row >= 201);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContactFromOrgHeaderGetsToExcel()
		{
			testPack.Organisation = Factory.New<OrgHeader>();

			var instructions = new DeliveryInstructions(testPack);
			instructions.Destination = DeliveryInstructionDestination.Disk;
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.OutputDirectory = Env.GetTempFileName();

			var contact = new MockAutoDocumentDelivery().GetDeliveryContactsForDocPack(null, new MockDocSupportBizO(testPack.Organisation).DocumentSupporter, null)[0];
			instructions.Recipients.Add(contact);

			File.Delete(instructions.OutputDirectory);
			var testOutput = Directory.CreateDirectory(instructions.OutputDirectory);

			try
			{
				var template = new ExcelTemplateForUnitTesting("RecipientName.xls", TestFilesSubFolder.ReportTestFiles);
				using (var report = new Report(testPack, template))
				{
					testPack.ActuallyDeliver = true;
					testPack.Add(report);
					testPack.Run(instructions);
					AssertEquals("Output file count", 1, testOutput.GetFiles().Length);

					using (var @interface = new ExcelInterface())
					{
						@interface.LoadExcelFile(testOutput.GetFiles()[0].FullName);
						AssertEquals("Should have passed the recipient name to report, run the report, saved it to disk in the appropriate folder, and replaced the recipient name macro properly", "Lorenzo", @interface.WorkSheets[0][0, 1]);
					}
				}
			}
			finally
			{
				testOutput.Delete(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContactFromOrgContactGetsToExcel()
		{
			testPack.Contact = Factory.New<OrgContact>();
			var instructions = new DeliveryInstructions(testPack);
			instructions.Destination = DeliveryInstructionDestination.Disk;
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.OutputDirectory = Env.GetTempFileName();
			File.Delete(instructions.OutputDirectory);
			var testOutput = Directory.CreateDirectory(instructions.OutputDirectory);
			var contact = new MockAutoDocumentDelivery().GetDeliveryDetailsForContact(null, testPack.StmMenuCommand);
			instructions.Recipients.Add(contact);

			try
			{
				var template = new ExcelTemplateForUnitTesting("RecipientName.xls", TestFilesSubFolder.ReportTestFiles);
				using (var report = new Report(testPack, template))
				{
					testPack.ActuallyDeliver = true;
					testPack.Add(report);
					testPack.Run(instructions);
					AssertEquals("Output file count", 1, testOutput.GetFiles().Length);

					using (var @interface = new ExcelInterface())
					{
						@interface.LoadExcelFile(testOutput.GetFiles()[0].FullName);
						AssertEquals("Should have passed the recipient name to report, run the report, saved it to disk in the appropriate folder, and replaced the recipient name macro properly", "Jimmy", @interface.WorkSheets[0][0, 1]);
					}
				}
			}
			finally
			{
				testOutput.Delete(true);
			}
		}

		public void TestAddDocWrappersAsReports()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = testMenuItem.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			var mockBizO = new MockDocSupportBizO();
			testMenuItem.Parent = mockBizO;

			var info = new DocWrapperCopyInfoForTesting();
			info.DeliveryMethod = PrintCopyType.EML;
			info.Name = (NoResString)"Copy Document";
			info.CopyCount = 2;

			var docWrapperOriginal = new DummyDocumentWrapper(mockBizO, Factory);
			var docWrapperCopy = new DummyDocumentWrapper(mockBizO, Factory);
			docWrapperCopy.SetAdditionalCopyInfoForTesting(info);

			var docWrappers = new[] { docWrapperOriginal, docWrapperCopy };

			var excelTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.EmailCoverSheet);
			testPack.AddDocWrappersAsReportsForTesting(pivot, docWrappers, testMenuItem, testMenuItem, mockBizO, null, excelTemplate);
			AssertEquals("Pack should have two reports", 2, testPack.Count);

			var firstReport = testPack[0];
			var secondReport = testPack[1];

			AssertEquals("First report name should be 'TestDoc1' taken from menu item pivot and replaced by the supporter", "TestDoc1", firstReport.Name);
			AssertEquals("Second report name should be 'Copy Document' taken from  copy info", "Copy Document", secondReport.Name);

			AssertEquals("First report copy count from mockbizo", 42, firstReport.PrinterDetails.NumberOfCopies);
			AssertEquals("Second report should be 2 taken from copy info", 2, secondReport.PrinterDetails.NumberOfCopies);

			AssertEquals("First report should have delivery mode of all taken from pivot", "ALL", firstReport.DeliveryMode);
			AssertEquals("Second report should have delivery mode of EML taken from copy info", "EML", secondReport.DeliveryMode);

			AssertEquals("Report should have stored pivot PK for later use", pivot.PK, firstReport.SourcePivotPK);
			AssertEquals("Report should have stored pivot PK for later use", pivot.PK, secondReport.SourcePivotPK);
		}

		public void TestAddDocWrappersAsReportsWithShareTemplateInPack()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = testMenuItem.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			var mockBizO = new MockDocSupportBizO();
			testMenuItem.Parent = mockBizO;

			var info = new DocWrapperCopyInfoForTesting();
			info.DeliveryMethod = PrintCopyType.EML;
			info.Name = (NoResString)"Copy Document";
			info.CopyCount = 2;

			var docWrapperOriginal = new DummyDocumentWrapper(mockBizO, Factory);
			var docWrapperCopy = new DummyDocumentWrapper(mockBizO, Factory);
			docWrapperCopy.SetAdditionalCopyInfoForTesting(info);
			var docWrappers = new[] { docWrapperOriginal, docWrapperCopy };

			var excelTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.EmailCoverSheet);
			testPack.AddDocWrappersAsReportsForTesting(pivot, docWrappers, testMenuItem, testMenuItem, mockBizO, null, excelTemplate);
			AssertEquals("Pack should have two reports", 2, testPack.Count);
			AssertNotEquals(((Report)testPack[0]).Template, ((Report)testPack[1]).Template);

			testPack.RemoveAll();
			testPack.ShareSameTemplateInPack = true;
			testPack.AddDocWrappersAsReportsForTesting(pivot, docWrappers, testMenuItem, testMenuItem, mockBizO, null, excelTemplate);
			AssertEquals("Pack should have two reports", 2, testPack.Count);
			AssertEquals("Reports should share the same template object", ((Report)testPack[0]).Template, ((Report)testPack[1]).Template);
		}

		public void TestGetPrintCopyType()
		{
			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			mockBiz.Description = "Dummy1";
			DummyDocumentWrapper dummyDocWrapper = (DummyDocumentWrapper)mockBiz.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.None, null)[0];

			TestableDocumentPack pack = new TestableDocumentPack(menu, mockBiz, null);

			AssertEquals("gets value from pivot if docwrapper is not a copy. Default from pivot is ALL", PrintCopyType.ALL, pack.GetPrintCopyTypeForTesting(dummyDocWrapper, pivot));

			pivot.SI_PrintCopyType = "EML";
			AssertEquals("gets value from pivot if docwrapper is not a copy", PrintCopyType.EML, pack.GetPrintCopyTypeForTesting(dummyDocWrapper, pivot));

			pivot.SI_PrintCopyType = "ABC";
			AssertEquals("if value on pivot is invalid, then it will default ALL", PrintCopyType.ALL, pack.GetPrintCopyTypeForTesting(dummyDocWrapper, pivot));

			DocWrapperCopyInfo info = new DocWrapperCopyInfoForTesting();
			dummyDocWrapper.SetAdditionalCopyInfoForTesting(info);
			AssertEquals("If the docwrapper is a copy, it will use the PrintCopyType from the docwrapper copy info - default ALL", PrintCopyType.ALL, pack.GetPrintCopyTypeForTesting(dummyDocWrapper, pivot));

			info.DeliveryMethod = PrintCopyType.PRN;
			dummyDocWrapper.SetAdditionalCopyInfoForTesting(info);
			AssertEquals("If the docwrapper is a copy, it will use the PrintCopyType from the docwrapper copy info ", PrintCopyType.PRN, pack.GetPrintCopyTypeForTesting(dummyDocWrapper, pivot));
		}

		public void TestGetTitleCopyCount()
		{
			DocumentCommand menu = Factory.New<DocumentCommand>();

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			MockDocSupportBizO mockBiz = new MockDocSupportBizO();
			mockBiz.Description = "Dummy1";
			DummyDocumentWrapper dummyDocWrapper = (DummyDocumentWrapper)mockBiz.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.None, null)[0];
			TestableDocumentPack pack = new TestableDocumentPack(menu, mockBiz, null);

			TitleCopyCountPair generatedPair = pack.GetTitleCopyCountForTesting(dummyDocWrapper, mockBiz, "Test", mockBiz, pivot);
			AssertEquals("gets value from replacement on DocumentSupporter if docwrapper is not a copy", "TestDoc1", generatedPair.Title);

			//null ref
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: docDataProvider", delegate
			{ pack.GetTitleCopyCountForTesting(null, null, "Test", mockBiz, pivot); });

			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: docSupportProvider", delegate
			{ pack.GetTitleCopyCountForTesting(dummyDocWrapper, null, "Test", mockBiz, pivot); });

			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: docDataProvider", delegate
			{ pack.GetTitleCopyCountForTesting(null, mockBiz, "Test", mockBiz, pivot); });

			pivot.SI_DocumentTitle = "A Different Title";
			generatedPair = pack.GetTitleCopyCountForTesting(dummyDocWrapper, mockBiz, "Test", mockBiz, pivot);
			AssertEquals("gets value from pivot if docwrapper is not a copy and replacement doesn't match", "A Different Title", generatedPair.Title);

			DocWrapperCopyInfo info = new DocWrapperCopyInfoForTesting();
			dummyDocWrapper.SetAdditionalCopyInfoForTesting(info);
			generatedPair = pack.GetTitleCopyCountForTesting(dummyDocWrapper, mockBiz, "Test", mockBiz, pivot);
			AssertEquals("If the docwrapper is a copy, it will use the PrintCopyType from the docwrapper copy info - default 'document'", "Document", generatedPair.Title);
			AssertEquals("If the docwrapper is a copy, it will use the count from the docwrapper copy info - default 1", (short)1, generatedPair.CopyCount);

			info.Name = (NoResString)"Testing";
			info.CopyCount = 2;
			dummyDocWrapper.SetAdditionalCopyInfoForTesting(info);
			generatedPair = pack.GetTitleCopyCountForTesting(dummyDocWrapper, mockBiz, "Test", mockBiz, pivot);
			AssertEquals("If the docwrapper is a copy, it will use the PrintCopyType from the docwrapper copy info", "Testing", generatedPair.Title);
			AssertEquals("If the docwrapper is a copy, it will use the CopyCount from the docwrapper copy info", (short)2, generatedPair.CopyCount);
		}

		public void TestCoverSheetOnlyAddedInEmailAndFaxAndPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			AssertEquals("Precondition: there should be no print jobs in database", 0, Factory.GetDatabaseCount(typeof(StmPrintJob)));
			var docPack = new DocumentPack();
			using (var report = new Report(docPack, TestReport, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				docPack.Add(report);

				StmPrintQueue printQueue = Factory.New<StmPrintQueue>();
				printQueue.SQ_QueueName = "TestPrinter";
				printQueue.SQ_DisplayName = "TestPrinter";
				Factory.Save();

				DeliveryInstructions instructions = new DeliveryInstructions(docPack);

				instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
				instructions.IncludeCoverNote = true;
				instructions.CoverNote = "Test";
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.SetAndSaveDeliveryGroupSubjectLine(docPack, new PrintTask.ReportSubjectLineMapping(null, "test"));

				DocDeliveryContact contact = instructions.Recipients[0];
				contact.DeliveryMethod = "EML";
				contact.Email = "test@test.com";

				instructions.Recipients.AddNew();
				DocDeliveryContact contact2 = instructions.Recipients[1];
				contact2.DeliveryMethod = "FAX";
				contact2.Fax = "90251199";

				instructions.Recipients.AddNew();
				DocDeliveryContact contact3 = instructions.Recipients[2];
				contact3.DeliveryMethod = "PRN";

				docPack.Run(instructions);
				AssertEquals("Must be 4 print jobs", 4, Factory.GetDatabaseCount(typeof(StmPrintJob)));

				StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_JobType, nameof(PrintCopyType.EML)));
				AssertNotNull(printJob);

				using (TempFile tmpFile = TempFile.NewWithExtension("xls"))
				{
					using (FileStream stream = new FileStream(tmpFile.Filename, FileMode.Create))
					{
						stream.Write(printJob.SP_CustomProperties, 0, printJob.SP_CustomProperties.Length);
					}

					using (ExcelInterface excel = new ExcelInterface())
					{
						excel.LoadExcelFile(tmpFile.Filename);
						AssertEquals("Email must contain Cover Sheet, Template sheet and the original hidden sheet", 3, excel.WorkSheets.Count);
					}
				}
				printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_JobType, nameof(PrintCopyType.FAX)));
				AssertNotNull(printJob);

				using (TempFile tmpFile = TempFile.NewWithExtension("xls"))
				{
					using (FileStream stream = new FileStream(tmpFile.Filename, FileMode.Create))
					{
						stream.Write(printJob.SP_CustomProperties, 0, printJob.SP_CustomProperties.Length);
					}

					using (ExcelInterface excel = new ExcelInterface())
					{
						excel.LoadExcelFile(tmpFile.Filename);
						AssertEquals("Fax must contain Cover Sheet, Template sheet and the original hidden sheet", 3, excel.WorkSheets.Count);
					}
				}

				printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_JobType, nameof(PrintCopyType.PRN)));
				AssertNotNull(printJob);

				using (TempFile tmpFile = TempFile.NewWithExtension("xls"))
				{
					using (FileStream stream = new FileStream(tmpFile.Filename, FileMode.Create))
					{
						stream.Write(printJob.SP_CustomProperties, 0, printJob.SP_CustomProperties.Length);
					}

					using (ExcelInterface excel = new ExcelInterface())
					{
						excel.LoadExcelFile(tmpFile.Filename);
						AssertEquals("Cover Sheet shouldn't be in printed document, only has to be original template sheet (there is no hidden sheet because we didn't have to merge the file)", 1, excel.WorkSheets.Count);
					}
				}
			}
		}

		public void TestDeserializeDocPackFromReportCollection()
		{
			ReportCommand command = Factory.LoadTop1<ReportCommand>(new DocumentZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep"));
			DocumentPack pack = new DocumentPack(command);

			using (Report deserializedReport = new Report(null, null))
			{
				NotificationBuffer notifications = new NotificationBuffer();
				pack.DeserializeDocPackFromReportCollection(deserializedReport, notifications);
				Assert("There should be at least one report.", pack.Count > 0);
				foreach (Report report in pack)
				{
					AssertEquals("DeserializedReport should be set on all reports in the pack.", deserializedReport, report.DeserializedReport);
					AssertEquals("ScheduleTaskNotifications should be set on all reports in the pack.", notifications, report.ScheduleTaskNotifications);
					AssertEquals("ScheduleTask should be null on all reports in the pack.", null, report.ScheduleTask);
				}
			}

			// let us add a ScheduleTask now...
			using (Report deserializedReport = new Report(null, null))
			{
				NotificationBuffer notifications = new NotificationBuffer();
				ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				deserializedReport.SetScheduleTask(scheduleTask);

				pack.DeserializeDocPackFromReportCollection(deserializedReport, notifications);
				Assert("There should be at least one report.", pack.Count > 0);
				foreach (Report report in pack)
				{
					AssertEquals("DeserializedReport should be set on all reports in the pack.", deserializedReport, report.DeserializedReport);
					AssertEquals("ScheduleTaskNotifications should be set on all reports in the pack.", notifications, report.ScheduleTaskNotifications);
					AssertEquals("ScheduleTask should be set on all reports in the pack.", scheduleTask, report.ScheduleTask);
				}
			}
		}

		public void TestDocumentDeliveredLogAddedToBusinessObjectAndIsSavedIfStrategySaysTo()
		{
			TestDocumentDeliveredLogAddedToBusinessObject("Event from RefDocType.RT_SE_NKDocumentReceivedEvent logged against the parent", DeliveryInstructionDestination.Print, true, new FactoryStrategy.SaveInChunks());
		}

		public void TestDocumentDeliveredLogAddedToBusinessObjectDontAddLogForPreview()
		{
			TestDocumentDeliveredLogAddedToBusinessObject("Event not raised when in preview mode", DeliveryInstructionDestination.Preview, false, new FactoryStrategy.SaveInChunks());
		}

		public void TestDocumentDeliveredLogAddedToBusinessObjectButNotSavedIfStrategySaysNotTo()
		{
			TestDocumentDeliveredLogAddedToBusinessObject("Event not raised when in preview mode", DeliveryInstructionDestination.Print, true, new FactoryStrategy.PopulateButDoNotSave(Factory));
		}

		void TestDocumentDeliveredLogAddedToBusinessObject(string message, DeliveryInstructionDestination deliveryMethod, bool expectEventToBeRaised, FactoryStrategy saveStrategy)
		{
			OrgHeader organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Organization Notes"));
			AssertNotNull("The 'Organisation Notes' must exist for the test", command);
			PrintQueue.Factory.Save();

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_SE_NKDocumentReceivedEvent = Events.OrderConfirmed.Code;
			command.Documents[0].SI_RT_DocType = docType.PK;
			using (PrintTask task = new PrintTask())
			using (TestableDocumentPack pack = new TestableDocumentPack(command, organisation, null))
			{
				int factorySaveCount = BusinessObjectFactory.GlobalSaveCount;
				pack.ActuallyDeliver = false;
				task.Add(pack);
				DeliveryInstructions instructions = new DeliveryInstructions(saveStrategy);
				instructions.Destination = deliveryMethod;
				instructions.PrinterDelivery.PrintQueuePK = PrintQueue.PK;
				task.Run(instructions);

				bool expectedToSave = saveStrategy is FactoryStrategy.SaveInChunks;
				StmALog documentDeliveredLog = organisation.Logs.MostRecentLogByEventTime(Events.OrderConfirmed);
				if (expectEventToBeRaised)
				{
					AssertNotNull(message, documentDeliveredLog);
					AssertEquals("Event saved to the database", expectedToSave, documentDeliveredLog.IsInDatabase);
				}
				else
				{
					AssertNull(message, documentDeliveredLog);
				}
				if (expectedToSave && expectEventToBeRaised)
				{
					AssertNotEquals("BusinessObjectFactory.GlobalSaveCount should have changed if FactoryStrategy.SaveInChunks indicated on Instructions.", factorySaveCount, BusinessObjectFactory.GlobalSaveCount);
				}
				else
				{
					AssertEquals("BusinessObjectFactory.GlobalSaveCount should not have changed if FactoryStrategy.PopulateButDoNotSave indicated on Instructions.", factorySaveCount, BusinessObjectFactory.GlobalSaveCount);
				}
			}
		}

		public void TestPrintByDefault()
		{
			DummyBODocSupportable docSupportedBO = Factory.New<DummyBODocSupportable>();

			DocumentCommand documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_MenuName = "Test Doc";

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = ".DummyBODocSupportable";
			template.SO_Template = TestReport.GetAsByteArray();

			StmMenuTemplatePivotBase pivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pivot1.SI_SO = template.PK;
			pivot1.SI_SU = documentCommand.PK;
			pivot1.SI_PrintByDefault = true;
			pivot1.SI_DocumentTitle = "Test Doc1";

			StmMenuTemplatePivotBase pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SO = template.PK;
			pivot2.SI_SU = documentCommand.PK;
			pivot2.SI_PrintByDefault = false;
			pivot2.SI_DocumentTitle = "Test Doc2";

			using (DocumentPack documentPack = new DocumentPack(documentCommand, docSupportedBO, null, null))
			{
				AssertEquals("Prerequisite", 2, documentPack.Count);
				AssertEquals(true, documentPack[0].IncludedInPrint);
				AssertEquals(false, documentPack[1].IncludedInPrint);

				DeliveryInstructions instructions = new DeliveryInstructions(documentPack);
				instructions.PrinterDelivery.PrintQueuePK = PrintQueue.PK;
				instructions.Destination = DeliveryInstructionDestination.DummyDestinationForTesting;
				documentPack.Run(instructions);

				AssertEquals(2, instructions.DeliverablesToBePrinted.Count);
				AssertEquals(true, instructions.DeliverablesToBePrinted[0].IncludedInPrint);
				AssertEquals(false, instructions.DeliverablesToBePrinted[1].IncludedInPrint);
			}
		}

		public void TestDefaultLanguage()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.Italian;
			try
			{
				var clientOrg = Factory.New<OrgHeader>();
				clientOrg.OH_Language = Core.SharedConstants.Languages.Italian;

				var docSupportedBO = Factory.New<DummyBODocSupportable>();
				docSupportedBO.Z0_Guid = clientOrg.PK;
				var command = Factory.NewWithValidTestData<DocumentCommand>();
				var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
				templateRecord.SO_DataContext = ".DummyBODocSupportable";
				var excelTemplate = CustomisableSectionTest;
				templateRecord.SO_Template = excelTemplate.GetAsByteArray();
				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SO = templateRecord.PK;
				pivot.SI_SU = command.PK;
				var pack = new DocumentPack(command, docSupportedBO, null, null);
				AssertEquals(SharedConstants.Languages.Italian, pack.Language);

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					docSupportedBO = Factory.New<DummyBODocSupportable>();
					docSupportedBO.Z0_Guid = clientOrg.PK;
					command = Factory.NewWithValidTestData<DocumentCommand>();
					templateRecord = Factory.New<StmTemplateBase>();
					templateRecord.SO_DataContext = ".DummyBODocSupportable";
					templateRecord.SO_Template = TestXls.GetAsByteArray();
					pivot = Factory.New<StmMenuTemplatePivotBase>();
					pivot.SI_SO = templateRecord.PK;
					pivot.SI_SU = command.PK;
					pack = new DocumentPack(command, docSupportedBO, null, null);
					AssertEquals("Language should always default to English for Non-DocBuilder documents", Core.SharedConstants.Languages.EnglishAmerican, pack.Language);
				}

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var reportCommand = Factory.NewWithValidTestData<ReportCommand>();
					templateRecord = Factory.New<StmTemplateBase>();
					templateRecord.SO_DataContext = ".DummyBODocSupportable";
					templateRecord.SO_Template = TestXls.GetAsByteArray();
					pivot = Factory.New<StmMenuTemplatePivotBase>();
					pivot.SI_SO = templateRecord.PK;
					pivot.SI_SU = reportCommand.PK;
					pack = new DocumentPack(reportCommand);
					AssertEquals("Language should always default to Current Language for reports", Core.SharedConstants.Languages.ChineseSimplified, pack.Language);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = Res.DefaultLanguage;
			}
		}

		class DocumentDeliveredLogSupporterBizObj : NonPersistentBusinessObject, IDocumentDeliveredLogSupporter
		{
			public DocumentDeliveredLogSupporterBizObj(OrgHeader org)
				: base(org.Factory)
			{
				this.org = org;
			}

			public readonly OrgHeader org;

			#region IDocumentDeliveredLogSupporter Members

			public Type BusinessObjectTypeToLogAgainst
			{
				get { return typeof(OrgHeader); }
			}

			public ZGuid Identifier
			{
				get { return org.PK; }
			}

			#endregion
		}

		public void TestDocumentDeliveredLogAddedForIDocumentDeliveredLogSupporter()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var bizObj = new DocumentDeliveredLogSupporterBizObj(org);
			var report = new MockReport(TestReport);
			((IReportForUnitTesting)report).SetBusinessObjectForTesting(BODocDataProvider.Get(bizObj));
			report.DocumentDeliveredEventCode = Events.DocumentDelivered.Code;
			var command = Factory.New<DocumentCommand>();
			using (var pack = new TestableDocumentPack())
			{
				pack.AddAndSetMenuItem(report, command);
				pack.Deliver(new Enterprise.DocumentEngine.DeliveryMethods.DummyMethodForTesting(), new DeliveryInstructions());
				var documentDeliveredLog = org.Logs.MostRecentLogByEventTime(Events.DocumentDelivered);
				AssertNotNull("Document Delivered log should be added for IDocumentDeliveredLogSupporter", documentDeliveredLog);
			}
		}

		public void TestDocumentDeliveredLogNotAddedForDocumentNotIncludeInPrint()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var bizObj = new DocumentDeliveredLogSupporterBizObj(org);

			var report = new MockReport(TestReport);
			((IReportForUnitTesting)report).SetBusinessObjectForTesting(BODocDataProvider.Get(bizObj));
			report.DocumentDeliveredEventCode = Events.DocumentDelivered.Code;
			report.Name = "IncludeInPrint";
			var report1 = new MockReport(TestReport);
			((IReportForUnitTesting)report1).SetBusinessObjectForTesting(BODocDataProvider.Get(bizObj));
			report1.DocumentDeliveredEventCode = Events.DocumentDelivered.Code;
			report1.IncludedInPrint = false;
			report1.Name = "NotIncludeInPrint";

			var command = Factory.New<DocumentCommand>();
			using (var pack = new TestableDocumentPack())
			{
				pack.AddAndSetMenuItem(report, command);
				pack.AddAndSetMenuItem(report1, command);
				pack.Deliver(new Enterprise.DocumentEngine.DeliveryMethods.DummyMethodForTesting(), new DeliveryInstructions());
				var documentDeliveredLog = org.Logs.MostRecentLogByEventTime(Events.DocumentDelivered);
				Assert("Document Delivered log should not be added for document not included in print", !documentDeliveredLog.ReferenceFreeText.Contains("NotIncludeInPrint"));
			}
		}

		public void TestClone()
		{
			ReportCommand command = Factory.Load<ReportCommand>(ReportScheduleTaskTest.TestReportPK);
			using (DocumentPack original = new DocumentPack(command))
			{
				((Report)original[0]).OverrideReportDbOption = true;
				original.Language = Core.SharedConstants.Languages.French;
				var clone = original.Clone();
				AssertNotEquals(original, clone);
				AssertEquals("Should be 1 report in the pack", 1, clone.Count);
				Assert("Must be SynchronisedWithDeserialisedReport", ((IReportForUnitTesting)clone[0]).SynchronisedWithDeserialisedReport);
				AssertNotNull("Deserialised Report should be set", ((Report)clone[0]).DeserializedReport);
				AssertEquals("Report.OverrideReportDbOption should be synchronized", ((Report)original[0]).OverrideReportDbOption, ((Report)clone[0]).OverrideReportDbOption);
				AssertEquals(original.Language, clone.Language);
			}
		}

		public void TestCloneForDocumentCommand()
		{
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery());
			using (DocumentPack original = new DocumentPack(command))
			{
				DocumentPack clone = original.Clone();
				AssertNotEquals(original, clone);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestRunForRecipients_WhenAttachmentTypeChanges_ShouldResetCachedExcelFile()
		{
			using (var pack = new DocumentPack())
			{
				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Xls });
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Csv });
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Xls });

				var report = new ReportForTestingJustTheResetCachedExcelFileMethodBecauseBusinessObjectsCantBeMocked(
					pack, new ExcelTemplateForUnitTesting("DateFilter.xls", TestFilesSubFolder.ReportTestFiles));
				pack.Add(report);

				pack.RunForRecipients(instructions);
				AssertEquals(2, report.ResetCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestRunForRecipients_WithSameAttachment_ShouldNotResetCachedExcelFile()
		{
			using (var pack = new DocumentPack())
			{
				var instructions = new DeliveryInstructions(pack);
				instructions.Recipients.RemoveAll();
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Xls });
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Xls });
				instructions.Recipients.Add(new DocDeliveryContact(Factory) { DeliveryMethod = "EML", AttachmentType = AttachmentTypeList.Codes.Xls });

				var report = new ReportForTestingJustTheResetCachedExcelFileMethodBecauseBusinessObjectsCantBeMocked(
					pack, new ExcelTemplateForUnitTesting("DateFilter.xls", TestFilesSubFolder.ReportTestFiles));
				pack.Add(report);

				pack.RunForRecipients(instructions);
				AssertEquals(0, report.ResetCount);
			}
		}

		public void TestLanguageAfterLoadedForTranslatableLegacyDocument()
		{
			AssertLanguageAfterLoadedForTranslatableLegacyDocument(true);
			AssertLanguageAfterLoadedForTranslatableLegacyDocument(false);
		}

		void AssertLanguageAfterLoadedForTranslatableLegacyDocument(bool forChildTemplate)
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var mockBizO = new MockDocSupportBizO(orgProxy);
			mockBizO.Description = "Dummy1";

			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "test";
			contact.OC_Email = "www.test@test.com";
			contact.OC_Language = Core.Constants.Languages.ChineseSimplified;

			var contactDocs = contact.Documents.AddNew();
			contactDocs.OD_DefaultContact = true;
			contactDocs.OD_DocumentGroup = "ALL";
			Factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[DataContext=ForwardingShipment]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]");

			var menu = Factory.NewWithValidTestData<DocumentCommand>();
			menu.Parent = mockBizO;
			menu.SU_BusinessContext = nameof(BusinessContext.Test);
			menu.SU_FilterList = "";

			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = excelTemplate.GetAsByteArray();
			template.SO_DataContext = "ForwardingShipment";
			template.SO_Name = "Test Template";

			if (forChildTemplate)
			{
				var childMenu = Factory.NewWithValidTestData<DocumentCommand>();
				childMenu.Parent = mockBizO;
				childMenu.SU_BusinessContext = nameof(BusinessContext.Test);
				childMenu.SU_FilterList = "";

				var menuPivot = menu.ChildMenus.AddNew();
				menuPivot.SF_SU_Inward = menu.PK;
				menuPivot.SF_SU_Outward = childMenu.PK;

				var pivot = childMenu.Documents.AddNew();
				pivot.SI_SO = template.PK;
				pivot.SI_SU = childMenu.PK;
				pivot.SI_DocumentTitle = "TestDoc";
			}
			else
			{
				var pivot = menu.Documents.AddNew();
				pivot.SI_SU = menu.PK;
				pivot.SI_SO = template.PK;
			}

			using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DocumentDeliveryDefaultLanguagesCollection { new DocumentDeliveryDefaultLanguages { Fallback = Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, Order = 1 } }))
			using (var printTask = new DocumentPrintSet(menu, null))
			{
				var instruction = new DeliveryInstructions(printTask[0]);
				printTask.Run(instruction);
				AssertEquals(Core.Constants.Languages.ChineseSimplified, printTask[0].Language);
			}
		}

		public void TestSupportsLanguageSelectionWorksForChildDocument()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.Constants.Languages.German;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;

			var menu = Factory.NewWithValidTestData<DocumentCommand>();
			var childMenu = Factory.NewWithValidTestData<DocumentCommand>();

			var template = Factory.NewWithValidTestData<StmTemplateBase>();
			template.SO_Name = "Customized Document Elements";

			var menuPivot = Factory.NewWithValidTestData<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = menu.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = childMenu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			var mockBizO = new MockDocSupportBizO();
			mockBizO.Description = "Dummy1";

			using (var pack = new TestableDocumentPack(menu, mockBizO, null))
			{
				pack.Organisation = orgProxy;
				AssertEquals(Core.Constants.Languages.German, pack.Language);
			}
		}

		public void TestSupportsLanguageSelectionWorksForMultiChildDocument()
		{
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.OH_Language = Core.Constants.Languages.German;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;

			var menu = Factory.NewWithValidTestData<DocumentCommand>();
			var pivotMenu = Factory.NewWithValidTestData<DocumentCommand>();
			var childMenu = Factory.NewWithValidTestData<DocumentCommand>();

			var template = Factory.NewWithValidTestData<StmTemplateBase>();
			template.SO_Name = "Customized Document Elements";

			var menuPivotIterated = Factory.NewWithValidTestData<StmMenuMenuPivot>();
			menuPivotIterated.SF_SU_Inward = menu.PK;
			menuPivotIterated.SF_SU_Outward = pivotMenu.PK;

			var menuPivot = Factory.NewWithValidTestData<StmMenuMenuPivot>();
			menuPivot.SF_SU_Inward = pivotMenu.PK;
			menuPivot.SF_SU_Outward = childMenu.PK;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = childMenu.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			var mockBizO = new MockDocSupportBizO();
			mockBizO.Description = "Dummy1";

			using (var pack = new TestableDocumentPack(menu, mockBizO, null))
			{
				pack.Organisation = orgProxy;
				AssertEquals(Core.Constants.Languages.German, pack.Language);
			}
		}

		public void TestPrintCoptyTypeSetInTime()
		{
			var menu = Factory.NewWithValidTestData<DocumentCommand>();

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
			template.SO_Name = "System Document Elements";
			template.SO_DataContext = "GenericFreightJob";

			var printCopyType = nameof(PrintCopyType.PRN);
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			pivot.SI_DocumentTitle = "TestDoc";
			pivot.SI_PrintCopyType = printCopyType;
			pivot.SI_MenuTemplateFilter = string.Format(@"""<DeliveryMode>"" == ""{0}""", printCopyType);

			var config = pivot.DocConfigs.AddNew();
			var configItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");

			var mockBizO = new MockDocSupportBizO();
			using (var pack = new DocumentPack(menu, mockBizO, null, null))
			{
				Assert("Report should be added as filter matched.", pack.Count == 1);
			}

			printCopyType = nameof(PrintCopyType.EML);
			pivot.SI_MenuTemplateFilter = string.Format(@"""<DeliveryMode>"" == ""{0}""", printCopyType);
			using (var pack = new DocumentPack(menu, mockBizO, null, null))
			{
				Assert("Report should not be added as filter not matched.", pack.Count == 0);
			}

			pivot.SI_PrintCopyType = printCopyType;
			using (var pack = new DocumentPack(menu, mockBizO, null, null))
			{
				Assert("Report should be added as filter matched.", pack.Count == 1);
			}
		}

		public void TestCustomWatermarkFromDocSupporter()
		{
			var docCommand = Factory.NewWithValidTestData<DocumentCommand>();
			docCommand.SU_MenuName = "Unit Test";

			StmTemplateBase template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = docCommand.PK;
			pivot.SI_DocumentTitle = "TestDoc";

			var mockBizO1 = new MockDocSupportBizO();
			using (var pack = new DocumentPack(docCommand, mockBizO1, null, null))
			{
				AssertEquals(1, pack.Count);
				Assert(pack[0] is Report);
				var report = (Report)pack[0];

				AssertNull(report.CustomWatermarkText);
			}

			var mockBizO2 = new MockDocSupportBizO(new MockDocSupportBizODocumentSupporterCustomWatermark(mockBizO1, mockBizO1.Wrappers, 1));
			using (var pack = new DocumentPack(docCommand, mockBizO2, null, null))
			{
				AssertEquals(1, pack.Count);
				Assert(pack[0] is Report);
				var report = (Report)pack[0];

				AssertEquals((NoResString)"UT-Watermark", report.CustomWatermarkText);
			}

			var mockBizO3 = new MockDocSupportBizO(new MockDocSupportBizODocumentSupporterGetCustomWatermark(mockBizO1, mockBizO1.Wrappers, 1));
			using (var pack = new DocumentPack(docCommand, mockBizO3, null, null))
			{
				AssertEquals(1, pack.Count);
				Assert(pack[0] is Report);
				var report = (Report)pack[0];

				AssertEquals((NoResString)"UT-Unit Test", report.CustomWatermarkText);
			}
		}

		[GuiTest, ExpectNoExceptions]
		public void TestFilterOutContactsWhoDoNotDeliverForAutoDeliveryOfMultiDocPack()
		{
			var menu = Factory.NewWithValidTestData<DocumentCommand>();
			menu.SU_PreventAutoDelivery = true;
			menu.SU_ContactType = ContactType.Consignee.Code;

			var instructions = new DeliveryInstructions();
			instructions.Recipients.RemoveAll();
			instructions.Destination = DeliveryInstructionDestination.Auto;
			instructions.DocumentPackCount = 2;

			var org = OrgHeader.New(Factory);
			org.OH_FullName = "Jerry Org";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "jerry1test@test.com";
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.DoNotDeliver;
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = menu.SU_ContactType;

			var bizO = new MockDocSupportBizO(org);

			using (var pack = new DocumentPack(menu, bizO, null, null))
			{
				pack.Run(instructions);
			}
		}

		public void TestInsertAtStart()
		{
			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			var templateRecord = Factory.New<StmTemplateBase>();
			templateRecord.SO_DataContext = ".DummyBODocSupportable";
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var pack = new DocumentPack();
			pack.AddReportsToPack(command, null, docSupportedBO, null);
			AssertEquals("Precondition: documentpack has 1 report", 1, pack.Count);

			using (var report = new MockReport(TestReport))
			{
				AssertNotEquals("mock report's parent", pack, report.Parent);
				pack.InsertAtStart(report);

				AssertEquals("documentpack has 2 reports", 2, pack.Count);
				var firstReport = pack.GetFirstReport();
				AssertEquals(firstReport, report);
				AssertNotNull(report.Parent);
				AssertEquals("mock report's parent is changed.", pack, report.Parent);
			}
		}

		public void TestDeliverDocumentWithEDocDeliveryMethod()
		{
			using (var templateStream = new MemoryStream())
			using (var testPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(testPack, templateStream,
				@"{A}-[#Config]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					var instructions = new DeliveryInstructions();
					instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					instructions.Recipients.RemoveAndDeleteAll();

					var docContact = instructions.Recipients.AddNew();
					docContact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.EDoc;
					docContact.AttachmentType = AttachmentTypeList.Codes.Pdf;
					docContact.Name = "Test";

					var dummy = Factory.New<DummyDocManagerTestBizO>();
					testPack.Add(report);

					printTask.Add(testPack);
					printTask.Run(instructions);

					var printJobs = new StmPrintJobCollection(Factory);
					printJobs.Load();

					AssertEquals("Pre-condition: printJobs.Count", 1, printJobs.Count);

					AssertEquals("PrintJob.SP_JobType", "DDS", printJobs[0].SP_JobType);
					AssertEquals("PrintJob.SP_EmailFromAddress", string.Empty, printJobs[0].SP_EmailFromAddress);
					AssertEquals("PrintJob.SP_EmailSignature", string.Empty, printJobs[0].SP_EmailSignature);
				}
			}
		}

		public void TestSetSupportsLanguageSelectionForTranslatableLegacyDocument()
		{
			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Legacy Document",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var template2 = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Translate Legacy Document",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]", ".DummyBODocSupportable");

			var dummy = Factory.New<DummyBODocSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot1 = documentCommand.Documents.AddNew();
			pivot1.SI_SU = documentCommand.PK;
			pivot1.SI_SO = template1.PK;

			var pivot2 = documentCommand.Documents.AddNew();
			pivot2.SI_SU = documentCommand.PK;
			pivot2.SI_SO = template2.PK;

			using (var testPack = new DocumentPack(documentCommand))
			{
				testPack.AddReportsToPack(documentCommand, null, dummy, null);
				testPack.SetSupportsLanguageSelectionForTranslatableLegacyDocument();
				Assert(testPack.SupportsLanguageSelection);
			}
		}

		public void TestBusinessObjectsForPrintJob()
		{
			var shipment1 = Factory.New<IForwardingShipment>() as BusinessObject;
			var shipment2 = Factory.New<IForwardingShipment>() as BusinessObject;

			var biz1 = Factory.New<DummyBODocSupportable_DocDataProvider>();
			biz1.SetBusinessObjectForPrintJob(shipment1);
			var biz2 = Factory.New<DummyBODocSupportable_DocDataProvider>();
			biz2.SetBusinessObjectForPrintJob(shipment2);

			using (var pack = new DocumentPack())
			{
				pack.Add(new Report(pack, null, null, "a", null, DocumentDirection.ANY, false));
				pack.Add(new Report(pack, null, new DataProviderList(biz1), "b", null, DocumentDirection.ANY, false) { IncludedInPrint = true });
				pack.Add(new Report(pack, null, new DataProviderList(biz2), "c", null, DocumentDirection.ANY, false) { IncludedInPrint = false });

				AssertContainsExactElementsInAnyOrder(new List<BusinessObject> { shipment1, shipment2 }, pack.BusinessObjectsForPrintJob().ToList());
				AssertContainsExactElementsInAnyOrder(new List<BusinessObject> { shipment1 }, pack.BusinessObjectsForPrintJob(true).ToList());
				AssertContainsExactElementsInAnyOrder(new List<BusinessObject> { shipment2 }, pack.BusinessObjectsForPrintJob(false).ToList());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportIsPasswordProtectedForOpeningSameWithPivotInDocumentPack()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var excelTemplate = new ExcelTemplateForUnitTesting("TestColumnHeading.xls", TestFilesSubFolder.ReportTestFiles);
			templateRecord.SO_Template = excelTemplate.GetAsByteArray();
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;
			pivot.SI_IsPasswordProtectedForOpening = true;

			using (var documentPack = new DocumentPack(command))
			{
				AssertEquals("IsPasswordProtectedForOpening should be same.", pivot.SI_IsPasswordProtectedForOpening, (documentPack[0] as Report).IsPasswordProtectedForOpening);
			}
		}

		StmPrintQueue PrintQueue
		{
			get
			{
				if (printQueue == null)
				{
					printQueue = Factory.New<StmPrintQueue>();
					printQueue.SQ_DisplayName = "DisplayName";
					printQueue.SQ_QueueName = "QueueName";
				}
				return printQueue;
			}
		}
		StmPrintQueue printQueue;

		protected override DocumentPack GetCollectionToTest()
		{
			return new DocumentPack();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Report(new DocumentPack(), null);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var mockDocSupportBizO = new MockDocSupportBizO();
			testMenuItem = Factory.New<DocumentCommand>();
			testPack = new TestableDocumentPack(testMenuItem, mockDocSupportBizO, null);
			contactFinder = (MockDeliveryContactFinder)testPack.AutoDocumentDelivery;
			instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.None;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		TestableDocumentPack testPack;
		DeliveryInstructions instructions;
		DocumentCommand testMenuItem;
		MockDeliveryContactFinder contactFinder;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting testReport;
		ExcelTemplateForUnitTesting TestReport
		{
			get
			{
				if (testReport == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testReport = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testReport;
			}
		}

		ExcelTemplateForUnitTesting customisableSectionTest;
		ExcelTemplateForUnitTesting CustomisableSectionTest
		{
			get
			{
				if (customisableSectionTest == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls", "CustomisableSectionTest.xls");
					customisableSectionTest = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", Path.GetFullPath(tempFileName));
				}
				return customisableSectionTest;
			}
		}

		ExcelTemplateForUnitTesting testXls;
		ExcelTemplateForUnitTesting TestXls
		{
			get
			{
				if (testXls == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Test.xls", "Test.xls");
					testXls = new ExcelTemplateForUnitTesting("Test.xls", Path.GetFullPath(tempFileName));
				}
				return testXls;
			}
		}

		void AssertIgnoreDuplicatedDeliveryInfos(bool isAuto, int expected)
		{
			var pack = testPack;
			var task = new PrintTask();
			task.IsAutoDocumentDelivery = isAuto;
			task.Add(pack);

			using (var report = new MockReport(TestReport))
			{
				report.IncludedInPrint = true;

				var deliveryInstructions = new DeliveryInstructions();
				var deliveryMethod = new DeliveryMethod();

				pack.RenderDeliverable_Exposed(null, deliveryInstructions, deliveryMethod, report);
				AssertEquals(1, deliveryMethod.FileCount);

				pack.RenderDeliverable_Exposed(null, deliveryInstructions, deliveryMethod, report);
				AssertEquals(expected, deliveryMethod.FileCount);
			}
		}

		void RunReportForMutiJobs(DocumentMenuCustomisation customization, DocumentCommand documentCommand)
		{
			using (var printTask = customization.GetPrintTask(documentCommand) as DocumentPrintSet)
			{
				var instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.Recipients.RemoveAndDeleteAll();
				DocDeliveryContact docContact = instructions.Recipients.AddNew();
				docContact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				docContact.AttachmentType = AttachmentTypeList.Codes.Pdf;
				docContact.Email = "unit.test@cargowise.com";
				docContact.Name = "Test";
				printTask.Run(instructions);
			}
		}

		void AssertEdocParentBO(DocumentMenuCustomisation customization, DocumentCommand documentCommand, BusinessObject expectedBO)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_ParentTableName, expectedBO.TableName);
			query.AddToFilter(StmPrintJobSchema.SP_ParentGuid, expectedBO.PK);
			var printJob = Factory.LoadTop1<StmPrintJob>(query);
			AssertNotNull($"Excepted parent type: {expectedBO.GetType()}", printJob);
		}

		void AssertDDVEventExists(DocumentMenuCustomisation customization, DocumentCommand documentCommand, BusinessObject expectedBO)
		{
			var query = new ZQuery(StmALogSchema.SL_Table, expectedBO.TableName);
			query.AddToFilter(StmALogSchema.SL_Parent, expectedBO.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DocumentDeliveredCode);
			var log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull($"{expectedBO.GetType()} should have DDV event ", log);
		}

		DocumentCommand CreateDocPackWithDocumentAndForm(DocumentMenuCustomisation customization)
		{
			var docTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Document",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=Consol]
{A}-[#SectionBody]
{B}-[Hello From Document]
{A}-[#EndOfReport]");
			docTemplate.SO_DataContext = "Consol";

			// including both body and end in string literal below confuses .net parser causing release build failure due to CS1024 error
			const string body = "#Body";
			const string endOfSection = "#End";

			var formTemplate = ObjectFactory.Get<Enterprise.DocumentVisualizer.Integration.IDocumentVisualizerTestHelper>().CreateTemplate(Factory,
$@"#Config:Name=""Test Form""
DataContext=""UXML""
{endOfSection}
{body}
	Hello From Form
{endOfSection}");

			var docPack = customization.Menus.AddNew();
			docPack.SU_BusinessContext = nameof(BusinessContext.Consol);
			docPack.SU_MenuName = "Test Primary Doc Pack";

			var consolDocumentMenu = customization.AvailableChildMenus.AddNew();
			consolDocumentMenu.SU_MenuName = "Test Consol Document";
			consolDocumentMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			consolDocumentMenu.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;

			var consolDocumentPivot = Factory.New<StmMenuTemplatePivotBase>();
			consolDocumentPivot.SI_SO = docTemplate.PK;
			consolDocumentPivot.SI_SU = consolDocumentMenu.PK;

			var consolChildPivot = docPack.ChildMenus.AddNew();
			consolChildPivot.SF_SU_Inward = docPack.PK;
			consolChildPivot.SF_SU_Outward = consolDocumentMenu.PK;

			var shipmentFormMenu = customization.AvailableChildMenus.AddNew();
			shipmentFormMenu.SU_MenuName = "Test Shipment Form";
			shipmentFormMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			shipmentFormMenu.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;

			var shipmentFormPivot = Factory.New<StmMenuTemplatePivotBase>();
			shipmentFormPivot.SI_SO = formTemplate.PK;
			shipmentFormPivot.SI_SU = shipmentFormMenu.PK;
			shipmentFormPivot.SI_DataStoreName = "test";

			var shipmentChildPivot = docPack.ChildMenus.AddNew();
			shipmentChildPivot.SF_SU_Inward = docPack.PK;
			shipmentChildPivot.SF_SU_Outward = shipmentFormMenu.PK;

			docPack.SU_IsDocPack = true;
			docPack.SU_PrimaryDocPackItemId = consolChildPivot.PK;

			return docPack;
		}

		void Culture_CultureChanged(object sender, Culture.CultureChangedEventArgs e)
		{
			oldCulture.Add(e.OldCulture);
			newCulture.Add(e.NewCulture);
		}

		readonly List<CultureInfo> oldCulture = new List<CultureInfo>();
		readonly List<CultureInfo> newCulture = new List<CultureInfo>();

		class ReportForTestingJustTheResetCachedExcelFileMethodBecauseBusinessObjectsCantBeMocked : Report
		{
			internal ReportForTestingJustTheResetCachedExcelFileMethodBecauseBusinessObjectsCantBeMocked(DocumentPack pack, ExcelTemplate template)
				: base(pack, template)
			{
			}

			internal override void ResetCachedExcelFile()
			{
				base.ResetCachedExcelFile();
				ResetCount++;
			}

			internal int ResetCount { get; private set; }
		}

		sealed class MockDeliveryContactFinder : DocAutoDelivery
		{
			public DocumentCommand ExpectedMenuItem;
			public int CallCount;
			public int ContactsToReturn;

			public override DocDeliveryContactCollection GetDeliveryContactsForDocPack(IStmMenuItem menuItem, DocumentSupporter deliveryFilter, ZString documentGroup, IStmMenuItem parentMenuCommand = null)
			{
				CallCount++;
				AssertEquals("Unexpected menu item!", ExpectedMenuItem, menuItem);

				InitialiseDeliveryDetails(menuItem, deliveryFilter, documentGroup, parentMenuCommand);

				DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
				for (int index = 0; index < ContactsToReturn; index++)
				{
					DocDeliveryContact contact = contacts.AddNew();
					contact.DeliveryMethod = "FAX";
				}

				return contacts;
			}

			public override DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact)
			{
				return GetDeliveryDetailsForContact(contact, null);
			}

			public override DocDeliveryContact GetDeliveryDetailsForContact(OrgContact contact, IStmMenuItem menuItem)
			{
				CallCount++;
				StmMenuItem concreteMenuItem = menuItem as StmMenuItem;
				return new DocDeliveryContact(concreteMenuItem != null ? concreteMenuItem.Factory : null);
			}

			public OrgHeader OrganisationGiven
			{
				get { return fOrganisation; }
			}

			public OrgHeader RelatedOrganisationGiven
			{
				get { return fRelatedParty; }
			}
		}

		public class DocumentPackForDeliveredTest : DocumentPack
		{
			public DocumentPackForDeliveredTest()
			{
			}

			public DocumentPackForDeliveredTest(StmMenuItem menuItem)
				: base(menuItem)
			{
			}

			public DocumentPackForDeliveredTest(DocumentCommand documentCommand, IDocumentSupportable parentBusinessObject, UserControlProviderList userFieldList, DocumentCommand parentCommand)
				: base(documentCommand, parentBusinessObject, userFieldList, parentCommand)
			{
			}

			protected override void DeliverCore(DeliveryMethod method, INotifications notifications = null)
			{
				if (ActuallyDeliver)
				{
					base.DeliverCore(method, notifications);
				}
				OnDelivered();
			}

			public bool ActuallyDeliver;

			internal event EventHandler Delivered;

			void OnDelivered()
			{
				Delivered?.Invoke(this, EventArgs.Empty);
			}
		}

		public sealed class TestableDocumentPack : DocumentPackForDeliveredTest
		{
			public TestableDocumentPack()
			{
			}

			public TestableDocumentPack(StmMenuItem menuItem)
				: base(menuItem)
			{
			}

			public TestableDocumentPack(DocumentCommand documentCommand, IDocumentSupportable parentBusinessObject, UserControlProviderList userFieldList)
				: base(documentCommand, parentBusinessObject, userFieldList, null)
			{
			}

			public new void AddAndSetMenuItem(IDeliverable deliverable, StmMenuItemBase stmMenuItem)
			{
				base.AddAndSetMenuItem(deliverable, stmMenuItem);
			}

			public override DocAutoDelivery AutoDocumentDelivery
			{
				get
				{
					if (fDocumentDelivery == null)
					{
						MockDeliveryContactFinder contactFinder = new MockDeliveryContactFinder();
						contactFinder.CallCount = 0;
						contactFinder.ExpectedMenuItem = (DocumentCommand)StmMenuCommand;
						fDocumentDelivery = contactFinder;
					}
					return fDocumentDelivery;
				}
			}
			DocAutoDelivery fDocumentDelivery;

			internal protected override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				LastRunFileCount = 0;
				base.Run(deliveryInstructions);
			}

			public int LastRunFileCount;

			protected override void DeliverCore(DeliveryMethod method, INotifications notifications = null)
			{
				base.DeliverCore(method);
				LastRunFileCount = method.FileCount;
			}

			public ZString ContactTypeString
			{
				get { return ContactType.Code; }
			}

			public void AddDocWrappersAsReportsForTesting(StmMenuTemplatePivotBase pivot, DocumentWrapper[] docWrappers, DocumentCommand command, DocumentCommand parentCommand, IDocumentSupportable bizObject, UserControlProviderList userFieldList, ExcelTemplate exlTemplate)
			{
				pivot.Template.SO_Template = exlTemplate.GetAsByteArray();
				base.AddDocWrappersAsReports(pivot, docWrappers, command, parentCommand, bizObject, userFieldList, GetTemplateGenerator(pivot));
			}

			public PrintCopyType GetPrintCopyTypeForTesting(DocumentWrapper docWrapper, StmMenuTemplatePivotBase pivot)
			{
				return base.GetPrintCopyType(docWrapper, pivot);
			}

			public TitleCopyCountPair GetTitleCopyCountForTesting(DocumentWrapper docWrapper, IDocumentSupportable bizObject, string documentMenuName, IDocumentSupportable bizObjectToGetTitleFor, StmMenuTemplatePivotBase pivot)
			{
				return base.GetTitleCopyCount(docWrapper, bizObject, documentMenuName, bizObjectToGetTitleFor, pivot, null);
			}

			public void RenderDeliverable_Exposed(DocDeliveryContact contact, DeliveryInstructions instruction, DeliveryMethod method, IDeliverable deliverable)
			{
				base.RenderDeliverable(contact, instruction, method, deliverable);
			}

			public void RebuildIfLanguageChangedCore_Exposed()
			{
				RebuildIfLanguageChangedCore();
			}
		}

		sealed class NonPersistentBusinessObjectForTesting : NonPersistentBusinessObject
		{
		}

		sealed class DummyBODocSupportable_DocDataProvider : DummyBODocSupportable, IBODocDataProviderWithBOForPrintJob
		{
			public DummyBODocSupportable_DocDataProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DocWrapperCopyInfo AdditionalCopyInfo { get { return null; } }
			public string[] ImageNamesToRemove { get { return Array.Empty<string>(); } }
			public BusinessObject BusinessObjectToLogAgainst { get { return ParentBusinessObject; } }
			public BusinessObject ParentBusinessObject
			{
				get
				{
					if (fParentBusinessObject == null)
					{
						fParentBusinessObject = Factory.NewWithValidTestData<DummyBODocSupportable>();
					}
					return fParentBusinessObject;
				}
			}
			internal BusinessObject fParentBusinessObject;

			public IZType GetCustomField(string fieldName, string typeName) { return ZString.Empty; }
			public string GetCustomFieldCodeDescription(string fieldName, string typeName) { return ""; }
			public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) { return ""; }
			public ZDateTime GetEventLastDateTime(string eventCode) { return ZDateTime.UtcNow; }
			public void SetDocWrapperContext(Dictionary<string, object> constants) { }

			public BusinessObject BusinessObjectForPrintJob => businessObjectForPrintJob ?? BusinessObjectToLogAgainst;
			BusinessObject businessObjectForPrintJob;

			internal void SetBusinessObjectForPrintJob(BusinessObject bo)
			{
				businessObjectForPrintJob = bo;
			}
		}

		sealed class DummyBODocSupportableWithNullDocumentSupporter : DummyBusinessObject, IDocumentSupportable
		{
			public DummyBODocSupportableWithNullDocumentSupporter(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return null; }
			}
		}
	}
}

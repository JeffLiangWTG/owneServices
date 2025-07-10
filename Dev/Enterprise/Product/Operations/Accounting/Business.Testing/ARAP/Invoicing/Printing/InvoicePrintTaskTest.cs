using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.Printing.InvoicePrintTask;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	class InvoicePrintTaskTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachARInvoiceToEdocsWithPDFContent()
		{
			// Need to get the registry value before the UT logic. Otherwise it will be reset to empty once UT finished and fail other UTs.
			// Accounting logic didn't set the registry value.
			var cachedValue = AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.Value;

			var mockDocumentSupporter =
				new Mock<DocumentSupporter>(Factory.New<DummyBusinessObject>()) { CallBase = true };
			mockDocumentSupporter.Setup(m => m.BusinessContext).Returns(BusinessContext.Shipment);
			var mockEDocsProvider = new Mock<IEDocsProvider> { CallBase = true };
			mockEDocsProvider.Setup(m => m.DocumentSupporter).Returns(mockDocumentSupporter.Object);

			var supporter = new EDocsProviderSupporter(mockEDocsProvider.Object);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_BusinessContext = nameof(BusinessContext.ARInvoice);
			menuItem.SU_MenuName = "Invoice";

			var mainCommand = supporter.CreateProviderPlaceholder<DocumentCommand>(menuItem);
			mainCommand.Factory.Save();

			var childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

			var pivot = mainCommand.ChildMenus.AddNew();
			pivot.SF_SU_Inward = mainCommand.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			var eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.MiscellaneousDocument)).PK;
			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S001", true);
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var dsbCharge = testObjectCreator.CreateDSBCharge(job, testObjectCreator.AUD, testObjectCreator.AUD, 190m);
			dsbCharge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;

			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0M, 190m, 0m, 190m, 0m);
			invoice.AH_JH = job.PK;
			invoice.AH_OH = testObjectCreator.AALSHI.PK;

			var line = invoice.Lines[0];
			line.AL_JH = job.PK;
			line.AL_AC = dsbCharge.ChargeCode.PK;
			line.AL_AT = dsbCharge.JR_AT_SellGSTRate;
			dsbCharge.JR_AL_ARLine = line.PK;
			Factory.Save();

			var pdfFilePath = TestCase.BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF";
			var pdfFileName = "PDFFile";
			shipment.DocManagerInfo.AddFileOrDocument(DocumentScanning.DocumentUtilities.GetFileAsBytes(pdfFilePath), pdfFileName, Core.Constants.RefDocTypes.MiscellaneousDocument, description: "PDF as eDoc");
			shipment.DocManagerInfo.Save();

			AssertEquals("Precondition", true, (bool)ObjectFactory.Get<Enterprise.Integration.Customs.AU.IAUCustomsRegistry>().PrintEntryWhenPrintingInvoice.Value);
			AssertEquals(Core.Constants.ChargeType.Disbursement, line.ChargeCode.AC_ChargeType);
			AssertEquals(ChargeCodeGroupList.Codes.CustomsDuty, line.ChargeCode.AC_ChargeGroup);
			AssertEquals(true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals("Shipment should have 1 document in EDocs", 1, shipment.DocManagerInfo.AllEDocs.Count);

			AssertNoExceptionThrown(() => AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
		}

		public void TestPrintRecipientsCompanyNameShouldNotTruncated()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "CIK1234");
			consol.JK_IsForwarding = ZBool.True;
			consol.JK_IsCFS = ZBool.True;
			var shipment = TestObjectCreator.CreateShipment("SHP1", "AUSYD", "USLAX", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "line 1 desc", 100m);
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoice.AH_JH = job.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = TestObjectCreator.AALSHI.PK;
			orgAddress.CompanyName = "LongCompanyName LongCompanyName LongCompanyName LongCompanyName LongCompanyName";

			invoice.AH_OA_InvoiceAddressOverride = orgAddress.PK;

			TestObjectCreator.CreateCharge(invoice.Lines[0]);

			Factory.Save();

			Assert(orgAddress.CompanyName.Length > JobDocAddress.Schema.E2_CompanyNameTruncatedLength);

			var command = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"));

			var docPack = new DocumentPack(command, invoice, null, null, true, null, "");
			var contacts = new DocAutoDelivery().GetDeliveryContactsForDocPack(docPack.StmMenuCommand, docPack.DocumentSupporter, docPack.DocumentGroup, docPack.Parent?.ParentMenuCommand);

			AssertEquals(orgAddress.CompanyName, contacts[0].CompanyName);
		}

		public void TestInvoidPrintTaskDeliveryWithEmailSubjectMacroCouldGetJobParentFieldValue()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "USLAX", "CIK1234");
			consol[JobConsolSchema.JK_IsForwarding] = ZBool.True;
			consol[JobConsolSchema.JK_IsCFS] = ZBool.True;
			var shipment = creator.CreateShipment("SHP1", "AUSYD", "USLAX", consol);
			var job = creator.CreateJob(shipment);
			job.JH_GE = creator.NonCurrentDepartment.PK;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1.0m);
			creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job, creator.CC1, creator.AUD, 1m, "line 1 desc", 100m);
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoice.AH_JH = job.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;

			creator.CreateCharge(invoice.Lines[0]);

			Factory.Save();

			var printTask1 = new InvoicePrintTask(new Configuration(invoice) { JobParent = shipment });
			AssertEquals(shipment, printTask1.Task_ForTestOnly.MostTopLevelBusinessObject);

			Statement.TestAboveMaxPreviewCount = false;
			var printTask2 = new InvoicePrintTask(new Configuration(invoice) { JobParent = shipment });
			AssertEquals(shipment, printTask2.Task_ForTestOnly.MostTopLevelBusinessObject);

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), company.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var printTask3 = new InvoicePrintTask(new Configuration(invoice) { JobParent = shipment });
				AssertEquals(shipment, printTask3.Task_ForTestOnly.MostTopLevelBusinessObject);
			}
		}

		public void TestTaskCreatedMoreThanOnceThrowError()
		{
			var newMenu = Factory.NewWithValidTestData<StmMenuItem>();
			newMenu.SU_MenuName = "a new menu";
			newMenu.SU_BusinessContext = "ARInvoice";
			newMenu.SU_MenuPath = "";
			newMenu.SU_IsSystemDefined = true;
			newMenu.SU_IsPublished = false;
			newMenu.SU_GS_NKStaffCode = "";
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var command = InvoicePrintCommandManager.New(invoice, newMenu.PK).Command;
			var printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertNull(printTask.Task_ForTestOnly);

			ErrorReporter.Clear();
			printTask.Task_ForTestOnly = new DocumentPrintSet(command, null);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
			printTask.Task_ForTestOnly = new DocumentPrintSet(command, null);
			AssertContains("To improve performance of invoice printing, the task should be created only once.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestConstructorSetIsLegacyDocumentFlag()
		{
			ZString[] menuNames = { "Cost Confirmation Document" };
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice) { MenuNames = menuNames, IsLegacyDocument = true });
			AssertEquals(true, task.IsLegacyDocument_ForTestOnly);
			InvoicePrintTask task2 = new InvoicePrintTask(new Configuration(invoice) { MenuNames = menuNames, IsLegacyDocument = false });
			AssertEquals(false, task2.IsLegacyDocument_ForTestOnly);
		}

		public void TestJobLoaderLoadingConsolInvoicesWhereConsolIsAlsoCFS()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "USLAX", "CIK1234");
			consol[JobConsolSchema.JK_IsForwarding] = ZBool.True;
			consol[JobConsolSchema.JK_IsCFS] = ZBool.True;
			IJobInvoicingPlugIn shipment1 = creator.CreateShipment("SHP1", "AUSYD", "USLAX", consol);
			IJobInvoicingPlugIn shipment2 = creator.CreateShipment("SHP2", "AUSYD", "USLAX", consol);
			Job job1 = creator.CreateJob(shipment1);
			Job job2 = creator.CreateJob(shipment2);
			job1.JH_GE = creator.NonCurrentDepartment.PK;

			job2.JH_GE = creator.NonCurrentDepartment.PK;

			Factory.Save();

			var invoice = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1.0m);
			creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job1, creator.CC1, creator.AUD, 1m, "line 1 desc", 100m);
			creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job2, creator.CC1, creator.AUD, 1m, "line 2 desc", 200m);
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoice.AH_JH = ZGuid.Empty;

			creator.CreateCharge(invoice.Lines[0]);
			creator.CreateCharge(invoice.Lines[1]);

			Factory.Save();

			ParentJobLoader loader = new ParentJobLoader(Factory);
			AssertNotNull("Should find the consol", loader.Load(invoice));
			AssertEquals("Should be a Forwarding Consol", consol, loader.Load(invoice));
		}

		public void TestJobLoaderLoadingJob_IsAmendingTransactionForPeriodicInvoice()
		{
			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "USLAX", "CIK1234");
			var shipment = testObjectCreator.CreateShipment("SHP1", "AUSYD", "USLAX", consol);
			var shipmentJob = testObjectCreator.CreateJob(shipment);
			shipmentJob.JH_GE = testObjectCreator.NonCurrentDepartment.PK;
			newFactory.Save();

			var invoice = (ARInvoice)testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.AUD, 1.0m);
			testObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, shipmentJob, testObjectCreator.CC1, testObjectCreator.AUD, 1m, "line 1 desc", 100m);
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			invoice.AH_JH = ZGuid.Empty;

			var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();

			var (isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = creditNote.CheckIsAmendingTransactionForPeriodicInvoice();

			AssertEquals("The credit note is amending transaction for periodic invoice", true, isAmendingTransactionForPeriodicInvoice);
			AssertEquals("The credit note is single job credit note", true, isSingleJob);
			AssertEquals("Return job from CheckIsAmendingTransactionForPeriodicInvoice is same as the job of the first line of credit note", shipmentJob, job);

			var loader = new ParentJobLoader(newFactory);
			var result = loader.Load(creditNote);
			AssertNotNull("Should find the shipment", result);
			AssertEquals("Should be a shipment", shipment, result);
		}

		#region Concurrency Test

		[ExpectNoExceptions]
		public void TestConcurrencyIssueOnAH_InvoicePrintedDoesntHappen()
		{
			TransactionHeader transaction = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			Assert("AH_InvoicePrinted is not printed yet", !transaction.AH_InvoicePrinted);

			InvoicePrintTask invoicePrintTaskInSession1 = new InvoicePrintTaskWithNoRefreshFactoryForTest(transaction.PK);
			InvoicePrintTask invoicePrintTaskInSession2 = new InvoicePrintTaskWithNoRefreshFactoryForTest(transaction.PK);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			invoicePrintTaskInSession1.UpdateTransactionsAsPrinted_ForTestOnly();
			invoicePrintTaskInSession2.UpdateTransactionsAsPrinted_ForTestOnly();

			var anotherFactory = new BusinessObjectFactory();
			TransactionHeader transactionReloaded = anotherFactory.Load<ARInvoice>(transaction.PK);
			Assert("AH_InvoicePrinted must have updated value, and this", transactionReloaded.AH_InvoicePrinted);
		}

		class InvoicePrintTaskWithNoRefreshFactoryForTest : InvoicePrintTask
		{
			public InvoicePrintTaskWithNoRefreshFactoryForTest(ZGuid pk)
				: base(new Configuration(pk))
			{
			}
			protected override BusinessObjectFactory GetNewFactory()
			{
				BusinessObjectFactory factory = base.GetNewFactory();
				factory.RefreshEnabled = false;
				return factory;
			}
		}

		[ExpectNoExceptions]
		public void TestConcurrencyErrorSavingRecords()
		{
			var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			var task = new InvoicePrintTaskForConcurrencySavingTest(aRInvoice.PK);
			task.UpdateTransactionsAsPrinted_ForTestOnly();
		}

		class InvoicePrintTaskForConcurrencySavingTest : InvoicePrintTaskWithNoRefreshFactoryForTest
		{
			public InvoicePrintTaskForConcurrencySavingTest(ZGuid pk)
				: base(pk)
			{
				this.aRInvoicePK = pk;
			}
			readonly ZGuid aRInvoicePK;

			protected override BusinessObjectFactory GetNewFactory()
			{
				BusinessObjectFactory factory = base.GetNewFactory();
				factory.Saving += new BusinessObjectFactory.SavingEventHandler(ConcurrencyEvent);
				return factory;
			}

			void ConcurrencyEvent(BusinessObjectFactory factory)
			{
				var anotherFactory = new BusinessObjectFactory();
				var aRInvoiceReloaded = anotherFactory.Load<ARInvoice>(aRInvoicePK);
				aRInvoiceReloaded.AH_Desc = "azerty";
				anotherFactory.Save();
			}
		}

		#endregion

		public void TestInvoicePrintTaskSetMenuCommandCorrectlyForDocBuilderInvoice()
		{
			DocumentCommand docBuilderCommandInShipmentContext = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment))));
			DocumentCommand docBuilderCommandInARInvoiceContext = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.ARInvoice))));

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			Factory.Save();

			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicePrintTask printTask1 = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("invoice print task should use the docBuilder invoice menu in the AR invoice context", docBuilderCommandInARInvoiceContext.PK, printTask1.Task_ForTestOnly.ParentMenuCommand.PK);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.Job.Parent = shipment;
			Factory.Save();

			InvoicePrintTask printTask2 = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("invoice print task should use the docBuilder invoice menu in the shipment context", docBuilderCommandInShipmentContext.PK, printTask2.Task_ForTestOnly.ParentMenuCommand.PK);
		}

		public void TestInvoicePrintTaskSetMenuCommandCorrectlyForLegacyInvoice()
		{
			DocumentCommand legacyInvoiceCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.OldStyleInvoiceName), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.ARInvoice))));

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			Factory.Save();

			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			InvoicePrintTask printTask1 = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("invoice print task should use the legacy invoice menu", legacyInvoiceCommand.PK, printTask1.Task_ForTestOnly.ParentMenuCommand.PK);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.Job.Parent = shipment;
			Factory.Save();

			InvoicePrintTask printTask2 = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("invoice print task should use the legacy invoice menu", legacyInvoiceCommand.PK, printTask2.Task_ForTestOnly.ParentMenuCommand.PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatePacksWithDocBuilderStrips()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Shipment))));

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

			StmMenuMenuPivot pivot = command.ChildMenus.AddNew();
			pivot.SF_SU_Inward = command.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			Factory.Save();

			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.Job.Parent = shipment;

			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			documentFactory.AddFileOrDocument(shipment.PK, null, File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif")), null, eDoc.DocType.RT_DocType, "DEF", false);
			documentFactory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK) { JobParent = shipment });
			try
			{
				int expected = printTask.Task_ForTestOnly is DocumentPrintSetWithStreaming ? 1 : 2; // PrintStreaming can only group Invoices by Org and count groups
				var report = GetDocumentPack(printTask, 0).OfType<Report>().FirstOrDefault();
				AssertEquals("Number of packs should be created.", expected, printTask.Task_ForTestOnly.Count);
				AssertEquals("The report should be for DocBuilder Invoice.", JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName, report.MenuItem.SU_MenuName);
				AssertEquals("The 2nd pack should belong to the child menu attached to the document.", childCommand.PK, GetDocumentPack(printTask, 1)[0].MenuItem.PK);
			}
			finally
			{
				if (!(printTask.Task_ForTestOnly is DocumentPrintSetWithStreaming))
				{
					DisposePacks(printTask.Task_ForTestOnly.GetDocumentPacks());
				}
			}
		}

		public void TestNotSavedInvoices()
		{
			ARInvoice invoice1 = Factory.New<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1));
			AssertEquals(1, task.TaskCount);
			task.UpdateTransactionsAsPrinted_ForTestOnly();
			AssertEquals(false, invoice1.IsInDatabase);
		}

		public void TestInvoicePrintedFlagUpdateStatusForSpecialCountries()
		{
			AssertInvoicePrintedFlagForCountry(Core.Constants.CountryCodes.Peru, false);
			AssertInvoicePrintedFlagForCountry(Core.Constants.CountryCodes.China, true);
			AssertInvoicePrintedFlagForCountry(Core.Constants.CountryCodes.VietNam, false);
			AssertInvoicePrintedFlagForCountry(Core.Constants.CountryCodes.Indonesia, false);
			AssertInvoicePrintedFlagForCountry(Core.Constants.CountryCodes.Australia, true);
		}

		void AssertInvoicePrintedFlagForCountry(ZString countryCode, bool expectedIsPrinted)
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				task.UpdateTransactionsAsPrinted_ForTestOnly();
				AssertEquals(expectedIsPrinted, invoice.AH_InvoicePrinted);
			}
		}

		public void TestInvoicePrintedFlagStaysFalseForUndeliverables()
		{
			var objectCreator = TestObjectCreator;

			var org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			var invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			var invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org1.PK;
			var invoice3 = Factory.New<ARInvoice>();
			invoice3.AH_OH = org1.PK;
			var creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org1.PK;

			Factory.Save();

			var task = new InvoicePrintTask(new Configuration(invoice1, invoice2, invoice3, creditNote1));

			AssertEquals("Should only be one document pack in print task", 1, task.TaskCount);
			AssertEquals("Should have 4 documents in the pack", 4, GetDocumentPack(task, 0).Count);

			task.Task_ForTestOnly.invalidDocumentParentPKs = new List<ZGuid> { invoice1.PK, invoice2.PK };
			task.UpdateTransactionsAsPrinted_ForTestOnly();

			var loadFactory = new BusinessObjectFactory();
			invoice1 = loadFactory.Load<ARInvoice>(invoice1.PK);
			Assert("Invoice should not be marked as printed.", !invoice1.AH_InvoicePrinted);
			invoice2 = loadFactory.Load<ARInvoice>(invoice2.PK);
			Assert("Invoice should not be marked as printed.", !invoice2.AH_InvoicePrinted);
			invoice2 = loadFactory.Load<ARInvoice>(invoice3.PK);
			Assert("Invoice should be marked as printed.", invoice3.AH_InvoicePrinted);
		}

		public void TestInvoicePrintedFlag()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org1.PK;
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org1.PK;

			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1, invoice2, creditNote1));

			AssertEquals("Should only be one document pack in print task", 1, task.TaskCount);
			AssertEquals("Should have 3 documents in the pack", 3, GetDocumentPack(task, 0).Count);

			task.UpdateTransactionsAsPrinted_ForTestOnly();

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			invoice1 = loadFactory.Load<ARInvoice>(invoice1.PK);
			Assert(invoice1.AH_InvoicePrinted);
			invoice2 = loadFactory.Load<ARInvoice>(invoice2.PK);
			Assert(invoice2.AH_InvoicePrinted);
		}

		[ExpectNoExceptions]
		public void TestRunTask()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice));
			task.RunTaskWithInstructions(new DeliveryInstructions());
			AssertEquals(1, task.TaskCount);
		}

		[ExpectNoExceptions]
		public void TestRunDraftInvoiceWillSetBusinessObjectToLogAgainstToJobObject()
		{
			TestObjectCreator objectCreator = TestObjectCreator;
			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org1.PK;
			invoice.AH_JH = job.PK;
			Assert("invoice is not in database", !invoice.IsInDatabase);

			using (InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true }))
			{
				Assert("BusinessObjectToLogAgainst should be set to JobHeader", GetDocumentPack(task, 0).BusinessObjectToLogAgainst is JobHeader);
				task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.All);
			}
		}

		public void TestRunDraftInvoiceUseLanguageFromFirstDocPackInPreviewMode()
		{
			Action<InvoicePrintTaskWithDummyRunDocPackForTest> printLogic = x =>
			{
				x.isProFormaInvoice = true;
				x.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.PreviewOnly);
			};
			AssertDeliveryInstructionsUseLanguageFromFirstDocPack(printLogic);
		}

		public void TestRunWithoutShowDeliveryInstructionsUseLanguageFromFirstDocPack()
		{
			Action<InvoicePrintTaskWithDummyRunDocPackForTest> printLogic = x => x.Run(false);
			AssertDeliveryInstructionsUseLanguageFromFirstDocPack(printLogic);
		}

		void AssertDeliveryInstructionsUseLanguageFromFirstDocPack(Action<InvoicePrintTaskWithDummyRunDocPackForTest> runAcion)
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Core.SharedConstants.Languages.Spanish;
			TestObjectCreator objectCreator = TestObjectCreator;
			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			org1.OH_Language = Core.SharedConstants.Languages.Spanish;
			Factory.Save();
			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			using (InvoicePrintTaskWithDummyRunDocPackForTest task = new InvoicePrintTaskWithDummyRunDocPackForTest(invoice1))
			{
				runAcion(task);
				AssertEquals(Core.SharedConstants.Languages.Spanish, task.GetTask().GetFirstDocumentPack().Language);
			}
		}

		class InvoicePrintTaskWithDummyRunDocPackForTest : InvoicePrintTask
		{
			public InvoicePrintTaskWithDummyRunDocPackForTest(params TransactionHeader[] invoices)
				: base(new Configuration(invoices))
			{ }

			protected override DocumentPack CreateNewDocumentPackForInvoice(DocumentCommand command, InvoicingBase invoice, bool shouldCreateDocs)
			{
				return new DummyRunDocumentPackForTest(command, invoice, null, null, shouldCreateDocs);
			}
		}

		public class DummyRunDocumentPackForTest : DocumentPack
		{
			public DummyRunDocumentPackForTest()
			{ }

			public DummyRunDocumentPackForTest(DocumentCommand documentCommand, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, DocumentCommand parentCommand, bool shouldAutoAddeDocs = true)
				: base(documentCommand, documentSupportable, userFieldList, parentCommand, shouldAutoAddeDocs)
			{ }

			protected override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
			{
				return;
			}
		}

		[ExpectNoExceptions]
		public void TestRunDraftInvoiceWillNotThrowExceptionIfJobIsNull()
		{
			TestObjectCreator objectCreator = TestObjectCreator;
			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org1.PK;
			AssertEquals("invoice's job should not be set", ZGuid.Empty, invoice.AH_JH);

			using (InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice)))
			{
				Assert("BusinessObjectToLogAgainst should be set to ARInvoice", GetDocumentPack(task, 0).BusinessObjectToLogAgainst is ARInvoice);
				task.isProFormaInvoice = true;
				task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.All);
			}
		}

		[ExpectException(typeof(InvoiceIsDeletedException))]
		public void TestRunDraftInvoiceWillNotThrowExceptionIfInvoiceIsDeleted()
		{
			TestObjectCreator objectCreator = TestObjectCreator;
			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org1.PK;
			invoice.AH_JH = job.PK;

			using (InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice)))
			{
				invoice.Delete();
				Assert(invoice.IsDeleted);
				task.isProFormaInvoice = true;
				task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.All);
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		[ExpectExceptionMessage(typeof(ApplicationException), "RunDraftInvoiceWithAllDeliveryOptions should only be used for printing proforma invoice")]
		public void TestRunDraftInvoiceWillThrowExceptionIfItsNotUsedForProformaInvoice()
		{
			TestObjectCreator objectCreator = TestObjectCreator;
			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = org1.PK;
			BusinessObject shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = new Job.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_JH = job.PK;
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice));
			AssertEquals(task.isProFormaInvoice, false);
			task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.All);
		}

		public void TestSetOneBusinessObjectToLogAgainstForAllPrintTasks()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();

			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1, invoice2));

			task.SetOneBusinessObjectToLogAgainstForAllPrintTasks(invoice1.Header.CompanyData);

			AssertEquals("BusinessObjectToLogAgainst of all the DocumentPack in the tasks should be set to one BO", invoice1.Header.CompanyData, GetDocumentPack(task, 0).BusinessObjectToLogAgainst);
			AssertEquals("BusinessObjectToLogAgainst of all the DocumentPack in the tasks should be set to one BO", invoice1.Header.CompanyData, GetDocumentPack(task, 1).BusinessObjectToLogAgainst);
		}

		public void TestNoInvoices()
		{
			APDiscount trans1 = Factory.New(typeof(APDiscount)) as APDiscount;
			CashBook.DirectReceipt.DirectReceipt trans2 = Factory.New(typeof(CashBook.DirectReceipt.DirectReceipt)) as CashBook.DirectReceipt.DirectReceipt;
			ARInvoice trans3 = Factory.New(typeof(ARInvoice)) as ARInvoice;

			InvoicePrintTask task = new InvoicePrintTask(new Configuration());
			AssertEquals(0, task.TaskCount);
		}

		public void TestNoInvoicesWhenCannotPrint()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				Assert("CanPrint", arInvoice.CheckCanPrintPostedInvoicingBase().Result);
				Factory.Save();

				AssertEquals("AR Invoice should not have any document in EDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);
				Assert("CanPrint", arInvoice.CheckCanPrintPostedInvoicingBase().Result);

				Assert("Attaching should be successful", AttachARInvoiceToEdocs_ForTestOnly(arInvoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = arInvoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);
				AssertEquals("CanPrint", false, arInvoice.CheckCanPrintPostedInvoicingBase().Result);

				Statement.TestAboveMaxPreviewCount = false;
				var expectedExceptionMessage = $@"Reprinting Invoice document is not allowed for some countries and transaction types. It should be prevented before it gets here.
Current company country: {GlbCompany.CurrentCompany.Country.Code}, Transaction country: {arInvoice.Company?.Country.Code}, Transaction: AR INV, Is in database: True. ";
				AssertExceptionThrown(typeof(ReprintingInvoiceException), expectedExceptionMessage, () => new InvoicePrintTask(new Configuration(arInvoice)));
			}
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			CombineAssertions(() =>
			{
				var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();
				var aRInvoicePrintTask = new InvoicePrintTask(new Configuration(aRInvoice));
				ZGuid arInvoiceMenuItemPk = new ZGuid(MenuPKForARInvoice);
				AssertEquals("ARInvoice should return MenuPKForARInvoice", arInvoiceMenuItemPk, aRInvoicePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				Factory.Save();
				var aRCreditNotePrintTask = new InvoicePrintTask(new Configuration(aRCreditNote));
				AssertEquals("ARCreditNote must return MenuPKForARInvoice", arInvoiceMenuItemPk, aRCreditNotePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				var aRAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
				Factory.Save();
				var aRAdjustmentNotePrintTask = new InvoicePrintTask(new Configuration(aRAdjustmentNote));
				AssertEquals("ARAdjustmentNote must return MenuPKForARInvoice", arInvoiceMenuItemPk, aRAdjustmentNotePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				var proformaInvoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();
				var proformaInvoicePrintTask = new InvoicePrintTask(new Configuration(proformaInvoice) { JobParent = proformaInvoice.Job?.Parent, IsProFormaInvoice = true });
				ZGuid proformaInvoiceMenuItemPK = new ZGuid(MenuPkForProformaInvoice);
				AssertEquals("Proforma Invoice must return MenuPkForProformaInvoice", proformaInvoiceMenuItemPK, proformaInvoicePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				ZString[] menuNames = { "Cost Confirmation Document" };
				var aPInvoice = Factory.NewWithValidTestData<APInvoice>();
				Factory.Save();
				InvoicePrintTask aPInvoicePrintTask = new InvoicePrintTask(new Configuration(aPInvoice) { MenuNames = menuNames, IsLegacyDocument = true });
				ZGuid aPInvoiceMenuItemPK = new ZGuid(MenuPkForAPInvoice);
				AssertEquals("APInvoice must return MenuPkForAPInvoice", aPInvoiceMenuItemPK, aPInvoicePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				var aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
				Factory.Save();
				var aPCreditNotePrintTask = new InvoicePrintTask(new Configuration(aPCreditNote) { MenuNames = menuNames, IsLegacyDocument = true });
				AssertEquals("APCreditNote must return MenuPkForAPInvoice", aPInvoiceMenuItemPK, aPCreditNotePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);

				var aPAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
				Factory.Save();
				var aPAdjustmentNotePrintTask = new InvoicePrintTask(new Configuration(aPAdjustmentNote) { MenuNames = menuNames, IsLegacyDocument = true });
				AssertEquals("APAdjustmentNote must return MenuPkForAPInvoice", aPInvoiceMenuItemPK, aPAdjustmentNotePrintTask.Task_ForTestOnly.DeliveryInstructionsDefaultPK);
			});
		}

		public void TestNoPK()
		{
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(ZGuid.Empty));
			AssertEquals(0, task.TaskCount);
		}

		public void TestFilterForInvoicesDoNotHaveOrderBy()
		{
			var invoice1 = Factory.New<ARInvoice>();
			var invoice2 = Factory.New<ARInvoice>();

			var filter = FilterForInvoices_ForTestOnly(new BusinessObject[] { invoice1 });
			Assert("ORDER BY should be empty", filter.OrderBy.IsEmpty);

			filter = FilterForInvoices_ForTestOnly(new BusinessObject[] { invoice1, invoice2 });
			Assert("ORDER BY should be empty", filter.OrderBy.IsEmpty);
		}

		public void TestTransactionsAndJobParent()
		{
			var mockJobParent = new Mock<IJobHeaderParent>();
			mockJobParent.Setup(m => m.PK).Returns(ZGuid.Empty);
			mockJobParent.Setup(m => m.TableName).Returns("");
			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(ZGuid.Empty) { JobParent = mockJobParent.Object });
			AssertEquals("JobParent", mockJobParent.Object, printTask.JobParent);

			GlbCompany newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newCompany.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			GlbBranch newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;

			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
			TransactionHeader[] testTransactions = printTask.Transactions_ForTestOnly(filter, null);
			int originalCount = testTransactions.Length;

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			ARInvoice invoice3 = Factory.New<ARInvoice>();
			ARInvoice invoice4 = Factory.New<ARInvoice>();

			SetUpTestInvoice(invoice1);
			SetUpTestInvoice(invoice2);
			SetUpTestInvoice(invoice3);
			SetUpTestInvoice(invoice4);

			invoice3.AH_GB = newBranch.PK;
			Factory.Save();

			testTransactions = printTask.Transactions_ForTestOnly(filter, null);
			AssertEquals("Count", originalCount + 3, testTransactions.Length);

			bool foundJob = false;
			foreach (TransactionHeader transaction in testTransactions)
			{
				JobHeader job = transaction.Job;
				if (job != null)
				{
					foundJob = true;
					AssertEquals("transaction.Job.Parent", mockJobParent.Object, transaction.Job.Parent);
				}
			}

			AssertEquals("At least one transaction should have a job.", true, foundJob);
		}

		public void TestMenuPK()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(ZGuid.Empty) { MenuNames = ["Denis's document"] });
			AssertEquals("MenuPK", ZGuid.Empty, task.GetMenuPK(invoice));
		}

		public void TestMenuName()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration(ZGuid.Empty) { MenuNames = ["Denis's document"] });
			AssertEquals("MenuName", "Denis's document", task.GetMenuNames(invoice)[0]);
		}

		public void TestMenuName_Invoice()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ARInvoice invoice = Factory.New<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration());
			AssertEquals("MenuName", "Invoice", task.GetMenuNames(invoice)[0]);
		}

		public void TestMenuName_DocBuilderInvoice()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ARInvoice invoice = Factory.New<ARInvoice>();
			InvoicePrintTask task = new InvoicePrintTask(new Configuration());
			AssertEquals("MenuName", "DocBuilder Invoice", task.GetMenuNames(invoice)[0]);
		}

		public void TestInvoicesGroupedByOrgAndWatermark()
		{
			AccountingConfigurationRegistry.Instance.PrintWatermarkForTransactionAwaitingApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "watermark1");
			var objectCreator = TestObjectCreator;

			var org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			var org2 = objectCreator.CreateOrgHeader("ORG2", false, true, false, false, true, true);

			var arInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice1.AH_OH = org1.PK;
			var arInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice2.AH_OH = org1.PK;
			var arInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice3.AH_OH = org2.PK;
			var arInvoice4 = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice4.AH_OH = org2.PK;

			var aRCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote1.AH_OH = org1.PK;
			var aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = org1.PK;
			var aRCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote3.AH_OH = org2.PK;
			var aRCreditNote4 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote4.AH_OH = org2.PK;

			var aRAdjustmentNote1 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdjustmentNote1.AH_OH = org1.PK;
			var aRAdjustmentNote2 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdjustmentNote2.AH_OH = org1.PK;
			var aRAdjustmentNote3 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdjustmentNote3.AH_OH = org2.PK;
			var aRAdjustmentNote4 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdjustmentNote4.AH_OH = org2.PK;

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice1, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice2, Core.Constants.EInvoicingPivotState.Failed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice3, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice4, Core.Constants.EInvoicingPivotState.Queued);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRCreditNote1, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRCreditNote2, Core.Constants.EInvoicingPivotState.Sent);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRCreditNote3, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRCreditNote4, Core.Constants.EInvoicingPivotState.Pending);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRAdjustmentNote1, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRAdjustmentNote2, Core.Constants.EInvoicingPivotState.Discarded);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRAdjustmentNote3, Core.Constants.EInvoicingPivotState.Succeed);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, aRAdjustmentNote4, Core.Constants.EInvoicingPivotState.Failed);

			Factory.Save();

			Assert(!arInvoice1.IsAwaitingApprovalFromGovt);
			Assert(arInvoice2.IsAwaitingApprovalFromGovt);
			Assert(!arInvoice3.IsAwaitingApprovalFromGovt);
			Assert(arInvoice4.IsAwaitingApprovalFromGovt);
			Assert(!aRCreditNote1.IsAwaitingApprovalFromGovt);
			Assert(aRCreditNote2.IsAwaitingApprovalFromGovt);
			Assert(!aRCreditNote3.IsAwaitingApprovalFromGovt);
			Assert(aRCreditNote4.IsAwaitingApprovalFromGovt);
			Assert(!aRAdjustmentNote1.IsAwaitingApprovalFromGovt);
			Assert(aRAdjustmentNote2.IsAwaitingApprovalFromGovt);
			Assert(!aRAdjustmentNote3.IsAwaitingApprovalFromGovt);
			Assert(aRAdjustmentNote4.IsAwaitingApprovalFromGovt);

			var task = new InvoicePrintTask(new Configuration(
				arInvoice1, arInvoice2, arInvoice3, arInvoice4,
				aRCreditNote1, aRCreditNote2, aRCreditNote3, aRCreditNote4,
				aRAdjustmentNote1, aRAdjustmentNote2, aRAdjustmentNote3, aRAdjustmentNote4));

			AssertEquals("invoices are grouped in 4 different groups", 4, task.invoicesGroupedByOrg.Count);
			AssertContainsExactElementsInAnyOrder("group with invoices from org1 without watermark", new[] { arInvoice1.PK, aRCreditNote1.PK, aRAdjustmentNote1.PK }, task.invoicesGroupedByOrg[org1.PK.ToStringKey()].Select(x => x.PK).ToArray());
			AssertContainsExactElementsInAnyOrder("group with invoices from org1 with custom watermark", new[] { arInvoice2.PK, aRCreditNote2.PK, aRAdjustmentNote2.PK }, task.invoicesGroupedByOrg[org1.PK.ToStringKey() + "watermark1"].Select(x => x.PK).ToArray());
			AssertContainsExactElementsInAnyOrder("group with invoices from org2 without watermark", new[] { arInvoice3.PK, aRCreditNote3.PK, aRAdjustmentNote3.PK }, task.invoicesGroupedByOrg[org2.PK.ToStringKey()].Select(x => x.PK).ToArray());
			AssertContainsExactElementsInAnyOrder("group with invoices from org2 with custom watermark", new[] { arInvoice4.PK, aRCreditNote4.PK, aRAdjustmentNote4.PK }, task.invoicesGroupedByOrg[org2.PK.ToStringKey() + "watermark1"].Select(x => x.PK).ToArray());

			Assert(task.invoicesGroupedByOrg[org1.PK.ToStringKey()].All(x => x.CustomWatermarkText == null));
			Assert(task.invoicesGroupedByOrg[org1.PK.ToStringKey() + "watermark1"].All(x => x.CustomWatermarkText == "watermark1"));
			Assert(task.invoicesGroupedByOrg[org2.PK.ToStringKey()].All(x => x.CustomWatermarkText == null));
			Assert(task.invoicesGroupedByOrg[org2.PK.ToStringKey() + "watermark1"].All(x => x.CustomWatermarkText == "watermark1"));
		}

		public void TestAddInovicesToPack()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org1.PK;
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org1.PK;

			OrgHeader org2 = objectCreator.CreateOrgHeader("ORG2", false, true, false, false, true, true);
			ARReceipt receipt1 = Factory.New<ARReceipt>();
			receipt1.AH_OH = org2.PK;

			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1, invoice2, creditNote1, receipt1));

			AssertEquals("Should only be one document pack in print task", 1, task.TaskCount);
			AssertNull("Should be none shown error messages", ErrorReporter.LastExceptionReported);
			AssertEquals("Should have 3 documents in the pack", 3, GetDocumentPack(task, 0).Count);
		}

		public void TestAddInovicesToPackPreservesInvoicesOrder()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			OrgHeader org2 = objectCreator.CreateOrgHeader("ORG2", false, true, false, false, true, true);
			OrgHeader org3 = objectCreator.CreateOrgHeader("ORG3", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org2.PK;
			ARInvoice invoice3 = Factory.New<ARInvoice>();
			invoice3.AH_OH = org3.PK;
			ARInvoice invoice4 = Factory.New<ARInvoice>();
			invoice4.AH_OH = org1.PK;
			ARInvoice invoice5 = Factory.New<ARInvoice>();
			invoice5.AH_OH = org2.PK;

			int numTransactions = 5, i;

			Factory.Save();

			TransactionHeader[] invoicesSortedBackward = (from invoice in new TransactionHeader[] { invoice1, invoice2, invoice3, invoice4, invoice5 } orderby invoice.PK descending select invoice).ToArray();

			// Registry Item value should be set to False to prevent grouping by Organisation
			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoicesSortedBackward));

			//we must create a new document pack for each transaction in order to preserve the grid order
			AssertEquals("Should only be five document packs because there is 5 transactions", 5, task.TaskCount);
			AssertNull("Should be none shown error messages", ErrorReporter.LastExceptionReported);
			AssertEquals("Should have 5 transactions to print", 5, task.TransactionsToPrint_ForTestOnly.Length);
			AssertEquals("Should only be five invoice grouuped by Org in the dictionary", 5, task.invoicesGroupedByOrg.Count);

			for (i = 0; i < numTransactions; i++)
			{
				AssertEquals("Should have 1 document in each pack", 1, GetDocumentPack(task, i).Count);
			}

			for (i = 0; i < numTransactions; i++)
			{
				AssertEquals("Should preserve the invoices order in transactions to print", invoicesSortedBackward[i].PK, task.TransactionsToPrint_ForTestOnly[i].PK);
			}

			//check that there is one invoice in each group and that they are indexed my the transaction header PK
			foreach (TransactionHeader txnHeader in invoicesSortedBackward)
			{
				AssertEquals("Should be 1 invoice in each group", 1, task.invoicesGroupedByOrg[txnHeader.PK.ToStringKey()].Count);
			}
		}

		public void TestPrintInvoicesForOneOrgGetAddedToSinglePack()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org1.PK;
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org1.PK;

			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1, invoice2, creditNote1));

			AssertEquals("Should only be one document pack in print task", 1, task.TaskCount);

			AssertEquals("Should have 3 documents in the pack", 3, GetDocumentPack(task, 0).Count);
		}

		public void TestPrintInvoicesForMultipleOrgsOnlyMakesRequiredPacks()
		{
			TestObjectCreator objectCreator = TestObjectCreator;

			OrgHeader org1 = objectCreator.CreateOrgHeader("ORG1", false, true, false, false, true, true);
			OrgHeader org2 = objectCreator.CreateOrgHeader("ORG2", false, true, false, false, true, true);

			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_OH = org1.PK;
			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_OH = org2.PK;

			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice1, invoice2, creditNote1));

			AssertEquals("Should have 2 document packs due to credit note having different organisation", 2, task.TaskCount);

			bool foundOrg1Pack = false;
			bool foundOrg2Pack = false;

			foreach (DocumentPack pack in task.Task_ForTestOnly.GetDocumentPacks())
			{
				if (!foundOrg1Pack)
				{
					foundOrg1Pack = pack.Count == 2;
				}

				if (!foundOrg2Pack)
				{
					foundOrg2Pack = pack.Count == 1;
				}
			}

			Assert("Should have Pack with 2 items for Org 1", foundOrg1Pack);
			Assert("Should have pack with 1 item for Org 2", foundOrg2Pack);
		}

		public void TestPrintInvoiceWithShipmentEntryPrint()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Disbursement);
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Normal invoice - No Entry Print", 1, GetDocumentPack(task, 0).Count);

			CommonShipment shipment = Factory.New<CommonShipment>();
			JobHeader job = SetUpJobHeader(invoice, shipment.PK, JobShipmentSchema.Constants.Prefix, "ABC123");

			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Shipment doesnt have any declaration - no Entry Print", 1, GetDocumentPack(task, 0).Count);

				BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

				BusinessObject cusEntryHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
				cusEntryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

				Factory.Save();

				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Should include Entry Print", 2, GetDocumentPack(task, 0).Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestDcoumentPacksWhenPrintingInvoicesWithOrgsSupportWithConsolidateEmailSubject()
		{
			var org1 = TestObjectCreator.AALSHI;
			var org2 = TestObjectCreator.ABIGAS;
			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv1 with Org1", TestObjectCreator.AUD, 1m, org1);
			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv2 with Org2", TestObjectCreator.AUD, 1m, org1);
			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv3 with Org1", TestObjectCreator.AUD, 1m, org2);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AlwaysGroupInvoicesPrintingForSingleDebtor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var task = new InvoicePrintTask(new Configuration(invoice1, invoice2));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);
			AssertEquals("Eagle Datamation International - BN - AUBNE - Multiple Invoices - AALSHI - Total 2", (task[0]).EmailSubjectForConsolidateReports);

			task = new InvoicePrintTask(new Configuration(invoice3));
			AssertEquals(null, (task[0]).EmailSubjectForConsolidateReports);
		}

		public void TestDcoumentPacksWhenPrintingInvoicesWithOrgs()
		{
			var org1 = TestObjectCreator.AALSHI;
			var org2 = TestObjectCreator.ABIGAS;
			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv1 with Org1", TestObjectCreator.AUD, 1m, org1);
			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv2 with Org2", TestObjectCreator.AUD, 1m, org1);
			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv3 with Org1", TestObjectCreator.AUD, 1m, org2);
			var invoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("Inv4 with Org2", TestObjectCreator.AUD, 1m, org2);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AlwaysGroupInvoicesPrintingForSingleDebtor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var task = new InvoicePrintTask(new Configuration(invoice1, invoice2));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice3, invoice4));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice1, invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			task = new InvoicePrintTask(new Configuration(invoice1, invoice2));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice3, invoice4));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice1, invoice2, invoice3));
			AssertEquals(3, task.invoicesGroupedByOrg.Count);

			AccountingConfigurationRegistry.Instance.AlwaysGroupInvoicesPrintingForSingleDebtor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task = new InvoicePrintTask(new Configuration(invoice1, invoice2));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice3, invoice4));
			AssertEquals(1, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice1, invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			AccountingConfigurationRegistry.Instance.InvoicePrintingGroupByOrganization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			task = new InvoicePrintTask(new Configuration(invoice1, invoice2));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice3, invoice4));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice2, invoice3));
			AssertEquals(2, task.invoicesGroupedByOrg.Count);

			task = new InvoicePrintTask(new Configuration(invoice1, invoice2, invoice3));
			AssertEquals(3, task.invoicesGroupedByOrg.Count);
		}

		public void TestPrintDocBuilderInvoiceWithCustomsEntryPrint()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = organization.PK;

			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Enterprise.Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Enterprise.Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Enterprise.Core.Constants.ChargeType.Disbursement);

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "XXXX";
			declaration.JE_MessageType = "IMP";
			declaration.CustomsEntryHeaders.AddNew();
			invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");

			var invoiceLine = invoice.Lines.AddNew() as InvoiceLine;
			invoiceLine.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			Factory.Save();

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var useNewDocBuilderARInvoice = DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.Value;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
				DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var invoicePrintTask = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Should include Entry Print", 2, GetDocumentPack(invoicePrintTask, 0).Count);

				using (var printTask = invoicePrintTask.Task_ForTestOnly)
				{
					var deliveryInstructions = new DeliveryInstructions();
					AssertNoExceptionThrown("Should be able to run this print task.", () => printTask.Run(deliveryInstructions));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, useNewDocBuilderARInvoice);
			}
		}

		public void TestPrintInvoiceWithCustomsEntryPrint()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Disbursement);
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Normal invoice - No Entry Print", 1, GetDocumentPack(task, 0).Count);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CustomsEntryHeaders.AddNew();
			SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");

			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Should include Entry Print", 2, GetDocumentPack(task, 0).Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestNoPrintInvoiceWithShipmentEntryPrintNonAU()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Disbursement);
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Normal invoice - No Entry Print", 1, GetDocumentPack(task, 0).Count);

			CommonShipment shipment = Factory.New<CommonShipment>();
			JobHeader job = SetUpJobHeader(invoice, shipment.PK, JobShipmentSchema.Constants.Prefix, "ABC123");

			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Shipment doesnt have any declaration - no Entry Print", 1, GetDocumentPack(task, 0).Count);

				BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
				declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

				BusinessObject cusEntryHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
				cusEntryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

				Factory.Save();

				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Should include Entry Print", 1, GetDocumentPack(task, 0).Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestNoPrintInvoiceWithCustomsEntryPrintForNonAU()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Disbursement);
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Normal invoice - No Entry Print", 1, GetDocumentPack(task, 0).Count);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CustomsEntryHeaders.AddNew();
			SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");

			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				task = new InvoicePrintTask(new Configuration(invoice.PK));
				AssertEquals("Should include Entry Print", 1, GetDocumentPack(task, 0).Count);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestPrintInvoiceWithCustomsEntryPrintWhenRegistryIsFalse()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Revenue);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Disbursement);
			Factory.Save();

			InvoicePrintTask task = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Normal invoice - No Entry Print", 1, GetDocumentPack(task, 0).Count);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CustomsEntryHeaders.AddNew();
			SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");

			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			using (ObjectFactory.Get<Enterprise.Integration.Customs.AU.IAUCustomsRegistry>().PrintEntryWhenPrintingInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
					task = new InvoicePrintTask(new Configuration(invoice.PK));
					AssertEquals("Should not include Entry Print", 1, GetDocumentPack(task, 0).Count);
			}
		}

		public void TestGetShipmentEntryPrintCommand()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobHeader header = SetUpJobHeader(invoice, shipment.PK, JobShipmentSchema.Constants.Prefix, "ABC123");
			Factory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			DocumentCommand entryPrintCommand = printTask.GetEntryPrintCommand_ForTestOnly(header);
			AssertEquals("EntryPrint BusinessContext is Shipment", nameof(BusinessContext.Shipment), entryPrintCommand.SU_BusinessContext);
			AssertEquals("EntryPrint Filter contain AU", ZBool.True, entryPrintCommand.SU_FilterList.Contains("AU"));
		}

		public void TestGetCustomsEntryPrintCommand()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			JobHeader header = SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");
			Factory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			DocumentCommand entryPrintCommand = printTask.GetEntryPrintCommand_ForTestOnly(header);
			AssertEquals("EntryPrint BusinessContext is Declaration", nameof(BusinessContext.Customs), entryPrintCommand.SU_BusinessContext);
			AssertEquals("EntryPrint Filter contain AU", ZBool.True, entryPrintCommand.SU_FilterList.Contains("AU"));
		}

		public void TestIndexer()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ARInvoice invoice = Factory.New(typeof(ARInvoice)) as ARInvoice;
			invoice.AH_OH = org.PK;
			SetUpTestInvoice(invoice);
			SetUpInvoiceLinesAndChargeCode(invoice, Core.Constants.ChargeType.Margin);
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			JobHeader header = SetUpJobHeader(invoice, declaration.PK, JobDeclarationSchema.Constants.Prefix, "ABC123");
			Factory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("InvoicePrintTask should contain single document pack", 1, printTask.TaskCount);
			if (printTask.Task_ForTestOnly is DocumentPrintSetWithStreaming)
			{
				AssertNotEquals("DocumentPack returned should be different from expected as PrintStreaming creates new for every enumerating", GetDocumentPack(printTask, 0), printTask[0]);
			}
			else
			{
				AssertEquals("DocumentPack returned differs from expected", GetDocumentPack(printTask, 0), printTask[0]);
			}
		}

		public void TestAddInvoiceToPackCore()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = org.PK;
			Factory.Save();

			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
			var packs = new List<DocumentPack>();
			printTask.AddInvoiceToPackCore_ForTestOnly(aRInv, packs, "Invoice", "Class A Invoice Preprinted");
			AssertEquals("There should be 2 reports in the pack", 2, packs[0].Count);
		}

		public void TestReprintingInvoiceExceptionNotThrown_WhenIsInterCompanyInvoiceImportIsTrue()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.AALSHI, TestObjectCreator.RevenueChargeCode.PK);
				invoice.Factory.Save();

				var expectedExceptionMessage = $@"Reprinting Invoice document is not allowed for some countries and transaction types. It should be prevented before it gets here.
Current company country: {GlbCompany.CurrentCompany.Country.Code}, Transaction country: {invoice.Company?.Country.Code}, Transaction: AR INV, Is in database: True. ";
				Assert("Precondition: invoice.CheckCanPrintPostedInvoicingBase()", invoice.CheckCanPrintPostedInvoicingBase().Result);
				AssertNoExceptionThrown(() => new InvoicePrintTask(new Configuration(invoice) { ShouldForcePrintPostedTransaction = false, ShouldCreateeDocs = false }));

				invoice.AH_InvoicePrinted = true;
				invoice.Factory.Save();
				Assert("Precondition: invoice.CheckCanPrintPostedInvoicingBase()", !invoice.CheckCanPrintPostedInvoicingBase().Result);
				AssertExceptionThrown(typeof(ReprintingInvoiceException), expectedExceptionMessage, () => new InvoicePrintTask(new Configuration(invoice) { ShouldForcePrintPostedTransaction = false, ShouldCreateeDocs = false }));

				AssertNoExceptionThrown(() => new InvoicePrintTask(new Configuration(invoice) { ShouldForcePrintPostedTransaction = true, ShouldCreateeDocs = false }));
			}
		}

		public void TestCreatePacks()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			List<DocumentPack> packs = printTask.CreatePacks_ForTestOnly(invoice, "Class A Invoice Preprinted");
			try
			{
				AssertEquals("Only one pack should be created.", 1, packs.Count);
				AssertEquals("The report should be for Class A Invoice Preprinted.", "Class A Invoice Preprinted", packs[0][0].MenuItem.SU_MenuName);
			}
			finally
			{
				DisposePacks(packs);
			}
		}

		public void TestCreatePacksForProFormaInvoiceHasCorrectReportName()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			printTask.isProFormaInvoice = true;
			List<DocumentPack> packs = printTask.CreatePacks_ForTestOnly(invoice, "DocBuilder Invoice");
			try
			{
				AssertEquals("Only one pack should be created.", 1, packs.Count);
				AssertEquals("The report name should be Pro Forma Invoice", "Pro Forma Invoice", packs[0][0].Name);
			}
			finally
			{
				DisposePacks(packs);
			}
		}

		public void TestCreatePacksForNonProFormaInvoiceHasCorrectReportName()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			printTask.isProFormaInvoice = false;
			List<DocumentPack> packs = printTask.CreatePacks_ForTestOnly(invoice, "DocBuilder Invoice");
			try
			{
				AssertEquals("Only one pack should be created.", 1, packs.Count);
				AssertEquals("The report name should be Invoice", "Invoice", packs[0][0].Name);
			}
			finally
			{
				DisposePacks(packs);
			}
		}

		[ExpectNoExceptions]
		public void TestCreatePacksSetBusinessObjectToLogAgainstToInvoice()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_JH = jobHeader.PK;
			invoice.Job.Parent = shipment;
			Factory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			AssertEquals("Only one pack should be created.", 1, printTask.invoicesGroupedByOrg.Count);
			//				AssertEquals("The report name should be Invoice", "Invoice", printTask.invoicesGroupedByOrg.First().Value[0].Name);
			Assert("Pack's BusinessObjectToLogAgainst should be ARInvoice", printTask.invoicesGroupedByOrg.First().Value[0] is ARInvoice);
		}

		public void TestCreatePacksForProFormaInvoiceDoesNotModifyPivotTitle()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK));
			printTask.isProFormaInvoice = true;
			InvoicePrintCommandManager manager = InvoicePrintCommandManager.New(invoice, "DocBuilder Invoice");
			AssertEquals(1, manager.Command.Documents.ToList().Count(n => ((StmMenuTemplatePivot)n).SI_DocumentTitle.EqualsIgnoringCase("invoice")));
			List<DocumentPack> packs = printTask.CreatePacks_ForTestOnly(invoice, "DocBuilder Invoice");
			try
			{
				AssertEquals(1, manager.Command.Documents.ToList().Count(n => ((StmMenuTemplatePivot)n).SI_DocumentTitle.EqualsIgnoringCase("invoice")));
				AssertEquals("Only one pack should be created.", 1, packs.Count);
				AssertEquals("The report name should be Invoice", "Pro Forma Invoice", packs[0][0].Name);
			}
			finally
			{
				DisposePacks(packs);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatePacksWithProviderPlaceholder_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			EDocsProviderSupporter supporter = ((IEDocsProvider)shipment).GetEDocsProviderSupporter();
			DocumentCommand placeholder = supporter.GetProviderPlaceholder<DocumentCommand>(command) ?? supporter.CreateProviderPlaceholder<DocumentCommand>(command);

			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

			StmMenuMenuPivot pivot = placeholder.ChildMenus.AddNew();
			pivot.SF_SU_Inward = placeholder.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			invoice.Job.Parent = shipment;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			documentFactory.AddFileOrDocument(shipment.PK, null, File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif")), null, eDoc.DocType.RT_DocType, "DEF", false);
			documentFactory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK) { JobParent = shipment });
			try
			{
				int expected = printTask.Task_ForTestOnly is DocumentPrintSetWithStreaming ? 1 : 2; // PrintStreaming can only group Invoices by Org and count groups
				AssertEquals("Number of packs should be created.", expected, printTask.Task_ForTestOnly.Count);
				AssertEquals("The report should be for Invoice.", "Invoice", GetDocumentPack(printTask, 0)[0].MenuItem.SU_MenuName);
				AssertEquals("The 2nd pack should belong to the child menu attached to the placeholder.", childCommand.PK, GetDocumentPack(printTask, 1)[0].MenuItem.PK);
			}
			finally
			{
				if (!(printTask.Task_ForTestOnly is DocumentPrintSetWithStreaming))
				{
					DisposePacks(printTask.Task_ForTestOnly.GetDocumentPacks());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatePacksWithProviderPlaceholder_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			EDocsProviderSupporter supporter = ((IEDocsProvider)shipment).GetEDocsProviderSupporter();
			DocumentCommand placeholder = supporter.GetProviderPlaceholder<DocumentCommand>(command) ?? supporter.CreateProviderPlaceholder<DocumentCommand>(command);

			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

			StmMenuMenuPivot pivot = placeholder.ChildMenus.AddNew();
			pivot.SF_SU_Inward = placeholder.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			invoice.Job.Parent = shipment;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			documentFactory.AddFileOrDocument(shipment.PK, null, File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif")), null, eDoc.DocType.RT_DocType, "DEF", false);
			documentFactory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK) { JobParent = shipment });
			try
			{
				AssertEquals("One pack should be created.", 1, printTask.Task_ForTestOnly.Count);
				AssertEquals("The report should be for Invoice.", "DocBuilder Invoice", GetDocumentPack(printTask, 0)[0].MenuItem.SU_MenuName);
			}
			finally
			{
				DisposePacks(printTask.Task_ForTestOnly.GetDocumentPacks());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatePacksWithProviderPlaceholder_SameOrg_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			EDocsProviderSupporter supporter = ((IEDocsProvider)shipment).GetEDocsProviderSupporter();
			DocumentCommand placeholder = supporter.GetProviderPlaceholder<DocumentCommand>(command) ?? supporter.CreateProviderPlaceholder<DocumentCommand>(command);

			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			childCommand.SU_ContactType = ContactType.Consignee.Code;

			StmMenuMenuPivot pivot = placeholder.ChildMenus.AddNew();
			pivot.SF_SU_Inward = placeholder.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = consignee.PK;
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			invoice.Job.Parent = shipment;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			documentFactory.AddFileOrDocument(shipment.PK, null, File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif")), null, eDoc.DocType.RT_DocType, "DEF", false);
			documentFactory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK) { JobParent = shipment });
			try
			{
				AssertEquals("One pack should be created.", 1, printTask.Task_ForTestOnly.Count);
				AssertEquals("The report should be for Invoice.", "Invoice", GetDocumentPack(printTask, 0)[0].MenuItem.SU_MenuName);
				AssertEquals("The report should be for the child command.", childCommand.PK, GetDocumentPack(printTask, 0)[1].MenuItem.PK);
			}
			finally
			{
				DisposePacks(printTask.Task_ForTestOnly.GetDocumentPacks());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreatePacksWithProviderPlaceholder_SameOrg_DocBuilderInvoice()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			EDocsProviderSupporter supporter = ((IEDocsProvider)shipment).GetEDocsProviderSupporter();
			DocumentCommand placeholder = supporter.GetProviderPlaceholder<DocumentCommand>(command) ?? supporter.CreateProviderPlaceholder<DocumentCommand>(command);

			DocumentCommand childCommand = Factory.New<DocumentCommand>();
			childCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			childCommand.SU_ContactType = ContactType.Consignee.Code;

			StmMenuMenuPivot pivot = placeholder.ChildMenus.AddNew();
			pivot.SF_SU_Inward = placeholder.PK;
			pivot.SF_SU_Outward = childCommand.PK;

			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = childCommand.PK;
			eDoc.SX_RT_DocType = Factory.LoadTop1<RefDocType>(new ZQuery()).PK;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = consignee.PK;
			JobHeader header = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoice.AH_JH = header.PK;
			invoice.Job.Parent = shipment;
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			documentFactory.AddFileOrDocument(shipment.PK, null, File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Squares_100dpi.tif")), null, eDoc.DocType.RT_DocType, "DEF", false);
			documentFactory.Save();

			InvoicePrintTask printTask = new InvoicePrintTask(new Configuration(invoice.PK) { JobParent = shipment });
			try
			{
				AssertEquals("One pack should be created.", 1, printTask.Task_ForTestOnly.Count);
				AssertEquals("The report should be for Invoice.", "DocBuilder Invoice", GetDocumentPack(printTask, 0)[0].MenuItem.SU_MenuName);
			}
			finally
			{
				DisposePacks(printTask.Task_ForTestOnly.GetDocumentPacks());
			}
		}

		public void TestRunSilentlyEmailOnly()
		{
			var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact.OC_ContactName = "Fred";
			contact.OC_Email = "fred@wisetechglobal.com.au";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;
			Factory.Save();

			AssertNoDocumentPack(true);
			AssertNoDocumentPack(false);

			var invoice = CreateARInvoice("001");

			var task = new InvoicePrintTask(new Configuration(invoice));
			AssertEquals("If Invoice.AH_OH has a default contact, then RunSilentlyEmailOnly should return true", RunTaskResult.Success, task.RunSilentlyEmailOnly());
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);

			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertEquals("AALSHI should have no contacts", 0, TestObjectCreator.AALSHI.Contacts.Count);
			InvoicePrintTask task2 = new InvoicePrintTask(new Configuration(invoice));
			AssertEquals("If Invoice.AH_OH does not have a default contact, then RunSilentlyEmailOnly should return false", RunTaskResult.NoRecipient, task2.RunSilentlyEmailOnly());

			invoice = CreateARInvoice("003");
			var task3 = new InvoicePrintTask(new Configuration(invoice) { Factory = Factory });
			AssertEquals("If Invoice.AH_OH has a default contact, then RunSilentlyEmailOnly should return true", RunTaskResult.Success, task3.RunSilentlyEmailOnly(true));
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, invoice.PK));
			AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);
			AssertEquals("print job should be in the database", true, printJobs[0].IsInDatabase);

			invoice = CreateARInvoice("004");
			var task4 = new InvoicePrintTask(new Configuration(invoice) { Factory = Factory });
			AssertEquals("If Invoice.AH_OH has a default contact, then RunSilentlyEmailOnly should return true", RunTaskResult.Success, task4.RunSilentlyEmailOnly(false));
			printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, invoice.PK));
			AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);
			AssertEquals("print job should not be in the database", false, printJobs[0].IsInDatabase);

			ARInvoice CreateARInvoice(ZString transactionNum)
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNum, TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				return arInvoice;
			}
		}

		void AssertNoDocumentPack(bool isSaveInChunks)
		{
			var task = new InvoicePrintTask(new Configuration() { Factory = Factory });
			AssertEquals(RunTaskResult.NoDocumentPack, task.RunSilentlyEmailOnly(isSaveInChunks));

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
		}

		public void TestRunSilentlyEmailOnly_WhenEPrintRecipientHasNoEmail_ShouldRemoveRecipientAndReturnNoRecipient()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("test", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact.OC_ContactName = "EPRContact";
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			doc.OD_DefaultContact = true;
			doc.OD_DeliverBy = Core.Constants.ContactNotifyModes.EPrint;

			Factory.Save();

			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			var ePrintEmailAddress = "";
			var task = new InvoicePrintTask(new Configuration(invoice));
			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				AssertEquals("Should return RunTaskResult.NoRecipient when all Email or EPrint recipients have invalid (null or whitespace) emails.", RunTaskResult.NoRecipient, task.RunSilentlyEmailOnly());
			}

			ePrintEmailAddress = "printer@eprint.com";
			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				AssertEquals("Should return Success when EPrint email is configured.", RunTaskResult.Success, task.RunSilentlyEmailOnly());
			}
		}

		public void TestRunSilentlyEmailOnly_RunLogWalker()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "RNV";
			var trigger = ((IWorkflowProvider)template1).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "DUMMY TASK";
			((Enterprise.Integration.ITriggerConditions)trigger).TriggerEventCode = "ADD";
			trigger.ReferenceCode = "REF";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendARInvoice;

			Factory.Save();

			TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact.OC_ContactName = "CW1";
			contact.OC_Email = "xxx@wisetechglobal.com";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;

			Factory.Save();

			var query = new ZQuery();
			var printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("Should not create a print job before run Log Walker", 0, printJobs.Length);

			MasterFiles.Business.Testing.MasterFilesTestHelper.RunLogWalker();

			printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);
			AssertEquals("print job should be in the database", true, printJobs[0].IsInDatabase);
		}

		public void TestAttachARInvoiceToEdocs_FactorySaveTogether()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
			Factory.Save();

			AssertEquals("AR Invoice should not have any document in EDocs", 0, invoice.DocManagerInfo.AllEDocs.Count);
			AttachARInvoiceToEdocs(invoice, new NotificationBuffer(), CancellationToken.None);

			AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
			var eDoc = invoice.DocManagerInfo.AllEDocs[0];
			AssertEquals("AR Invoice should have invoice attached in EDocs, but not yet saved in database", false, ((BusinessObject)eDoc).IsInDatabase);

			Factory.Save();
			AssertEquals("AR Invoice should have invoice attached in EDocs and saved in database", true, ((BusinessObject)eDoc).IsInDatabase);

			var pdfFileName = string.Format("AR {0} {1}.pdf", invoice.AH_TransactionType, invoice.AH_TransactionNum);
			AssertEquals("AR Invoice should have invoice attached in EDocs", pdfFileName, eDoc.FileName);
		}

		public void TestAttachARInvoiceToEdocs()
		{
			var standAloneARInvoiceNotifications = new NotificationBuffer();
			var standAloneARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
			Factory.Save();
			AssertEquals("AR Invoice should not have any document in EDocs", 0, standAloneARInvoice.DocManagerInfo.AllEDocs.Count);
			using (new DisposableAction(() => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0, () => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0))
			{
				AssertEquals("Non-commercial watermark call count must be 0 in order for the test to proceed", 0, WatermarkHelper.NonCommercialUseWatermarkCallCount);
				AttachARInvoiceToEdocs_ForTestOnly(standAloneARInvoice, standAloneARInvoiceNotifications);
				AssertEquals("Non-commercial watermark call count is expected to be called exactly once for each printed document", 1, WatermarkHelper.NonCommercialUseWatermarkCallCount);
			}
			AssertEquals("AR Invoice should have invoice attached in EDocs", 1, standAloneARInvoice.DocManagerInfo.AllEDocs.Count);
			var pdfFileName = string.Format("AR {0} {1}.pdf", standAloneARInvoice.AH_TransactionType, standAloneARInvoice.AH_TransactionNum);
			AssertEquals("AR Invoice should have invoice attached in EDocs", pdfFileName, standAloneARInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(string.Format("Successfully attached AR {0} {1} to EDocs Tab.", standAloneARInvoice.AH_TransactionType, standAloneARInvoice.AH_TransactionNum), standAloneARInvoiceNotifications.AsString);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var jobInvoicingARInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("10002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var jobInvoicingARInvoiceLine = TestObjectCreator.CreateARInvoiceLine(jobInvoicingARInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC10, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);
			charge.JR_AL_ARLine = jobInvoicingARInvoiceLine.PK;
			Factory.Save();
			var jobInvoicingARInvoiceNotifications = new NotificationBuffer();
			AssertEquals("AR Invoice should not have any document in EDocs", 0, jobInvoicingARInvoice.DocManagerInfo.AllEDocs.Count);
			using (new DisposableAction(() => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0, () => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0))
			{
				AssertEquals("Non-commercial watermark call count must be 0 in order for the test to proceed", 0, WatermarkHelper.NonCommercialUseWatermarkCallCount);
				AttachARInvoiceToEdocs_ForTestOnly(jobInvoicingARInvoice, jobInvoicingARInvoiceNotifications);
				AssertEquals("Non-commercial watermark call count is expected to be called exactly once for each printed document", 1, WatermarkHelper.NonCommercialUseWatermarkCallCount);
			}
			AssertEquals("AR Invoice should have invoice attached in EDocs", 1, jobInvoicingARInvoice.DocManagerInfo.AllEDocs.Count);
			pdfFileName = string.Format("AR {0} {1}.pdf", jobInvoicingARInvoice.AH_TransactionType, jobInvoicingARInvoice.AH_TransactionNum);
			AssertEquals("AR Invoice should have invoice attached in EDocs", pdfFileName, jobInvoicingARInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(string.Format("Successfully attached AR {0} {1} to EDocs Tab.", jobInvoicingARInvoice.AH_TransactionType, jobInvoicingARInvoice.AH_TransactionNum), jobInvoicingARInvoiceNotifications.AsString);
		}

		public void TestAttachARInvoiceToEdocsLanguageFallback()
		{
			var company = GlbCompany.CurrentCompany;
			company.GC_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("COPROX", true, true).PK;

			var branch = GlbBranch.CurrentBranch;
			branch.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("BRPROX", true, true).PK;

			var debtor = TestObjectCreator.CreateOrgHeader("DEBTORG", false, true);
			var address = debtor.MainAddress;
			var contact = debtor.Contacts.AddNew();
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "A/R";
			doc.OD_DeliverBy = "EPR";

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), $"10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, debtor, TestObjectCreator.CC10.PK);

			address.Language = Core.SharedConstants.Languages.Korean;
			branch.OrgProxy.OH_Language = Core.SharedConstants.Languages.German;
			company.OrgProxy.OH_Language = Core.SharedConstants.Languages.Bulgarian;
			contact.OC_Language = Core.SharedConstants.Languages.Arabic;
			debtor.OH_Language = Core.SharedConstants.Languages.Czech;
			Env.Registry.RawRegistry.EnglishSpelling.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.SharedConstants.Languages.EnglishBritish);

			Factory.Save();

			AssertEquals("Precondition: AR Invoice should not have any document in EDocs", 0, invoice.DocManagerInfo.AllEDocs.Count);

			var copies = 0;

			var allCases = new[]
			{
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Address, ExpectedLanguage: address.Language, Source: "debtor organization address"),
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Branch, ExpectedLanguage: branch.OrgProxy.OH_Language, Source: "current branch organization proxy details"),
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Company, ExpectedLanguage: company.OrgProxy.OH_Language, Source: "current company organization proxy details"),
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Contact, ExpectedLanguage: contact.Language, Source: "debtor contact person"),
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.Organization, ExpectedLanguage: debtor.OH_Language, Source: "debtor organization details"),
				(FallbackType: Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System, ExpectedLanguage: (ZString)DataRegistry.Instance.EnglishSpelling, Source: "system English spelling registry")
			};

			foreach (var (fallbackType, expectedLanguage, source) in allCases)
			{
				AssertNotEquals("Precondition: To prevent confusion and false positives, all test cases should be different from the default, EN-US.", Core.SharedConstants.Languages.EnglishAmerican, expectedLanguage);

				var deliveryLanguageFallbacks = new DocumentDeliveryDefaultLanguagesCollection { new DocumentDeliveryDefaultLanguages { Fallback = fallbackType, Order = 1 } };

				using (DocumentsDataRegistry.Instance.DocumentDeliveryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deliveryLanguageFallbacks))
				{
					copies++;
					var notifications = new NotificationBuffer();

					AttachARInvoiceToEdocs_ForTestOnly(invoice, notifications);

					AssertContains($"Attachment events must be reported via notifications.", "Successfully attached", notifications.AsString);
					AssertContains($"With language fallbacks, final preferred language must be reported via notifications.", "Preferred language is", notifications.AsString);
					AssertContains($"Language Fallback '{fallbackType}' must use the language from {source}.", $"Preferred language is {expectedLanguage}.", notifications.AsString);
					AssertEquals($"AR Invoice should have {copies} invoice(s) attached in EDocs", copies, invoice.DocManagerInfo.AllEDocs.Count);
				}
			}
		}

		public void TestAttachARInvoiceToEdocs_CheckCanPrintPostedInvoicingBase()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var standAloneARInvoiceNotifications = new NotificationBuffer();
				var standAloneARInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
				Assert("CanPrint", standAloneARInvoice.CheckCanPrintPostedInvoicingBase().Result);
				Factory.Save();

				AssertEquals("AR Invoice should not have any document in EDocs", 0, standAloneARInvoice.DocManagerInfo.AllEDocs.Count);
				Assert("CanPrint", standAloneARInvoice.CheckCanPrintPostedInvoicingBase().Result);

				Assert("Attaching should be successful", AttachARInvoiceToEdocs_ForTestOnly(standAloneARInvoice, standAloneARInvoiceNotifications));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, standAloneARInvoice.DocManagerInfo.AllEDocs.Count);
				var pdfFileName = string.Format("AR {0} {1}.pdf", standAloneARInvoice.AH_TransactionType, standAloneARInvoice.AH_TransactionNum);
				AssertEquals("AR Invoice should have invoice attached in EDocs", pdfFileName, standAloneARInvoice.DocManagerInfo.AllEDocs[0].FileName);
				AssertContains(string.Format("Successfully attached AR {0} {1} to EDocs Tab.", standAloneARInvoice.AH_TransactionType, standAloneARInvoice.AH_TransactionNum), standAloneARInvoiceNotifications.AsString);

				AssertEquals("CanPrint", false, standAloneARInvoice.CheckCanPrintPostedInvoicingBase().Result);
				Assert("Attaching second Invoice eDoc should fail", !AttachARInvoiceToEdocs_ForTestOnly(standAloneARInvoice, standAloneARInvoiceNotifications));
				AssertEquals("AR Invoice should have original Invoice document attached in EDocs", 1, standAloneARInvoice.DocManagerInfo.AllEDocs.Count);
				AssertEquals("AR Invoice should have original invoice attached in EDocs", pdfFileName, standAloneARInvoice.DocManagerInfo.AllEDocs[0].FileName);
				AssertContains($"Could not attach AR {standAloneARInvoice.AH_TransactionType} {standAloneARInvoice.AH_TransactionNum} to eDocs.\r\nReason:", standAloneARInvoiceNotifications.AsString);

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var jobInvoicingARInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("10002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
				var jobInvoicingARInvoiceLine = TestObjectCreator.CreateARInvoiceLine(jobInvoicingARInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 1000.00m);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC10, "Description", TestObjectCreator.AUD, 1000.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1000.00m, TestObjectCreator.ABIGAS);
				charge.JR_AL_ARLine = jobInvoicingARInvoiceLine.PK;
				Assert("CanPrint", jobInvoicingARInvoice.CheckCanPrintPostedInvoicingBase().Result);
				Factory.Save();

				var jobInvoicingARInvoiceNotifications = new NotificationBuffer();
				AssertEquals("AR Invoice should not have any document in EDocs", 0, jobInvoicingARInvoice.DocManagerInfo.AllEDocs.Count);
				Assert("CanPrint", jobInvoicingARInvoice.CheckCanPrintPostedInvoicingBase().Result);

				Assert("Attaching should be successful", AttachARInvoiceToEdocs_ForTestOnly(jobInvoicingARInvoice, jobInvoicingARInvoiceNotifications));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, jobInvoicingARInvoice.DocManagerInfo.AllEDocs.Count);
				pdfFileName = string.Format("AR {0} {1}.pdf", jobInvoicingARInvoice.AH_TransactionType, jobInvoicingARInvoice.AH_TransactionNum);
				AssertEquals("AR Invoice should have invoice attached in EDocs", pdfFileName, jobInvoicingARInvoice.DocManagerInfo.AllEDocs[0].FileName);
				AssertContains(string.Format("Successfully attached AR {0} {1} to EDocs Tab.", jobInvoicingARInvoice.AH_TransactionType, jobInvoicingARInvoice.AH_TransactionNum), jobInvoicingARInvoiceNotifications.AsString);

				AssertEquals("CanPrint", false, jobInvoicingARInvoice.CheckCanPrintPostedInvoicingBase().Result);
				Assert("Attaching second Invoice eDoc should fail", !AttachARInvoiceToEdocs_ForTestOnly(jobInvoicingARInvoice, jobInvoicingARInvoiceNotifications));
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, jobInvoicingARInvoice.DocManagerInfo.AllEDocs.Count);
				AssertContains($"Could not attach AR {jobInvoicingARInvoice.AH_TransactionType} {jobInvoicingARInvoice.AH_TransactionNum} to eDocs.\r\nReason:", jobInvoicingARInvoiceNotifications.AsString);
			}
		}

		public void TestAttachARInvoiceWithExtraContainersToEdocs()
		{
			AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "USLAX", "CIK1234");
			consol[JobConsolSchema.JK_IsForwarding] = ZBool.True;
			consol[JobConsolSchema.JK_IsCFS] = ZBool.True;

			for (var idx = 0; idx < 10; idx++)
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "ABC00000" + idx.ToString();
			}

			IJobInvoicingPlugIn shipment1 = creator.CreateShipment("SHP1", "AUSYD", "USLAX", consol);
			IJobInvoicingPlugIn shipment2 = creator.CreateShipment("SHP2", "AUSYD", "USLAX", consol);
			Job job1 = creator.CreateJob(shipment1);
			Job job2 = creator.CreateJob(shipment2);
			job1.JH_GE = creator.NonCurrentDepartment.PK;
			job2.JH_GE = creator.NonCurrentDepartment.PK;

			Factory.Save();

			var invoice = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1.0m, creator.AALSHI);
			creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job1, creator.CC1, creator.AUD, 1m, "line 1 desc", 100m);
			creator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job2, creator.CC1, creator.AUD, 1m, "line 2 desc", 200m);
			invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			invoice.AH_JH = ZGuid.Empty;

			creator.CreateCharge(invoice.Lines[0]);
			creator.CreateCharge(invoice.Lines[1]);

			Factory.Save();

			var jobInvoicingARInvoiceNotifications = new NotificationBuffer();
			AssertEquals("AR Invoice should not have any document in EDocs", 0, invoice.DocManagerInfo.AllEDocs.Count);
			using (new DisposableAction(() => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0, () => WatermarkHelper.NonCommercialUseWatermarkCallCount = 0))
			{
				AssertEquals("Non-commercial watermark call count must be 0 in order for the test to proceed", 0, WatermarkHelper.NonCommercialUseWatermarkCallCount);
				AttachARInvoiceToEdocs_ForTestOnly(invoice, jobInvoicingARInvoiceNotifications);
				AssertEquals("Non-commercial watermark call count is expected to be called exactly once for each printed document", 2, WatermarkHelper.NonCommercialUseWatermarkCallCount);
			}
			AssertEquals("AR Invoice and  should have invoice attached in EDocs", 2, invoice.DocManagerInfo.AllEDocs.Count);

			var invoicePdfFileName = string.Format("AR {0} {1}.pdf", invoice.AH_TransactionType, invoice.AH_TransactionNum);
			var containerListPdfFileName = string.Format("AR {0} {1} Container List.pdf", invoice.AH_TransactionType, invoice.AH_TransactionNum);

			var eDocInvoice = invoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.DocType == "INV");
			var eDocpdfContainerList = invoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.DocType == "CLI");

			AssertEquals(invoicePdfFileName, eDocInvoice.FileName);
			AssertEquals("Invoice", eDocInvoice.Description);

			AssertEquals(containerListPdfFileName, eDocpdfContainerList.FileName);
			AssertEquals("Container List", eDocpdfContainerList.Description);

			AssertContains(string.Format("Successfully attached AR {0} {1} Container List to EDocs Tab.", invoice.AH_TransactionType, invoice.AH_TransactionNum), jobInvoicingARInvoiceNotifications.AsString);
			AssertContains(string.Format("Successfully attached AR {0} {1} to EDocs Tab.", invoice.AH_TransactionType, invoice.AH_TransactionNum), jobInvoicingARInvoiceNotifications.AsString);
		}

		public void TestManipulateExcelOperationOnInvoiceWhichHasReportRelatedTypeFilesInItseDocs()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testFileName1 = "StorageFile.txt";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var storageFile = invoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), testFileName1, "ACV", description: "StorageFile type file");
			Assert(storageFile is StorageFile);

			var testFileName2 = "StorageDocs.tif";
			var storageMain = invoice.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(invoice, Core.Constants.DocManagerCodes.ReceivableInvoice);
			var storageDocs = storageMain.AddFileOrDocument(new byte[] { 0, 1, 2, 3 }, testFileName2, "ACV", false, description: "StorageDocs type file");
			Assert(storageDocs is StorageDocs);

			var testFileName3 = "StorageDocs1.tif";
			storageDocs = storageMain.AddFileOrDocument(new byte[] { 0, 1, 2, 3 }, testFileName3, "ACV", false, description: "StorageDocs type file");
			Assert(storageDocs is StorageDocs);
			invoice.DocManagerInfo.Save();

			var command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			var acvDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			command.AddEDoc(acvDocType);

			Factory.Save();

			var invoicePrintTask = new InvoicePrintTask(new Configuration(invoice));

			AssertAttachARInvoiceToEdocs(invoice);
		}

		void AssertAttachARInvoiceToEdocs(InvoicingBase invoice)
		{
			AssertNoExceptionThrown(() => AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
		}

		public void TestResetEDocStatusService()
		{
			var docQuery = new ZQuery(RefDocTypeSchema.RT_DocType, "INV");
			docQuery.AddToFilter(RefDocTypeSchema.RT_ReferenceType, "ALL");
			var docType = Factory.LoadTop1<RefDocType>(docQuery);
			docType.RT_IsPublished = true;
			Factory.Save();

			var notificationBuffer = new NotificationBuffer();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			invoice.AH_TransactionNum = "000020";
			Factory.Save();

			AttachARInvoiceToEdocs(invoice, notificationBuffer, CancellationToken.None);
			Factory.Save();
			AssertEquals(1, invoice.DocManagerInfo.AllEDocs.Count);

			var originalEDoc = invoice.DocManagerInfo.AllEDocs[0] as StorageDocsBase;
			AssertEquals(true, originalEDoc.IsInDatabase);
			AssertEquals(true, originalEDoc.SC_IsPublished);

			AttachARInvoiceToEdocs(invoice, notificationBuffer, CancellationToken.None);
			var publishedEDocs = invoice.DocManagerInfo.AllEDocs.OfType<StorageDocsBase>().Where(x => x.SC_IsPublished);
			AssertEquals(2, publishedEDocs.Count());
			AssertEquals(2, invoice.DocManagerInfo.AllEDocs.Count);
			Factory.Save();

			Factory.ServiceContainer.AddService(new ResetEDocStatusService());
			AttachARInvoiceToEdocs(invoice, notificationBuffer, CancellationToken.None);
			Factory.Save();

			var allEDocs = invoice.DocManagerInfo.AllEDocs.OfType<StorageDocsBase>();
			AssertEquals(3, allEDocs.Count());
			AssertEquals(true, allEDocs.Single(x => x.SC_FileName == "AR INV 000020[3]").SC_IsPublished);
			AssertEquals(false, allEDocs.Single(x => x.SC_FileName == "AR INV 000020[2]").SC_IsPublished);
			AssertEquals(false, allEDocs.Single(x => x.SC_FileName == "AR INV 000020").SC_IsPublished);

			AssertEquals(false, notificationBuffer.HasWarnings);
			AssertEquals(false, notificationBuffer.HasErrors);
		}

		public void TestGetFileNamesAndTheirContents()
		{
			using (AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertGetFileNamesAndTheirContents("C001", "SHP1", 4, "00001001", InvoiceTypesList.Codes.DestinationChargesInvoice, false);
				AssertGetFileNamesAndTheirContents("C002", "SHP2", 5, "00001002", InvoiceTypesList.Codes.DestinationChargesInvoice_Batching, false);
				AssertGetFileNamesAndTheirContents("C003", "SHP3", 6, "00001003", InvoiceTypesList.Codes.DestinationChargesInvoice, true);
			}

			void AssertGetFileNamesAndTheirContents(string consolNum, string shipmentNumber, int containers, string transactionNumber, string transactionCategory, bool invoiceRelatedToShipment)
			{
				var shouldCreateContainerListDoc = containers >= 5;
				var shouldCreatePeriodicInvoiceDoc = transactionCategory == InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;

				var consol = TestObjectCreator.CreateConsol(consolNum: consolNum);
				var shipment = TestObjectCreator.CreateShipment(shipmentNumber, "AUSYD", "USLAX", consol);
				for (var idx = 0; idx < containers; idx++)
				{
					var container = consol.Containers.AddNew();
					container.JC_ContainerNum = $"ABC00000{idx.ToString()}";
					var packLine = (PackLine)shipment.OuterPackLines.AddNew();
					container.PackLines.Add(packLine);
				}

				var job = TestObjectCreator.CreateJob(shipment);
				Factory.Save();

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNumber, TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
				invoice.AH_TransactionCategory = transactionCategory;
				if (invoiceRelatedToShipment)
				{
					invoice.AH_JH = job.PK;
				}
				TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Revenue, invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "line 1 desc", 100m);
				TestObjectCreator.CreateCharge(invoice.Lines[0]);
				invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

				var invoicePrintTask = new InvoicePrintTask(new Configuration(invoice) { ShouldForcePrintPostedTransaction = true, ShouldCreateeDocs = false });
				var result = invoicePrintTask.GetFileNamesAndTheirContents("Invoice 001", "Invoice 001 Container Details", "Invoice 001 Periodic Details");
				var expectedResultCount = 1;

				var invoiceDoc = result.FirstOrDefault(x => x.FileName == "Invoice 001");
				AssertNotNull(invoiceDoc);
				AssertNotNull(invoiceDoc.Content);
				AssertEquals("INV", invoiceDoc.DocumentType);

				if (shouldCreateContainerListDoc)
				{
					var containerListDoc = result.FirstOrDefault(x => x.FileName == "Invoice 001 Container Details");
					AssertNotNull(containerListDoc);
					AssertNotNull(containerListDoc.Content);
					AssertEquals(invoiceRelatedToShipment ? "INV" : "CLI", containerListDoc.DocumentType);
					expectedResultCount++;
				}

				if (shouldCreatePeriodicInvoiceDoc)
				{
					var periodicInvoiceDoc = result.FirstOrDefault(x => x.FileName == "Invoice 001 Periodic Details");
					AssertNotNull(periodicInvoiceDoc);
					AssertNotNull(periodicInvoiceDoc.Content);
					AssertEquals("INV", periodicInvoiceDoc.DocumentType);
					expectedResultCount++;
				}

				AssertEquals("Result Count", expectedResultCount, result.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndAttachInvoicePdf()
		{
			var content = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\ExcelComparator\ExcelComparator.Test\TestFiles\File1.xls"));
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			AssertEquals("AR Invoice should not have any document in EDocs", 0, invoice.DocManagerInfo.AllEDocs.Count);
			GenerateAndAttachInvoicePdf(invoice, $"Invoice001", content, "CLI");
			AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);

			var eDocInvoice = invoice.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName == "Invoice001.pdf");
			AssertNotNull(eDocInvoice);
			AssertEquals("Container List", eDocInvoice.Description);
			AssertEquals("CLI", eDocInvoice.DocType);
			AssertEquals(true, eDocInvoice.IsSystemGenerated);
		}

		public void TestRunToStreamExcelOnly()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			var task = new InvoicePrintTask(new Configuration(invoice.PK) { ShouldCreateeDocs = false });
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			task.RunToStreamExcelOnly();
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			task = new InvoicePrintTask(new Configuration(invoice.PK));
			task.RunToStreamExcelOnly();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("shouldCreateeDocsIsTrueWhenRunToStreamExcelOnly", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestRunSilentlyEmailOnly_ContactEmailIsEmptyWhenDeliveryMethodIsEmail()
		{
			ErrorReporter.Clear();

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "RNV";
			var trigger = ((IWorkflowProvider)template1).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send AR Invoice";
			((Enterprise.Integration.ITriggerConditions)trigger).TriggerEventCode = "ADD";
			trigger.ReferenceCode = "REF";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendARInvoice;

			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			var contact1 = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact1.OC_ContactName = "Test Name1";
			contact1.OC_Email = "testname1@wisetechglobal.com";
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.Receivables.Code;

			var contact2 = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact2.OC_ContactName = "Test Name2";
			contact2.OC_Email = "";
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Receivables.Code;

			Factory.Save();

			var task = new InvoicePrintTask(new Configuration(invoice));
			task.RunSilentlyEmailOnly();

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertNotEquals("ContactEmailIsEmpty_WhenDeliveryMethodIsEmail", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestRunSilentlyEmailOnly_InstructionsRecipientsIsEmpty()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "RNV";
			var trigger = ((IWorkflowProvider)template1).WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send AR Invoice";
			((Enterprise.Integration.ITriggerConditions)trigger).TriggerEventCode = "ADD";
			trigger.ReferenceCode = "REF";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendARInvoice;

			Factory.Save();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			var contact1 = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact1.OC_ContactName = "Test Name1";
			contact1.OC_Email = "";
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.Receivables.Code;

			var contact2 = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact2.OC_ContactName = "Test Name2";
			contact2.OC_Email = "";
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Receivables.Code;

			Factory.Save();

			var task = new InvoicePrintTask(new Configuration(invoice));
			var result = task.RunSilentlyEmailOnly();

			AssertEquals(RunTaskResult.NoRecipient, result);
		}

		public void TestNullTransactionThrowsArgumentNullException()
		{
			var creator = new TestObjectCreator(Factory);
			var invoice1 = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1.0m);
			var invoice2 = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 2.0m);

			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>("Null transactions should throw in constructor", () => new InvoicePrintTask(new Configuration(null)));

			AssertExceptionThrown<ArgumentNullException>("Null transactions in array should throw in constructor", () => new InvoicePrintTask(new Configuration([null])));
			AssertExceptionThrown<ArgumentNullException>("Null transactions may not hide amongst real transactions", () => new InvoicePrintTask(new Configuration(invoice1, null, invoice2)));
		}

		[TestDate(2024, 11, 08)]
		public void TestAttachARInvoiceToEdocs_ShouldAddDDAEvent()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
			Factory.Save();

			Assert("AR Invoice should not have any DDA event log", !invoice.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code));

			AttachARInvoiceToEdocs(invoice, new NotificationBuffer(), CancellationToken.None);

			AssertEquals("AR Invoice should have one DDA event log", 1, invoice.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code));

			var log = invoice.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code);
			var eDoc = invoice.DocManagerInfo.AllEDocs[0] as StorageDocsBase;
			var exceptedLog = "INV|EAGLE DATAMATION INTERNATIONAL - BN - AUBNE - INVOICE 00001000 AALSHI (08-Nov-24)";
			AssertEquals(exceptedLog, log.SL_ReferenceForBinding);
			AssertEquals(exceptedLog + "|" + eDoc.PK, log.SL_Reference);

			Assert("DDA event log was not saved", !log.IsInDatabase);

			Factory.Save();
			Assert("DDA event log has been saved", log.IsInDatabase);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndAttachInvoicePdf_AddDDAEvent()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", TestObjectCreator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, TestObjectCreator.AALSHI, TestObjectCreator.CC10.PK);
			Factory.Save();

			Assert("AR Invoice should not have any DDA event log", !invoice.Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code));

			var content = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\ExcelComparator\ExcelComparator.Test\TestFiles\File1.xls"));
			GenerateAndAttachInvoicePdf(invoice, "test", content, "INV", addDDA: true);

			AssertEquals("AR Invoice should have one DDA event log", 1, invoice.Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code));

			var log = invoice.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code);
			var eDoc = invoice.DocManagerInfo.AllEDocs[0] as StorageDocsBase;
			AssertEquals("INV", log.SL_ReferenceForBinding);
			AssertEquals("INV|" + eDoc.PK, log.SL_Reference);

			Assert("DDA event log was not saved", !log.IsInDatabase);

			Factory.Save();
			Assert("DDA event log has been saved", log.IsInDatabase);
		}

		#region Implementation

		protected void SetUpTestInvoice(Invoice invoiceToSet)
		{
			invoiceToSet.AH_PostDate = Env.Time.CurrentLocalDateTime;
			invoiceToSet.AH_InvoiceDate = Env.Time.CurrentLocalDateTime;
			invoiceToSet.AH_DueDate = Env.Time.CurrentLocalDateTime;

			invoiceToSet.AH_TransactionNum = TestObjectCreator.GetRandomString(15);
			invoiceToSet.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceToSet.AH_GE = GlbDepartment.CurrentDepartment.PK;

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			invoiceToSet.AH_JH = job.PK;
		}

		protected void SetUpInvoiceLinesAndChargeCode(Invoice invoice, ZString chargeType)
		{
			InvoiceLine line = (InvoiceLine)invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.GetChargeCode(chargeType).PK;
		}

		protected JobHeader SetUpJobHeader(Invoice invoice, ZGuid foreignKey, ZString parentTableCode, ZString jobNum)
		{
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = foreignKey;
			header.JH_ParentTableCode = parentTableCode;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_JobNum = jobNum;
			invoice.AH_JH = header.PK;
			return header;
		}

		protected virtual InvoicePrintTask GetPrintTask(ZGuid invoicePK)
		{
			return new InvoicePrintTask(new Configuration(invoicePK));
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		void DisposePacks(IEnumerable<DocumentPack> packs)
		{
			foreach (DocumentPack pack in packs)
			{
				pack.Dispose();
			}
		}

		protected virtual DocumentPack GetDocumentPack(InvoicePrintTask task, int index)
		{
			return task.Task_ForTestOnly[index];
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(DocumentPseudoApplicator))]
	internal sealed class DocumentPseudoApplicatorTest : PseudoApplicatorTest<DocumentPseudoApplicator>
	{
		public void TestPrintDocumentsWithOverridePrintDetailsWhenCancelDeliveryForm()
		{
			var query = new DocumentZQuery(BusinessContext.Shipment, "Delivery Order");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Arrival");
			var command = Factory.LoadTop1<DocumentCommand>(query);
			command.SU_IsDocPack = true;
			var childCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			childCommand.SU_ContactType = ContactType.Consignee.Code;
			childCommand.SU_FilterList = "";
			childCommand.SU_PreventAutoDelivery = false;
			var pivot = command.ChildMenus.AddNew();
			pivot.SF_SU_Inward = command.PK;
			pivot.SF_SU_Outward = childCommand.PK;
			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";
			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";
			var stmPrintQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue3.SQ_DisplayName = "Printer 3";
			stmPrintQueue3.SQ_ServerName = "TEST3";
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("S00000100", consigneeEmail);
			var shipment2 = TestHelper.NewShipmentWithConsignee("S00000101", consigneeEmail);
			var shipment3 = TestHelper.NewShipmentWithConsignee("S00000102", consigneeEmail, Core.Constants.TransportModes.Air);
			Factory.Save();
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK, shipment3.PK } };
			var runner = new OperationalActionRunner(Action, shipment1.GetType(), selectedRecords)
			{ Printer = stmPrintQueue1.PK, BulkDeliveryMethod = ForcePrinterBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			var applicators = runner.MethodApplicators.GetApplicators(OperationalActionMethodUIMode.All).OfType<DocumentPseudoApplicator>().FirstOrDefault();
			if (applicators != null)
			{
				((DeliverDocumentsProcessor)applicators.DocumentProcessor).BeginRunningPrintSet += (_, arg) =>
				{
					arg.Instructions.DocumentsToBeDelivered.OfType<IDeliverable>().ForEach(x =>
					{
						if (arg.DataSourcePK == shipment1.PK)
						{
							if (x.Name == "COPY")
							{
								x.IncludedInPrint = false;
							}
							else
							{
								x.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
								x.PrinterDetails.NumberOfCopies = 10;
							}
						}
						else if (arg.DataSourcePK == shipment3.PK)
						{
							x.PrinterDetails.PrintQueuePK = stmPrintQueue3.PK;
							x.PrinterDetails.NumberOfCopies = 20;
						}
					});
				};
			}

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(false);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				runner.Run(dummyLog, factoryForChanges);
				var printJobs1 = TestHelper.PrintJobs(shipment1.PK);
				AssertEquals(0, printJobs1.Length);
				var printJobs2 = TestHelper.PrintJobs(shipment2.PK);
				AssertEquals(0, printJobs2.Length);
				var printJobs3 = TestHelper.PrintJobs(shipment3.PK);
				AssertEquals(0, printJobs3.Length);
			}

			mockPrintTaskUIProvider.VerifyAll();
		}

		public void TestPrintDocumentsWithOverridePrintDetails()
		{
			var query = new DocumentZQuery(BusinessContext.Shipment, "Delivery Order");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "Arrival");
			var command = Factory.LoadTop1<DocumentCommand>(query);
			command.SU_IsDocPack = true;
			var childCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			childCommand.SU_ContactType = ContactType.Consignee.Code;
			childCommand.SU_FilterList = "";
			childCommand.SU_PreventAutoDelivery = false;
			var pivot = command.ChildMenus.AddNew();
			pivot.SF_SU_Inward = command.PK;
			pivot.SF_SU_Outward = childCommand.PK;
			var stmPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue1.SQ_DisplayName = "Printer 1";
			stmPrintQueue1.SQ_ServerName = "TEST1";
			var stmPrintQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue2.SQ_DisplayName = "Printer 2";
			stmPrintQueue2.SQ_ServerName = "TEST2";
			var stmPrintQueue3 = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue3.SQ_DisplayName = "Printer 3";
			stmPrintQueue3.SQ_ServerName = "TEST3";
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("S00000100", consigneeEmail);
			var shipment2 = TestHelper.NewShipmentWithConsignee("S00000101", consigneeEmail);
			var shipment3 = TestHelper.NewShipmentWithConsignee("S00000102", consigneeEmail, Core.Constants.TransportModes.Air);
			Factory.Save();
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK, shipment3.PK } };
			var runner = new OperationalActionRunner(Action, shipment1.GetType(), selectedRecords)
			{ Printer = stmPrintQueue1.PK, BulkDeliveryMethod = ForcePrinterBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			var applicators = runner.MethodApplicators.GetApplicators(OperationalActionMethodUIMode.All).OfType<DocumentPseudoApplicator>().FirstOrDefault();
			if (applicators != null)
			{
				((DeliverDocumentsProcessor)applicators.DocumentProcessor).BeginRunningPrintSet += (_, arg) =>
				{
					arg.Instructions.DocumentsToBeDelivered.OfType<IDeliverable>().ForEach(x =>
					{
						if (arg.DataSourcePK == shipment1.PK)
						{
							if (x.Name == "COPY")
							{
								x.IncludedInPrint = false;
							}
							else
							{
								x.PrinterDetails.PrintQueuePK = stmPrintQueue2.PK;
								x.PrinterDetails.NumberOfCopies = 10;
							}
						}
						else if (arg.DataSourcePK == shipment3.PK)
						{
							x.PrinterDetails.PrintQueuePK = stmPrintQueue3.PK;
							x.PrinterDetails.NumberOfCopies = 20;
						}
					});
				};
			}

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				runner.Run(dummyLog, factoryForChanges);
			}

			var printJobsForShipment1 = TestHelper.PrintJobs(shipment1.PK);
			AssertEquals(1, printJobsForShipment1.Length);
			AssertEquals(stmPrintQueue2.PK, printJobsForShipment1[0].SP_SQ);
			AssertEquals(10, (int)printJobsForShipment1[0].SP_Copies);
			var printJobsForShipment2 = TestHelper.PrintJobs(shipment2.PK);
			AssertEquals(1, printJobsForShipment2.Length);
			AssertEquals(stmPrintQueue2.PK, printJobsForShipment2[0].SP_SQ);
			AssertEquals(10, (int)printJobsForShipment2[0].SP_Copies);
			var printJobForShipment3 = TestHelper.PrintJobs(shipment3.PK);
			AssertEquals(1, printJobsForShipment2.Length);
			AssertEquals(stmPrintQueue3.PK, printJobForShipment3[0].SP_SQ);
			AssertEquals(20, (int)printJobForShipment3[0].SP_Copies);
			mockPrintTaskUIProvider.VerifyAll();
		}

		public void TestPrintDocumentWithDraftMode()
		{
			using (MockProductEnvironment())
			{
				var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
				command.SU_ContactType = ContactType.Consignee.Code;
				command.SU_DraftOption = DraftOptionsList.Codes.Draft;
				var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
				var shipment = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
				shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
				shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;
				Factory.Save();
				Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
				var selectedRecords = new SelectedRecords()
				{ PrimaryKeys = new[] { shipment.PK } };
				var runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords)
				{ Printer = ZGuid.NewZGuid() };
				var dummyLog = new DummyOperationalActionLog();
				var factoryForChanges = new BusinessObjectFactory();
				runner.Run(dummyLog, factoryForChanges);
				AssertEquals("should have printed 1 document", 1, TestHelper.CountPrintJobs(shipment.PK));
				var printJob = TestHelper.PrintJobs(shipment.PK)[0];
				AssertEquals("DRAFT", printJob.SP_WatermarkText);
			}

			IDisposable MockProductEnvironment()
			{
				var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var savedDbType = keyForTest.DatabaseTypeForTest;
				keyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				return new DisposableAction(() =>
				{
					keyForTest.DatabaseTypeForTest = savedDbType;
				});
			}
		}

		public void TestDocDataIsUsed()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Bill Of Lading' for 'Shipment S00000100'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			var printerPK = ZGuid.NewZGuid();
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
			shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;
			using (var note = DocumentNote.LoadNote((IStmNoteParent)shipment))
			{
				((FilterFieldValueSerialisable)note.UserDefinedFieldList["Consignee - Importer"]).ValueAsStringForSerialisation = "Overridden Consignee XXX";
				Factory.Save();
			}

			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			command.SU_ContactType = ContactType.Consignee.Code;
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			Factory.Save();
			AssertEquals("precondition:", 0, TestHelper.CountPrintJobs(shipment.PK));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			var runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.Printer = printerPK;
			RunRunner(runner, expectedLog);
			AssertEquals("should have printed 1 document", 1, TestHelper.CountPrintJobs(shipment.PK));
			var printJob = TestHelper.PrintJobs(shipment.PK)[0];
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				var workSheet = excelInterface.WorkSheets[0];
				AssertContains("Overridden Consignee XXX", workSheet.ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestDocNotExists()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "WARNING: Unable to find the target file with type as Enterprise.Freight.Forwarding.Business.ForwardingShipment and PK as 00000000-0000-0000-0000-000000000000\n" + "";
			ZGuid printerPK = ZGuid.NewZGuid();
			ZGuid emptyPK = ZGuid.Empty;
			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { emptyPK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.Printer = printerPK;
			RunRunner(runner, expectedLog);
		}

		public void TestWarningForCreditOnHold()
		{
			ZGuid printerPK = ZGuid.NewZGuid();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "consignee on hold";
			org.OH_Code = "~org1~";
			org.MainAddress.OA_Address1 = "zzz";
			org.OH_IsDebtor = true;
			org.CompanyData.OB_AROnCreditHold = true;
			BusinessObject shipment = TestHelper.NewShipmentWithConsignee("Shipment1", org);
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.Printer = printerPK;
			string expected = @"INFO: Starting Section: Documents ...
WARNING: Delivery of this document is restricted because:
       The Consignee, Consignor, Local Client or any Debtors for charges on the Billing tab
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold. (Shipment Shipment1, Shipment : Bill Of Lading : Departure/Bill Of Lading : FES)";
			RunRunner(runner, expected, false);
			AssertEquals("should not have printed any document", 0, TestHelper.CountPrintJobs(shipment.PK));
		}

		public void TestPrintDocuments()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Bill Of Lading' for 'Shipment S00000100'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			ZGuid printerPK = ZGuid.NewZGuid();
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
			shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;
			Factory.Save();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Bill Of Lading"));
			command.SU_ContactType = ContactType.Consignee.Code;
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			Factory.Save();
			AssertEquals("precondition:", 0, TestHelper.CountPrintJobs(shipment.PK));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.Printer = printerPK;
			RunRunner(runner, expectedLog);
			AssertEquals("should have printed 1 document", 1, TestHelper.CountPrintJobs(shipment.PK));
		}

		public void TestPrintDocuments_BulkDeliveryMethod()
		{
			const string expectedLog1 = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment FAX'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment PRINT'\n" + "WARNING: There are no contacts found that can be used to deliver 'Arrival Notice'\n" + "('Shipment PRINT')\n" + "";
			const string expectedLog2 = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment FAX'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment PRINT'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			OrgHeader consigneeFax = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Fax);
			OrgHeader consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			OrgHeader consigneePrint = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Print);
			BusinessObject shipmentFax = TestHelper.NewShipmentWithConsignee("FAX", consigneeFax);
			BusinessObject shipmentEmail = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
			BusinessObject shipmentPrint = TestHelper.NewShipmentWithConsignee("PRINT", consigneePrint);
			Factory.Save();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			AssertEquals("precondition: shipmentFax", 0, TestHelper.CountPrintJobs(shipmentFax.PK));
			AssertEquals("precondition: shipmentPrint", 0, TestHelper.CountPrintJobs(shipmentPrint.PK));
			AssertEquals("precondition: shipmentEmail", 0, TestHelper.CountPrintJobs(shipmentEmail.PK));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipmentEmail.PK, shipmentFax.PK, shipmentPrint.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipmentEmail.GetType(), selectedRecords);
			runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
			RunRunner(runner, expectedLog1);
			AssertEquals("shipmentFax", 1, TestHelper.CountPrintJobs(shipmentFax.PK));
			AssertEquals("shipmentPrint", 0, TestHelper.CountPrintJobs(shipmentPrint.PK));
			AssertEquals("shipmentEmail", 1, TestHelper.CountPrintJobs(shipmentEmail.PK));
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			runner.Printer = printer.PK;
			RunRunner(runner, expectedLog2);
			AssertEquals("shipmentFax", 2, TestHelper.CountPrintJobs(shipmentFax.PK));
			AssertEquals("shipmentPrint", 1, TestHelper.CountPrintJobs(shipmentPrint.PK));
			AssertEquals("shipmentEmail", 2, TestHelper.CountPrintJobs(shipmentEmail.PK));
		}

		public void TestPrintDocuments_NoEmailToRecipientsFound()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL'\n" + "WARNING: There is no Email Recipient found when delivering document 'Arrival Notice'\n" + "('Shipment EMAIL')";
			var printer = Factory.New<StmPrintQueue>();
			var consignee = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee("EMAIL", consignee);
			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			command.SU_ContactType = ContactType.All.Code;
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			var runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			runner.Printer = printer.PK;
			RunRunner(runner, expectedLog);
			AssertEquals("shipmentPrint", 0, TestHelper.CountPrintJobs(shipment.PK));
		}

		public void TestPrintDocuments_EmailRelatedBroker()
		{
			const string expectedLog1 = @"INFO: Starting Section: Documents ...
DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL1'
DEBUG: ... found 1 contact(s).
DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL2'
DEBUG: ... found 1 contact(s).";
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			OrgHeader consigneeA = TestHelper.NewConsigneeWithContactAndBroker(Core.Constants.ContactNotifyModes.Email, "TestPrintDocuments_EmailRelatedBroker@a.com");
			OrgHeader consigneeB = TestHelper.NewConsigneeWithContactAndBroker(Core.Constants.ContactNotifyModes.Email, "TestPrintDocuments_EmailRelatedBroker@b.com");
			BusinessObject shipmentA = TestHelper.NewShipmentWithConsignee("EMAIL1", consigneeA);
			BusinessObject shipmentB = TestHelper.NewShipmentWithConsignee("EMAIL2", consigneeB);
			Factory.Save();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmPrintJobCopyRecipient JOIN dbo.StmPrintJob on SPR_SP = SP_PK WHERE SPR_EmailAddress = 'TestPrintDocuments_EmailRelatedBroker@a.com'"));
			AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmPrintJobCopyRecipient JOIN dbo.StmPrintJob on SPR_SP = SP_PK WHERE SPR_EmailAddress = 'TestPrintDocuments_EmailRelatedBroker@b.com'"));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipmentA.PK, shipmentB.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipmentA.GetType(), selectedRecords);
			runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
			runner.Printer = printer.PK;
			RunRunner(runner, expectedLog1);
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmPrintJobCopyRecipient JOIN dbo.StmPrintJob on SPR_SP = SP_PK WHERE SPR_EmailAddress = 'TestPrintDocuments_EmailRelatedBroker@a.com'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.StmPrintJobCopyRecipient JOIN dbo.StmPrintJob on SPR_SP = SP_PK WHERE SPR_EmailAddress = 'TestPrintDocuments_EmailRelatedBroker@b.com'"));
		}

		public void TestPrintDocuments_PrintSequence()
		{
			const string expectedLog1 = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment PRINT1'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Delay Alert' for 'Shipment PRINT1'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment PRINT2'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Delay Alert' for 'Shipment PRINT2'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			OrgHeader consignee1 = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Print);
			OrgHeader consignee2 = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Print);
			BusinessObject shipment1 = TestHelper.NewShipmentWithConsignee("PRINT1", consignee1);
			BusinessObject shipment2 = TestHelper.NewShipmentWithConsignee("PRINT2", consignee2);
			Dictionary<ZGuid, string> map = new Dictionary<ZGuid, string>();
			map[shipment1.PK] = "Shipment1";
			map[shipment2.PK] = "Shipment2";
			Factory.Save();
			DocumentCommand command1 = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			DocumentCommand command2 = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"));
			OperationalActionDocumentPivot pivot1 = Action.DocumentPivots.AddNew();
			pivot1.SF_SU_Outward = command1.PK;
			pivot1.SF_Index = 1;
			OperationalActionDocumentPivot pivot2 = Action.DocumentPivots.AddNew();
			pivot2.SF_SU_Outward = command2.PK;
			pivot2.SF_Index = 2;
			AssertEquals("precondition: shipmentPrint", 0, TestHelper.CountPrintJobs(shipment1.PK, shipment1.PK));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipment1.GetType(), selectedRecords);
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			runner.Printer = printer.PK;
			RunRunner(runner, expectedLog1);
			StmPrintJob[] jobs = TestHelper.PrintJobs(shipment1.PK, shipment2.PK);
			Array.Sort(jobs, (j1, j2) => j1.SP_Sequence - j2.SP_Sequence);
			int offset = jobs.Length > 0 ? 1 - jobs[0].SP_Sequence : 0;
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			foreach (StmPrintJob job in jobs)
			{
				string label;
				builder.Append(job.SP_Sequence + offset);
				builder.Append("|");
				if (map.TryGetValue(job.SP_ParentGuid, out label))
				{
					builder.Append(label);
				}
				else
				{
					builder.Append(job.SP_ParentGuid);
				}

				builder.Append("|");
				builder.AppendLine(job.SP_DocumentType);
			}

			const string expected = @"
1|Shipment1|ARN
2|Shipment1|DAL
3|Shipment2|ARN
4|Shipment2|DAL
";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestPrintDocuments_CoverNote()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment EMAIL'\n" + "DEBUG: ... found 1 contact(s).\n" + "DEBUG: Delivering 'Arrival Notice' for 'Shipment PRINT'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			OrgHeader consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			OrgHeader consigneePrint = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Print);
			BusinessObject shipmentEmail = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
			BusinessObject shipmentPrint = TestHelper.NewShipmentWithConsignee("PRINT", consigneePrint);
			Factory.Save();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			CombineAssertions(delegate
			{
				AssertEquals("precondition: shipmentPrint", 0, TestHelper.CountPrintJobs(shipmentPrint.PK));
				AssertEquals("precondition: shipmentEmail", 0, TestHelper.CountPrintJobs(shipmentEmail.PK));
			});
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipmentEmail.PK, shipmentPrint.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, shipmentEmail.GetType(), selectedRecords);
			runner.BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			runner.Printer = printer.PK;
			runner.IncludeCoverNote = true;
			runner.CoverNoteText = "Cover Note";
			RunRunner(runner, expectedLog);
			CombineAssertions(delegate
			{
				AssertEquals("shipmentPrint", 1, TestHelper.CountPrintJobs(shipmentPrint.PK));
				AssertEquals("shipmentEmail", 1, TestHelper.CountPrintJobs(shipmentEmail.PK));
			});
		}

		[ExpectNoExceptions]
		[TestDate(2013, 04, 02, 10, 53, 0, 0)]
		public void TestPrintingCartageAdviceThrowNoException()
		{
			const string expectedLog = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Cartage Advice' for 'Declaration S00000100'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			ZGuid printerPK = ZGuid.NewZGuid();
			BusinessObject aUDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee("EMAIL", consigneeEmail);
			shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;
			var jobDeclaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			jobDeclaration.JE_OH_Importer = consigneeEmail.PK;
			jobDeclaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			Factory.Save();
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Customs, "Cartage Advice").AddToFilter(StmMenuItemSchema.SU_DocumentDirection, "DEP"));
			command.SU_ContactType = ContactType.Consignee.Code;
			Action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			if (command.ChildMenus.Count > 0)
			{
				var childCommand = Factory.Load<DocumentCommand>(command.ChildMenus[0].SF_SU_Outward);
				childCommand.SU_BusinessContext = "Customs";
			}

			Factory.Save();
			AssertEquals("precondition:", 0, TestHelper.CountPrintJobs(shipment.PK));
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { jobDeclaration.PK } };
			OperationalActionRunner runner = new OperationalActionRunner(Action, jobDeclaration.GetType(), selectedRecords);
			runner.Printer = printerPK;
			runner.BulkDeliveryMethod = "Only Electronic";
			RunRunner(runner, expectedLog);
			AssertEquals("should have printed 1 document", 1, TestHelper.CountPrintJobs(shipment.PK));
			const string expectedLog2 = "INFO: Starting Section: Documents ...\n" + "DEBUG: Delivering 'Cartage Advice' for 'Shipment S00000100'\n" + "DEBUG: ... found 1 contact(s).\n" + "";
			selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			runner = new OperationalActionRunner(Action, shipment.GetType(), selectedRecords);
			runner.Printer = printerPK;
			runner.BulkDeliveryMethod = "Only Electronic";
			RunRunner(runner, expectedLog2);
			shipment = (BusinessObject)new BusinessObjectFactory().Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);
			var docsAndCartage = shipment.GetType().GetProperty("DocsAndCartage", (System.Type)shipment.GetType().GetProperty("DocsAndCartageType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(shipment, null)).GetValue(shipment, null);
			ZDateTime jP_PickupCartageAdvised = (ZDateTime)(docsAndCartage.GetType().GetProperty("JP_PickupCartageAdvised", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(docsAndCartage, null));
			ZDateTime jP_DeliveryCartageAdvised = (ZDateTime)(docsAndCartage.GetType().GetProperty("JP_DeliveryCartageAdvised", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(docsAndCartage, null));
			Assert("DocumentEventSource_DocumentPrinted was hit", new ZDateTime(2013, 04, 02, 10, 53, 0, 0) == jP_PickupCartageAdvised || new ZDateTime(2013, 04, 02, 10, 53, 0, 0) == jP_DeliveryCartageAdvised);
		}

		public void TestPrintDocuments_GetsBusinessObjectFactoryFromModule()
		{
			var booking = Factory.New<DummyBusinessObject>();
			var primaryKeys = new[] { booking.PK };
			using (var zFilterGridModule = new DummyDtbBookingConsolidationModule())
			{
				var selection = new ModuleSelection(zFilterGridModule);
				var log = new NullOperationalActionLog();
				var runner = new OperationalActionRunner(Action, booking.GetType(), selection);
				var documentPseudoApplicator = new DocumentPseudoApplicator(runner);
				((IOperationalActionMethodApplicatorDelayingBizoLoading)documentPseudoApplicator).Apply(log, primaryKeys, booking.GetType());
				Assert("Module was not asked for a business object factory. This means that when print cartage advice is done for bookings from a filter, it will incorrectly attach to the shipment rather than the booking, as the DtbFormState of the factory is incorrect.", zFilterGridModule.GetNewFactoryForFilterModeCalled);
			}
		}

		#region Implementation
		protected override void ConfigureForInclusion(OperationalAction action)
		{
			action.DocumentPivots.AddNew();
		}

		protected override void ConfigureForExclusion(OperationalAction action)
		{
			action.DocumentPivots.RemoveAndDeleteAll();
		}

		protected override DocumentPseudoApplicator NewApplicator(OperationalActionRunner runner)
		{
			return new DocumentPseudoApplicator(Runner);
		}

		DeliverDocumentsTestHelper testHelper;
		DeliverDocumentsTestHelper TestHelper => testHelper ?? (testHelper = new DeliverDocumentsTestHelper(Factory));
		#endregion
	}
}

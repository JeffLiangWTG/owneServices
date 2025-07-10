using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class AccTransactionsExportTaskTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestAccTransactionsExportTaskConstructor()
		{
			AccTransactionsExportTask task = new AccTransactionsExportTask(null, null, null, null);
		}

		public void TestIsEnvironmentDataValid()
		{
			Assert(!Task.IsEnvironmentDataValidExposed());
			SetupTheRegistry(false, ZDateTime.Now, System.Environment.CurrentDirectory, 1);
			Assert(!Task.IsEnvironmentDataValidExposed());
			SetupTheRegistry(true, ZDateTime.Now, System.Environment.CurrentDirectory, 1);
			Assert(Task.IsEnvironmentDataValidExposed());
		}

		[TestDate(2008, 1, 1, 19, 7, 7)]
		public void TestExportSuccess()
		{
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				Task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertEquals("1 File was exported", 1, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

				string exportedFileName = string.Format("20080101190707-{0}.xml", Task.Exporter.FilterProvider.CurrentBatchNo);
				AssertEquals("Exported file name", exportedFileName, Path.GetFileName(Directory.GetFiles(dir)[0]));

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				var expectedSubject = string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Success (Batch {1})", GlbCompany.CurrentCompany.GC_Code, Task.Exporter.FilterProvider.CurrentBatchNo);
				var expectedBody = string.Format(@"Running Accounting Transaction Export For Company {0} (Branch CAN) ...
Batch {1} was exported successfully.

The following transactions were exported successfully: Invoices
 - Transactions in Batch: 2
 - Exported to Standard Format: 2

The following transactions were exported successfully: Credit Notes
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Adjustment Notes
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: WIP Posting
 - Transactions in Batch: 1
 - Exported to Standard Format: 1

The following transactions were exported successfully: WIP Reversing
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Accrual Posting
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Accrual Reversing
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Unallocated Transactions
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The XML file for Batch {1} was created successfully:
- File Path: {2}
- File Size: 11 KB


Accounting Transaction Export...finished
", GlbCompany.CurrentCompany.GC_Code, Task.Exporter.FilterProvider.CurrentBatchNo, Path.GetFullPath(Directory.GetFiles(dir)[0]));
				AssertMultilineASCIIEquals("NotificationEmailSubject", expectedSubject, email.Subject);
				AssertMultilineASCIIEquals("NotificationEmailBody", expectedBody, email.Body);

				AssertEquals("Should contain attached xls report file", 1, email.Attachments.Count);
				string xlsFileName = string.Format("{0}-{1}.{2}", email.Subject, Path.GetFileNameWithoutExtension(Task.CurrentFileName), TFileFormats.Xls);
				AssertEquals("Excel report file name", xlsFileName, email.Attachments[0].DisplayName);
			}
		}

		[TestDate(2008, 1, 1, 20, 7, 7)]
		public void TestCreditNoteExport()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				CreateTestData();
				Factory.Save();

				CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
							{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
							{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
							{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", true },
							{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", true },
							{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", true },
							{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", true },
							{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", true },
							{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", true },
							{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", true },
							{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", true },
							{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", true },
							{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", true } };
				SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

				Task.Run();

				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertEquals("1 File was exported", 1, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

				string exportedFileName = string.Format("20080101200707-{0}.xml", Task.Exporter.FilterProvider.CurrentBatchNo);
				AssertEquals("Exported file name", exportedFileName, Path.GetFileName(Directory.GetFiles(dir)[0]));

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("NotificationEmailSubject", string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Success (Batch {1})", GlbCompany.CurrentCompany.GC_Code, Task.Exporter.FilterProvider.CurrentBatchNo), email.Subject);
				AssertEquals("Should contain attached xls report file", 1, email.Attachments.Count);
				string xlsFileName = string.Format("{0}-{1}.{2}", email.Subject, Path.GetFileNameWithoutExtension(Task.CurrentFileName), TFileFormats.Xls);
				AssertEquals("Excel report file name", xlsFileName, email.Attachments[0].DisplayName);
				AssertEquals("Should contain the notification in the body", true, email.Body.Contains("was exported successfully."));
				Assert("Should contain the company code in the body", email.Body.Contains(string.Format("Running Accounting Transaction Export For Company {0} (Branch CAN) ...", GlbCompany.CurrentCompany.GC_Code)));
				AssertContains("Body should contain", ExpectedMessageWhenEnablingFilters(), email.Body);

				using (ExcelInterface xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].Data);

					AssertEquals("AR Invoices", xlInterface.WorkSheets[0].GetCell(10, 0).Value);
					AssertEquals("AP Invoices", xlInterface.WorkSheets[0].GetCell(16, 0).Value);
					AssertEquals("AR Credit Notes", xlInterface.WorkSheets[0].GetCell(22, 0).Value);
					AssertEquals("AP Credit Notes", xlInterface.WorkSheets[0].GetCell(28, 0).Value);
				}
			}
		}

		public void TestExcelFileReportListsAllAPInvoicesAndARCreditNotes()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				CreateTestData();

				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));

				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
				PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));

				Factory.Save();

				CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
							{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
							{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
							{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", true },
							{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", true },
							{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", true },
							{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", true },
							{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", true },
							{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", true },
							{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", true },
							{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", true },
							{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", true },
							{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", true } };
				SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);

				Task.Run();

				using (ExcelInterface xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].Data);

					AssertEquals("AR Invoices", xlInterface.WorkSheets[0].GetCell(10, 0).Value);
					AssertEquals("AP Invoices", xlInterface.WorkSheets[0].GetCell(16, 0).Value);
					AssertEquals("AR Credit Notes", xlInterface.WorkSheets[0].GetCell(31, 0).Value);
					AssertEquals("AP Credit Notes", xlInterface.WorkSheets[0].GetCell(46, 0).Value);
				}
			}
		}

		[TestDate(2008, 1, 1, 7, 7, 7)]
		public void TestExportFilters()
		{
			try
			{
				CodeDescriptionBoolCollection collection = new CodeDescriptionBoolCollection {
							{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", false },
							{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", false },
							{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", false },
							{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", false },
							{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", false },
							{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", false },
							{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", false },
							{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", false },
							{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", false },
							{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", false },
							{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", false },
							{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", false } };

				SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				CreateTestData();
				Factory.Save();

				using (TempDirectory dir = new TempDirectory())
				{
					Assert(!Task.IsEnvironmentDataValidExposed());

					SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
					AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

					Task.Run();
					AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
					AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
					AssertEquals("0 File was exported", 0, Directory.GetFiles(dir).Length);
					AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

					AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals("NotificationEmailSubject", "Accounting Transactions XML Export EDI (Branch CAN) - No Transactions To Export", email.Subject);
					AssertEquals("Should contain in the notification in the body", true, email.Body.Contains("There are no transactions to export."));
					Assert("Should contain the company code in the body", email.Body.Contains(string.Format("Running Accounting Transaction Export For Company {0} (Branch CAN) ...", GlbCompany.CurrentCompany.GC_Code)));
				}
			}
			finally
			{
				CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection {
							{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
							{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
							{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", true },
							{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", true },
							{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", true },
							{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", true },
							{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", true },
							{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", true },
							{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", true },
							{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", true },
							{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", true },
							{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", true } };
				SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCollection);
			}

			task = new AccTransactionsExportTaskForTest(new NotificationBuffer(), new AccountingTransactionExporter(Factory), Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				Task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertEquals("1 File was exported", 1, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

				string exportedFileName = string.Format("20080101070707-{0}.xml", Task.Exporter.FilterProvider.CurrentBatchNo);
				AssertEquals("Exported file name", exportedFileName, Path.GetFileName(Directory.GetFiles(dir)[0]));

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("NotificationEmailSubject", string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Success (Batch {1})", GlbCompany.CurrentCompany.GC_Code, Task.Exporter.FilterProvider.CurrentBatchNo), email.Subject);
				AssertEquals("Should contain attached xls report file", 1, email.Attachments.Count);
				string xlsFileName = string.Format("{0}-{1}.{2}", email.Subject, Path.GetFileNameWithoutExtension(Task.CurrentFileName), TFileFormats.Xls);
				AssertEquals("Excel report file name", xlsFileName, email.Attachments[0].DisplayName);
				AssertEquals("Should contain the notification in the body", true, email.Body.Contains("was exported successfully."));
				Assert("Should contain the company code in the body", email.Body.Contains(string.Format("Running Accounting Transaction Export For Company {0} (Branch CAN) ...", GlbCompany.CurrentCompany.GC_Code)));
				AssertContains("Body should contain", ExpectedMessageWhenEnablingFilters(), email.Body);
			}
		}

		BaseWIPAccrual WipForExport;
		BaseWIPAccrual AccrualForExport;
		TransactionPendingAllocation TransactionPendingAllocationForExport;
		Job job;
		CommonShipment Shipment;

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}
		TestObjectCreator objectCreator;

		void CreateTestData()
		{
			Shipment = Factory.NewWithValidTestData<CommonShipment>();
			Shipment.JS_INCO = "FOB";
			Shipment.JS_UniqueConsignRef = "S12341234";
			Shipment.JS_HouseBill = "UVWXYZ";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			job = Factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = "JS";
			job.JH_ParentID = Shipment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = Shipment.JS_UniqueConsignRef;

			PopulateTransactionHeaderAndSetBatchNumber(typeof(ARInvoice));
			PopulateTransactionHeaderAndSetBatchNumber(typeof(ARCreditNote));
			PopulateTransactionHeaderAndSetBatchNumber(typeof(ARAdjustmentNote));
			PopulateTransactionHeaderAndSetBatchNumber(typeof(APInvoice));
			PopulateTransactionHeaderAndSetBatchNumber(typeof(APCreditNote));
			PopulateTransactionHeaderAndSetBatchNumber(typeof(APAdjustmentNote));

			WipForExport = FillWIPAccrualBizObjWithTestData(typeof(WIP), ObjectCreator.CC1, 100M);
			WipForExport.RelatedJobCharge.ReverseWIP(ZDateTime.Today);

			AccrualForExport = FillWIPAccrualBizObjWithTestData(typeof(Accrual), ObjectCreator.CC1, 100M);
			AccrualForExport.RelatedJobCharge.ReverseAccrual(ZDateTime.Today);

			TransactionPendingAllocationForExport = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			TransactionPendingAllocationForExport.AH_PostDate = ZDateTime.Today;
			TransactionPendingAllocationForExport.AH_RX_NKTransactionCurrency = ObjectCreator.AUD.RX_Code;
			TransactionPendingAllocationForExport.AH_OSTotal = 100m;
			TransactionPendingAllocationForExport.AH_InvoiceAmount = 100m;
		}

		InvoicingBase PopulateTransactionHeaderAndSetBatchNumber(Type typeToCreate)
		{
			InvoicingBase invoiceForExport = PopulateInvoice(typeToCreate, 100.00M, 10.00M, 0.50M, ObjectCreator.USD);
			return invoiceForExport;
		}

		protected virtual InvoicingBase PopulateInvoice(Type type, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, RefCurrency currency)
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(type);

			invoice.AH_OH = ObjectCreator.AALSHI.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_Desc = "This is a test description to see how the XML Export works";
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			invoice.AH_Ledger = type.Name.Substring(0, 2) == "AP" ? "AP" : "AR";
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
			invoice.AH_InvoiceTerm = "INV";
			invoice.AH_InvoiceTermDays = 1;
			invoice.AH_DueDate = ZDateTime.Today.AddDays(1);
			invoice.AH_TransactionReference = @"Shipment ABC123";
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				invoice.AH_TransactionNum = Enterprise.Accounting.Business.TestObjectCreator.GetRandomString(10);
			}

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			PopulateInvoiceLine(line1, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), exchangeRate, invoice.PK);

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			PopulateInvoiceLine(line2, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), exchangeRate, invoice.PK);
			Factory.Save();

			return invoice;
		}

		void PopulateInvoiceLine(InvoicingLineBase line, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, ZGuid header)
		{
			line.AL_AC = ObjectCreator.CC2.PK;
			line.AL_AH = header;
			line.AL_AT = ObjectCreator.GST1.PK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			line.AL_PostDate = ZDateTime.Today;
			line.AL_Sequence = 1;
			line.AL_JH = ObjectCreator.Job2.PK;
			line.AL_OSTaxAmount = oSTaxAmount;

			var charge = ObjectCreator.CreateJobCharge(line, ObjectCreator.Job2, ObjectCreator.CC2, ObjectCreator.USD);
			if (charge.JR_AL_ARLine.IsEmpty)
			{
				charge.JR_OSSellAmt = 0m;   // Should not create WIP 
			}
			if (charge.JR_AL_APLine.IsEmpty)
			{
				charge.JR_OSCostAmt = 0m;   // Should not create Accrual
			}
		}

		protected virtual BaseWIPAccrual FillWIPAccrualBizObjWithTestData(Type type, AccChargeCode chargeCode, decimal localExTaxAmount)
		{
			BaseWIPAccrual wip = (BaseWIPAccrual)Factory.NewWithValidTestData(type);
			wip.AL_AC = chargeCode.PK;
			wip.AL_LocalExTaxAmount = localExTaxAmount;
			wip.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			wip.AL_PostDate = ZDateTime.Today;
			wip.AL_AC = chargeCode.PK;
			wip.AL_Desc = "Description of Accrual or WIP";
			wip.AL_JH = job.PK;

			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			if (wip.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = wip.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
				charge.JR_OSCostAmt = 0m;
			}
			if (wip.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = wip.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				charge.JR_OSSellAmt = 0m;
			}

			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wip);

			return wip;
		}

		ZString ExpectedMessageWhenEnablingFilters()
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append("Running Accounting Transaction Export For Company EDI (Branch CAN) ...");
			result.Append(string.Format("Batch {0} was exported successfully.", Task.Exporter.FilterProvider.CurrentBatchNo));
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Invoices");
			result.Append(" - Transactions in Batch: 2");
			result.Append(" - Exported to Standard Format: 2");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Credit Notes");
			result.Append(" - Transactions in Batch: 2");
			result.Append(" - Exported to Standard Format: 2");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Adjustment Notes");
			result.Append(" - Transactions in Batch: 2");
			result.Append(" - Exported to Standard Format: 2");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: WIP Posting");
			result.Append(" - Transactions in Batch: 2");
			result.Append(" - Exported to Standard Format: 2");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: WIP Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Posting");
			result.Append(" - Transactions in Batch: 2");
			result.Append(" - Exported to Standard Format: 2");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Accrual Reversing");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append("The following transactions were exported successfully: Unallocated Transactions");
			result.Append(" - Transactions in Batch: 1");
			result.Append(" - Exported to Standard Format: 1");
			result.Append(ZString.Empty);
			result.Append(string.Format("The XML file for Batch {0} was created successfully:", Task.Exporter.FilterProvider.CurrentBatchNo));
			return result.ToStringWithNewLineBetweenAppends();
		}

		[TestDate(2008, 1, 1, 7, 7, 7)]
		public void TestResetNewBatchNumber()
		{
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				Task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertContains(string.Format("Batch {0} was exported successfully.", Task.Exporter.FilterProvider.CurrentBatchNo), Task.NotificationBuffer.AsString);
				AssertEquals("1 File was exported", 1, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

				string exportedFileName = string.Format("20080101070707-{0}.xml", Task.Exporter.FilterProvider.CurrentBatchNo);
				AssertEquals("Exported file name", exportedFileName, Path.GetFileName(Directory.GetFiles(dir)[0]));

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("NotificationEmailSubject", string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Success (Batch {1})", GlbCompany.CurrentCompany.GC_Code, Task.Exporter.FilterProvider.CurrentBatchNo), email.Subject);
				AssertEquals("Should contain attached xls report file", 1, email.Attachments.Count);
				string xlsFileName = string.Format("{0}-{1}.{2}", email.Subject, Path.GetFileNameWithoutExtension(Task.CurrentFileName), TFileFormats.Xls);
				AssertEquals("Excel report file name", xlsFileName, email.Attachments[0].DisplayName);
				AssertEquals("Should contain the notification in the body", true, email.Body.IndexOf("was exported successfully.") > -1);

				File.Delete(Directory.GetFiles(dir)[0]);
				AssertEquals("Must be 0 Files", 0, Directory.GetFiles(dir).Length);
				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				Task.NotificationBuffer.Clear();
				SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				Task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertContains("There are no transactions to export.", Task.NotificationBuffer.AsString);
				AssertEquals("0 Files were created", 0, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[TestDate(2008, 1, 1, 7, 7, 7)]
		public void TestExportSuccessWhenNothingToExport()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				Task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", Task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.AsString);
				AssertEquals("0 Emty xml File was exported", 0, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				var expectedSubject = string.Format("Accounting Transactions XML Export {0} (Branch CAN) - No Transactions To Export", GlbCompany.CurrentCompany.GC_Code);
				var expectedBody = @"Running Accounting Transaction Export For Company EDI (Branch CAN) ...

There are no transactions to export.

Accounting Transaction Export...finished";
				AssertEquals("NotificationEmailSubject", expectedSubject, email.Subject);
				AssertMultilineASCIIEquals("NotificationEmailBody", expectedBody, email.Body);
				AssertEquals("Shouldn't contain an xls report file", 0, email.Attachments.Count);
			}
		}

		[TestDate(2008, 1, 1)]
		public void TestExportError()
		{
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			AccTransactionsExportErrorTaskForTest task = new AccTransactionsExportErrorTaskForTest(new NotificationBuffer(), new AccountingTransactionExporterWithErrorForTest(Factory), Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);
			using (TempDirectory dir = new TempDirectory())
			{
				task.Exporter.FilterProvider.IncludeARInvoices = true;

				Factory.Save();

				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", task.NotificationBuffer.AsString);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("NotificationEmailSubject", string.Format("Accounting Transactions XML Export EDI (Branch CAN) - Export Failed (Batch {0})", task.Exporter.FilterProvider.CurrentBatchNo), email.Subject);
				AssertEquals("Shouldn't contain attached xls report file", 0, email.Attachments.Count);
				AssertContains("Should contain the notification in the body", string.Format("BATCH ERROR: There were problems exporting Batch {0}.", task.Exporter.FilterProvider.CurrentBatchNo), email.Body);
				Assert("Should contain the company code in the body", email.Body.Contains(string.Format("Running Accounting Transaction Export For Company {0} (Branch CAN) ...", GlbCompany.CurrentCompany.GC_Code)));
			}
		}

		[TestDate(2008, 1, 1, 7, 7, 7)]
		public void TestExportErrorWhenBatchWasCreatedButExceptionWasThrown()
		{
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			AccTransactionsExportTaskForTestWithEnvironmentDataValid task = new AccTransactionsExportTaskForTestWithEnvironmentDataValid(new NotificationBuffer(), new AccountingTransactionExporter(Factory), Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);
			using (TempDirectory dir = new TempDirectory())
			{
				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);

				TempDirectory.DeleteDirectory(dir.DirectoryName);

				AssertEquals("Precondition: IsEnvironmentDataValid", true, task.IsEnvironmentDataValidExposed());
				task.Run();
				ExceptionReporterTestListener.Instance.Clear();

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				var expectedSubject = string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Export Failed (Batch {1})", GlbCompany.CurrentCompany.GC_Code, task.Exporter.FilterProvider.CurrentBatchNo);
				var expectedBody = string.Format(@"Running Accounting Transaction Export For Company {0} (Branch CAN) ...
Batch {1} was exported successfully.

The following transactions were exported successfully: Invoices
 - Transactions in Batch: 2
 - Exported to Standard Format: 2

The following transactions were exported successfully: Credit Notes
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Adjustment Notes
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: WIP Posting
 - Transactions in Batch: 1
 - Exported to Standard Format: 1

The following transactions were exported successfully: WIP Reversing
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Accrual Posting
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Accrual Reversing
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

The following transactions were exported successfully: Unallocated Transactions
 - Transactions in Batch: 0
 - Exported to Standard Format: 0

Error: Error Exporting Transaction For Company {0} (Branch CAN) - Could not find a part of the path", GlbCompany.CurrentCompany.GC_Code, task.Exporter.FilterProvider.CurrentBatchNo);
				AssertMultilineASCIIEquals("NotificationEmailSubject", expectedSubject, email.Subject);
				AssertContains("NotificationEmailBody", expectedBody, email.Body);
				AssertContains("NotificationEmailBody", "Accounting Transaction Export...finished", email.Body);
				AssertEquals("Shouldn't contain attached xls report file", 0, email.Attachments.Count);
				AssertEquals("Next run must be set", ZDateTime.Now.AddDays(1), task.AccountingTransactionsExportNextRun);
				AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(ExpectedHighWaterMarkBuffer), SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);
			}
		}

		[TestDate(2008, 1, 1)]
		public void TestExportTooManyWipAndAccrualTransactions()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var job = testObjectCreator.CreateJobHeader();

			Accrual accrual = testObjectCreator.CreateAccrual(job);
			Accrual accrualRev = testObjectCreator.CreateAccrual(job);
			accrual.AL_PostDate = ZDateTime.Today;
			accrualRev.AL_PostDate = ZDateTime.Today;

			BaseCharge linkedCharge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_APLine, accrualRev.PK));
			linkedCharge.ReverseAccrual(ZDateTime.Today);

			WIP wip = testObjectCreator.CreateWIP();
			WIP wipRev = testObjectCreator.CreateWIP();
			wip.AL_PostDate = ZDateTime.Today;
			wipRev.AL_PostDate = ZDateTime.Today;

			linkedCharge = Factory.LoadTop1<BaseCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, wipRev.PK));
			linkedCharge.ReverseWIP(ZDateTime.Today);

			Factory.Save();

			AccTransactionsExportErrorTaskForTest task = new AccTransactionsExportErrorTaskForTest(new NotificationBuffer(), new AccountingTransactionExporter(Factory), Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);
			task.MaxNumberOfWipAndAccrualTransactionsToExportOverride = 3;

			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				task.Exporter.FilterProvider.IncludeAccrualsPosting = true;
				task.Exporter.FilterProvider.IncludeAccrualsReversing = true;
				task.Exporter.FilterProvider.IncludeWIPsPosting = true;
				task.Exporter.FilterProvider.IncludeWIPsReversing = true;

				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				task.Run();
				AssertContains("Running Accounting Transaction Export For Company " + Env.CurrentCompany.Code + " (Branch CAN) ...", task.NotificationBuffer.AsString);
				AssertContains("Accounting Transaction Export...finished", task.NotificationBuffer.AsString);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);
				AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				var expectedSubject = string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Export Failed (No Batch Created)", GlbCompany.CurrentCompany.GC_Code);
				var expectedBody = string.Format(@"Running Accounting Transaction Export For Company {0} (Branch CAN) ...
Error: Batch was not created.  Unable to export more than 3 WIP and Accrual transactions at a time.  However, there are 7 WIP and Accrual transactions for export.
Run the manual export, located at Accounts -> General Ledger -> Export Transactions, with date filters to create a number of files that have less than 3 WIP and Accrual transactions per file.
After this backlog is cleared, the automatic batch export will be able to resume.
Accounting Transaction Export...finished
", GlbCompany.CurrentCompany.GC_Code);
				AssertMultilineASCIIEquals("NotificationEmailSubject", expectedSubject, email.Subject);
				AssertMultilineASCIIEquals("NotificationEmailBody", expectedBody, email.Body);
				AssertEquals("Shouldn't contain attached xls report file", 0, email.Attachments.Count);
			}
		}

		[TestDate(2008, 1, 1, 19, 7, 7)]
		public void TestTryToExportWithInvalidRegistrySettings()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistryWithInvalidSettings(false, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);
				Task.Run();
				AssertEquals(string.Empty, Task.NotificationBuffer.AsString);

				SetupTheRegistryWithInvalidSettings(true, ZDateTime.Now.AddDays(-1), dir, 1);
				Task.Run();
				AssertContains("Cannot notify group, System/Data Export Settings/Accounting Transactions/Notify Group is empty or invalid.", Task.NotificationBuffer.AsString);
			}
		}

		[TestDate(2008, 1, 1, 20, 7, 7)]
		public void TestExportBatchSavingFileWithException()
		{
			using (var dir = new TempDirectory())
			{
				Assert(!Task.IsEnvironmentDataValidExposed());

				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("0 Files were exported", 0, Directory.GetFiles(dir).Length);

				CreateTestData();
				Factory.Save();

				CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection
				{
					{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
					{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
					{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", true },
					{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", true },
					{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", true },
					{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", true },
					{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", true },
					{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", true },
					{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", true },
					{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", true },
					{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", true },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", true }
				};
				SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty,
					Guid.Empty, defaultCollection);

				var account = WindowsIdentity.GetCurrent().Name;
				var dirInfo = new DirectoryInfo(dir.DirectoryName);
				var dirSecurity = dirInfo.GetAccessControl();
				dirSecurity.AddAccessRule(new FileSystemAccessRule(account, FileSystemRights.CreateFiles, AccessControlType.Deny));
				dirInfo.SetAccessControl(dirSecurity);

				try
				{
					Task.Run();
				}
				finally
				{
					dirSecurity.RemoveAccessRule(new FileSystemAccessRule(account, FileSystemRights.CreateFiles, AccessControlType.Deny));
					dirInfo.SetAccessControl(dirSecurity);
				}

				AssertContains("Running Accounting Transaction Export", Task.NotificationBuffer.Events[0].Message);
				AssertContains("Batch 1 was exported successfully", Task.NotificationBuffer.Events[1].Message);
				AssertContains("1st attempt", "Error: Error Exporting Transaction", Task.NotificationBuffer.Events[2].Message);
				AssertContains("2nd attempt", "Error: Error Exporting Transaction", Task.NotificationBuffer.Events[3].Message);
				AssertContains("3rd attempt", "Error: Error Exporting Transaction", Task.NotificationBuffer.Events[4].Message);
				AssertContains("Accounting Transaction Export...finished", Task.NotificationBuffer.Events[5].Message);
			}
		}

		[TestDate(2008, 1, 1)]
		public void TestExportErrorWasThrownForNeitherIOExceptionNorUnathorizzedAccessException()
		{
			var helper = new TransactionExportTestDataHelper(Factory);
			var task = new AccTransactionsExportTaskForTest(new NotificationBuffer(), new AccountingTransactionsDataExporterForTestWithNotIOAndUnathorizzedAccessException(Factory), SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);
			using (TempDirectory dir = new TempDirectory())
			{
				SetupTheRegistry(true, ZDateTime.Now.AddDays(-1), dir, 1);
				AssertEquals("Precondition: IsEnvironmentDataValid", true, task.IsEnvironmentDataValidExposed());
				try
				{
					task.Run();
				}
				catch (Exception)
				{
				}
				ExceptionReporterTestListener.Instance.Clear();

				AssertEquals("One new email should have been created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				var expectedSubject = string.Format("Accounting Transactions XML Export {0} (Branch CAN) - Export Failed (Batch {1})", GlbCompany.CurrentCompany.GC_Code, task.Exporter.FilterProvider.CurrentBatchNo);
				var expectedBody = string.Format(@"Running Accounting Transaction Export For Company {0} (Branch CAN) ...
Error: Error Exporting Transaction For Company {0} (Branch CAN) - This is neither an IOException nor an UnauthorizedAccessException, but user should be able to see its message.
Accounting Transaction Export...finished
", GlbCompany.CurrentCompany.GC_Code);
				AssertMultilineASCIIEquals("NotificationEmailSubject", expectedSubject, email.Subject);
				AssertMultilineASCIIEquals("NotificationEmailBody", expectedBody, email.Body);
				AssertEquals("Shouldn't contain attached xls report file", 0, email.Attachments.Count);
			}
		}

		#region Implementaion

		protected virtual
		TimeSpan ExpectedHighWaterMarkBuffer
		{
			get { return new TimeSpan(48, 0, 0); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			task = new AccTransactionsExportTaskForTest(new NotificationBuffer(), new AccountingTransactionExporter(Factory), Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport, GlbCompany.CurrentCompany);

			var brisbaneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			brisbaneBranch.GB_IsActive = false;
			AssertEquals("Standard Time Zone Offset", 10m, brisbaneBranch.HomePort.StandardZoneUTCOffset);

			var melbourneBranch = Factory.NewWithValidTestData<GlbBranch>();
			melbourneBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			melbourneBranch.GB_Code = "MEL";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";
			melbourneBranch.GB_IsActive = false;
			AssertEquals("Standard Time Zone Offset", 10m, melbourneBranch.HomePort.StandardZoneUTCOffset);

			var canberraBranch = Factory.NewWithValidTestData<GlbBranch>();
			canberraBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			canberraBranch.GB_Code = "CAN";
			canberraBranch.GB_RL_NKHomePort = "AUCBR";
			AssertEquals("Standard Time Zone Offset", 10m, canberraBranch.HomePort.StandardZoneUTCOffset);

			var perthBranch = Factory.NewWithValidTestData<GlbBranch>();
			perthBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			perthBranch.GB_Code = "PER";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			AssertEquals("Standard Time Zone Offset", 8m, perthBranch.HomePort.StandardZoneUTCOffset);

			var adelaideBranch = Factory.NewWithValidTestData<GlbBranch>();
			adelaideBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			adelaideBranch.GB_Code = "ADL";
			adelaideBranch.GB_RL_NKHomePort = "AUADL";
			AssertEquals("Standard Time Zone Offset", 9.5m, adelaideBranch.HomePort.StandardZoneUTCOffset);

			Factory.Save();
		}
		protected ZString StoredCountry;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
		}

		AccTransactionsExportTaskForTest Task
		{
			get { return task; }
		}
		AccTransactionsExportTaskForTest task;

		void SetupTheRegistryWithInvalidSettings(bool enableInterface, ZDateTime nextRun, string directory, ZInt interval)
		{
			DataTransferSwitchRegistryBusinessObject itemObject = new DataTransferSwitchRegistryBusinessObject(Factory);
			itemObject.EnableInterface = enableInterface;
			itemObject.Directory = directory;
			itemObject.NextRunDateTime = nextRun;
			itemObject.Interval = interval;
			DataTransferSwitchRegistryItem item = (DataTransferSwitchRegistryItem)Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport;
			item.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemObject);
			Factory.Save();
		}

		void SetupTheRegistry(bool enableInterface, ZDateTime nextRun, string directory, ZInt interval)
		{
			DataTransferSwitchRegistryBusinessObject itemObject = new DataTransferSwitchRegistryBusinessObject(Factory);
			itemObject.EnableInterface = enableInterface;
			itemObject.Directory = directory;
			itemObject.NextRunDateTime = nextRun;
			itemObject.Interval = interval;
			itemObject.GroupPK = NotificationGroup.PK;
			DataTransferSwitchRegistryItem item = (DataTransferSwitchRegistryItem)Registry.Business.SystemDataRegistry.Instance.AccountingTransactionsExport;
			item.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemObject);
			Factory.Save();
		}

		GlbGroup NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
					GlbStaff mrBob = notificationGroup.Staff.AddNew();
					mrBob.GS_FullName = "Mr. Bob";
					mrBob.GS_EmailAddress = "bob@bob.com";
					Factory.Save();
				}
				return notificationGroup;
			}
		}
		GlbGroup notificationGroup;

		class AccTransactionsExportTaskForTest : AccTransactionsExportTask
		{
			public AccTransactionsExportTaskForTest(INotifications notifications, AccountingTransactionExporter transactionsDataExporter, IRegistryItem dataTransferSwitchRegistryItem, GlbCompany companyPK) : base(notifications, transactionsDataExporter, dataTransferSwitchRegistryItem, companyPK) { }

			public bool IsEnvironmentDataValidExposed()
			{
				return IsEnvironmentDataValid();
			}

			public NotificationBuffer NotificationBuffer
			{
				get { return (NotificationBuffer)Notify; }
			}

			public string CurrentFileName
			{
				get { return currentFileName; }
			}

			protected override short MaxNumberOfWipAndAccrualTransactionsToExport
			{
				get
				{
					if (MaxNumberOfWipAndAccrualTransactionsToExportOverride.HasValue)
					{
						return MaxNumberOfWipAndAccrualTransactionsToExportOverride.Value;
					}
					return base.MaxNumberOfWipAndAccrualTransactionsToExport;
				}
			}
			public short? MaxNumberOfWipAndAccrualTransactionsToExportOverride;
		}

		class AccTransactionsExportErrorTaskForTest : AccTransactionsExportTaskForTest
		{
			public AccTransactionsExportErrorTaskForTest(INotifications notifications, AccountingTransactionExporter transactionsDataExporter, IRegistryItem dataTransferSwitchRegistryItem, GlbCompany company) : base(notifications, transactionsDataExporter, dataTransferSwitchRegistryItem, company) { }
			protected override void SetupExportFilter(AccountingTransactionExporter exporter) { }
		}

		class AccountingTransactionExporterWithErrorForTest : AccountingTransactionExporter
		{
			public AccountingTransactionExporterWithErrorForTest(BusinessObjectFactory factory) : base(factory) { }
			protected override ZBool ErrorHasOccuredCore
			{
				get { return true; }
			}
		}

		class AccTransactionsExportTaskForTestWithEnvironmentDataValid : AccTransactionsExportTaskForTest
		{
			public AccTransactionsExportTaskForTestWithEnvironmentDataValid(INotifications notifications, AccountingTransactionExporter transactionsDataExporter, IRegistryItem dataTransferSwitchRegistryItem, GlbCompany company) : base(notifications, transactionsDataExporter, dataTransferSwitchRegistryItem, company) { }

			protected override bool IsEnvironmentDataValid()
			{
				return true;
			}
		}

		class AccountingTransactionsDataExporterForTestWithNotIOAndUnathorizzedAccessException : AccountingTransactionExporter
		{
			public AccountingTransactionsDataExporterForTestWithNotIOAndUnathorizzedAccessException(BusinessObjectFactory factory) : base(factory) { }

			protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
			{
				throw new Exception("This is neither an IOException nor an UnauthorizedAccessException, but user should be able to see its message.");
			}
		}

		#endregion
	}
}

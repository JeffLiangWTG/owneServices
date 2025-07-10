using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.XmlExport.Testing
{
	[TestedType(typeof(XmlExportGUIWrapper))]
	[GuiTest]
	class XmlExportGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2005, 09, 27, 12, 01, 0)]
		public void TestFileName()
		{
			string expected = "2005-09-27_1201";
			AssertEquals(expected, GUIWrapper.FileName_ForTestOnly);
		}

		public void TestInitialDirectory()
		{
			AssertEquals("\\", GUIWrapper.InitialDirectory_ForTestOnly);
		}

		public void TestDefaultDialogFilter()
		{
			AssertEquals("XML Files | *.xml", GUIWrapper.DialogFilter_ForTestOnly);
		}

		public void TestDefailtFileExtention()
		{
			AssertEquals(".xml", GUIWrapper.FileExtention_ForTestOnly);
		}

		public void TestIncludeUnallocatedCreditNotes()
		{
			AssertEquals("Include AR Invoices should be false by default", false, GUIWrapper.IncludeUnallocatedAPCreditNotes);
			AssertEquals("Include AR Invoices should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPCreditNotes);

			GUIWrapper.IncludeUnallocatedAPCreditNotes = true;

			AssertEquals("Include AR Invoices should be true", true, GUIWrapper.IncludeUnallocatedAPCreditNotes);
			AssertEquals("Include AR Invoices should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPCreditNotes);

			GUIWrapper.IncludeUnallocatedAPCreditNotes = false;

			AssertEquals("Include AR Invoices should be false", false, GUIWrapper.IncludeUnallocatedAPCreditNotes);
			AssertEquals("Include AR Invoices should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPCreditNotes);
		}

		public void TestIncludeUnallocatedInvoices()
		{
			AssertEquals("Include AR Invoices should be false by default", false, GUIWrapper.IncludeUnallocatedAPInvoices);
			AssertEquals("Include AR Invoices should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPInvoices);

			GUIWrapper.IncludeUnallocatedAPInvoices = true;

			AssertEquals("Include AR Invoices should be true", true, GUIWrapper.IncludeUnallocatedAPInvoices);
			AssertEquals("Include AR Invoices should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPInvoices);

			GUIWrapper.IncludeUnallocatedAPInvoices = false;

			AssertEquals("Include AR Invoices should be false", false, GUIWrapper.IncludeUnallocatedAPInvoices);
			AssertEquals("Include AR Invoices should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeUnallocatedAPInvoices);
		}

		public void TestIncludeARInvoices()
		{
			AssertEquals("Include AR Invoices should be false by default", false, GUIWrapper.IncludeARInvoices);
			AssertEquals("Include AR Invoices should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeARInvoices);

			GUIWrapper.IncludeARInvoices = true;

			AssertEquals("Include AR Invoices should be true", true, GUIWrapper.IncludeARInvoices);
			AssertEquals("Include AR Invoices should be true", true, GUIWrapper.DataExporter.FilterProvider.IncludeARInvoices);

			GUIWrapper.IncludeARInvoices = false;

			AssertEquals("Include AR Invoices should be false", false, GUIWrapper.IncludeARInvoices);
			AssertEquals("Include AR Invoices should be false", false, GUIWrapper.DataExporter.FilterProvider.IncludeARInvoices);
		}

		public void TestIncludeARCreditNotes()
		{
			AssertEquals("IncludeARCreditNotes should be false by default", false, GUIWrapper.IncludeARCreditNotes);
			AssertEquals("IncludeARCreditNotes should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeARCreditNotes);

			GUIWrapper.IncludeARCreditNotes = true;

			AssertEquals("IncludeARCreditNotes should be true", true, GUIWrapper.IncludeARCreditNotes);
			AssertEquals("IncludeARCreditNotes should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeARCreditNotes);

			GUIWrapper.IncludeARCreditNotes = false;

			AssertEquals("IncludeARCreditNotes should be false", false, GUIWrapper.IncludeARCreditNotes);
			AssertEquals("IncludeARCreditNotes should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeARCreditNotes);
		}

		public void TestIncludeARAdjustmentNotes()
		{
			AssertEquals("IncludeARAdjustmentNotes should be false by default", false, GUIWrapper.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARAdjustmentNotes should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeARAdjustmentNotes);

			GUIWrapper.IncludeARAdjustmentNotes = true;

			AssertEquals("IncludeARAdjustmentNotes should be true", true, GUIWrapper.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARAdjustmentNotes should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeARAdjustmentNotes);

			GUIWrapper.IncludeARAdjustmentNotes = false;

			AssertEquals("IncludeARAdjustmentNotes should be false", false, GUIWrapper.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARAdjustmentNotes should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeARAdjustmentNotes);
		}

		public void TestIncludeAPInvoices()
		{
			AssertEquals("Include AP Invoices should be false by default", false, GUIWrapper.IncludeAPInvoices);
			AssertEquals("Include AP Invoices should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPInvoices);

			GUIWrapper.IncludeAPInvoices = true;

			AssertEquals("Include AP Invoices should be true", true, GUIWrapper.IncludeAPInvoices);
			AssertEquals("Include AP Invoices should be true", true, GUIWrapper.DataExporter.FilterProvider.IncludeAPInvoices);

			GUIWrapper.IncludeAPInvoices = false;

			AssertEquals("Include AP Invoices should be false", false, GUIWrapper.IncludeAPInvoices);
			AssertEquals("Include AP Invoices should be false", false, GUIWrapper.DataExporter.FilterProvider.IncludeAPInvoices);
		}

		public void TestIncludeAPCreditNotes()
		{
			AssertEquals("IncludeAPCreditNotes should be false by default", false, GUIWrapper.IncludeAPCreditNotes);
			AssertEquals("IncludeAPCreditNotes should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPCreditNotes);

			GUIWrapper.IncludeAPCreditNotes = true;

			AssertEquals("IncludeAPCreditNotes should be true", true, GUIWrapper.IncludeAPCreditNotes);
			AssertEquals("IncludeAPCreditNotes should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPCreditNotes);

			GUIWrapper.IncludeAPCreditNotes = false;

			AssertEquals("IncludeAPCreditNotes should be false", false, GUIWrapper.IncludeAPCreditNotes);
			AssertEquals("IncludeAPCreditNotes should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPCreditNotes);
		}

		public void TestIncludeAPAdjustmentNotes()
		{
			AssertEquals("IncludeAPAdjustmentNotes should be false by default", false, GUIWrapper.IncludeAPAdjustmentNotes);
			AssertEquals("IncludeAPAdjustmentNotes should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPAdjustmentNotes);

			GUIWrapper.IncludeAPAdjustmentNotes = true;

			AssertEquals("IncludeAPAdjustmentNotes should be true", true, GUIWrapper.IncludeAPAdjustmentNotes);
			AssertEquals("IncludeAPAdjustmentNotes should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPAdjustmentNotes);

			GUIWrapper.IncludeAPAdjustmentNotes = false;

			AssertEquals("IncludeAPAdjustmentNotes should be false", false, GUIWrapper.IncludeAPAdjustmentNotes);
			AssertEquals("IncludeAPAdjustmentNotes should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAPAdjustmentNotes);
		}

		public void TestIncludeWipPosting()
		{
			AssertEquals("IncludeWipsPosting should be false by default", false, GUIWrapper.IncludeWipsPosting);
			AssertEquals("IncludeWipsPosting should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeWIPsPosting);

			GUIWrapper.IncludeWipsPosting = true;

			AssertEquals("IncludeWipsPosting should be true", true, GUIWrapper.IncludeWipsPosting);
			AssertEquals("IncludeWipsPosting should be true", true, GUIWrapper.DataExporter.FilterProvider.IncludeWIPsPosting);

			GUIWrapper.IncludeWipsPosting = false;

			AssertEquals("IncludeWipsPosting should be false", false, GUIWrapper.IncludeWipsPosting);
			AssertEquals("IncludeWipsPosting should be false", false, GUIWrapper.DataExporter.FilterProvider.IncludeWIPsPosting);
		}

		public void TestIncludeWipReversing()
		{
			AssertEquals("IncludeWipsReversing should be false by default", false, GUIWrapper.IncludeWipsReversing);
			AssertEquals("IncludeWipsReversing should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeWIPsReversing);

			GUIWrapper.IncludeWipsReversing = true;

			AssertEquals("IncludeWipsReversing should be true", true, GUIWrapper.IncludeWipsReversing);
			AssertEquals("IncludeWipsReversing should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeWIPsReversing);

			GUIWrapper.IncludeWipsReversing = false;

			AssertEquals("IncludeWipsReversing should be false", false, GUIWrapper.IncludeWipsReversing);
			AssertEquals("IncludeWipsReversing should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeWIPsReversing);
		}

		public void TestIncludeAccrualPosting()
		{
			AssertEquals("IncludeAccrualsPosting should be false by default", false, GUIWrapper.IncludeAccrualsPosting);
			AssertEquals("IncludeAccrualsPosting should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsPosting);

			GUIWrapper.IncludeAccrualsPosting = true;

			AssertEquals("IncludeAccrualsPosting should be true", true, GUIWrapper.IncludeAccrualsPosting);
			AssertEquals("IncludeAccrualsPosting should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsPosting);

			GUIWrapper.IncludeAccrualsPosting = false;

			AssertEquals("IncludeAccrualsPosting should be false", false, GUIWrapper.IncludeAccrualsPosting);
			AssertEquals("IncludeAccrualsPosting should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsPosting);
		}

		public void TestIncludeAccrualReversing()
		{
			AssertEquals("IncludeAccrualsReversing should be false by default", false, GUIWrapper.IncludeAccrualsReversing);
			AssertEquals("IncludeAccrualsReversing should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsReversing);

			GUIWrapper.IncludeAccrualsReversing = true;

			AssertEquals("IncludeAccrualsReversing should be true", true, GUIWrapper.IncludeAccrualsReversing);
			AssertEquals("IncludeAccrualsReversing should be true", true,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsReversing);

			GUIWrapper.IncludeAccrualsReversing = false;

			AssertEquals("IncludeAccrualsReversing should be false", false, GUIWrapper.IncludeAccrualsReversing);
			AssertEquals("IncludeAccrualsReversing should be false", false,
				GUIWrapper.DataExporter.FilterProvider.IncludeAccrualsReversing);
		}

		public void TestExcludeJobRelatedTransactionsForAR()
		{
			AssertEquals("ExcludeJobRelatedTransactionsForAR should be false by default", false,
				GUIWrapper.ExcludeJobRelatedTransactionsForAR);
			AssertEquals("ExcludeJobRelatedTransactionsForAR should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);

			GUIWrapper.ExcludeJobRelatedTransactionsForAR = true;

			AssertEquals("ExcludeJobRelatedTransactionsForAR should be true", true,
				GUIWrapper.ExcludeJobRelatedTransactionsForAR);
			AssertEquals("ExcludeJobRelatedTransactionsForAR should be true", true,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);

			GUIWrapper.ExcludeJobRelatedTransactionsForAR = false;

			AssertEquals("ExcludeJobRelatedTransactionsForAR should be false", false,
				GUIWrapper.ExcludeJobRelatedTransactionsForAR);
			AssertEquals("ExcludeJobRelatedTransactionsForAR should be false", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);
		}

		public void TestExcludeNonJobRelatedTransactionsForAR()
		{
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be false by default", false,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);

			GUIWrapper.ExcludeNonJobRelatedTransactionsForAR = true;

			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be true", true,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be true", true,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);

			GUIWrapper.ExcludeNonJobRelatedTransactionsForAR = false;

			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be false", false,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAR should be false", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
		}

		public void TestExcludeJobRelatedTransactionsForAP()
		{
			AssertEquals("ExcludeJobRelatedTransactionsForAP should be false by default", false,
				GUIWrapper.ExcludeJobRelatedTransactionsForAP);
			AssertEquals("ExcludeJobRelatedTransactionsForAP should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAP);

			GUIWrapper.ExcludeJobRelatedTransactionsForAP = true;

			AssertEquals("ExcludeJobRelatedTransactionsForAP should be true", true,
				GUIWrapper.ExcludeJobRelatedTransactionsForAP);
			AssertEquals("ExcludeJobRelatedTransactionsForAP should be true", true,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAP);

			GUIWrapper.ExcludeJobRelatedTransactionsForAP = false;

			AssertEquals("ExcludeJobRelatedTransactionsForAP should be false", false,
				GUIWrapper.ExcludeJobRelatedTransactionsForAP);
			AssertEquals("ExcludeJobRelatedTransactionsForAP should be false", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeJobRelatedTransactionsForAP);
		}

		public void TestExcludeNonJobRelatedTransactionsForAP()
		{
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be false by default", false,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAP);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be false by default", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);

			GUIWrapper.ExcludeNonJobRelatedTransactionsForAP = true;

			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be true", true,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAP);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be true", true,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);

			GUIWrapper.ExcludeNonJobRelatedTransactionsForAP = false;

			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be false", false,
				GUIWrapper.ExcludeNonJobRelatedTransactionsForAP);
			AssertEquals("ExcludeNonJobRelatedTransactionsForAP should be false", false,
				GUIWrapper.DataExporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
		}

		public void TestMessageWhenCreatingANewBatchAndThereAreTransactionsToExport()
		{
			Factory.NewWithValidTestData(typeof(ARInvoice));
			Factory.Save();

			GUIWrapperNoSave.IncludeARInvoices = true;
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			GUIWrapperNoSave.Export();

			AssertEquals("Export process should be called", true, GUIWrapperNoSave.ExportProcessHasBeenRun);
			AssertEquals("Batch number should have changed", 1, GUIWrapperNoSave.ExistingBatchNumberToExport);
		}

		public void TestMessageWhenCreatingANewBatchAndThereAreNoTransactionsToExport()
		{
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			GUIWrapperNoSave.Export();

			AssertEquals("Message should be shown",
				"Based on the criteria provided, there are currently no transactions to export",
				UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Export process should not be called", false, GUIWrapperNoSave.ExportProcessHasBeenRun);
		}

		public void TestMessageWhenExportingAnExistingBatchWithTransactions()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, invoice.PK, 1);
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, apInvoice.PK, 2);
			ARCreditNote arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, arCreditNote.PK, 3);
			APCreditNote apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, apCreditNote.PK, 4);
			ARAdjustmentNote arAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, arAdjustmentNote.PK, 5);
			APAdjustmentNote apAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, apAdjustmentNote.PK, 6);
			TransactionPendingAllocation unallocatedTransactionAPCreditNote =
				Factory.NewWithValidTestData<TransactionPendingAllocation>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, unallocatedTransactionAPCreditNote.PK, 7);
			unallocatedTransactionAPCreditNote.AH_OSExTaxAmount = 100m;
			TransactionPendingAllocation unallocatedTransactionAPInvoice =
				Factory.NewWithValidTestData<TransactionPendingAllocation>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, unallocatedTransactionAPInvoice.PK, 8);
			unallocatedTransactionAPInvoice.AH_OSExTaxAmount = -100m;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			WIP wip = Factory.NewWithValidTestData<WIP>();
			wip.AL_AG = ObjectCreator.GLHeader1.PK;
			wip.AL_JH = job.PK;
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, wip.PK, 9);
			wip.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			charge.JR_AL_ARLine = wip.PK;
			Accrual accrual = Factory.NewWithValidTestData<Accrual>();
			accrual.AL_AG = ObjectCreator.GLHeader1.PK;
			accrual.AL_JH = job.PK;
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, accrual.PK, 10);
			accrual.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			charge.JR_AL_APLine = accrual.PK;
			WIP wip2 = Factory.NewWithValidTestData<WIP>();
			wip2.AL_AG = ObjectCreator.GLHeader1.PK;
			wip2.AL_JH = job.PK;
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, wip2.PK, 11);
			wip2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			wip2.AL_ReverseDate = ZDateTime.Today;
			Accrual accrual2 = Factory.NewWithValidTestData<Accrual>();
			accrual2.AL_AG = ObjectCreator.GLHeader1.PK;
			accrual2.AL_JH = job.PK;
			ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, accrual2.PK, 12);
			accrual2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			accrual2.AL_ReverseDate = ZDateTime.Today;

			Factory.Save();

			GUIWrapperNoSave.IncludeAPAdjustmentNotes = true;
			GUIWrapperNoSave.IncludeARAdjustmentNotes = true;
			GUIWrapperNoSave.IncludeAPCreditNotes = true;
			GUIWrapperNoSave.IncludeARCreditNotes = true;
			GUIWrapperNoSave.IncludeWipsPosting = true;
			GUIWrapperNoSave.IncludeWipsReversing = true;

			GUIWrapperNoSave.ExistingBatchNumberToExport = 1;
			GUIWrapperNoSave.Export();

			AssertEquals("Export process should be called", true, GUIWrapperNoSave.ExportProcessHasBeenRun);
			AssertEquals("Should be no error", false, GUIWrapperNoSave.DataExporter.ErrorHasOccured);
		}

		public void TestMessageWhenExportingAnExistingBatchWithNoTransactions()
		{
			GUIWrapperNoSave.ExistingBatchNumberToExport = 1;
			GUIWrapperNoSave.Export();

			AssertEquals("Message should be shown", "Unable to find any transactions in Batch Number 1",
				UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Export process should not be called", false, GUIWrapperNoSave.ExportProcessHasBeenRun);
		}

		public void TestHighWaterMarkMessage()
		{
			CodeDescriptionBoolCollection defaultCollection = new CodeDescriptionBoolCollection
				{
					{ ExportTransactionTypes.ARInvoice, (NoResString)"AR Invoice", true },
					{ ExportTransactionTypes.APInvoice, (NoResString)"AP Invoice", true },
					{ ExportTransactionTypes.ARCreditNote, (NoResString)"AR Credit Note", false },
					{ ExportTransactionTypes.APCreditNote, (NoResString)"AP Credit Note", false },
					{ ExportTransactionTypes.ARAdjustmentNote, (NoResString)"AR Adjustment Note", false },
					{ ExportTransactionTypes.APAdjustmentNote, (NoResString)"AP Adjustment Note", false },
					{ ExportTransactionTypes.WIPPosting, (NoResString)"WIP Posting", false },
					{ ExportTransactionTypes.AccrualPosting, (NoResString)"Accrual Posting", false },
					{ ExportTransactionTypes.WIPReversal, (NoResString)"WIP Reversal", false },
					{ ExportTransactionTypes.AccrualReversal, (NoResString)"Accrual Reversal", false },
					{ ExportTransactionTypes.UnallocatedAPInvoices, (NoResString)"Unallocated AP Invoices", false },
					{ ExportTransactionTypes.UnallocatedAPCreditNotes, (NoResString)"Unallocated AP Credit Notes", false }
				};
			SystemDataRegistry.Instance.AccountingTransactionTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty,
				Guid.Empty, defaultCollection);

			using (SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.SetValue(Env.CurrentCompany.PK, Guid.Empty,
					Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}

			var highWaterMarkMessage =
				string.Format(
					@"You have selected the same transaction types as nominated in the registry ('System/Data Export Settings/Accounting Transaction Types').
To aid performance of the export, the system will only search for un-batched transactions that were created or edited since {0}.
If you need to export transactions from before this date, please use the date filtering and this will search for all un-batched transactions.",
					SystemDataRegistry.Instance.AccountingTransactionsExportHighWaterMark.Value);

			var gUIWrapper = (XmlExportGUIWrapper)GetNewBusinessObject();
			gUIWrapper.IncludeARInvoices = true;

			AssertEquals("Message should not be shown because transaction types do not match registry", string.Empty,
				gUIWrapper.HighWaterMarkMessage);

			gUIWrapper.IncludeAPInvoices = true;

			if (gUIWrapper.DataExporter is ISupportHighWaterMark)
			{
				AssertEquals("Message should be shown because transaction types match registry", highWaterMarkMessage,
					gUIWrapper.HighWaterMarkMessage);
			}
			else
			{
				AssertNotEquals("Message should not be shown because high water mark is not applicable", string.Empty,
					gUIWrapper.HighWaterMarkMessage);
			}
		}

		public void TestDateFromValidation()
		{
			GUIWrapper.DateFromInfo.ClearAllNotifications();
			GUIWrapper.DateFrom = ZDateTime.Now.AddYears(-2);
			GUIWrapper.ValidateDateFrom();
			AssertHasErrors("Date from should have errors", GUIWrapper.DateFromInfo);

			GUIWrapper.DateFromInfo.ClearAllNotifications();
			GUIWrapper.DateFrom = new ZDateTime(1899, 12, 31);
			GUIWrapper.ValidateDateFrom();
			AssertHasErrors("Date from should have errors", GUIWrapper.DateFromInfo);
			GUIWrapper.DateFromInfo.ClearAllNotifications();
			GUIWrapper.DateFrom = new ZDateTime(2079, 6, 7);
			GUIWrapper.ValidateDateFrom();
			AssertHasErrors("Date from should have errors", GUIWrapper.DateFromInfo);
		}

		public void TestDateToValidation()
		{
			GUIWrapper.DateToInfo.ClearAllNotifications();
			GUIWrapper.DateTo = ZDateTime.Now.AddYears(2);
			GUIWrapper.ValidateDateTo();
			AssertHasErrors("Date to should have errors", GUIWrapper.DateToInfo);

			GUIWrapper.DateToInfo.ClearAllNotifications();
			GUIWrapper.DateTo = new ZDateTime(1899, 12, 31);
			GUIWrapper.ValidateDateTo();
			AssertHasErrors("Date to should have errors", GUIWrapper.DateToInfo);
			GUIWrapper.DateToInfo.ClearAllNotifications();
			GUIWrapper.DateTo = new ZDateTime(2079, 6, 7);
			GUIWrapper.ValidateDateTo();
			AssertHasErrors("Date to should have errors", GUIWrapper.DateToInfo);
		}

		public void TestEmptyDatesTogether()
		{
			GUIWrapper.DateFrom = ZDateTime.Now;
			GUIWrapper.DateTo = ZDateTime.Empty;
			GUIWrapper.ValidateDatesTogether_ForTestOnly();
			AssertHasErrors("DateTo should have errors", GUIWrapper.DateToInfo);

			GUIWrapper.DateTo = ZDateTime.Now;
			GUIWrapper.DateFrom = ZDateTime.Empty;
			GUIWrapper.ValidateDatesTogether_ForTestOnly();
			AssertHasErrors("DateFrom should have errors", GUIWrapper.DateFromInfo);

			GUIWrapper.DateTo = ZDateTime.Empty;
			GUIWrapper.DateFrom = ZDateTime.Empty;
			GUIWrapper.ValidateDatesTogether_ForTestOnly();
			AssertNoErrors("Should be no Errors", GUIWrapper.DateFromInfo);
			AssertNoErrors("Should be no Errors", GUIWrapper.DateToInfo);
		}

		public void TestPeriodFromValidation()
		{
			GUIWrapper.PeriodFromInfo.ClearAllNotifications();
			GUIWrapper.PeriodFrom = 200401;
			GUIWrapper.ValidatePeriodFrom();
			AssertHasErrors("Period from should have errors", GUIWrapper.PeriodFromInfo);
		}

		public void TestPeriodToValidation()
		{
			GUIWrapper.PeriodToInfo.ClearAllNotifications();
			GUIWrapper.PeriodTo = 200512;
			GUIWrapper.ValidatePeriodTo();
			AssertHasErrors("Period to should have errors", GUIWrapper.PeriodToInfo);
		}

		public void TestDateToOutOfBoundsValidation()
		{
			GUIWrapper.DateToInfo.ClearAllNotifications();
			GUIWrapper.DateTo = ZDateTime.MaxSmallDateTime.AddDays(+10);
			AssertNoExceptionThrown(GUIWrapper.ValidateDateTo);
		}

		public void TestEmptyPeriodsTogether()
		{
			GUIWrapper.PeriodFrom = 200601;
			GUIWrapper.PeriodTo = 0;
			GUIWrapper.ValidatePeriodsTogether_ForTestOnly();
			AssertHasErrors("PeriodTo should have errors", GUIWrapper.PeriodToInfo);

			GUIWrapper.PeriodTo = 200501;
			GUIWrapper.PeriodFrom = 0;
			GUIWrapper.ValidatePeriodsTogether_ForTestOnly();
			AssertHasErrors("PeriodFrom should have errors", GUIWrapper.PeriodFromInfo);

			GUIWrapper.PeriodTo = 0;
			GUIWrapper.PeriodFrom = 0;
			GUIWrapper.ValidatePeriodsTogether_ForTestOnly();
			AssertNoErrors("Should be no Errors", GUIWrapper.PeriodFromInfo);
			AssertNoErrors("Should be no Errors", GUIWrapper.PeriodToInfo);
		}

		public void TestTransactionNumbersTogether()
		{
			GUIWrapper.TransactionNumberFromInfo.ClearAllNotifications();
			GUIWrapper.TransactionNumberToInfo.ClearAllNotifications();

			GUIWrapper.TransactionNumberFrom = "ABC";
			GUIWrapper.TransactionNumberTo = "123";
			GUIWrapper.ValidateTransactionNumbersTogether_ForTestOnly();
			AssertHasErrors("Transaction Number FROM should have Error", GUIWrapper.TransactionNumberFromInfo);

			GUIWrapper.TransactionNumberFromInfo.ClearAllNotifications();
			GUIWrapper.TransactionNumberToInfo.ClearAllNotifications();

			GUIWrapper.TransactionNumberTo = "ABC";
			GUIWrapper.TransactionNumberFrom = "a";
			GUIWrapper.ValidateTransactionNumbersTogether_ForTestOnly();
			AssertNoErrors("Should be no Errors", GUIWrapper.TransactionNumberFromInfo);
			AssertNoErrors("Should be no Errors", GUIWrapper.TransactionNumberToInfo);
		}

		public void TestExistingBatchNumberChanged()
		{
			XmlExportGUIWrapper gUIWrapper = new XmlExportGUIWrapper(Factory);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeARCreditNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeARAdjustmentNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeAPInvoicesInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeAPCreditNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeAPAdjustmentNotesInfo.ReadOnly);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeWipsPostingInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeWipsReversingInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeAccrualsPostingInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.IncludeAccrualsReversingInfo.ReadOnly);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.ExcludeJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.ExcludeJobRelatedTransactionsForAPInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForAPInfo.ReadOnly);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.DateFromInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.DateToInfo.ReadOnly);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.PeriodFromInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.PeriodToInfo.ReadOnly);

			AssertEquals("Should not be readonly by default", false, gUIWrapper.TransactionNumberFromInfo.ReadOnly);
			AssertEquals("Should not be readonly by default", false, gUIWrapper.TransactionNumberToInfo.ReadOnly);

			// Set the wrapper to an existing batch number
			gUIWrapper.ExistingBatchNumberToExport = 1;

			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeARCreditNotesInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.IncludeARAdjustmentNotesInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeAPInvoicesInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeAPCreditNotesInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.IncludeAPAdjustmentNotesInfo.ReadOnly);

			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeWipsPostingInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeWipsReversingInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.IncludeAccrualsPostingInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.IncludeAccrualsReversingInfo.ReadOnly);

			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.ExcludeJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.ExcludeJobRelatedTransactionsForAPInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForAPInfo.ReadOnly);

			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.DateFromInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.DateToInfo.ReadOnly);

			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.PeriodFromInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", true, gUIWrapper.PeriodToInfo.ReadOnly);

			AssertEquals("Should be readonly due to existing batch number", false, gUIWrapper.TransactionNumberFromInfo.ReadOnly);
			AssertEquals("Should be readonly due to existing batch number", false, gUIWrapper.TransactionNumberToInfo.ReadOnly);

			// Reset the wrapper to no existing batch number; ie. a new batch number
			gUIWrapper.ExistingBatchNumberToExport = 0;

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeARCreditNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeARAdjustmentNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeAPInvoicesInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeAPCreditNotesInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeAPAdjustmentNotesInfo.ReadOnly);

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeWipsPostingInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeWipsReversingInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeAccrualsPostingInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.IncludeAccrualsReversingInfo.ReadOnly);

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.ExcludeJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForARInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.ExcludeJobRelatedTransactionsForAPInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.ExcludeNonJobRelatedTransactionsForAPInfo.ReadOnly);

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.DateFromInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.DateToInfo.ReadOnly);

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.PeriodFromInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.PeriodToInfo.ReadOnly);

			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.TransactionNumberFromInfo.ReadOnly);
			AssertEquals("Should not be readonly as existing batch number is not selected", false,
				gUIWrapper.TransactionNumberToInfo.ReadOnly);
		}

		public void TestCollectionsAreReadOnly()
		{
			AssertEquals("Selected Jobs should not be readonly", false, GUIWrapper.SelectedJobs.ReadOnly);
			AssertEquals("Selected Organisations should not be readonly", false, GUIWrapper.SelectedOrganisations.ReadOnly);
			AssertEquals("Selected Branches should not be readonly", false, GUIWrapper.SelectedBranches.ReadOnly);
			AssertEquals("Selected Departments should not be readonly", false, GUIWrapper.SelectedDepartments.ReadOnly);
		}

		public void TestExistingBatchChanged()
		{
			GUIWrapperWithoutSaveDialog wrapper = new GUIWrapperWithoutSaveDialog(Factory);
			AssertEquals("Event should not have been fired at this point", false, wrapper.EventFired);
			wrapper.ExistingBatchNumberToExport = 5;
			AssertEquals("Event should have been fired at this point", true, wrapper.EventFired);
		}

		public void TestCaption()
		{
			GUIWrapperWithoutSaveDialog wrapper = new GUIWrapperWithoutSaveDialog(Factory);
			AssertEquals("Export Accounting Transactions", wrapper.FormCaption);
		}

		public void TestErrorIsDisplayedIfIncorrectNumberOfTransactionsAreExported()
		{
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).FilterProvider.IncludeARInvoices = true;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).FilterProvider.IncludeWIPsPosting = true;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).FilterProvider.IncludeAccrualsReversing =
				true;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ObjectCreator.CreateGenExportBatchSequenceHeader(1, invoice.PK, 1);
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.Job.JH_GB = charge.JR_GB;
			WIP wip = Factory.NewWithValidTestData<WIP>();
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, wip.PK, 2);
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AG = ObjectCreator.GLHeader1.PK;
			Accrual accrual = Factory.NewWithValidTestData<Accrual>();
			ObjectCreator.CreateGenExportBatchSequencePostLine(1, accrual.PK, 3);
			accrual.AL_ReverseDate = ZDateTime.Now;
			accrual.AL_JH = charge.JR_JH;
			accrual.AL_AG = ObjectCreator.GLHeader1.PK;

			Factory.Save();

			GUIWrapperNoSave.ExistingBatchNumberToExport = 1;
			GUIWrapperNoSave.LoadFormAndExport_ForTestOnly();

			ZString expectedMessage = "Batch 1 was exported successfully";
			ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			Assert("Error should be shown", lastMessage.Contains(expectedMessage));
		}

		public void TestExportShowsFriendlyMessageToUserIfMultipleUsersArePerformingExport()
		{
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).FilterProvider.IncludeARInvoices = true;

			ObjectCreator.CreateTestPeriods(ZDate.Today);

			ARInvoice invoice = ObjectCreator.CreateARInvoice<ARInvoice>("tst 001", ObjectCreator.AUD, 1, ObjectCreator.ABIGAS);
			ObjectCreator.CreateARInvoiceLine(invoice, ObjectCreator.Job1, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "Test line",
				100m);
			ObjectCreator.CreateJobCharge(invoice.Lines[0], ObjectCreator.Job1, ObjectCreator.CC1, ObjectCreator.AUD);
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_FullyPaidDate = invoice.AH_OutstandingAmount == 0 ? ZDateTime.Now : ZDateTime.Empty;
			invoice.Validation.ValidateAll();
			AssertNoErrors(invoice);
			Factory.Save();

			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				string sql = @"EXEC sp_getapplock @Resource = 'AccountingTransactionExportBatchCreationEDI',
														@LockMode = 'Exclusive',
														@LockOwner =  'Transaction', 
														@LockTimeout = '0',
														@DbPrincipal = 'public'";

				extraConnection.BeginTransaction(); // for test reason
				extraConnection.ExecuteNonQuery(sql); // for test reason

				GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
				GUIWrapperNoSave.LoadFormAndExport_ForTestOnly();

				extraConnection.RollbackTransaction(); // for test reason
			}

			ZString expectedMessage =
				"Another user is performing an export. Please wait for sometime and try again. If the problem persists then please contact support.";
			ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			Assert("Error should be shown", lastMessage.Contains(expectedMessage));
		}

		public void TestProgressForm()
		{
			Factory.NewWithValidTestData(typeof(ARInvoice));
			Factory.Save();
			GUIWrapperNoSave.IncludeARInvoices = true;
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			AssertNull("Pre-Condition: Progress Form should be NULL", GUIWrapperNoSave.ProgressForm_ForTestOnly);
			GUIWrapperNoSave.Export(ParentForm);
			Assert("Temp File should be created", File.Exists(GUIWrapperNoSave.TempFile));
			Assert("Progress Bar was NOT shown", GUIWrapperNoSave.ProgressFormWasShown);
			Assert("Processing Progress Event was NOT fired", GUIWrapperNoSave.ProcessingProgressEventWasFired);
			Assert("Processing Progress Form was not disposed", GUIWrapperNoSave.ProgressForm_ForTestOnly.IsDisposed);
			Assert("ShowCancelButton should be false", !GUIWrapperNoSave.ShowCancelButton);
			AssertNotNull("Parent Form should NOT be Null", GUIWrapperNoSave.ParentForm_ForTestOnly);
			AssertEquals(
				"Progress Form Text should be 'Export Accounting Transactions' but was " + GUIWrapperNoSave.ProgressFormText,
				"Export Accounting Transactions", GUIWrapperNoSave.ProgressFormText);
		}

		[ExpectNoExceptions()]
		public void TestExportFileAlreadyExists()
		{
			Factory.NewWithValidTestData(typeof(ARInvoice));
			Factory.Save();
			GUIWrapperNoSave.IncludeARInvoices = true;
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			File.Create(GUIWrapperNoSave.TempFile).Close();
			GUIWrapperNoSave.Export(ParentForm);
			Assert("Temp File should over write the existing file", File.Exists(GUIWrapperNoSave.TempFile));
		}

		public void TestFileDialogResultIsNotOK()
		{
			Factory.NewWithValidTestData(typeof(ARInvoice));
			Factory.Save();
			GUIWrapperNoSave.IncludeARInvoices = true;
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			GUIWrapperNoSave.ExposedDialogResult = DialogResult.No;
			GUIWrapperNoSave.Export(ParentForm);
			Assert("Temp File should not be exported/created", !File.Exists(GUIWrapperNoSave.TempFile));
		}

		[ExpectNoExceptions("Invalid dates may cause the Form to Freeze and throw an Exception")]
		public void TestDatesBeforeExport()
		{
			BusinessObject invoice = Factory.NewWithValidTestData(typeof(ARInvoice));
			Factory.Save();
			GUIWrapperNoSave.IncludeARInvoices = true;
			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;

			GUIWrapperNoSave.DateFromInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.DateFromInfo.ClearAllNotifications();
			GUIWrapperNoSave.DateToInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.DateToInfo.ClearAllNotifications();
			GUIWrapperNoSave.PeriodFromInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.PeriodFromInfo.ClearAllNotifications();
			GUIWrapperNoSave.PeriodToInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.PeriodToInfo.ClearAllNotifications();
			GUIWrapperNoSave.TransactionNumberToInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.TransactionNumberToInfo.ClearAllNotifications();
			GUIWrapperNoSave.TransactionNumberFromInfo.AddError("error");
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);

			GUIWrapperNoSave.TransactionNumberFromInfo.ClearAllNotifications();
			GUIWrapperNoSave.Export();
			AssertEquals("Export should not be run", true, GUIWrapperNoSave.ExportProcessHasBeenRun);
		}

		public void TestTotalNumberOfWIPsAndAccrualValidation()
		{
			GUIWrapperNoSave.IncludeAccrualsPosting = true;
			GUIWrapperNoSave.IncludeAccrualsReversing = true;
			GUIWrapperNoSave.IncludeWipsPosting = true;
			GUIWrapperNoSave.IncludeWipsReversing = true;

			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).UseBaseTransactionCounts = false;

			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfAccrualPostingsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfAccrualReversalsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfWipPostingsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfWipReversalsInBatch = 10000;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.Job.JH_GB = charge.JR_GB;
			WIP wip = Factory.NewWithValidTestData<WIP>();
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AG = ObjectCreator.GLHeader1.PK;
			Factory.Save();
			//Now the total WIPs and Accruals being exported is greater than 32767

			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			GUIWrapperNoSave.Export();
			try
			{
				AssertEquals("Export should not be run", false, GUIWrapperNoSave.ExportProcessHasBeenRun);
				AssertEquals("Error Message",
					"You cannot export more than 32767 WIP and Accrual transactions in a single batch.\r\nYou must modify the filter criteria to reduce the number of WIPs and Accruals being exported.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestTotalNumberOfWIPsAndAccrualValidationWithNoFiltersSet()
		{
			GUIWrapperNoSave.IncludeAccrualsPosting = false;
			GUIWrapperNoSave.IncludeAccrualsReversing = false;
			GUIWrapperNoSave.IncludeWipsPosting = false;
			GUIWrapperNoSave.IncludeWipsReversing = false;
			GUIWrapperNoSave.IncludeARInvoices = true;

			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).UseBaseTransactionCounts = false;

			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfAccrualPostingsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfAccrualReversalsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfWipPostingsInBatch = 10000;
			((XmlAccountingTransactionExporterForTest)GUIWrapperNoSave.DataExporter).fNumberOfWipReversalsInBatch = 10000;

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.Job.JH_GB = charge.JR_GB;
			WIP wip = Factory.NewWithValidTestData<WIP>();
			charge.JR_AL_ARLine = wip.PK;
			wip.AL_JH = charge.JR_JH;
			wip.AL_AG = ObjectCreator.GLHeader1.PK;
			Factory.Save();
			//Now the total WIPs and Accruals possibly being exported is greater than 32767, but the WIP/Accrual options aren't selected

			GUIWrapperNoSave.ExistingBatchNumberToExport = 0;
			GUIWrapperNoSave.Export();
			Assert("Export should run OK", GUIWrapperNoSave.ExportProcessHasBeenRun);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new XmlExportGUIWrapper(Factory);
		}

		Form ParentForm;

		TestObjectCreator ObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			GUIWrapper = new XmlExportGUIWrapper(Factory);
			GUIWrapperNoSave = new GUIWrapperWithoutSaveDialog(Factory);
			GUIWrapperNoSave.TempFile = Path.Combine(EnvProxy.Instance.TempPath, "TemporaryFile");
			ParentForm = new ZForm();
		}

		protected override void TearDown()
		{
			DeleteIfExists(GUIWrapperNoSave.TempFile);
			((ZForm)ParentForm).Dispose();
			base.TearDown();
		}

		#region GUIWrapperWithoutSaveDialog

		class GUIWrapperWithoutSaveDialog : XmlExportGUIWrapper
		{
			public GUIWrapperWithoutSaveDialog(BusinessObjectFactory factory)
				: base(factory)
			{
				ExistingBatchChanged += new EventHandler(GUIWrapperWithoutSaveDialog_ExistingBatchChanged);
				DataExporter.ProcessingProgressed += new EventHandler(DataExporter_ProcessingProgressed);
			}

			protected override void LoadFormAndExport()
			{
				base.LoadFormAndExport();
				fExportProcessHasBeenRun = true;
			}

			public DialogResult ExposedDialogResult = DialogResult.OK;

			protected override DialogResult ShowDialog(IFileDialog dialog)
			{
				dialog.FileName = TempFile;
				return ExposedDialogResult;
			}

			protected override void ShowProgressForm()
			{
				base.ShowProgressForm();
				ProgressFormWasShown = true;
				FormWasDisplayedModally = ProgressForm.Modal;
			}

			public bool EventFired
			{
				get { return fEventFired; }
			}

			bool fEventFired;

			public bool ExportProcessHasBeenRun
			{
				get { return fExportProcessHasBeenRun; }
			}

			bool fExportProcessHasBeenRun;

			public ZString TempFile = ZString.Empty;
			public ZString ProgressFormText = ZString.Empty;
			public bool ProgressFormWasShown;
			public bool ProcessingProgressEventWasFired;
			public bool ShowCancelButton;
			public bool FormWasDisplayedModally;

			void GUIWrapperWithoutSaveDialog_ExistingBatchChanged(object sender, EventArgs e)
			{
				fEventFired = true;
			}

			protected internal override AccountingTransactionsDataExporter DataExporter
			{
				get
				{
					if (fDataExporter == null)
					{
						fDataExporter = new XmlAccountingTransactionExporterForTest(Factory);
					}

					return fDataExporter;
				}
			}

			void DataExporter_ProcessingProgressed(object sender, EventArgs e)
			{
				ProcessingProgressEventWasFired = true;
				ProgressFormText = ProgressForm.Text;
				ShowCancelButton = ProgressForm.ShowCancelButton;
			}

			protected override string InitialDirectory
			{
				get { return Env.TempPath; }
			}
		}

		class XmlAccountingTransactionExporterForTest : XmlAccountingTransactionExporter
		{
			public XmlAccountingTransactionExporterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZInt NumberOfAccrualPostingsInBatch
			{
				get
				{
					return UseBaseTransactionCounts ? base.NumberOfAccrualPostingsInBatch : fNumberOfAccrualPostingsInBatch;
				}
			}

			public ZInt fNumberOfAccrualPostingsInBatch;

			public override ZInt NumberOfAccrualReversalsInBatch
			{
				get
				{
					return UseBaseTransactionCounts ? base.NumberOfAccrualReversalsInBatch : fNumberOfAccrualReversalsInBatch;
				}
			}

			public ZInt fNumberOfAccrualReversalsInBatch;

			public override ZInt NumberOfWipPostingsInBatch
			{
				get
				{
					return UseBaseTransactionCounts ? base.NumberOfWipPostingsInBatch : fNumberOfWipPostingsInBatch;
				}
			}

			public ZInt fNumberOfWipPostingsInBatch;

			public override ZInt NumberOfWipReversalsInBatch
			{
				get
				{
					return UseBaseTransactionCounts ? base.NumberOfWipReversalsInBatch : fNumberOfWipReversalsInBatch;
				}
			}

			public ZInt fNumberOfWipReversalsInBatch;

			public ZBool UseBaseTransactionCounts = true;
		}

		#endregion

		XmlExportGUIWrapper GUIWrapper;
		GUIWrapperWithoutSaveDialog GUIWrapperNoSave;

		#endregion
	}
}

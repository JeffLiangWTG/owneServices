using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	[TestedType(typeof(AccountingVoucherPrintWrapper))]
	public class AccountingVoucherPrintWrapperTest : NonPersistentBusinessObjectTestCase
	{
		// New Test
		public void TestLedgerTypeList()
		{
			OptionalFilterCriteriaList ledgerList = new LedgerTransactionAssociator().GetLedgerList();
			AssertEquals(ledgerList.Count, TestWrapper.LedgerTypeList.Count);
			int i = 0;
			foreach (OptionalFilterCriteria criteria in ledgerList)
			{
				AssertEquals(criteria.Description, TestWrapper.LedgerTypeList[i].Description);
				i++;
			}
		}

		// New Test
		public void TestTransactionTypeInitially()
		{
			AssertNotNull(TestWrapper.TransactionTypeList);
			AssertEquals(0, TestWrapper.TransactionTypeList.Count);
		}

		// New Test
		public void TestValidateFromPeriodReturnNoError()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.True;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.ValidateFromPeriod();
			AssertNoErrors(TestWrapper.FromPeriodInfo);
		}

		//New Test
		public void TestPeriodEntered()
		{
			TestWrapper.ValidateFromPeriod();
			AssertHasErrorContaining(TestWrapper.FromPeriodInfo, "Please enter a value.");
		}

		public void TestValidateFromPeriodWhenPeriodIsClosed()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.True;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.ValidateFromPeriod();
			Assert(!TestWrapper.FromPeriodInfo.HasError("This period is not closed."));
		}

		public void TestValidateFromPeriodWhenPeriodIsOpen()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.False;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.ValidateFromPeriod();
			AssertHasError(TestWrapper.FromPeriodInfo, "This period is not closed.");
		}

		public void TestValidatePeriodIfAccrualIsSelected()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 201502;
			SetAccrualSelected(TestWrapper);
			TestWrapper.ValidateFromPeriod();
			AssertHasError("The period must be closed if Accrual is selected.", TestWrapper.FromPeriodInfo, "This period is not closed.");
			TestWrapper.FromPeriod = 201501;
			TestWrapper.ValidateFromPeriod();
			AssertNoError(TestWrapper.FromPeriodInfo, "This period is not closed.");
		}

		public void TestValidatePeriodIfWIPIsSelected()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 201502;
			SetWIPSelected(TestWrapper);
			TestWrapper.ValidateFromPeriod();
			AssertHasError("The period must be closed if WIP is selected.", TestWrapper.FromPeriodInfo, "This period is not closed.");
			TestWrapper.FromPeriod = 201501;
			TestWrapper.ValidateFromPeriod();
			AssertNoError(TestWrapper.FromPeriodInfo, "This period is not closed.");
		}

		public void TestValidatePeriodIfGeneralJournalIsSelected()
		{
			SetupPeriods();
			TestWrapper.LedgerTypeList[4].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.LedgerTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[0].Enabled = true;
			TestWrapper.ValidateFromPeriod();
			AssertHasError("The period must be closed if Accrual is selected.", TestWrapper.FromPeriodInfo, "This period is not closed.");
			TestWrapper.FromPeriod = 201501;
			TestWrapper.ValidateFromPeriod();
			AssertNoError(TestWrapper.FromPeriodInfo, "This period is not closed.");
		}

		//New Test
		public void TestFromDateCheckValidZDateTimeWithoutRange()
		{
			TestWrapper.FromDate = ZDateTime.Invalid;
			TestWrapper.ValidateFromDate();
			AssertHasErrorContaining(TestWrapper.FromDateInfo, "Enter a valid selection.");
		}

		// New Test
		public void TestFromDateCheckValidZDateTimeRange()
		{
			ZDateTime tenYearsEarlier = new ZDateTime(ZDateTime.Now.AddYears(-10));
			TestWrapper.FromDate = tenYearsEarlier;
			TestWrapper.ValidateFromDate();
			AssertHasWarnings(TestWrapper.FromDateInfo);
		}

		// New Test
		public void TestFromDateCheckEntered()
		{
			TestWrapper.ValidateFromDate();
			AssertHasErrorContaining(TestWrapper.FromDateInfo, "Please enter a value.");
		}

		public void TestValidateFromDateIfFromPeriodSetForVoucher()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 201502;
			TestWrapper.FromDate = new ZDateTime(2015, 1, 15);
			TestWrapper.ValidateFromDate();
			AssertHasError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
			TestWrapper.FromDate = new ZDateTime(2015, 2, 15);
			TestWrapper.ValidateFromDate();
			AssertNoError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
			TestWrapper.FromDate = new ZDateTime(2015, 3, 15);
			TestWrapper.ValidateFromDate();
			AssertHasError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
		}

		//New Test
		public void TestEndDateCheckValidZDateTimeWithoutRange()
		{
			TestWrapper.EndDate = ZDateTime.Invalid;
			TestWrapper.ValidateEndDate();
			AssertHasErrorContaining(TestWrapper.EndDateInfo, "Enter a valid selection.");
		}

		// New Test
		public void TestEndDateCheckValidZDateTimeRange()
		{
			ZDateTime tenYearsEarlier = new ZDateTime(ZDateTime.Now.AddYears(-10));
			TestWrapper.EndDate = tenYearsEarlier;
			TestWrapper.ValidateEndDate();
			AssertHasWarnings(TestWrapper.EndDateInfo);
		}

		// New Test
		public void TestEndDateCheckEntered()
		{
			TestWrapper.ValidateEndDate();
			AssertHasErrorContaining(TestWrapper.EndDateInfo, "Please enter a value.");
		}

		public void TestValidateToDateIfFromPeriodSetForVoucher()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 201502;
			TestWrapper.EndDate = new ZDateTime(2015, 1, 15);
			TestWrapper.ValidateEndDate();
			AssertHasError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
			TestWrapper.EndDate = new ZDateTime(2015, 2, 15);
			TestWrapper.ValidateEndDate();
			AssertNoError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
			TestWrapper.EndDate = new ZDateTime(2015, 3, 15);
			TestWrapper.ValidateEndDate();
			AssertHasError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
		}

		public void TestGenerateDocWrapperWithAccTransactionHeaders()
		{
			AccTransactionHeader[] transactionHeaders = new AccTransactionHeader[22];
			ARInvoice testARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARCreditNote testARCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			APCreditNote testAPCreditNote = Factory.NewWithValidTestData(typeof(APCreditNote)) as APCreditNote;
			ARAdjustmentNote testARAdjustmentNote = Factory.NewWithValidTestData(typeof(ARAdjustmentNote)) as ARAdjustmentNote;
			APAdjustmentNote testAPAdjustmentNote = Factory.NewWithValidTestData(typeof(APAdjustmentNote)) as APAdjustmentNote;
			ARJournal testARJournal = Factory.NewWithValidTestData(typeof(ARJournal)) as ARJournal;
			ARContraRow testARContraRow = Factory.NewWithValidTestData(typeof(ARContraRow)) as ARContraRow;
			ARReceipt testARReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			ARPayment testARPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			APJournal testAPJournal = Factory.NewWithValidTestData(typeof(APJournal)) as APJournal;
			APContraRow testAPContraRow = Factory.NewWithValidTestData(typeof(APContraRow)) as APContraRow;
			APReceipt testAPReceipt = Factory.NewWithValidTestData(typeof(APReceipt)) as APReceipt;
			APPayment testAPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			GLJournal testJournal = Factory.NewWithValidTestData(typeof(GLJournal)) as GLJournal;
			DirectPayment testDirectPayment = Factory.NewWithValidTestData(typeof(DirectPayment)) as DirectPayment;
			DirectReceipt testDirectReceipt = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			BankTransferFromRow testBankTransferFrom = Factory.NewWithValidTestData(typeof(BankTransferFromRow)) as BankTransferFromRow;
			BankTransferToRow testBankTransferTo = Factory.NewWithValidTestData(typeof(BankTransferToRow)) as BankTransferToRow;
			BankTransferFromRow testBankTransferFromReversal = Factory.NewWithValidTestData(typeof(BankTransferFromRow)) as BankTransferFromRow;
			testBankTransferFromReversal.AH_TransactionCount = 4;
			BankTransferToRow testBankTransferToReversal = Factory.NewWithValidTestData(typeof(BankTransferToRow)) as BankTransferToRow;
			testBankTransferToReversal.AH_TransactionCount = 5;
			JobRevenueJournal testJobRevenualJournal = Factory.NewWithValidTestData(typeof(JobRevenueJournal)) as JobRevenueJournal;
			testARInvoice.AH_TransactionNum = "0001";
			testAPInvocie.AH_TransactionNum = "0002";
			testAPInvocie.AH_ConsolidatedInvoiceRef = "0001";
			testARCreditNote.AH_TransactionNum = "0003";
			testAPCreditNote.AH_TransactionNum = "0004";
			testAPCreditNote.AH_ConsolidatedInvoiceRef = "0002";
			testARAdjustmentNote.AH_TransactionNum = "0005";
			testAPAdjustmentNote.AH_TransactionNum = "0006";
			testAPAdjustmentNote.AH_ConsolidatedInvoiceRef = "0003";
			testARJournal.AH_TransactionNum = "0007";
			testARContraRow.AH_TransactionNum = "0008";
			testARReceipt.AH_TransactionNum = "0009";
			testARPayment.AH_TransactionNum = "0010";
			testAPJournal.AH_TransactionNum = "0011";
			testAPContraRow.AH_TransactionNum = "0012";
			testAPReceipt.AH_TransactionNum = "0013";
			testAPPayment.AH_TransactionNum = "0014";
			testAPJournal.AH_ConsolidatedInvoiceRef = "0004";
			testAPContraRow.AH_ConsolidatedInvoiceRef = "0005";
			testAPReceipt.AH_ConsolidatedInvoiceRef = "0006";
			testAPPayment.AH_ConsolidatedInvoiceRef = "0007";
			testJournal.AH_TransactionNum = "0015";
			testDirectPayment.AH_TransactionNum = "0016";
			testDirectReceipt.AH_TransactionNum = "0017";
			testBankTransferFrom.AH_TransactionNum = "0018";
			testBankTransferTo.AH_TransactionNum = "0018";
			testBankTransferFrom.AH_TransactionNum = "0019";
			testBankTransferTo.AH_TransactionNum = "0019";
			testJobRevenualJournal.AH_TransactionNum = "0020";
			transactionHeaders[0] = testARInvoice;
			transactionHeaders[1] = testAPInvocie;
			transactionHeaders[2] = testARCreditNote;
			transactionHeaders[3] = testAPCreditNote;
			transactionHeaders[4] = testARAdjustmentNote;
			transactionHeaders[5] = testAPAdjustmentNote;
			transactionHeaders[6] = testARJournal;
			transactionHeaders[7] = testARContraRow;
			transactionHeaders[8] = testARReceipt;
			transactionHeaders[9] = testARPayment;
			transactionHeaders[10] = testAPJournal;
			transactionHeaders[11] = testAPContraRow;
			transactionHeaders[12] = testAPReceipt;
			transactionHeaders[13] = testAPPayment;
			transactionHeaders[14] = testJournal;
			transactionHeaders[15] = testDirectPayment;
			transactionHeaders[16] = testDirectReceipt;
			transactionHeaders[17] = testBankTransferFrom;
			transactionHeaders[18] = testBankTransferTo;
			transactionHeaders[19] = testBankTransferFromReversal;
			transactionHeaders[20] = testBankTransferToReversal;
			transactionHeaders[21] = testJobRevenualJournal;
			DocumentWrapper[] docWrappers = TestWrapper.GenerateDocWrapper(transactionHeaders);
			AssertEquals("should generate 20 DocWrapper", 20, docWrappers.Length);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[0].WrappedObject.GetType().Name);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[1].WrappedObject.GetType().Name);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[2].WrappedObject.GetType().Name);
			AssertEquals("ARAPContraVoucherProvider", docWrappers[3].WrappedObject.GetType().Name);
			AssertEquals("ARAPJournalVoucherProivder", docWrappers[4].WrappedObject.GetType().Name);
			AssertEquals("ReceiptPaymentVoucherProvider", docWrappers[5].WrappedObject.GetType().Name);
			AssertEquals("ReceiptPaymentVoucherProvider", docWrappers[6].WrappedObject.GetType().Name);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[7].WrappedObject.GetType().Name);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[8].WrappedObject.GetType().Name);
			AssertEquals("ARAPContraVoucherProvider", docWrappers[9].WrappedObject.GetType().Name);
			AssertEquals("InvoiceCreditAdjustmentVoucherProvider", docWrappers[10].WrappedObject.GetType().Name);
			AssertEquals("ARAPJournalVoucherProivder", docWrappers[11].WrappedObject.GetType().Name);
			AssertEquals("ReceiptPaymentVoucherProvider", docWrappers[12].WrappedObject.GetType().Name);
			AssertEquals("ReceiptPaymentVoucherProvider", docWrappers[13].WrappedObject.GetType().Name);
			AssertEquals("DirectReceiptPaymentVoucher", docWrappers[14].WrappedObject.GetType().Name);
			AssertEquals("DirectReceiptPaymentVoucher", docWrappers[15].WrappedObject.GetType().Name);
			AssertEquals("CBTransferVoucherProvider", docWrappers[16].WrappedObject.GetType().Name);
			AssertEquals("CBTransferVoucherProvider", docWrappers[17].WrappedObject.GetType().Name);
			AssertEquals("GLJournalVoucherProvider", docWrappers[18].WrappedObject.GetType().Name);
			AssertEquals("CFXVoucherProvider", docWrappers[19].WrappedObject.GetType().Name);
		}

		public void TestDocWrapperGeneratedWithoutFilterSpecified()
		{
			SetupTransaction();
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(0, wrapper.Length);
		}

		public void TestDocWrapperGeneratedWithFilterSetToAR()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(2, wrapper.Length);
		}

		public void TestDocWrapperGeneratedWithFilterSetToPayment()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.LedgerTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[3].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
		}

		public void TestDocWrapperGeneratedWithFilterSetToPaymentAndInvoice()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.LedgerTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[0].Enabled = true;
			TestWrapper.TransactionTypeList[3].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(3, wrapper.Length);
		}

		public void TestDocWrapperGeneratedWithFilterSetToARAndRec()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[4].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
		}

		public void TestGetWIPAccrualVoucherWrapper()
		{
			WIPAccrualDataSourceCollectionTest wIPAccrualTestHelper = new WIPAccrualDataSourceCollectionTest();
			wIPAccrualTestHelper.InsertTestWIPandACR(Factory);
			TestWrapper.FromPeriod = 201501;
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(2, wrapper.Length);
			AssertNotNull(wrapper[0]);
			AssertNotNull(wrapper[1]);
			TestWrapper.TransactionTypeList[0].Enabled = true;
			TestWrapper.TransactionTypeList[1].Enabled = false;
			wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
			TestWrapper.TransactionTypeList[0].Enabled = false;
			TestWrapper.TransactionTypeList[1].Enabled = true;
			wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
			TestWrapper.TransactionTypeList[0].Enabled = false;
			TestWrapper.TransactionTypeList[1].Enabled = false;
			wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(0, wrapper.Length);
		}

		public void TestGetWIPAccrualVoucherWrapperByAllBranch()
		{
			var wIPAccrualTestHelper = new WIPAccrualDataSourceCollectionTest();
			AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			wIPAccrualTestHelper.InsertTestWIPandACR(Factory);
			TestWrapper.FromPeriod = 201501;
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			var wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();

			AssertEquals(2, wrapper.Length);
			AssertNotNull(wrapper[0]);
			AssertNotNull(wrapper[1]);
		}

		public void TestGetWIPAccrualVoucherWrapperBySelectedBranches()
		{
			var wIPAccrualTestHelper = new WIPAccrualDataSourceCollectionTest();
			AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			try
			{
				TestWrapper.SelectedBranches.Add(GlbBranch.CurrentBranch);
				wIPAccrualTestHelper.InsertTestWIPandACR(Factory);
				TestWrapper.FromPeriod = 201501;
				TestWrapper.LedgerTypeList[3].Enabled = true;
				TestWrapper.ReloadTransactionList();
				var wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();

				AssertEquals(2, wrapper.Length);
				AssertNotNull(wrapper[0]);
				AssertNotNull(wrapper[1]);
			}
			finally
			{
				TestWrapper.SelectedBranches.RemoveAll();
			}
		}

		public void TestVoucherPrint_WhenOnlyAmountZeroVoucherLine_ShouldNotCreateVoucherPrintDoc()
		{
			var wIPAccrualTestHelper = new WIPAccrualDataSourceCollectionTest();
			wIPAccrualTestHelper.InsertTestWIPandACR(Factory,isZeroAmount: true);
			TestWrapper.FromPeriod = 201501;
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();

			AssertEquals("Voucher print wrapper should be 0, which means voucher print is not permitted", 0, wrapper.Length);
		}

		public void TestSetFromPeriodSetsDatesCorrectlyForVoucherPrinting()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201501, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			testHelper.SetupSinglePeriod(201502, new ZDateTime(2015, 2, 1), new ZDateTime(2015, 2, 28));
			TestWrapper.FromPeriod = 201501;
			AssertEquals(new ZDateTime(2015, 1, 1), TestWrapper.FromDate);
			AssertEquals(new ZDateTime(2015, 1, 31), TestWrapper.EndDate);
			TestWrapper.FromPeriod = 201502;
			AssertEquals(new ZDateTime(2015, 2, 1), TestWrapper.FromDate);
			AssertEquals(new ZDateTime(2015, 2, 28), TestWrapper.EndDate);
		}

		public void TestIsWIPSelected()
		{
			SetWIPSelected(TestWrapper);
			Assert(TestWrapper.IsWIPSelected_ForTestOnly);
			Assert(!TestWrapper.IsAccrualSelected_ForTestOnly);
		}

		public void TestIsAccrualSelected()
		{
			SetAccrualSelected(TestWrapper);
			Assert(TestWrapper.IsAccrualSelected_ForTestOnly);
			Assert(!TestWrapper.IsWIPSelected_ForTestOnly);
		}

		public void TestContraOnlyGeneratesOneVoucherIfARSelected()
		{
			SetupTransaction();
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra);
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[10].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
		}

		public void TestContraOnlyGeneratesOneVoucherIfAPSelected()
		{
			SetupTransaction();
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra);
			TestWrapper.LedgerTypeList[1].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[10].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
		}

		public void TestContraOnlyGeneratesOneVoucherIfBothLedgerSelected()
		{
			SetupTransaction();
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra);
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.LedgerTypeList[1].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[10].Enabled = true;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
			AssertEquals(1, wrapper.Length);
		}

		[ExpectNoExceptions]
		public void TestRunPreSaveValidation()
		{
			var mockTestWrapper = new Mock<AccountingVoucherPrintWrapper> { CallBase = true };
			mockTestWrapper.Setup(m => m.ValidateFromDate());
			mockTestWrapper.Setup(m => m.ValidateEndDate());
			mockTestWrapper.Setup(m => m.ValidateFromPeriod());
			mockTestWrapper.Object.RunPreSaveValidation();
			mockTestWrapper.VerifyAll();
		}

		public void TestGetWrapperCountForEmptyWrapper()
		{
			AssertEquals(0, TestWrapper.GetWrapperCount());
		}

		public void TestGetWrapperCount()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.LedgerTypeList[0].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.TransactionTypeList.ClearAllSelection();
			TestWrapper.TransactionTypeList[4].Enabled = true;
			TestWrapper.GenerateVoucherDocWrapper();
			AssertEquals(1, TestWrapper.GetWrapperCount());
		}

		[ExpectNoExceptions]
		public void TestPrintDocuments()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetupTransaction();
			var mockTestWrapper = new Mock<AccountingVoucherPrintWrapper>(Factory) { CallBase = true };
			AccountingVoucherPrintWrapper testPrintWrapper = mockTestWrapper.Object;
			testPrintWrapper.FromPeriod = TestPeriod;
			testPrintWrapper.LedgerTypeList[0].Enabled = true;
			testPrintWrapper.ReloadTransactionList();
			testPrintWrapper.TransactionTypeList.ClearAllSelection();
			testPrintWrapper.LedgerTypeList.ClearAllSelection();
			testPrintWrapper.TransactionTypeList[0].Enabled = true;
			testPrintWrapper.TransactionTypeList[3].Enabled = true;
			mockTestWrapper.Setup(m => m.PrintCore());
			testPrintWrapper.GenerateVoucherDocWrapper();
			testPrintWrapper.PrintVoucherDocument();
			mockTestWrapper.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestPrintDocumentsIfEmpty()
		{
			var mockTestWrapper = new Mock<AccountingVoucherPrintWrapper>(Factory) { CallBase = true };
			AccountingVoucherPrintWrapper testPrintWrapper = mockTestWrapper.Object;
			testPrintWrapper.FromPeriod = TestPeriod;
			testPrintWrapper.LedgerTypeList[0].Enabled = true;
			testPrintWrapper.ReloadTransactionList();
			testPrintWrapper.TransactionTypeList.ClearAllSelection();
			testPrintWrapper.LedgerTypeList.ClearAllSelection();
			testPrintWrapper.TransactionTypeList[0].Enabled = true;
			testPrintWrapper.TransactionTypeList[3].Enabled = true;
			mockTestWrapper.Verify(m => m.PrintCore(), Times.Never);
			testPrintWrapper.GenerateVoucherDocWrapper();
			testPrintWrapper.PrintVoucherDocument();
			mockTestWrapper.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestLegacyPrintCore()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SetupTransaction();
			var mockTestWrapper = new Mock<AccountingVoucherPrintWrapper>(Factory) { CallBase = true };
			var testPrintWrapper = mockTestWrapper.Object;

			testPrintWrapper.FromPeriod = TestPeriod;
			testPrintWrapper.LedgerTypeList[0].Enabled = true;
			testPrintWrapper.ReloadTransactionList();
			testPrintWrapper.TransactionTypeList.ClearAllSelection();
			testPrintWrapper.LedgerTypeList.ClearAllSelection();
			testPrintWrapper.TransactionTypeList[0].Enabled = true;
			testPrintWrapper.TransactionTypeList[3].Enabled = true;
			mockTestWrapper.Verify(m => m.GetAccountingVoucherPrintTask(It.IsAny<AccTransactionHeader[]>()), Times.Never);
			testPrintWrapper.GenerateVoucherDocWrapper();
			testPrintWrapper.PrintCore();
			mockTestWrapper.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestDocBuilderPrintCore()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mockTestWrapper = new Mock<AccountingVoucherPrintWrapper>(Factory) { CallBase = true };
			var testPrintWrapper = mockTestWrapper.Object;

			var mockTestPrintTask = new Mock<PrintTask>();

			mockTestWrapper.Setup(m => m.GetAccountingVoucherPrintTask(It.IsAny<AccTransactionHeader[]>())).Returns(mockTestPrintTask.Object);
			testPrintWrapper.PrintCore();
			mockTestWrapper.VerifyAll();
		}

		public void TestBranchListIncludeCurrentBranchOnly()
		{
			GlbBranch currentCompanyBranch = Factory.NewWithValidTestData(typeof(GlbBranch)) as GlbBranch;
			currentCompanyBranch.GB_Code = "CBR";
			currentCompanyBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch nonCurrentCompanyBranch = Factory.NewWithValidTestData(typeof(GlbBranch)) as GlbBranch;
			nonCurrentCompanyBranch.GB_Code = "NBR";
			nonCurrentCompanyBranch.GB_GC = NonCurrentCompany.PK;
			Factory.Save();
			TestWrapper.BranchesList.Load();
			Assert(TestWrapper.BranchesList.Contains(currentCompanyBranch.PK));
			Assert(!TestWrapper.BranchesList.Contains(NonCurrentCompany.PK));
		}

		public void TestBranchFilterOnly()
		{
			TestPeriod = 201501;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie.AH_PostDate = new ZDateTime(2015, 1, 15);
			testAPInvocie.AH_GB = NonCurrentBranch.PK;
			testAPInvocie.AH_TransactionNum = "TEST022";
			GlbBranch currentCompanyBranch = Factory.NewWithValidTestData(typeof(GlbBranch)) as GlbBranch;
			currentCompanyBranch.GB_Code = "CBR";
			currentCompanyBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			try
			{
				ARInvoice testInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
				testInvoice.AH_PostDate = new ZDateTime(2015, 1, 15);
				testInvoice.AH_TransactionNum = "TEST012";
				testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
				testPayment.AH_PostDate = new ZDateTime(2015, 1, 5);
				testPayment.AH_GB = currentCompanyBranch.PK;
				testPayment.AH_TransactionNum = "TEST032";
				TestWrapper.SelectedBranches.Add(GlbBranch.CurrentBranch);
				TestWrapper.SelectedBranches.Add(currentCompanyBranch);
				ZQuery testFilter = new ZQuery();
				testFilter.OrderBy = "AH_TransactionNum";
				TestWrapper.AddBranchesFilter(testFilter);
				testFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				AccTransactionHeader[] transactions = Factory.Load(typeof(AccTransactionHeader), testFilter) as AccTransactionHeader[];
				AssertEquals(2, transactions.Length);
				AssertEquals("TEST012", transactions[0].AH_TransactionNum);
				AssertEquals("TEST032", transactions[1].AH_TransactionNum);
			}
			finally
			{
				TestWrapper.SelectedBranches.RemoveAll();
			}
		}

		public void TestNoBranchSelectedLoadAllBranches()
		{
			TestPeriod = 201501;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			ARInvoice testInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2015, 1, 15);
			testInvoice.AH_Desc = "TEST012";
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie.AH_PostDate = new ZDateTime(2015, 1, 15);
			testAPInvocie.AH_GB = NonCurrentBranch.PK;
			testAPInvocie.AH_Desc = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2015, 1, 5);
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_Desc = "TEST032";
			Factory.Save();
			ZQuery testFilter = new ZQuery();
			testFilter.OrderBy = "AH_Desc";
			TestWrapper.AddBranchesFilter(testFilter);
			testFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			AccTransactionHeader[] transactions = Factory.Load(typeof(AccTransactionHeader), testFilter) as AccTransactionHeader[];
			AssertEquals(3, transactions.Length);
			AssertEquals("TEST012", transactions[0].AH_Desc);
			AssertEquals("TEST022", transactions[1].AH_Desc);
			AssertEquals("TEST032", transactions[2].AH_Desc);
		}

		public void TestGeneratedWrapperWithBranchFilter()
		{
			TestPeriod = 201501;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			APInvoice testInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2015, 1, 15);
			testInvoice.AH_TransactionNum = "TEST012";
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			APInvoice testAPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvoice.AH_PostDate = new ZDateTime(2015, 1, 15);
			testAPInvoice.AH_GB = NonCurrentBranch.PK;
			testAPInvoice.AH_TransactionNum = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2015, 1, 5);
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_TransactionNum = "TEST032";
			Factory.Save();
			TestWrapper.SelectedBranches.Add(GlbBranch.CurrentBranch);
			Assert(!TestWrapper.SelectedBranches.ReadOnly);
			try
			{
				TestWrapper.LedgerTypeList[1].Enabled = true;
				TestWrapper.ReloadTransactionList();
				DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper_ForTestOnly();
				AssertEquals(2, wrapper.Length);
			}
			finally
			{
				TestWrapper.SelectedBranches.Remove(GlbBranch.CurrentBranch);
			}
		}

		public void TestValidateBranchWithJobCosting()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.ValidateBranchWithJobCostingOptionSelected();
			Assert(!TestWrapper.SelectedBranches.HasErrors());
			TestWrapper.SelectedBranches.Add(branch);
			TestWrapper.ValidateBranchWithJobCostingOptionSelected();
			Assert(TestWrapper.SelectedBranches.HasErrors());

			AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestWrapper.ValidateBranchWithJobCostingOptionSelected();
			Assert(!TestWrapper.SelectedBranches.HasErrors());

			TestWrapper.SelectedBranches.Remove(branch);
			TestWrapper.ValidateBranchWithJobCostingOptionSelected();
			Assert(!TestWrapper.SelectedBranches.HasErrors());
		}

		[TestDate(2015, 1, 15)]
		public void TestBranchWithJobCosting_RunPresaveValidation()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			SetupPeriods();
			TestWrapper.FromPeriod = 201501;
			TestWrapper.FromDate = new ZDateTime(2015, 1, 10);
			TestWrapper.EndDate = new ZDateTime(2015, 1, 15);
			TestWrapper.LedgerTypeList[3].Enabled = true;
			TestWrapper.ReloadTransactionList();
			TestWrapper.RunPreSaveValidation();
			AssertNoErrors(TestWrapper);
			TestWrapper.SelectedBranches.Add(branch);
			TestWrapper.RunPreSaveValidation();
			Assert(TestWrapper.HasErrors);
			TestWrapper.SelectedBranches.Remove(branch);
			TestWrapper.RunPreSaveValidation();
			AssertNoErrors(TestWrapper);
		}

		void SetUpForContraDetailTest(Contra testContra)
		{
			testContra.AH_ARAccount = FromAccount.PK;
			testContra.AH_APAccount = ToAccount.PK;
			testContra.AH_OSTotal = 120m;
			testContra.AH_Desc = "TESTDESC";
			AccGLHeader aPControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLAccountDescriptor aRControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aRControlLocal.ParentGLHeaderPK = aRControl.PK;
			aRControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			aRControlLocal.AJ_AccountDescription = "ARControlDescription";
			aRControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccGLAccountDescriptor aPControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aPControlLocal.ParentGLHeaderPK = aPControl.PK;
			aPControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			aPControlLocal.AJ_AccountDescription = "APControlDescription";
			aPControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControl.PK.ToGuid());
		}

		void SetupPeriods()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(201501, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			testHelper.SetupSinglePeriod(201502, new ZDateTime(2015, 2, 1), new ZDateTime(2015, 2, 28));
			AccPeriodManagement period201502 = Factory.LoadTop1(typeof(AccPeriodManagement), new ZQuery(AccPeriodManagementSchema.AM_Period, 201501)) as AccPeriodManagement;
			period201502.AM_IsGeneralLedgerClosed = ZBool.True;
			period201502.AM_IsSubLedgerClosed = ZBool.True;
			Factory.Save();
		}

		void SetAccrualSelected(AccountingVoucherPrintWrapper testWrapper)
		{
			testWrapper.LedgerTypeList[3].Enabled = true;
			testWrapper.ReloadTransactionList();
			testWrapper.TransactionTypeList.ClearAllSelection();
			testWrapper.LedgerTypeList.ClearAllSelection();
			testWrapper.TransactionTypeList[0].Enabled = true;
			testWrapper.TransactionTypeList[1].Enabled = false;
		}

		void SetWIPSelected(AccountingVoucherPrintWrapper testWrapper)
		{
			testWrapper.LedgerTypeList[3].Enabled = true;
			testWrapper.ReloadTransactionList();
			testWrapper.TransactionTypeList.ClearAllSelection();
			testWrapper.LedgerTypeList.ClearAllSelection();
			testWrapper.TransactionTypeList[0].Enabled = false;
			testWrapper.TransactionTypeList[1].Enabled = true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccountingVoucherPrintWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fTestWrapper = null;
		}

		int TestPeriod;
		AccountingVoucherPrintWrapper fTestWrapper;
		AccountingVoucherPrintWrapper TestWrapper
		{
			get
			{
				if (fTestWrapper == null)
				{
					fTestWrapper = new AccountingVoucherPrintWrapper(Factory);
				}

				return fTestWrapper;
			}
		}

		void SetupTransaction()
		{
			TestPeriod = 201501;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 1, 31));
			ARInvoice testInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2015, 1, 15);
			testInvoice.AH_TransactionNum = "TEST012";
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie.AH_PostDate = new ZDateTime(2015, 1, 15);
			testAPInvocie.AH_TransactionNum = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2015, 1, 5);
			testPayment.AH_TransactionNum = "TES45i7";
			ARReceipt testARReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testARReceipt.AH_PostDate = new ZDateTime(2015, 1, 5);
			testARReceipt.AH_TransactionNum = "TES3452i7";
			DirectReceipt testDirectReceipt = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			testDirectReceipt.AH_PostDate = new ZDateTime(2015, 1, 5);
			testDirectReceipt.AH_TransactionNum = "TES345ew2i7";
			AccTransactionHeader testRCB = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testRCB.AH_GB = GlbBranch.CurrentBranch.PK;
			testRCB.AH_GE = GlbDepartment.CurrentDepartment.PK;
			testRCB.AH_TransactionType = TransactionTypes.ReceiptBatch;
			testRCB.AH_Ledger = LedgerTypes.CashBook;
			testRCB.AH_PostDate = new ZDateTime(2015, 1, 5);
			ARInvoice testInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice2.AH_PostDate = new ZDateTime(2003, 1, 15);
			testInvoice2.AH_TransactionNum = "TEST012a";
			APInvoice testAPInvocie2 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie2.AH_PostDate = new ZDateTime(2005, 1, 15);
			testAPInvocie2.AH_TransactionNum = "TEST022a";
			APPayment testPayment2 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment2.AH_PostDate = new ZDateTime(2003, 1, 5);
			testPayment2.AH_TransactionNum = "TES45i7a";
			ARReceipt testARReceipt2 = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testARReceipt2.AH_PostDate = new ZDateTime(2005, 1, 5);
			testARReceipt2.AH_TransactionNum = "TES3452i7a";
			Factory.Save();
		}

		OrgHeader fFromAccount;
		protected OrgHeader FromAccount
		{
			get
			{
				if (fFromAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(fromAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.PK, ToAccount.PK);
					fFromAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fFromAccount;
			}
		}

		OrgHeader fToAccount;
		protected OrgHeader ToAccount
		{
			get
			{
				if (fToAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery toAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(toAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					fToAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fToAccount;
			}
		}

		GlbBranch fNonCurrentBranch;
		protected GlbBranch NonCurrentBranch
		{
			get
			{
				if (fNonCurrentBranch == null)
				{
					fNonCurrentBranch = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)) as GlbBranch;
				}

				return fNonCurrentBranch;
			}
		}

		GlbCompany fNonCurrentCompany;
		protected GlbCompany NonCurrentCompany
		{
			get
			{
				if (fNonCurrentCompany == null)
				{
					fNonCurrentCompany = Factory.LoadTop1(typeof(GlbCompany), new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)) as GlbCompany;
				}

				return fNonCurrentCompany;
			}
		}
	}
}

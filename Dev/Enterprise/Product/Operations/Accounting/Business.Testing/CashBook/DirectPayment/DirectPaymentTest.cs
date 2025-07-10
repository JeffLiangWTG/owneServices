using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	[TestedType(typeof(DirectPayment))]
	public class DirectPaymentTest : DirectTransactionHeaderBaseTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<DirectPayment>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		#region Concurrency Policy

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_InvoiceAmount()
		{
			//TODO: Hello, fellow developer! If this test is failing, uncomment the "DISABLE TRIGGER" command below and remove this TODO line
			//HACK: Temporarily allow UPDATE of AH_InvoiceAmount to test concurrency checks. This should not be possible in production. Refer to WI00559931, WI00482153.
			//TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AccTransactionHeader_ProtectCriticalFieldsFromUpdating ON AccTransactionHeader");

			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<DirectPayment>(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_OSTotal()
		{
			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<DirectPayment>(AccTransactionHeaderSchema.Constants.AH_OSTotal, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_GSTAmount()
		{
			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<DirectPayment>(AccTransactionHeaderSchema.Constants.AH_GSTAmount, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		#endregion

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", Core.Constants.DocManagerCodes.CashBook, ((IDocManagerSupport)(Factory.New<DirectPayment>())).DocManagerInfo.DocManagerCode);
		}

		public void TestBusinessContext()
		{
			DirectPayment directPay = Factory.New<DirectPayment>();
			AssertEquals("BusinessContext", BusinessContext.CBDirectPayment, directPay.DocumentSupporter.BusinessContext);
		}

		public void TestPayeeAcountDetails()
		{
			DirectPayment testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals("AccountCurrency should be empty", string.Empty, testDirectPayment.AccountCurrency);
			AssertEquals("PayeeIBANNumber should be empty", string.Empty, testDirectPayment.PayeeIBANNumber);
			AssertEquals("PayeeBankName should be empty", string.Empty, testDirectPayment.PayeeBankName);
			AssertEquals("PayeeBankSwift should be empty", string.Empty, testDirectPayment.PayeeBankSwift);
			AssertEquals("PayeeCountryCode should be empty", string.Empty, testDirectPayment.PayeeCountryCode);
			AssertEquals("PayeeBankBranchName should be empty", string.Empty, testDirectPayment.PayeeBankBranchName);
			AssertEquals("PayeeBankAddress1 should be empty", string.Empty, testDirectPayment.PayeeBankAddress1);
			AssertEquals("PayeeBankAddress2 should be empty", string.Empty, testDirectPayment.PayeeBankAddress2);
			AssertEquals("PayeeBankAddress3 should be empty", string.Empty, testDirectPayment.PayeeBankAddress3);

			testDirectPayment.AH_ChequeDrawer = "bbb";
			testDirectPayment.AH_DrawerBank = "ccc";
			testDirectPayment.AH_DrawerBranch = "ddd";
			AssertEquals("ccc", testDirectPayment.PayeeBankAccountNumber);
			AssertEquals("bbb", testDirectPayment.AccountTitle);
			AssertEquals("ddd", testDirectPayment.PayeeBankBSB);

			testDirectPayment.AH_ChequeDrawer = string.Empty;
			testDirectPayment.AH_DrawerBank = string.Empty;
			testDirectPayment.AH_DrawerBranch = string.Empty;
			AssertEquals(string.Empty, testDirectPayment.PayeeBankAccountNumber);
			AssertEquals(string.Empty, testDirectPayment.AccountTitle);
			AssertEquals(string.Empty, testDirectPayment.PayeeBankBSB);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(DirectPaymentValidation); }
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(DirectPaymentLine);
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals("Lookups", typeof(DirectPaymentLookups), TestBizO.Lookups.GetType());
		}

		public void TestReceiptTypeChequeDetails()
		{
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash)
				{
					AssertEquals("CASH", TestBizO.AH_ChequeDrawer);
				}
				else
				{
					AssertEquals(ZString.Empty, TestBizO.AH_ChequeDrawer);
				}
			}
		}

		public void TestDebitCredit()
		{
			TestBizO.AH_OSTotal = 190m;
			AssertEquals("Debit should be 190", 190m, TestBizO.Debit);
			AssertEquals("Credit should be 0", 0m, TestBizO.Credit);

			TestBizO.AH_OSTotal = -90m;
			AssertEquals("Debit should be 0", 0m, TestBizO.Debit);
			AssertEquals("Credit should be 90", 90m, TestBizO.Credit);
		}

		public override void TestLocalCredit()
		{
			TestBizO.AH_AB = TestBankOtherCurrency.PK;
			AssertEquals(true, TestBizO.BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			TestBizO.AH_ExchangeRate = 3m;
			TestBizO.Lines.AddNew();
			DirectPaymentLine line = (DirectPaymentLine)TestBizO.Lines[0];
			line.AL_OSExTaxAmount = 30;

			AssertEquals(30m, TestBizO.AH_OSExTaxAmount);
			AssertEquals(10m, TestBizO.LocalCredit);
		}

		public override void TestLocalDebit()
		{
			TestBizO.AH_AB = TestBankOtherCurrency.PK;
			AssertEquals(true, TestBizO.BankAccount.AB_RX_NKAccountCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			TestBizO.AH_ExchangeRate = 2m;
			TestBizO.Lines.AddNew();
			DirectPaymentLine line = (DirectPaymentLine)TestBizO.Lines[0];
			line.AL_OSExTaxAmount = -40;

			AssertEquals(-40m, TestBizO.AH_OSExTaxAmount);
			AssertEquals(20m, TestBizO.LocalDebit);
		}

		public override void TestDirectDebitNumber()
		{
			TestBizO.AH_ReceiptBatchNo = "00003005";
			AssertEquals("DirectDebit number should be 00003005", "00003005", TestBizO.DirectDebitNumber);
		}

		public void TestAllowAutoDDRDetail()
		{
			DirectPayment directPay = Factory.New<DirectPayment>();
			directPay.AH_DrawerBank = "394734";
			directPay.AH_DrawerBranch = "1482393";
			directPay.AH_ChequeDrawer = "Smith";

			Assert(directPay.AllowAutoDDR);

			directPay.AH_DrawerBank = "";
			directPay.AH_DrawerBranch = "";
			Assert(!directPay.AllowAutoDDR);
		}

		public void TestBankAccount()
		{
			TestBizO.AH_AB = BankAccount.PK;
			TestBizO.ChequeBookPK = TestChequeBook.PK;

			AssertEquals("Bank account should be set", BankAccount.PK, TestBizO.BankAccount.PK);
			AssertEquals("ChequeBookPk should be set", TestChequeBook.PK, TestBizO.ChequeBookPK);

			TestBizO.AH_AB = TestBankAccount2.PK;
			AssertEquals("Changed to different bank account, it should reset ChequeBookPK", true, TestBizO.ChequeBookPK.IsEmpty);

			TestBizO.AH_AB = BankAccount.PK;
			TestBizO.AH_AB = ZGuid.Empty;
			AssertEquals("Invalid bank account should set BankAccount = null", null, TestBizO.BankAccount);
			AssertEquals("And it should reset ChequeBookPK", true, TestBizO.ChequeBookPK.IsEmpty);
		}

		public void TestPaymentTypeAndIncludeDDR()
		{
			BankAccount.AB_AllowAutoDDR = true;
			TestBizO.AH_AB = BankAccount.PK;
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectDebit)
				{
					AssertEquals("AH_DrawerBank should be readonly", false, TestBizO.AH_DrawerBankInfo.ReadOnly);
					AssertEquals("AH_DrawerBranch should be readonly", false, TestBizO.AH_DrawerBranchInfo.ReadOnly);
				}
				else
				{
					AssertEquals("AH_DrawerBank should be readonly", true, TestBizO.AH_DrawerBankInfo.ReadOnly);
					AssertEquals("AH_DrawerBranch should be readonly", true, TestBizO.AH_DrawerBranchInfo.ReadOnly);
				}
			}
		}

		public void TestChequeBookSetReadyOnly()
		{
			foreach (ICodeDescription paymentMethod in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = paymentMethod.Code;

				if (TestBizO.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					AssertEquals("ChequeBookPKInfo.ReadOnly", false, TestBizO.ChequeBookPKInfo.ReadOnly);
				}
				else
				{
					AssertEquals("ChequeBookPKInfo.ReadOnly", true, TestBizO.ChequeBookPKInfo.ReadOnly);
				}
			}

			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("ChequeBookPKInfo.ReadOnly", false, TestBizO.ChequeBookPKInfo.ReadOnly);

			TestBizO.IsReverseTransaction = true;
			AssertEquals("ChequeBookPKInfo.ReadOnly", true, TestBizO.ChequeBookPKInfo.ReadOnly);
		}

		public void TestDefaultAH_Desc()
		{
			AssertEquals("Default AH_Desc should be", "CASH BOOK DIRECT PAYMENT", TestBizO.AH_Desc);
		}

		public void TestChequeNumberIsIncrementedOnSaving()
		{
			DirectPayment testPayment = Factory.New<DirectPayment>();

			AssertEquals("Should be the same currency", BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			testPayment.AH_AB = BankAccount.PK;
			testPayment.AH_TransactionReference = "abc";
			testPayment.AH_Desc = "test receipt";
			testPayment.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_ChequeOrReference = "000123";
			testPayment.ChequeBookPK = TestChequeBook.PK;

			AssertEquals("Precondition: Cheque Number is same as Cheque Book Current Number",
				TestChequeBook.AK_CurrentNo.ToString().PadLeft(6, '0'), testPayment.AH_ChequeOrReference);

			testPayment.AH_ChequeDrawer = "bbb";
			testPayment.AH_DrawerBank = "ccc";
			testPayment.AH_DrawerBranch = "ddd";
			testPayment.AH_ExchangeRate = 1m;

			DirectPaymentLine testLine = (DirectPaymentLine)testPayment.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_Desc = "test line";

			ZDecimal expectedChequeNumber = ZDecimal.Parse(testPayment.AH_ChequeOrReference);
			AssertEquals("ChequeBook CurrentNumber", expectedChequeNumber, TestChequeBook.AK_CurrentNo);
			Factory.Save();

			expectedChequeNumber++;
			AssertEquals("ChequeBook CurrentNumber", expectedChequeNumber, TestChequeBook.AK_CurrentNo);
		}

		public void TestCanIncrementChequeNumber()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 93;

			Factory.Save();

			DirectPayment testPayment = Factory.New<DirectPayment>();
			testPayment.ChequeBookPK = testChequeBook.PK;

			testPayment.AH_ChequeOrReference = "94";

			Assert("CanIncrementChequeNumber should be true", testPayment.CanIncrementChequeBookNumber);

			testPayment.AH_ChequeOrReference = "94.5";

			Assert("CanIncrementChequeNumber should be false", !testPayment.CanIncrementChequeBookNumber);
		}

		public void TestGetChequeNumberStatus()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 45;

			Factory.Save();

			DirectPayment testPayment = Factory.New<DirectPayment>();
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = testChequeBook.PK;

			AssertEquals("Cheque Number is 00045", "00045", testPayment.AH_ChequeOrReference);

			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				testPayment.GetChequeNumberStatus());

			testChequeBook.AK_CurrentNo = 46;
			AssertEquals("ChequeNumStatus is LessThanCurrentNum", ChequeNumberStatus.LessThanCurrentNum, testPayment.GetChequeNumberStatus());

			testPayment.AH_ChequeOrReference = "";
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, testPayment.GetChequeNumberStatus());

			testPayment.AH_ChequeOrReference = "111";
			testPayment.ChequeBookPK = ZGuid.Empty;
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, testPayment.GetChequeNumberStatus());

			testPayment.AH_ChequeOrReference = "11";
			testPayment.ChequeBookPK = testChequeBook.PK;
			testChequeBook.AK_CurrentNo = 9;
			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				testPayment.GetChequeNumberStatus());

			testPayment.AH_ChequeOrReference = "11.5";
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, testPayment.GetChequeNumberStatus());
		}

		public void TestIncludeDDRFileAndChequeDetails()
		{
			AssertEquals("Default AH_ChequeDrawer should be enabled", false, TestBizO.AH_ChequeDrawerInfo.ReadOnly);
			AssertEquals("Default AH_DrawerBank should be readonly", true, TestBizO.AH_DrawerBankInfo.ReadOnly);
			AssertEquals("Default AH_DrawerBranch should be readonly", true, TestBizO.AH_DrawerBranchInfo.ReadOnly);

			BankAccount.AB_AllowAutoDDR = true;
			TestBizO.AH_AB = BankAccount.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			AssertEquals("AH_ChequeDrawer should remain enabled", false, TestBizO.AH_ChequeDrawerInfo.ReadOnly);
			AssertEquals("AH_DrawerBank should NOT be readonly", false, TestBizO.AH_DrawerBankInfo.ReadOnly);
			AssertEquals("AH_DrawerBranch NOT should be readonly", false, TestBizO.AH_DrawerBranchInfo.ReadOnly);
		}

		[TestDate(2000, 1, 21)]
		public void TestSavedData()
		{
			DirectPayment testPayment = Factory.New<DirectPayment>();

			AssertEquals("Should be the same currency", BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			testPayment.AH_AB = BankAccount.PK;
			testPayment.AH_TransactionReference = "abc";
			testPayment.AH_Desc = "test receipt";
			testPayment.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testPayment.AH_ChequeOrReference = "000123";
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_ChequeDrawer = "bbb";
			testPayment.AH_DrawerBank = "ccc";
			testPayment.AH_DrawerBranch = "ddd";
			testPayment.AH_ExchangeRate = 1m;

			DirectPaymentLine testLine = (DirectPaymentLine)testPayment.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			DirectPayment savedPayment = testFactory.Load<DirectPayment>(testPayment.PK);

			// AccTransactionHeader Columns
			AssertEquals("CB", savedPayment.AH_Ledger);
			AssertEquals("DPY", savedPayment.AH_TransactionType);
			AssertEquals(1, (int)savedPayment.AH_TransactionCount);
			AssertEquals("abc", savedPayment.AH_TransactionReference);
			AssertEquals("test receipt", savedPayment.AH_Desc);
			AssertEquals(new ZDateTime(2000, 1, 20), savedPayment.AH_InvoiceDate);
			AssertEquals("", savedPayment.AH_TransactionCategory);
			AssertEquals(savedPayment.AH_InvoiceDate, savedPayment.AH_DueDate);
			AssertEquals(-10m, savedPayment.AH_InvoiceAmount);
			AssertEquals(0m, savedPayment.AH_GSTAmount);
			AssertEquals(0m, savedPayment.AH_WithholdingTax);
			AssertEquals(-10m, savedPayment.AH_OSTotal);
			AssertEquals(savedPayment.AH_RX_NKTransactionCurrency, savedPayment.AH_RX_NKTransactionCurrency);
			AssertEquals(1m, savedPayment.AH_ExchangeRate);
			AssertEquals(0, savedPayment.AH_AgePeriod);
			AssertEquals(0, savedPayment.AH_PostPeriod);
			AssertEquals(new ZDateTime(2000, 1, 21), savedPayment.AH_PostDate);
			AssertEquals(false, savedPayment.AH_IsDisbursementCalc);
			AssertEquals("000123", savedPayment.AH_ChequeOrReference);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, savedPayment.AH_ReceiptType);
			AssertEquals(false, savedPayment.AH_CashBasisGSTIndicator);
			AssertEquals(false, savedPayment.AH_CashBasisGSTRealisedToGL);

			AssertEquals("bbb", savedPayment.AH_ChequeDrawer);
			AssertEquals("ccc", savedPayment.AH_DrawerBank);
			AssertEquals("ddd", savedPayment.AH_DrawerBranch);
			AssertEquals(false, savedPayment.AH_InvoiceApproved);
			AssertEquals("", savedPayment.AH_ConsolidatedInvoiceRef);
			AssertEquals(true, savedPayment.AH_FullyPaidDate.IsEmpty);
			AssertEquals(false, savedPayment.AH_InvoicePrinted);
			AssertEquals(false, savedPayment.AH_IsCancelled);
			//AssertEquals(false, SavedPayment.AH_IsClearedInCashbook);
			AssertEquals(ZDateTime.Empty, savedPayment.AH_DateClearedInCashbook);

			AssertEquals(false, savedPayment.AH_NotAllocated);
			AssertEquals(0m, savedPayment.AH_OutstandingAmount);
			AssertEquals(false, savedPayment.AH_PostedToEFT);
			AssertEquals("N", savedPayment.AH_PostToGL);
			AssertEquals("", savedPayment.AH_ReceiptBatchNo);
			AssertEquals(false, savedPayment.AH_TransactionCreatedByMatching);
			AssertEquals("", savedPayment.AH_InvoiceTerm);
			AssertEquals(0, (int)savedPayment.AH_InvoiceTermDays);

			AssertEquals(false, savedPayment.AH_POST1);
			AssertEquals(false, savedPayment.AH_POST2);
			AssertEquals(false, savedPayment.AH_POST3);
			AssertEquals(false, savedPayment.AH_POST4);

			AssertEquals(BankAccount.PK, savedPayment.AH_AB);
			AssertEquals(true, savedPayment.AH_OH.IsEmpty);
			AssertEquals(true, savedPayment.AH_JH.IsEmpty);
			AssertEquals(GlbBranch.CurrentBranch.PK, savedPayment.AH_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, savedPayment.AH_GE);
			AssertEquals(true, savedPayment.AH_AG.IsEmpty);
			AssertEquals(true, savedPayment.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals(true, savedPayment.AH_AH_InvoiceStatement.IsEmpty);

			// AccTransactionLines Columns
			AssertEquals(1, savedPayment.Lines.Count);

			DirectPaymentLine savedLine = (DirectPaymentLine)savedPayment.Lines[0];

			AssertEquals("DPY", savedLine.AL_LineType);
			AssertEquals(1, (int)savedLine.AL_Sequence);
			AssertEquals("test line", savedLine.AL_Desc);
			AssertEquals(-10m, savedLine.AL_LineAmount);
			AssertEquals(true, savedLine.AL_AT.IsEmpty);
			AssertEquals(0m, savedLine.AL_GSTVAT);
			AssertEquals(true, savedLine.AL_AW.IsEmpty);
			AssertEquals(0m, savedLine.AL_WithholdingTax);
			AssertEquals(0, savedLine.AL_UnitQty);
			AssertEquals(0m, savedLine.AL_UnitPrice);
			AssertEquals(0m, savedLine.AL_OSUnitPrice);
			AssertEquals(-10m, savedLine.AL_OSAmount);
			AssertEquals(1m, savedLine.AL_ExchangeRate);
			AssertEquals(0, savedLine.AL_PostPeriod);
			AssertEquals(new ZDateTime(2000, 1, 21), savedLine.AL_PostDate);
			AssertEquals("N", savedLine.AL_PostToGL);
			AssertEquals(0, savedLine.AL_ReversePeriod);
			AssertEquals(true, savedLine.AL_ReverseDate.IsEmpty);
			AssertEquals("N", savedLine.AL_ReverseToGL);
			AssertEquals(false, savedLine.AL_PreventInvoicePrintGrouping);
			AssertEquals(true, savedLine.AL_JH.IsEmpty);
			AssertEquals(true, savedLine.AL_AC.IsEmpty);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, savedLine.AL_GE);
			AssertEquals(GlbBranch.CurrentBranch.PK, savedLine.AL_GB);
			AssertEquals(GLAccount.PK, savedLine.AL_AG);
			AssertEquals(true, savedLine.AL_OH.IsEmpty);
			AssertEquals(true, savedLine.AL_AG_PercentOf.IsEmpty);
			AssertEquals(0, savedLine.AL_PercentageOfPeriod);
			//				AssertEquals(true, SavedLine.AL_AZ_TransactionGroup.IsEmpty);
		}

		[TestDate(2000, 1, 21)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSavedDataSameCurrencyBankAccount()
		{
			DirectPayment testPayment = Factory.New<DirectPayment>();

			AssertEquals("Should be the same currency", BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			testPayment.AH_AB = BankAccount.PK;
			testPayment.AH_TransactionReference = "abc";
			testPayment.AH_Desc = "test receipt";
			testPayment.AH_InvoiceDate = new ZDateTime(2000, 1, 20);
			testPayment.AH_ChequeOrReference = "00123";
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_ChequeDrawer = "bbb";
			testPayment.AH_DrawerBank = "ccc";
			testPayment.AH_DrawerBranch = "ddd";

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)testPayment.Lines.AddNew();
			testLine.AL_AG = GLAccount.PK;
			testLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testLine.AL_OSExTaxAmount = 10;
			testLine.AL_AT = TestObjectCreator.GST1.PK;
			testLine.AL_Desc = "test line";
			Factory.Save();

			BusinessObjectFactory testFactory = new BusinessObjectFactory();

			DirectPayment savedPayment = testFactory.Load<DirectPayment>(testPayment.PK);

			AssertEquals(savedPayment.BankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, savedPayment.AH_ExchangeRate);
			AssertEquals(-10m, savedPayment.AH_InvoiceAmount);
			AssertEquals(-11m, savedPayment.AH_OSTotal);

			DirectPaymentLine savedLine = (DirectPaymentLine)savedPayment.Lines[0];

			AssertEquals(-10m, savedLine.AL_LineAmount);
			AssertEquals(-1m, savedLine.AL_GSTVAT);
			AssertEquals(-11m, savedLine.AL_OSAmount);
			AssertEquals(1m, savedLine.AL_ExchangeRate);

			// AH_ExchangeRate and AH_OSTotal get recalculated on load, tests below ensures the correct values in the table
			string sQL = "SELECT * FROM dbo.AccTransactionHeader WHERE AH_PK = '" + savedPayment.PK + "'";
			DbCommand command = Db.Connection.Command(sQL); // This test specifically wants to avoid the BizOs calculations and hence needs direct database values
			DataSet dataSetResult = new DataSet(); // This test specifically wants to avoid the BizOs calculations and hence needs direct database values
			var resultDataAdpter = command.NewDataAdapter();
			resultDataAdpter.Fill(dataSetResult);

			AssertEquals(1m, dataSetResult.Tables[0].Rows[0]["AH_ExchangeRate"]);
			AssertEquals(-11m, dataSetResult.Tables[0].Rows[0]["AH_OSTotal"]);
		}

		public void TestIDirectDebitBatchHeader_AH_ReceiptType()
		{
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();

			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.AH_DrawerBank = "123456789";
			testPayment.AH_DrawerBranch = "123456";
			testPayment.AH_ChequeDrawer = "ChequeDrawer";

			((IDirectDebitBatchTransaction)testPayment).AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebitLine;
			AssertEquals(ZArchitecture.Core.ReceiptTypes.DirectDebitLine, testPayment.AH_ReceiptType);
			AssertEquals("123456789", testPayment.AH_DrawerBank);
			AssertEquals("123456", testPayment.AH_DrawerBranch);
			AssertEquals("ChequeDrawer", testPayment.AH_ChequeDrawer);
		}

		public override void TestIsChequeNumberAutoAllocated()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 3, 3);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_LastNo = 4;
			testChequeBook.AK_StartNo = 1;

			DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();

			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			testBizO.AH_AB = testBank.PK;
			Assert("ChequeBook is not set, AutoAllocation disabled", !((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
			Assert("ChequeOrReference number should not be read only", !testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
			AssertEquals("Cheque number should be defaulted", "CASH", testBizO.AH_ChequeOrReference);

			testBizO.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("PaymentType is Cash, AutoAllocation disabled", !((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
			Assert("ChequeOrReference number should not be read only", !testBizO.AH_ChequeOrReferenceInfo.ReadOnly);

			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
			Assert("ChequeOrReference number should be read only", testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("Cheque number field should become empty", testBizO.AH_ChequeOrReference.IsEmpty);

			testBizO.ChequeBookPK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, AutoAllocation disabled", !testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("Cheque Book is not IsAutoPrint, AutoAllocation disabled", !((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
			Assert("ChequeOrReference number should not be read only", !testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
			AssertEquals("Cheque number should be defaulted", "1", testBizO.AH_ChequeOrReference);

			testBizO.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled again", testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
			Assert("ChequeOrReference number should be read only", testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestIncludeInTheBatchReadOnly()
		{
			DirectPayment directPayment = Factory.New<DirectPayment>();
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = "DDR";
			Factory.Save();

			DirectDebitBatchHeader directDebitBatchHeader = Factory.New<DirectDebitBatchHeader>();
			directDebitBatchHeader.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(false, directDebitBatchHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
		}

		public override void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties().Count", 4, ((DirectPayment)Header).GetWritableProperties_ForTestOnly().Count);
			AssertEquals("GetWritableProperties()[0]", "OSPartialPaymentAmount", ((DirectPayment)Header).GetWritableProperties_ForTestOnly()[0]);
			AssertEquals("GetWritableProperties()[1]", "MatchStatus", ((DirectPayment)Header).GetWritableProperties_ForTestOnly()[1]);
			AssertEquals("GetWritableProperties()[2]", "MatchStatusReasonCode", ((DirectPayment)Header).GetWritableProperties_ForTestOnly()[2]);
			AssertEquals("GetWritableProperties()[3]", "IncludeInTheBatch", ((DirectPayment)Header).GetWritableProperties_ForTestOnly()[3]);
		}

		public void TestTransactionNumberResetOnSaveFailure()
		{
			Assert("DirectPayment", Header is DirectPayment);
			Header.AH_TransactionNum = "010";
			Header.AH_AB = ZGuid.NewZGuid();

			try
			{
				Factory.Save();
				Assert(false);
			}
			catch (Exception)
			{
			}

			AssertEquals("AH_TransactionNum", ZString.Empty, Header.AH_TransactionNum);
		}

		public void TestShouldNotSetTransactionNumberToEmptyOnFactorySavedWhenTransactionIsInDB()
		{
			SetupForSave();
			Factory.Save();
			AssertNotNullOrEmpty("Transaction Number is not empty", Header.AH_TransactionNum);
			AssertEquals(true, Header.IsInDatabase);

			var oldTransactionNum = Header.AH_TransactionNum;
			Header.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12);

			try
			{
				Factory.Save();
				Fail("Factory save should throw exception.");
			}
			catch (Exception)
			{
				Header.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
			}

			AssertEquals("Transaction number should not be reset if the header was in database.", oldTransactionNum, Header.AH_TransactionNum);
			ErrorReporter.Instance.Clear();
		}

		public void TestReproduceConcurrentMergeDirectPayment()
		{
			Factory.Save();
			AssertNotNullOrEmpty("Has proper transaction number.", Header.AH_TransactionNum);
			var oldTransactionNum = Header.AH_TransactionNum;

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var headerReload = newFactory.Load<BankReconTransaction>(Header.PK);
			headerReload.AH_Desc += "Test1";
			newFactory.Save();

			Header.AH_Desc += "Test2";

			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
				Fail("Should report concurrency exception.");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals("Transaction number should not be reset after merge.", oldTransactionNum, Header.AH_TransactionNum);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestDirectPaymentAuditDetails()
		{
			var directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_DrawerBank = "123456789";
			directPayment.AH_DrawerBranch = "123456";
			directPayment.AH_ChequeDrawer = "ChequeDrawer";

			AssertEquals(ZString.Empty, directPayment.BankCreateUser);
			AssertEquals(ZDateTime.Empty, directPayment.BankCreateTimeLocal);
			AssertEquals(ZString.Empty, directPayment.BankLastEditUser);
			AssertEquals(ZDateTime.Empty, directPayment.BankLastEditTimeLocal);
		}

		public void TestIncludeInTheBatch()
		{
			var directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_OSTotal = 11;
			directPayment.AH_InvoiceAmount = 10;

			var testHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			var testDDRCollection = new DirectDebitBatchLineCollection(Factory, testHeader);
			directPayment.SetDDRCollection_ForTestOnly(testDDRCollection);
			directPayment.IncludeInTheBatch = false;

			AssertEquals("No direct payment be included in batch, amount should zero.", 0m, directPayment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("No direct payment be included in batch, amount should zero.", 0m, directPayment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			directPayment.IncludeInTheBatch = false;

			AssertEquals("No direct payment be included in batch, amount should not change when IncludeInTheBatch is not change.", 0m, directPayment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("No direct payment be included in batch, amount should not change when IncludeInTheBatch is not change.", 0m, directPayment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			directPayment.IncludeInTheBatch = true;

			AssertEquals("Direct payment is included in batch, amount should be payment AH_OSTotal.", -11m, directPayment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("Direct payment is included in batch, amount should be payment AH_InvoiceAmount.", -10m, directPayment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			directPayment.IncludeInTheBatch = true;

			AssertEquals("Direct payment is included in batch, amount should not change when IncludeInTheBatch is not change.", -11m, directPayment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("Direct payment is included in batch, amount should not change when IncludeInTheBatch is not change.", -10m, directPayment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);
		}

		#region Implementation

		AccChequeBook TestChequeBook;
		AccBankAccount TestBankAccount2;

		protected override void SetUp()
		{
			base.SetUp();
			TestBankAccount2 = TestObjectCreator.CreateBankAccount("ABC", "Bank Account 2", GlbCompany.CurrentCompany.LocalCurrency, null);
			TestChequeBook = TestObjectCreator.CreateChequeBook(1, 1, 100, BankAccount);
		}

		protected override IEnumerable<(string PropertyName, Action<DirectTransactionHeaderBase> Init)> GetMostColumnsReadOnlyAffectedProp()
		{
			return new (string, Action<DirectTransactionHeaderBase>)[] {
				(DirectTransactionHeaderBase.Schema.AH_TransactionType, _ => { }),
				(DirectTransactionHeaderBase.Schema.AH_RX_NKTransactionCurrency,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ExchangeRate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ChequeDrawer,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ChequeOrReference,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_PostDate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_InvoiceDate,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_DrawerBranch,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_ReceiptType,SetMostColumnsReadOnlyPreConditionEvn),
				(DirectTransactionHeaderBase.Schema.AH_DrawerBank,SetMostColumnsReadOnlyPreConditionEvn)
			};

			void SetMostColumnsReadOnlyPreConditionEvn(DirectTransactionHeaderBase testHeader)
			{
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty
					, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				testHeader.AH_RX_NKTransactionCurrency = testHeader.AH_RX_NKTransactionCurrency == "USD"
					? "AUD"
					: "USD";
				testHeader.ExchangeRate.Currency_ReadOnly = false;

				TestObjectCreator.AUDChequeBook.AK_AutoPrintCheque = false;
				TestObjectCreator.AUDChequeBook.BankAccount.AB_AllowAutoDDR = true;
				testHeader.AH_AB = TestObjectCreator.AUDChequeBook.AK_AB;
				testHeader.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
				testHeader.IsReverseTransaction = false;
			}
		}

		#endregion
	}
}

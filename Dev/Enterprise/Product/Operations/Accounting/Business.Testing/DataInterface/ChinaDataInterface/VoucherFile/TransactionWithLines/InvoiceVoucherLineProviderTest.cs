using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class InvoiceVoucherLineProviderTest : TestCaseWithFactory
	{
		public void TestVoucherLineGLAccount()
		{
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, 120m, 6.5m, 18.46m);

			TestTransaction.Lines[0].AL_AG = ZGuid.Invalid;
			AssertNull("Pre-requisite", TestTransaction.Lines[0].GLHeader);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals("Voucher line's account should be transaction line's charge code's account if transaction line's GLHeader is null.", ChargeCode1.RevenueAccount.PK, TestVoucherLineProvider.VoucherLine.AccountPK);

			TestTransaction.Lines[0].AL_AG = GLAccount1.PK;
			AssertEquals("Pre-requisite", TestTransaction.Lines[0].GLHeader.PK, GLAccount1.PK);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals("We should firstly get transaction line's GLHeader as voucher line's account if transaction line's GLHeader is not null.", GLAccount1.PK, TestVoucherLineProvider.VoucherLine.AccountPK);
		}

		public void TestVoucherOSCredit()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, 120m, 6.5m, 18.46m);
			TestTransaction.Lines[0].AL_RX_NKTransactionCurrency = TestTransaction.Company.GC_RX_NKLocalCurrency;
			TestTransaction.Lines[0].AL_OSAmount = 18.46m;
			TestTransaction.Lines[0].AL_LineAmount = 120m;
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.OSCreditAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.OSDebitAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
			AssertEquals(TestTransaction.Company.GC_RX_NKLocalCurrency, TestVoucherLineProvider.VoucherLine.CurrencyCode);
			TestTransaction.AH_RX_NKTransactionCurrency = "USD";
			TestTransaction.Lines[0].AL_RX_NKTransactionCurrency = "USD";
			TestTransaction.AH_ExchangeRate = 6.5m;
			TestTransaction.Lines[0].AL_ExchangeRate = 6.5m;
			TestTransaction.Lines[0].AL_OSAmount = 18.46m;
			TestTransaction.Lines[0].AL_LineAmount = 120m;
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(18.46m, TestVoucherLineProvider.VoucherLine.OSCreditAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.OSDebitAmount);
			AssertEquals(6.5m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
			AssertEquals("USD", TestVoucherLineProvider.VoucherLine.CurrencyCode);
		}

		public void TestVoucherOSDebit()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, -120m, 6.5m, -18.46m);
			TestTransaction.Lines[0].AL_RX_NKTransactionCurrency = TestTransaction.Company.GC_RX_NKLocalCurrency;
			TestTransaction.Lines[0].AL_ExchangeRate = 6.5m;
			TestTransaction.Lines[0].AL_OSAmount = -18.46m;
			TestTransaction.Lines[0].AL_LineAmount = -120m;
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.OSCreditAmount);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.OSDebitAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
			AssertEquals(TestTransaction.Company.GC_RX_NKLocalCurrency, TestVoucherLineProvider.VoucherLine.CurrencyCode);
			TestTransaction.AH_RX_NKTransactionCurrency = "USD";
			TestTransaction.Lines[0].AL_RX_NKTransactionCurrency = "USD";
			TestTransaction.AH_ExchangeRate = 6.5m;
			TestTransaction.Lines[0].AL_ExchangeRate = 6.5m;
			TestTransaction.Lines[0].AL_OSAmount = -18.46m;
			TestTransaction.Lines[0].AL_LineAmount = -120m;
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.OSCreditAmount);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(18.46m, TestVoucherLineProvider.VoucherLine.OSDebitAmount);
			AssertEquals(6.5m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
			AssertEquals("USD", TestVoucherLineProvider.VoucherLine.CurrencyCode);
		}

		public void TestAccountNumber()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(LocalAccountNumber1, TestVoucherLineProvider.VoucherLine.AccountNumber);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDate()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			ZDateTime testDateTime = Env.Time.CurrentLocalDateTime;
			TestTransaction.Lines[0].AL_PostDate = testDateTime;
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(testDateTime, TestVoucherLineProvider.VoucherLine.VoucherDate);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherBranchAndDepartment()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "AAA";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "BBB";
			Factory.Save();

			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			var line = TestTransaction.Lines[0];
			line.AL_GB = TestTransaction.AH_GB = branch.PK;
			line.AL_GE = TestTransaction.AH_GE = department.PK;

			TestVoucherLineProvider = new InvoiceVoucherLineProvider(line);
			AssertEquals("AAA", TestVoucherLineProvider.VoucherLine.BranchCode);
			AssertEquals("BBB", TestVoucherLineProvider.VoucherLine.DepartmentCode);
		}

		public void TestVoucherTypeForAR()
		{
			//ZString ExpectedVoucherType = "应收帐款-发票";
			ZString expectedVoucherType = "AR-INV";
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(expectedVoucherType, TestVoucherLineProvider.VoucherLine.VoucherType);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherTypeForAP()
		{
			//ZString ExpectedVoucherType = "应付帐款-发票";
			ZString expectedVoucherType = "AP-INV";
			TestTransaction = GetInvoice(LedgerTypes.AccountsPayable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(expectedVoucherType, TestVoucherLineProvider.VoucherLine.VoucherType);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherNumberForAR()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(TestTransaction.AH_TransactionNum, TestVoucherLineProvider.VoucherLine.VoucherNumber);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherNumberForAP()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsPayable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestTransaction.AH_TransactionReference = "Voucher1";
			TestTransaction.AH_ConsolidatedInvoiceRef = "VoucherA";
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(TestTransaction.AH_ConsolidatedInvoiceRef, TestVoucherLineProvider.VoucherLine.VoucherNumber);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitForAP()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsPayable, ChargeCode1, -LineAmount1, ExchangeRate1, -OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherCreditForAP()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsPayable, ChargeCode1, -LineAmount1, ExchangeRate1, -OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherCreditForAR()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitForAR()
		{
			TestTransaction = GetInvoice(LedgerTypes.AccountsReceivable, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitCreditForARCreditNote()
		{
			TestTransaction = GetInvCrdAdj(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, ChargeCode1, -LineAmount1, ExchangeRate1, -OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitCreditForAPCreditNote()
		{
			TestTransaction = GetInvCrdAdj(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitCreditForARAdjustment()
		{
			TestTransaction = GetInvCrdAdj(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestVoucherDebitCreditForAPAdjustment()
		{
			TestTransaction = GetInvCrdAdj(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, ChargeCode1, -LineAmount1, ExchangeRate1, -OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestForDirectReceipt()
		{
			ZString expectedVoucherType = "CB-DRC";
			DirectReceipt receipt = GetDirectReceipt(GLAccount1, LineAmount1, ExchangeRate1, OSAmount1);
			receipt.AH_TransactionNum = "12345";
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(receipt.Lines[0]);
			AssertEquals(LocalAccountNumber1, TestVoucherLineProvider.VoucherLine.AccountNumber);
			AssertEquals(expectedVoucherType, TestVoucherLineProvider.VoucherLine.VoucherType);
			AssertEquals("12345", TestVoucherLineProvider.VoucherLine.VoucherNumber);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestForDirectPayment()
		{
			ZString expectedVoucherType = "CB-DPY";
			DirectPayment payment = GetDirectPayment(GLAccount1, -LineAmount1, ExchangeRate1, -OSAmount1);
			payment.AH_TransactionNum = "12345";
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(payment.Lines[0]);
			AssertEquals(LocalAccountNumber1, TestVoucherLineProvider.VoucherLine.AccountNumber);
			AssertEquals(expectedVoucherType, TestVoucherLineProvider.VoucherLine.VoucherType);
			AssertEquals("12345", TestVoucherLineProvider.VoucherLine.VoucherNumber);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestForAdditionalDescriptionForRolledUpAccount()
		{
			TestTransaction = GetInvCrdAdj(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, ChargeCode1, LineAmount1, ExchangeRate1, OSAmount1);
			TestVoucherLineProvider = new InvoiceVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
			AssertEquals(120m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(LocalAccountDescription, TestVoucherLineProvider.VoucherLine.AdditionalAccountDescription);
			AssertEquals(OSAmount1, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		decimal LineAmount1;
		decimal ExchangeRate1;
		decimal OSAmount1;
		AccChargeCode ChargeCode1;
		AccGLHeader GLAccount2;
		AccGLHeader GLAccount1;
		string LocalAccountNumber1;
		string LocalAccountDescription;
		InvoicingBase TestTransaction;
		InvoiceVoucherLineProvider TestVoucherLineProvider;
		void SetupAccountDescriptor(ZGuid accountPK)
		{
			AccGLAccountDescriptor gLAccountDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLAccountDescriptor.AJ_LocalAccountNumber = LocalAccountNumber1;
			gLAccountDescriptor.AJ_AccountDescription = LocalAccountDescription;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			gLAccountDescriptor.ParentGLHeaderPK = accountPK;
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			gLAccountDescriptor.AJ_DebitCredit = Constants.DebitCredit.Debit;
			Factory.Save();
			return;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		readonly ZString CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			base.SetUp();
			LocalAccountNumber1 = "1000.10.10InCNY";
			LocalAccountDescription = "TEST Descr";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Revenue);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			SetupAccountDescriptor(GLAccount1.PK);
			SetupAccountDescriptor(GLAccount2.PK);
			LineAmount1 = 120m;
			ExchangeRate1 = 0.5m;
			OSAmount1 = 60m;
		}

		DirectPayment GetDirectPayment(AccGLHeader gLAccount, decimal lineAmount, decimal exchangeRate, decimal oSAmount)
		{
			DirectPayment paymentToReturn = Factory.NewWithValidTestData<DirectPayment>();
			paymentToReturn.AH_ExchangeRate = exchangeRate;
			paymentToReturn.Lines.AddNew(typeof(DirectPaymentLine));
			paymentToReturn.Lines[0].AL_AG = gLAccount.PK;
			paymentToReturn.Lines[0].AL_ExchangeRate = exchangeRate;
			paymentToReturn.Lines[0].AL_OSAmount = oSAmount;
			paymentToReturn.Lines[0].AL_LineAmount = lineAmount;
			return paymentToReturn;
		}

		DirectReceipt GetDirectReceipt(AccGLHeader gLAccount, decimal lineAmount, decimal exchangeRate, decimal oSAmount)
		{
			DirectReceipt receiptToReturn = Factory.NewWithValidTestData<DirectReceipt>();
			receiptToReturn.AH_ExchangeRate = exchangeRate;
			receiptToReturn.Lines.AddNew(typeof(DirectReceiptLine));
			receiptToReturn.Lines[0].AL_AG = gLAccount.PK;
			receiptToReturn.Lines[0].AL_ExchangeRate = exchangeRate;
			receiptToReturn.Lines[0].AL_OSAmount = oSAmount;
			receiptToReturn.Lines[0].AL_LineAmount = lineAmount;
			return receiptToReturn;
		}

		InvoicingBase GetInvoice(string ledger, AccChargeCode chargeCode, decimal lineAmount, decimal exchangeRate, decimal oSAmount)
		{
			return GetInvCrdAdj(ledger, TransactionTypes.Invoice, chargeCode, lineAmount, exchangeRate, oSAmount);
		}

		InvoicingBase GetInvCrdAdj(string ledger, string type, AccChargeCode chargeCode, decimal lineAmount, decimal exchangeRate, decimal oSAmount)
		{
			Type transactionType = null;
			Type lineType = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				switch (type)
				{
					case TransactionTypes.Invoice:
						transactionType = typeof(ARInvoice);
						lineType = typeof(ARInvoiceLine);
						break;
					case TransactionTypes.CreditNote:
						transactionType = typeof(ARCreditNote);
						lineType = typeof(ARCreditNoteLine);
						break;
					case TransactionTypes.AdjustmentNote:
						transactionType = typeof(ARAdjustmentNote);
						lineType = typeof(ARAdjustmentNoteLine);
						break;
				}
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				switch (type)
				{
					case TransactionTypes.Invoice:
						transactionType = typeof(APInvoice);
						lineType = typeof(APInvoiceLine);
						break;
					case TransactionTypes.CreditNote:
						transactionType = typeof(APCreditNote);
						lineType = typeof(APCreditNoteLine);
						break;
					case TransactionTypes.AdjustmentNote:
						transactionType = typeof(APAdjustmentNote);
						lineType = typeof(APAdjustmentNoteLine);
						break;
				}
			}

			InvoicingBase invoiceToTest = Factory.NewWithValidTestData(transactionType) as InvoicingBase;
			invoiceToTest.AH_ExchangeRate = ExchangeRate1;
			invoiceToTest.Lines.AddNew(lineType);
			invoiceToTest.Lines[0].AL_AC = chargeCode.PK;
			invoiceToTest.Lines[0].AL_ExchangeRate = ExchangeRate1;
			invoiceToTest.Lines[0].AL_OSAmount = OSAmount1;
			invoiceToTest.Lines[0].AL_LineAmount = lineAmount;
			invoiceToTest.Lines[0].AL_Sequence = (ZShort)0;
			return invoiceToTest;
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
	}
}

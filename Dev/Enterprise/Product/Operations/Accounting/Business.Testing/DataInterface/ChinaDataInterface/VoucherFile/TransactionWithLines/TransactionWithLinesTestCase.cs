using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class TransactionWithLinesTestCase : VoucherProviderTestCase
	{
		public void TestDebitInFirstRow()
		{
			InvoiceToTest.AH_GSTAmount = 17.0m;
			InvoiceToTest.AH_InvoiceAmount = 170.0m;
			InvoiceCreditAdjustmentVoucherProvider testProvider = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest);
			AssertEquals(187m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[0].CreditAmount);
			InvoiceToTest.AH_InvoiceAmount = -170.0m;
			InvoiceToTest.Lines[0].AL_LineAmount = -100m;
			InvoiceToTest.Lines[0].AL_GSTVAT = 0m;
			InvoiceToTest.Lines[0].AL_OSAmount = -100m;
			InvoiceToTest.Lines[1].AL_LineAmount = -70m;
			InvoiceToTest.Lines[1].AL_GSTVAT = 0m;
			InvoiceToTest.Lines[1].AL_OSGSTAmount = 0m;
			InvoiceToTest.Lines[1].AL_OSAmount = -70m;
			testProvider = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest);
			AssertEquals(100m, testProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[0].CreditAmount);
		}

		public void TestGSTControlAccountAmount()
		{
			var mockControlAccount = SetMockControlAccount();
			InvoiceToTest.AH_RX_NKTransactionCurrency = "USD";
			InvoiceToTest.AH_InvoiceAmount = 170.0m;
			InvoiceToTest.AH_OSTotal = 110.50m;
			InvoiceToTest.AH_ExchangeRate = 0.65m;
			InvoiceToTest.AH_GSTAmount = -29.0m;
			InvoiceCreditAdjustmentVoucherProvider testProvider = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest, mockControlAccount.Object);
			AssertEquals(141m, testProvider.VoucherLines[2].DebitAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[2].CreditAmount);
			AssertEquals(110.50m, testProvider.VoucherLines[2].OSDebitAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[2].OSCreditAmount);
		}

		public void TestGSTOSAmount()
		{
			var mockControlAccount = SetMockControlAccount();
			InvoiceToTest.AH_RX_NKTransactionCurrency = "USD";
			InvoiceToTest.AH_ExchangeRate = 0.65m;
			InvoiceToTest.AH_GSTAmount = 29.0m;
			InvoiceToTest.AH_OSTotal = 18.85m;
			InvoiceCreditAdjustmentVoucherProvider testProvider = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest, mockControlAccount.Object);
			AssertEquals(0.65m, testProvider.VoucherLines[3].ExchangeRate);
			AssertEquals(0.0m, testProvider.VoucherLines[3].DebitAmount);
			AssertEquals(29.0m, testProvider.VoucherLines[3].CreditAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[3].OSDebitAmount);
			AssertEquals(18.85m, testProvider.VoucherLines[3].OSCreditAmount);
		}

		Mock<IControlAccountProvider> SetMockControlAccount()
		{
			LocalAccountNumber1 = "1000.10.10";
			LocalAccountNumber2 = "1000.10.20";
			LocalControlAccount = "1000.10.30";
			LocalGST = "1000.10.40";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ControlAccount = TestObjectCreator.InsertGLHeader();
			GSTAccount = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			GLAccountDescriptor1 = SetupAccountDescriptor(GLAccount1, LocalAccountNumber1);
			GLAccountDescriptor2 = SetupAccountDescriptor(GLAccount2, LocalAccountNumber2);
			GLAccountDescriptor3 = SetupAccountDescriptor(ControlAccount, LocalControlAccount);
			GLAccountDescriptor4 = SetupAccountDescriptor(GSTAccount, LocalGST);
			var mockControlAccount = new Mock<IControlAccountProvider>();
			mockControlAccount.Setup(m => m.SetTransaction(It.IsAny<AccTransactionHeader>()));
			mockControlAccount.Setup(m => m.PK).Returns(ControlAccount.PK);
			mockControlAccount.Setup(m => m.GST).Returns(GSTAccount.PK);
			return mockControlAccount;
		}

		protected AccGLAccountDescriptor GLAccountDescriptor3;
		protected AccGLAccountDescriptor GLAccountDescriptor4;
		protected AccGLHeader GSTAccount;
		protected ZDecimal GSTAmount1;
		protected ZDecimal GSTAmount2;
		protected InvoicingBase InvoiceToTest
		{
			get
			{
				if (fInvoiceToTest == null)
				{
					fInvoiceToTest = Factory.New(typeof(ARInvoice)) as ARInvoice;
					fInvoiceToTest.Lines.AddNew(typeof(ARInvoiceLine));
					fInvoiceToTest.Lines.AddNew(typeof(ARInvoiceLine));
					fInvoiceToTest.Lines[0].AL_AC = ChargeCode1.PK;
					fInvoiceToTest.Lines[0].AL_LineAmount = LineAmount1;
					fInvoiceToTest.Lines[0].AL_Sequence = 0;
					fInvoiceToTest.Lines[0].AL_GSTVAT = GSTAmount1;
					fInvoiceToTest.Lines[0].AL_OSAmount = LineAmount1 + GSTAmount1;
					fInvoiceToTest.Lines[0].AL_LineType = TransactionLineTypes.Revenue;
					fInvoiceToTest.Lines[1].AL_AG = GLAccount2.PK;
					fInvoiceToTest.Lines[1].AL_LineAmount = LineAmount2;
					fInvoiceToTest.Lines[1].AL_Sequence = 1;
					fInvoiceToTest.Lines[1].AL_GSTVAT = GSTAmount2;
					fInvoiceToTest.Lines[1].AL_OSGSTAmount = GSTAmount2;
					fInvoiceToTest.Lines[1].AL_OSAmount = LineAmount2 + GSTAmount2;
				}

				return fInvoiceToTest;
			}
		}

		protected InvoicingBase fInvoiceToTest;
		//protected new ZDecimal LineAmount1;
		//protected new ZDecimal LineAmount2;
		//protected new string LocalAccountNumber1;
		//protected new string LocalAccountNumber2;
		protected string LocalControlAccount;
		protected string LocalGST;
		protected InvoiceCreditAdjustmentVoucherProvider TestInvoiceCreditAdjustmentVoucher;
		readonly ZString CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			LocalAccountNumber1 = "1000.10.10InCNY";
			LocalAccountNumber2 = "1000.10.20InCNY";
			LocalControlAccount = "1000.10.30InCNY";
			LocalGST = "1000.10.40InCNY";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ControlAccount = TestObjectCreator.InsertGLHeader();
			GSTAccount = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			GLAccountDescriptor1 = SetupAccountDescriptor(GLAccount1, LocalAccountNumber1);
			GLAccountDescriptor2 = SetupAccountDescriptor(GLAccount2, LocalAccountNumber2);
			GLAccountDescriptor3 = SetupAccountDescriptor(ControlAccount, LocalControlAccount);
			GLAccountDescriptor4 = SetupAccountDescriptor(GSTAccount, LocalGST);
			LineAmount1 = 120m;
			LineAmount2 = 70m;
			GSTAmount1 = 12.0m;
			GSTAmount2 = 7.0m;
			base.SetUp();
			SetUpInvoice();
			TestInvoiceCreditAdjustmentVoucher = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest, new ControlAccountProvider());
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		protected AccGLAccountDescriptor SetupAccountDescriptor(AccGLHeader gLAccount, string localAccountNum)
		{
			AccGLAccountDescriptor gLAccountDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLAccountDescriptor.AJ_LocalAccountNumber = localAccountNum;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			gLAccountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			gLAccountDescriptor.AJ_DebitCredit = gLAccount.AG_DebitCredit.IsEmpty ? (ZString)Constants.DebitCredit.Debit : gLAccount.AG_DebitCredit;
			Factory.Save();
			return gLAccountDescriptor;
		}

		protected void SetUpInvoice()
		{
			AccTransactionHeader invoice = InvoiceToTest;
		}

		protected override ZString GetExpectedLineDescriptionForFirstLine()
		{
			return "- TRANSACTION_LINE_DESCRIPTION";
		}

		protected override ZString GetExpectedLineDescriptionForControlLine()
		{
			return "- TRANSACTION_HEADER_DESCRIPTION";
		}
	}
}

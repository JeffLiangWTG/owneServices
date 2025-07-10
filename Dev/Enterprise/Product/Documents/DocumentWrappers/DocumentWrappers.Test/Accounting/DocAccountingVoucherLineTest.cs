using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocAccountingVoucherLine))]
	sealed class DocAccountingVoucherLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocAccountingVoucherLine.New(new VoucherLine(), Factory) };
		}

		VoucherLine fVoucher;
		VoucherLine Voucher
		{
			get
			{
				if (fVoucher == null)
				{
					fVoucher = new VoucherLine(Factory);
					SetVoucherLine(fVoucher);
				}
				return fVoucher;
			}
		}

		DocAccountingVoucherLine fDocVoucherLineToTest;
		DocAccountingVoucherLine DocVoucherLineToTest
		{
			get { return fDocVoucherLineToTest ?? (fDocVoucherLineToTest = DocAccountingVoucherLine.New(Voucher, Factory)); }
		}

		public void TestParentGLAccountAndDescription()
		{
			AssertEquals("1111.11.11 - AG_Description", DocVoucherLineToTest.ParentGLAccountAndDescription);
		}

		public void TestGLAccountNumAndDescriptionForJCJRJ()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "DP1";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_Ledger = LedgerTypes.JobCosting;
				transaction.AH_TransactionType = TransactionTypes.JobRevenueJournal;
				var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				transactionLine.AL_AH = transaction.PK;
				transaction.AH_GB = transactionLine.AL_GB = branch.PK;
				transaction.AH_GE = transactionLine.AL_GE = department.PK;

				var gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
				var accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
				accountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
				accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
				accountDescriptor.AJ_LocalAccountNumber = "1010.101";
				accountDescriptor.AJ_AccountDescription = "My Description";
				accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				accountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
				accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
				Factory.Save();

				var testProvider = new InvoiceVoucherLineProvider(transactionLine);
				var voucherLine = testProvider.VoucherLine;
				voucherLine.AccountPK = gLAccount.PK;
				var docVoucherLine = DocAccountingVoucherLine.New(voucherLine, Factory);

				AssertEquals("1010.101 - My Description (BR1, DP1)", docVoucherLine.GLAccountNumAndDescription);
			}
		}

		public void TestGLAccountNumAndDescription()
		{
			AssertEquals("1010.101 - My Description", DocVoucherLineToTest.GLAccountNumAndDescription);
		}

		public void TestAppendix()
		{
			AssertEquals("Appendix", DocVoucherLineToTest.Appendix);
		}

		public void TestCreditAmount()
		{
			AssertEquals(70m, DocVoucherLineToTest.CreditAmount);
		}

		public void TestDebitAmount()
		{
			AssertEquals(120m, DocVoucherLineToTest.DebitAmount);
		}

		public void TestOSCreditAmount()
		{
			AssertEquals(170m, DocVoucherLineToTest.OSCreditAmount);
		}

		public void TestOSDebitAmount()
		{
			AssertEquals(130m, DocVoucherLineToTest.OSDebitAmount);
		}

		public void TestTransactionDecription()
		{
			AssertEquals("Description", DocVoucherLineToTest.TransactionDecription);
		}

		public void TestVoucherDate()
		{
			AssertEquals(new ZDateTime(2004, 3, 6), DocVoucherLineToTest.VoucherDate);
		}

		public void TestVoucherNumber()
		{
			AssertEquals("23842", DocVoucherLineToTest.VoucherNumber);
		}

		public void TestVoucherType()
		{
			AssertEquals("INV", DocVoucherLineToTest.VoucherType);
		}

		public void TestCurrencyCode()
		{
			Voucher.CurrencyCode = "XYZ";
			AssertEquals("XYZ", DocVoucherLineToTest.CurrencyCode);
		}

		public void TestForeignCurrencyAmount()
		{
			Voucher.ForeignCurrencyAmount = 123456789M;
			AssertEquals(Voucher.ForeignCurrencyAmount, DocVoucherLineToTest.ForeignCurrencyAmount);
		}

		public void TestExchangeRate()
		{
			Voucher.ExchangeRate = 1.23456M;
			AssertEquals(Voucher.ExchangeRate, DocVoucherLineToTest.ExchangeRate);
		}

		void SetVoucherLine(VoucherLine line)
		{
			AccGLHeader gLAccount1 = SetGLAccount();
			SetAccountDescriptor(gLAccount1);

			line.AccountDescription = "Account Description";
			line.AccountPK = gLAccount1.PK;
			line.Appendix = "Appendix";
			line.AttachmentCount = 1;
			line.Description = "Description";
			line.VoucherDate = new ZDateTime(2004, 3, 6);
			line.VoucherNumber = "23842";
			line.VoucherType = "INV";
			line.DebitAmount = 120m;
			line.CreditAmount = 70m;
			line.OSDebitAmount = 130m;
			line.OSCreditAmount = 170m;
		}

		void SetAccountDescriptor(AccGLHeader gLAccount)
		{
			AccGLAccountDescriptor accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;

			accountDescriptor.AJ_Language = Constants.Languages.ChineseSimplified;
			accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			accountDescriptor.AJ_LocalAccountNumber = "1010.101";
			accountDescriptor.AJ_AccountDescription = "My Description";
			accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			accountDescriptor.AJ_ReportCategory = ZArchitecture.Core.AccountTypeComboBoxConstants.BalanceSheetAccount;
			accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			Factory.Save();
			return;
		}

		AccGLHeader SetGLAccount()
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1111.11.11";
			gLAccount.AG_Description = "AG_Description";
			return gLAccount;
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.China; }
		}
	}
}

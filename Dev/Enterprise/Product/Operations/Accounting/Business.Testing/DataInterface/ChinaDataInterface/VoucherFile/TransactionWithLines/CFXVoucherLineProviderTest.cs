using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class CFXVoucherLineProviderTest : TestCaseWithFactory
	{
		public void TestCFXVoucherLineGLAccount()
		{
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			TestTransaction = GetJobCostingJournal();

			TestTransaction.Lines[0].AL_AG = ZGuid.Invalid;
			AssertNull("Pre-requisite", TestTransaction.Lines[0].GLHeader);
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals("Voucher line's account should be transaction line's charge code's account if transaction line's GLHeader is null.", ChargeCode1.RevenueAccount.PK, TestVoucherLineProvider.VoucherLine.AccountPK);

			TestTransaction.Lines[0].AL_AG = GLAccount1.PK;
			AssertEquals("Pre-requisite", TestTransaction.Lines[0].GLHeader.PK, GLAccount1.PK);
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals("We should firstly get transaction line's GLHeader as voucher line's account if transaction line's GLHeader is not null.", GLAccount1.PK, TestVoucherLineProvider.VoucherLine.AccountPK);
		}

		public void TestAccountNumber()
		{
			TestTransaction = GetJobCostingJournal();
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(LocalAccountNumber1, TestVoucherLineProvider.VoucherLine.AccountNumber);
		}

		public void TestVoucherDate()
		{
			TestTransaction = GetJobCostingJournal();
			ZDateTime testDateTime = Env.Time.CurrentLocalDateTime;
			TestTransaction.Lines[0].AL_PostDate = testDateTime;
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(testDateTime, TestVoucherLineProvider.VoucherLine.VoucherDate);
		}

		public void TestVoucherType()
		{
			ZString expectedVoucherType = "JC-JNL";
			TestTransaction = GetJobCostingJournal();
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(expectedVoucherType, TestVoucherLineProvider.VoucherLine.VoucherType);
		}

		public void TestVoucherNumber()
		{
			TestTransaction = GetJobCostingJournal();
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(TestTransaction.AH_TransactionNum, TestVoucherLineProvider.VoucherLine.VoucherNumber);
		}

		public void TestVoucherDebit()
		{
			TestTransaction = GetJobCostingJournal();
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(0.0m, TestVoucherLineProvider.VoucherLine.DebitAmount);
		}

		public void TestVoucherCredit()
		{
			TestTransaction = GetJobCostingJournal();
			TestVoucherLineProvider = new CFXVoucherLineProvider(TestTransaction.Lines[0]);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.CreditAmount);
			AssertEquals(120.0m, TestVoucherLineProvider.VoucherLine.ForeignCurrencyAmount);
			AssertEquals(1m, TestVoucherLineProvider.VoucherLine.ExchangeRate);
		}

		public void TestAccountPK()
		{
			var transaction1 = Factory.NewWithValidTestData<JCJournalHeader>();
			transaction1.Lines.AddNew().AL_AG = TestObjectCreator.GLHeader1.PK;
			transaction1.AH_TransactionType = TransactionTypes.Journal;
			var transaction2 = Factory.NewWithValidTestData<JobRevenueJournal>();
			transaction2.Lines.AddNew().AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();

			var journalControlAccount = TestObjectCreator.CreateAccGLHeader("5300.50.00", "AS", "EXCHANGE RESERVE Custom", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var jobRevenueJournalControlAccount = TestObjectCreator.CreateAccGLHeader("6555.55.55", "AS", "Job Revenue Journal Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			using (AccountingConfigurationRegistry.Instance.CFXAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, journalControlAccount.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()))
			{
				var testVoucherLineProvider1 = new CFXVoucherLineProvider(transaction1.Lines[0]);
				var testVoucherLineProvider2 = new CFXVoucherLineProvider(transaction2.Lines[0]);

				AssertEquals(journalControlAccount.PK, testVoucherLineProvider1.VoucherCFXLine.AccountPK);
				AssertEquals(jobRevenueJournalControlAccount.PK, testVoucherLineProvider2.VoucherCFXLine.AccountPK);
			}
		}

		AccChargeCode ChargeCode1;
		AccGLHeader GLAccount2;
		AccGLHeader GLAccount1;
		string LocalAccountNumber1;
		string LocalAccountNumber2;
		JCJournalHeader TestTransaction;
		CFXVoucherLineProvider TestVoucherLineProvider;
		AccGLAccountDescriptor SetupAccountDescriptor(ZString localAccountNumber, ZGuid gLAccountPK)
		{
			AccGLAccountDescriptor gLAccountDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLAccountDescriptor.AJ_LocalAccountNumber = localAccountNumber;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			gLAccountDescriptor.ParentGLHeaderPK = gLAccountPK;
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			gLAccountDescriptor.AJ_DebitCredit = Constants.DebitCredit.Debit;
			return gLAccountDescriptor;
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
			LocalAccountNumber2 = "1000.20.10InCNY";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Revenue);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			SetupAccountDescriptor(LocalAccountNumber1, GLAccount1.PK);
			SetupAccountDescriptor(LocalAccountNumber2, GLAccount2.PK);
			Factory.Save();
		}

		JCJournalHeader GetJobCostingJournal()
		{
			JCJournalHeader journal = Factory.NewWithValidTestData(typeof(JCJournalHeader)) as JCJournalHeader;
			journal.Lines.AddNew();
			journal.Lines[0].AL_LineType = TransactionLineTypes.Revenue;
			journal.Lines[0].AL_AC = ChargeCode1.PK;
			journal.Lines[0].AL_ExchangeRate = 1.0m;
			journal.Lines[0].AL_OSAmount = 120.0m;
			journal.Lines[0].AL_LineAmount = 120.0m;
			return journal;
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

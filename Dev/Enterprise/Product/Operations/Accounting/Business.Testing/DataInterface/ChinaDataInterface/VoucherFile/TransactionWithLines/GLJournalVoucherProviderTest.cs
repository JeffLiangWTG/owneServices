using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(GLJournalVoucherProvider))]
	public class GLJournalVoucherProviderTest : TransactionWithLinesTestCase
	{
		public void TestFXForJournalVoucher()
		{
			ZString curr = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AddGLJournalLine(JournalToTest, -120);
			AddGLJournalLine(JournalToTest, 110);
			JournalToTest.AH_ExchangeRate = 1m;
			JournalToTest.AH_RX_NKTransactionCurrency = curr;
			JournalToTest.Lines[0].AL_RX_NKTransactionCurrency = "USD";
			JournalToTest.Lines[1].AL_RX_NKTransactionCurrency = curr;
			JournalToTest.Lines[0].AL_ExchangeRate = 6m;
			JournalToTest.Lines[1].AL_ExchangeRate = 1m;
			JournalToTest.Lines[0].AL_LineAmount = -120m;
			JournalToTest.Lines[1].AL_LineAmount = 110m;
			JournalToTest.Lines[0].AL_OSAmount = -20m;
			JournalToTest.Lines[1].AL_OSAmount = 110m;
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(110m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(110m, TestProvider.VoucherLines[0].OSDebitAmount);
			AssertEquals(0.0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(0.0m, TestProvider.VoucherLines[0].OSCreditAmount);
			AssertEquals(110m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(curr, TestProvider.VoucherLines[0].CurrencyCode);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[1].OSDebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(20m, TestProvider.VoucherLines[1].OSCreditAmount);
			AssertEquals(20m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals("USD", TestProvider.VoucherLines[1].CurrencyCode);
			AssertEquals(6m, TestProvider.VoucherLines[1].ExchangeRate);
			JournalToTest.Lines[0].AL_LineAmount = -20m;
			JournalToTest.Lines[1].AL_LineAmount = 10m;
			JournalToTest.Lines[0].AL_OSAmount = 0m;
			JournalToTest.Lines[1].AL_OSAmount = 10m;
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(10m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(10m, TestProvider.VoucherLines[0].OSDebitAmount);
			AssertEquals(0.0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(0.0m, TestProvider.VoucherLines[0].OSCreditAmount);
			AssertEquals(10m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(curr, TestProvider.VoucherLines[0].CurrencyCode);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[1].OSDebitAmount);
			AssertEquals(20m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(20m, TestProvider.VoucherLines[1].OSCreditAmount);
			AssertEquals(0m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals(curr, TestProvider.VoucherLines[1].CurrencyCode);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
		}

		public void TestDebitInFirstRowForJournalVoucher()
		{
			AddGLJournalLine(JournalToTest, -120);
			AddGLJournalLine(JournalToTest, 80);
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(80m, TestProvider.VoucherLines[0].DebitAmount);
		}

		public void TestLineCount()
		{
			AddGLJournalLine(JournalToTest, 120);
			AddGLJournalLine(JournalToTest, -80);
			AddGLJournalLine(JournalToTest, -40);
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(3, TestProvider.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			AddGLJournalLine(JournalToTest, 120);
			AddGLJournalLine(JournalToTest, -120);
			JournalToTest.Lines[0].AL_AG = GLAccount1.PK;
			JournalToTest.Lines[1].AL_AG = GLAccount2.PK;
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
			AssertEquals(LocalAccountNumber2, TestProvider.VoucherLines[1].AccountNumber);
		}

		public void TestVoucherType()
		{
			AddGLJournalLine(JournalToTest, 120);
			AddGLJournalLine(JournalToTest, -120);
			JournalToTest.Lines[0].AL_AG = GLAccount1.PK;
			JournalToTest.Lines[1].AL_AG = GLAccount2.PK;
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals("GL-GJL", TestProvider.VoucherLines[0].VoucherType);
			AssertEquals("GL-GJL", TestProvider.VoucherLines[1].VoucherType);
		}

		public void TestVoucherNumber()
		{
			AddGLJournalLine(JournalToTest, 120);
			AddGLJournalLine(JournalToTest, -120);
			JournalToTest.AH_TransactionNum = "ASGRT";
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals("ASGRT", TestProvider.VoucherLines[0].VoucherNumber);
			AssertEquals("ASGRT", TestProvider.VoucherLines[1].VoucherNumber);
		}

		public void TestVoucherDebitCredit()
		{
			AddGLJournalLine(JournalToTest, 120);
			AddGLJournalLine(JournalToTest, -80);
			AddGLJournalLine(JournalToTest, -40);
			TestProvider = new GLJournalVoucherProvider(JournalToTest);
			AssertEquals(3, TestProvider.VoucherLines.Length);
			AssertEquals(120.0m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0.0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120.0m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(0.0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(80.0m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(80.0m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(0.0m, TestProvider.VoucherLines[2].DebitAmount);
			AssertEquals(40.0m, TestProvider.VoucherLines[2].CreditAmount);
			AssertEquals(40.0m, TestProvider.VoucherLines[2].ForeignCurrencyAmount);
			AssertEquals(1m, TestProvider.VoucherLines[2].ExchangeRate);
		}

		protected GLJournal JournalToTest;
		// protected new GLJournalVoucherProvider TestProvider;
		protected override void SetUp()
		{
			base.SetUp();
			JournalToTest = Factory.NewWithValidTestData(typeof(GLJournal)) as GLJournal;
		}

		protected void AddGLJournalLine(GLJournal gLJournal, ZDecimal lineAmount)
		{
			if (gLJournal != null)
			{
				gLJournal.Lines.AddNew(typeof(GLJournalLine));
				gLJournal.Lines[gLJournal.Lines.Count - 1].AL_ExchangeRate = 1m;
				gLJournal.Lines[gLJournal.Lines.Count - 1].AL_OSAmount = lineAmount;
				gLJournal.Lines[gLJournal.Lines.Count - 1].AL_LineAmount = lineAmount;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLJournalVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new GLJournalVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			GLJournal newJournal = Factory.NewWithValidTestData<GLJournal>();
			AddGLJournalLine(newJournal, 120);
			AddGLJournalLine(newJournal, -120);
			newJournal.Lines[0].AL_AG = GLAccount1.PK;
			newJournal.Lines[1].AL_AG = GLAccount2.PK;
			return newJournal;
		}

		protected override void SetLineDescription(AccTransactionHeader header)
		{
			base.SetLineDescription(header);
			GLJournal headerWithLines = Factory.Load<GLJournal>(header.PK);
			headerWithLines.AH_Desc = "TRANSACTION_HEADER_DESCRIPTION";
			foreach (AccTransactionLines line in headerWithLines.Lines)
			{
				line.AL_Desc = "TRANSACTION_LINE_DESCRIPTION";
			}
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}
	}
}

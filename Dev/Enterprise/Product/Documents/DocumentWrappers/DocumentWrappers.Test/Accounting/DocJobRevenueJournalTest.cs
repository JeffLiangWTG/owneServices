using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobRevenueJournal))]
	sealed class DocJobRevenueJournalTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobRevenueJournal.New(Journal, Factory)
			};
		}

		public void TestLines()
		{
			AddJournalLine(Journal, 100M, DebitCreditDataEntry.DR);
			AddJournalLine(Journal, 100M, DebitCreditDataEntry.CR);
			AddJournalLine(Journal, 20.76M, DebitCreditDataEntry.DR);
			AddJournalLine(Journal, 50.90M, DebitCreditDataEntry.DR);
			AddJournalLine(Journal, 71.66M, DebitCreditDataEntry.CR);

			var result = JournalWrapper.Lines;
			AssertEquals(5, result.Count);
			AssertEquals("100.00", result[0].DebitAmount);
			AssertEquals(100.00m, result[0].DebitAmountDecimal);
			AssertEquals("", result[0].CreditAmount);
			AssertEquals(0m, result[0].CreditAmountDecimal);
			AssertEquals("", result[1].DebitAmount);
			AssertEquals(0m, result[1].DebitAmountDecimal);
			AssertEquals("100.00", result[1].CreditAmount);
			AssertEquals(100.00m, result[1].CreditAmountDecimal);
			AssertEquals("20.76", result[2].DebitAmount);
			AssertEquals(20.76m, result[2].DebitAmountDecimal);
			AssertEquals("", result[2].CreditAmount);
			AssertEquals(0m, result[2].CreditAmountDecimal);
			AssertEquals("50.90", result[3].DebitAmount);
			AssertEquals(50.90m, result[3].DebitAmountDecimal);
			AssertEquals("", result[3].CreditAmount);
			AssertEquals(0m, result[3].CreditAmountDecimal);
			AssertEquals("", result[4].DebitAmount);
			AssertEquals(0m, result[4].DebitAmountDecimal);
			AssertEquals("71.66", result[4].CreditAmount);
			AssertEquals(71.66m, result[4].CreditAmountDecimal);

			AssertEquals(171.66m, JournalWrapper.TotalDebitAmount);
			AssertEquals(171.66m, JournalWrapper.TotalCreditAmount);
		}

		#region Implementation

		JobRevenueJournal Journal
		{
			get { return journal ?? (journal = Factory.New<JobRevenueJournal>()); }
		}
		JobRevenueJournal journal;
		DocJobRevenueJournal JournalWrapper
		{
			get { return journalWrapper ?? (journalWrapper = DocJobRevenueJournal.New(Journal, Factory)); }
		}

		DocJobRevenueJournal journalWrapper;

		void AddJournalLine(JobRevenueJournal journal, ZDecimal amount, ZString debitCredit)
		{
			var line = journal.JournalLines.AddNew();
			line.DebitCreditSign = debitCredit;
			line.LocalUnsignedLineAmount = amount;
		}

		#endregion
	}
}

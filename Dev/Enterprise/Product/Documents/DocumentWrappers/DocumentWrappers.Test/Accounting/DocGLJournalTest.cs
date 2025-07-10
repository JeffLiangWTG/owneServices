using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGLJournal))]
	sealed class DocGLJournalTest : ZArchitecture.Business.Testing.DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocGLJournal.New(Journal, Factory)
			};
		}

		public void TestHasForeignCurrencyLines()
		{
			AssertEquals("Precondition: Journal Header currency", "ERN", Journal.TransactionCurrency.RX_Code);
			AddGLJournalLine(Journal, "ERN", 1M, 100M, 100M, DebitCreditDataEntry.CR);
			AddGLJournalLine(Journal, "ERN", 1M, 100M, 100M, DebitCreditDataEntry.DR);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			var result = JournalWrapper.Lines;
			AssertEquals(2, result.Count);

			Assert(!JournalWrapper.HasForeignCurrencyLines);

			var foreignCurrencyLine = AddGLJournalLine(Journal, "USD", 0.90M, 90M, 100M, DebitCreditDataEntry.DR);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			result = JournalWrapper.Lines;
			AssertEquals(3, result.Count);

			Assert(JournalWrapper.HasForeignCurrencyLines);

			foreignCurrencyLine.AL_RX_NKTransactionCurrency = Journal.AH_RX_NKTransactionCurrency;
			Assert("Do not recalculate value on each property call from document template.", JournalWrapper.HasForeignCurrencyLines);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			Assert(!JournalWrapper.HasForeignCurrencyLines);
		}

		public void TestTotalDebitCreditAmountCaching()
		{
			var line1 = AddGLJournalLine(Journal, "AUD", 1M, 100M, 100M, DebitCreditDataEntry.DR);
			var line2 = AddGLJournalLine(Journal, "AUD", 1M, 100M, 100M, DebitCreditDataEntry.CR);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			AssertEquals(100m, JournalWrapper.TotalDebitAmount);
			AssertEquals(100m, JournalWrapper.TotalCreditAmount);

			line1.UnsignedLocalLineAmount = 50;
			line2.UnsignedLocalLineAmount = 10;
			AssertEquals("Do not recalculate value on each property call from document template.", 100m, JournalWrapper.TotalDebitAmount);
			AssertEquals("Do not recalculate value on each property call from document template.", 100m, JournalWrapper.TotalCreditAmount);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			AssertEquals(50m, JournalWrapper.TotalDebitAmount);
			AssertEquals(10m, JournalWrapper.TotalCreditAmount);
		}

		public void TestLines()
		{
			AddGLJournalLine(Journal, "USD", 0.90M, 90M, 100M, DebitCreditDataEntry.DR);
			AddGLJournalLine(Journal, "AUD", 1M, 100M, 100M, DebitCreditDataEntry.CR);
			AddGLJournalLine(Journal, "AUD", 1M, 20.76M, 20.76M, DebitCreditDataEntry.DR);
			AddGLJournalLine(Journal, "AUD", 1M, 50.90M, 50.90M, DebitCreditDataEntry.DR);
			AddGLJournalLine(Journal, "AUD", 1M, 71.66M, 71.66M, DebitCreditDataEntry.CR);

			JournalWrapper = DocGLJournal.New(Journal, Factory);
			DocTransactionLineCollection result = JournalWrapper.Lines;
			AssertEquals(5, result.Count);

			AssertEquals(typeof(DocGLJournalLine), result[0].GetType());
			AssertEquals("USD", result[0].Currency.ToString());
			AssertEquals(0.90M, result[0].ExchangeRate);
			AssertEquals("90.00 DR", result[0].ForeignCurrencyEquivalent);
			AssertEquals("100.00", result[0].DebitAmount);
			AssertEquals(100.00m, result[0].DebitAmountDecimal);
			AssertEquals("", result[0].CreditAmount);
			AssertEquals(0m, result[0].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[1].GetType());
			AssertEquals("AUD", result[1].Currency.ToString());
			AssertEquals(1M, result[1].ExchangeRate);
			AssertEquals("100.00 CR", result[1].ForeignCurrencyEquivalent);
			AssertEquals("", result[1].DebitAmount);
			AssertEquals(0m, result[1].DebitAmountDecimal);
			AssertEquals("100.00", result[1].CreditAmount);
			AssertEquals(100.00m, result[1].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[2].GetType());
			AssertEquals("AUD", result[2].Currency.ToString());
			AssertEquals(1M, result[2].ExchangeRate);
			AssertEquals("20.76 DR", result[2].ForeignCurrencyEquivalent);
			AssertEquals("20.76", result[2].DebitAmount);
			AssertEquals(20.76m, result[2].DebitAmountDecimal);
			AssertEquals("", result[2].CreditAmount);
			AssertEquals(0m, result[2].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[3].GetType());
			AssertEquals("AUD", result[3].Currency.ToString());
			AssertEquals(1M, result[3].ExchangeRate);
			AssertEquals("50.90 DR", result[3].ForeignCurrencyEquivalent);
			AssertEquals("50.90", result[3].DebitAmount);
			AssertEquals(50.90m, result[3].DebitAmountDecimal);
			AssertEquals("", result[3].CreditAmount);
			AssertEquals(0m, result[3].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[4].GetType());
			AssertEquals("AUD", result[4].Currency.ToString());
			AssertEquals(1M, result[4].ExchangeRate);
			AssertEquals("71.66 CR", result[4].ForeignCurrencyEquivalent);
			AssertEquals("", result[4].DebitAmount);
			AssertEquals(0m, result[4].DebitAmountDecimal);
			AssertEquals("71.66", result[4].CreditAmount);
			AssertEquals(71.66m, result[4].CreditAmountDecimal);

			AssertEquals(171.66m, JournalWrapper.TotalDebitAmount);
			AssertEquals(171.66m, JournalWrapper.TotalCreditAmount);
		}

		public void TestForeignCurrencyBalanceAdjustmentLines()
		{
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var line1 = AddForeignCurrencyBalanceAdjustmentJournalLine(ForeignCurrencyBalanceAdjustmentJournal, "AUD", 1M, 20.76M, 20.76M, DebitCreditDataEntry.DR, false);
			Assert("Precondition: line1 is local amount line", line1.IsUnrealizedLocalAmountLine);
			var line2 = AddForeignCurrencyBalanceAdjustmentJournalLine(ForeignCurrencyBalanceAdjustmentJournal, "AUD", 1M, 50.90M, 50.90M, DebitCreditDataEntry.DR, false);
			Assert("Precondition: line2 is local amount line", line2.IsUnrealizedLocalAmountLine);
			var line3 = AddForeignCurrencyBalanceAdjustmentJournalLine(ForeignCurrencyBalanceAdjustmentJournal, "USD", 0.90M, 0M, 71.66M, DebitCreditDataEntry.CR, true);
			Assert("Precondition: line3 is foreign amount line", line3.IsForeignCurrencyBalanceAdjustmentLine);

			JournalWrapper = DocGLJournal.New(ForeignCurrencyBalanceAdjustmentJournal, Factory);
			DocTransactionLineCollection result = JournalWrapper.Lines;
			AssertEquals(3, result.Count);

			AssertEquals(typeof(DocGLJournalLine), result[0].GetType());
			AssertEquals("AUD", result[0].Currency.ToString());
			AssertEquals(1M, result[0].ExchangeRate);
			AssertEquals("20.76", result[0].DebitAmount);
			AssertEquals("20.76 DR", result[0].ForeignCurrencyEquivalent);
			AssertEquals(20.76m, result[0].DebitAmountDecimal);
			AssertEquals("", result[0].CreditAmount);
			AssertEquals(0m, result[0].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[1].GetType());
			AssertEquals("AUD", result[1].Currency.ToString());
			AssertEquals(1M, result[1].ExchangeRate);
			AssertEquals("50.90", result[1].DebitAmount);
			AssertEquals("50.90 DR", result[1].ForeignCurrencyEquivalent);
			AssertEquals(50.90m, result[1].DebitAmountDecimal);
			AssertEquals("", result[1].CreditAmount);
			AssertEquals(0m, result[1].CreditAmountDecimal);

			AssertEquals(typeof(DocGLJournalLine), result[2].GetType());
			AssertEquals("USD", result[2].Currency.ToString());
			AssertEquals(0.90M, result[2].ExchangeRate);
			AssertEquals("", result[2].DebitAmount);
			AssertEquals("", result[2].ForeignCurrencyEquivalent);
			AssertEquals(0m, result[2].DebitAmountDecimal);
			AssertEquals("71.66", result[2].CreditAmount);
			AssertEquals(71.66m, result[2].CreditAmountDecimal);

			AssertEquals(71.66m, JournalWrapper.TotalDebitAmount);
			AssertEquals(71.66m, JournalWrapper.TotalCreditAmount);
		}

		public void TestPeriodDisplay()
		{
			AssertEquals("", JournalWrapper.PeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			AssertEquals("ENDING PERIOD", JournalWrapper.PeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLReversingJournal;
			AssertEquals("REVERSED IN", JournalWrapper.PeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLStandardJournal;
			AssertEquals("", JournalWrapper.PeriodDisplay);
		}

		public void AgePeriodDisplay()
		{
			AssertEquals("", JournalWrapper.AgePeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			AssertEquals("", JournalWrapper.AgePeriodDisplay);
			Journal.AH_AgePeriod = 200423;
			AssertEquals("200423", JournalWrapper.AgePeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLReversingJournal;
			AssertEquals("", JournalWrapper.PeriodDisplay);
			Journal.AH_AgePeriod = 200422;
			AssertEquals("200422", JournalWrapper.AgePeriodDisplay);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLStandardJournal;
			AssertEquals("", JournalWrapper.PeriodDisplay);
			Journal.AH_AgePeriod = 200422;
			AssertEquals("200422", JournalWrapper.AgePeriodDisplay);
		}

		public void TestJournalType()
		{
			AssertEquals("", JournalWrapper.JournalType);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			AssertEquals("Auto ", JournalWrapper.JournalType);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLReversingJournal;
			AssertEquals("Reversing ", JournalWrapper.JournalType);

			Journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLStandardJournal;
			AssertEquals("", JournalWrapper.JournalType);
		}

		GLJournal Journal;
		FCBAdjustmentJournal ForeignCurrencyBalanceAdjustmentJournal;
		DocGLJournal JournalWrapper;
		protected override void SetUp()
		{
			Journal = Factory.New<GLJournal>();
			ForeignCurrencyBalanceAdjustmentJournal = Factory.New<FCBAdjustmentJournal>();
			JournalWrapper = DocGLJournal.New(Journal, Factory);
			base.SetUp();
		}

		GLJournalLine AddGLJournalLine(GLJournal journal, string currency, ZDecimal exchangeRate, ZDecimal osAmount, ZDecimal localAmount, ZString debitCredit, GLJournalLine line = null)
		{
			var line1 = line ?? (GLJournalLine)journal.Lines.AddNew();
			line1.AL_RX_NKTransactionCurrency = currency;
			line1.AL_ExchangeRate = exchangeRate;
			line1.UnsignedOSLineAmount = osAmount;
			line1.UnsignedLocalLineAmount = localAmount;
			line1.DebitCreditSign = debitCredit;

			return line1;
		}

		FCBAdjustmentJournalLine AddForeignCurrencyBalanceAdjustmentJournalLine(FCBAdjustmentJournal journal, string currency, ZDecimal exchangeRate
			, ZDecimal osAmount, ZDecimal localAmount, ZString debitCredit, bool isForeignBalancingAmountLine)
		{
			FCBAdjustmentJournalLine line = (FCBAdjustmentJournalLine)journal.Lines.AddNew();
			if (isForeignBalancingAmountLine)
			{
				line.AL_AG = Guid.NewGuid();
			}
			else
			{
				line.AL_AG = AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
			}
			AddGLJournalLine(journal, currency, exchangeRate, osAmount, localAmount, debitCredit, line);

			return line;
		}
	}
}

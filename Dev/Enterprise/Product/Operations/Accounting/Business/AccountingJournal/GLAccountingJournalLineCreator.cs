using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class GJLAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public GJLAccountingJournalLineCreator(TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(line, "TransactionLine");
			this.transactionLine = line;
		}
		protected readonly TransactionLine transactionLine;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var glLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(glLine, transactionLine);
			result.Add(new AccountingJournalLine(glLine));

			return result;
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(sourceObject as ISupportMultiSubAccounts, line.Factory);

			if (sourceObject is TransactionLine sourceTransactionLine)
			{
				line.AL_ExchangeRate = sourceTransactionLine.GetHighPrecisionExchangeRate();
			}
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transactionLine.AL_LineType == TransactionTypes.GLStandardJournal;
		}
	}

	public class NJLAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public NJLAccountingJournalLineCreator(TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			transactionLine = Argument.NotNull(line, "TransactionLine");
		}
		protected readonly TransactionLine transactionLine;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var glJournal = Factory.New<GLJournal>();
			glJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			var glLine = glJournal.GLJournalLines.AddNew();
			PopulateLineProperties(glLine, transactionLine);

			var accountingJournalLine = new AccountingJournalLine(glLine);
			accountingJournalLine.SetCurrency(Factory.Load<NoteJournalCurrency>(accountingJournalLine.Currency.PK));
			accountingJournalLine.SetLocalCurrency(Factory.Load<NoteJournalCurrency>(accountingJournalLine.LocalCurrency.PK));

			result.Add(accountingJournalLine);

			return result;
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transactionLine.AL_LineType == TransactionTypes.GLNoteJournal;
		}
	}

	public class RJLAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public RJLAccountingJournalLineCreator(TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(line, "TransactionLine");
			this.transactionLine = line;
		}
		protected readonly TransactionLine transactionLine;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var glLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(glLine, transactionLine);
			result.Add(new AccountingJournalLine(glLine));

			var glRevLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(glRevLine, transactionLine);
			glRevLine.AL_LineAmount = transactionLine.AL_LineAmount * (-1);
			glRevLine.AL_OSAmount = transactionLine.AL_OSAmount * (-1);
			glRevLine.AL_GSTVAT = transactionLine.AL_GSTVAT * (-1);
			glRevLine.AL_PostDate = transactionLine.AL_ReverseDate;
			result.Add(new AccountingJournalLine(glRevLine));

			return result;
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(sourceObject as ISupportMultiSubAccounts, line.Factory);

			if (sourceObject is TransactionLine sourceTransactionLine)
			{
				line.AL_ExchangeRate = sourceTransactionLine.GetHighPrecisionExchangeRate();
			}
		}

		protected ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, transactionLine.AL_ExchangeRate, transactionLine.AL_RX_NKTransactionCurrency);
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transactionLine.AL_LineType == TransactionTypes.GLReversingJournal;
		}
	}

	public class AJLAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public AJLAccountingJournalLineCreator(ZInt startPeriod, ZInt endPeriod, AccountingPeriodCalculator periodCalculator, TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(line, "TransactionLine");
			this.transactionLine = line;
			this.startPeriod = startPeriod;
			this.endPeriod = endPeriod;
			this.periodCalculator = periodCalculator;
		}
		readonly TransactionLine transactionLine;
		readonly ZInt startPeriod;
		readonly ZInt endPeriod;
		readonly AccountingPeriodCalculator periodCalculator;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var currentPeriod = startPeriod;
			while (currentPeriod <= endPeriod && currentPeriod != 0)
			{
				var glLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(glLine, transactionLine);
				glLine.AL_PostDate = periodCalculator.GetLastDayForPeriod(currentPeriod);
				glLine.AL_PostPeriod = currentPeriod;
				result.Add(new AccountingJournalLineWithDifferentPostDateAndPeriod(glLine, currentPeriod));
				currentPeriod = periodCalculator.GetNextPeriod(currentPeriod);
			}

			return result;
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(sourceObject as ISupportMultiSubAccounts, line.Factory);

			if (sourceObject is TransactionLine sourceTransactionLine)
			{
				line.AL_ExchangeRate = sourceTransactionLine.GetHighPrecisionExchangeRate();
			}
		}

		protected ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, transactionLine.AL_ExchangeRate, transactionLine.AL_RX_NKTransactionCurrency);
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transactionLine.AL_LineType == TransactionTypes.GLAutoJournal && endPeriod >= startPeriod;
		}
	}
}

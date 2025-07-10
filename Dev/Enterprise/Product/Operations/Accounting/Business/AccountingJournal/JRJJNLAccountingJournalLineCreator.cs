using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class JRJJNLAccountingJournalCreator : AccountingJournalLineCreator
	{
		public JRJJNLAccountingJournalCreator(ZString transactionType, TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(line, "TransactionLine");
			this.transactionLine = line;
			this.transactionType = transactionType;
		}
		protected readonly TransactionLine transactionLine;
		protected readonly ZString transactionType;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			if (transactionLine.GLHeader != null)
			{
				int multiplier = -1;

				if (GLControlAccounts.Instance.ARSuspenseControlAccount == null)
				{
					throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Caption);
				}

				if (transactionType == TransactionTypes.Journal && GLControlAccounts.Instance.CFXAccount == null)
				{
					throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.CFXAccount.Caption);
				}

				if (transactionType == TransactionTypes.JobRevenueJournal && GLControlAccounts.Instance.JobRevenueJournalControlAccount == null)
				{
					throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Caption);
				}

				//Posting
				var drLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(drLine, transactionLine);
				drLine.AL_AG = GLControlAccounts.Instance.ARSuspenseControlAccount.PK;
				drLine.AL_Desc = GLControlAccounts.Instance.ARSuspenseControlAccount.AG_DescriptionMultilingual;
				drLine.AL_LineAmount = transactionLine.AL_LineAmount * multiplier;
				drLine.AL_OSAmount = transactionLine.AL_OSAmount * multiplier;
				drLine.AL_ReverseDate = ZDateTime.Empty;
				result.Add(new AccountingJournalLine(drLine));

				var crAccount = transactionType == TransactionTypes.Journal ? GLControlAccounts.Instance.CFXAccount : GLControlAccounts.Instance.JobRevenueJournalControlAccount;
				var crLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(crLine, transactionLine);
				crLine.AL_AG = crAccount.PK;
				crLine.AL_Desc = crAccount.AG_DescriptionMultilingual;
				crLine.AL_LineAmount = transactionLine.AL_LineAmount * multiplier * (-1);
				crLine.AL_OSAmount = transactionLine.AL_OSAmount * multiplier * (-1);
				crLine.AL_ReverseDate = ZDateTime.Empty;
				result.Add(new AccountingJournalLine(crLine));

				if (transactionLine.AL_ReverseDate.IsValid)
				{
					//Recognition
					drLine = Factory.New<AccTransactionLines>();
					PopulateLineProperties(drLine, transactionLine);
					drLine.AL_AG = transactionLine.GLHeader.PK;
					drLine.AL_Desc = transactionLine.GLHeader.AG_DescriptionMultilingual;
					drLine.AL_LineAmount = transactionLine.AL_LineAmount * multiplier;
					drLine.AL_OSAmount = transactionLine.AL_OSAmount * multiplier;
					drLine.AL_PostDate = transactionLine.AL_ReverseDate;
					drLine.AL_ReverseDate = ZDateTime.Empty;
					result.Add(new AccountingJournalLine(drLine));

					crLine = Factory.New<AccTransactionLines>();
					PopulateLineProperties(crLine, transactionLine);
					crLine.AL_AG = GLControlAccounts.Instance.ARSuspenseControlAccount.PK;
					crLine.AL_Desc = GLControlAccounts.Instance.ARSuspenseControlAccount.AG_DescriptionMultilingual;
					crLine.AL_LineAmount = transactionLine.AL_LineAmount * multiplier * (-1);
					crLine.AL_OSAmount = transactionLine.AL_OSAmount * multiplier * (-1);
					crLine.AL_PostDate = transactionLine.AL_ReverseDate;
					crLine.AL_ReverseDate = ZDateTime.Empty;
					result.Add(new AccountingJournalLine(crLine));
				}
			}

			return result;
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 0M;
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (transactionType == TransactionTypes.JobRevenueJournal || transactionType == TransactionTypes.Journal);
		}
	}
}


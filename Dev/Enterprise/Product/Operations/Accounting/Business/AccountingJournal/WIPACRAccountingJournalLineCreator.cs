using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class WIPACRAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public WIPACRAccountingJournalLineCreator(TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			this.transactionLine = line;
		}
		readonly TransactionLine transactionLine;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			if (GLControlAccounts.Instance.WIPControlAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Caption);
			}

			if (GLControlAccounts.Instance.ACRControlAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Caption);
			}

			var result = new List<AccountingJournalLine>();

			//Posting
			var drAccount = transactionLine.AL_LineType == TransactionLineTypes.Accrual ? GLControlAccounts.Instance.ACRControlAccount : GLControlAccounts.Instance.WIPControlAccount;
			var drLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(drLine, transactionLine);
			drLine.AL_AG = drAccount.PK;
			drLine.AL_Desc = drAccount.AG_DescriptionMultilingual;
			drLine.AL_LineAmount = transactionLine.AL_LineAmount * (-1);
			drLine.AL_OSAmount = transactionLine.AL_OSAmount * (-1);
			drLine.AL_ReverseDate = ZDateTime.Empty;
			result.Add(new AccountingJournalLine(drLine));

			var crLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(crLine, transactionLine);
			crLine.AL_AG = transactionLine.GLHeader.PK;
			crLine.AL_Desc = transactionLine.GLHeader.AG_DescriptionMultilingual;
			crLine.AL_LineAmount = transactionLine.AL_LineAmount;
			crLine.AL_OSAmount = transactionLine.AL_OSAmount;
			crLine.AL_ReverseDate = ZDateTime.Empty;
			result.Add(new AccountingJournalLine(crLine));

			if (transactionLine.AL_ReverseDate.IsValid)
			{
				//Recognition
				drLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(drLine, transactionLine);
				drLine.AL_AG = transactionLine.GLHeader.PK;
				drLine.AL_Desc = transactionLine.GLHeader.AG_DescriptionMultilingual;
				drLine.AL_LineAmount = transactionLine.AL_LineAmount * (-1);
				drLine.AL_OSAmount = transactionLine.AL_OSAmount * (-1);
				drLine.AL_PostDate = transactionLine.AL_ReverseDate;
				drLine.AL_ReverseDate = ZDateTime.Empty;
				result.Add(new AccountingJournalLine(drLine));

				crLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(crLine, transactionLine);
				crLine.AL_AG = drAccount.PK;
				crLine.AL_Desc = drAccount.AG_DescriptionMultilingual;
				crLine.AL_LineAmount = transactionLine.AL_LineAmount;
				crLine.AL_OSAmount = transactionLine.AL_OSAmount;
				crLine.AL_PostDate = transactionLine.AL_ReverseDate;
				crLine.AL_ReverseDate = ZDateTime.Empty;
				result.Add(new AccountingJournalLine(crLine));
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
			return (transactionLine.AL_LineType == TransactionLineTypes.Accrual || transactionLine.AL_LineType == TransactionLineTypes.WIP);
		}
	}
}

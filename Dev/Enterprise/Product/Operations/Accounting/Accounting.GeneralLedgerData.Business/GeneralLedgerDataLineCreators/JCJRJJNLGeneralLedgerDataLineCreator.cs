using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class JCJRJJNLGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var result = new List<DebitCreditEntryItem>();
			var header = ReadOnlyFactory.Load<AccTransactionHeader>((Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AH]);
			var transactionType = header.AH_TransactionType;
			var lineAmount = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount]);
			var oSAmount = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount]);
			var postDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate]);
			var recognizedDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ReverseDate]);
			var multiplier = -1;

			var glHeaderPKofLine = gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];

			if (glHeaderPKofLine != null)
			{
				if (GLControlAccounts.Instance.ARSuspenseControlAccount == null)
				{
					throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Caption);
				}

				if (gLDDataSourceRow.RowState == DataRowState.Added || gLDDataSourceRow.RowState == DataRowState.Unchanged)
				{
					if (transactionType == TransactionTypes.Journal && GLControlAccounts.Instance.CFXAccount == null)
					{
						throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.CFXAccount.Caption);
					}

					if (transactionType == TransactionTypes.JobRevenueJournal && GLControlAccounts.Instance.JobRevenueJournalControlAccount == null)
					{
						throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Caption);
					}

					AddPostDateLines();
					AddReverseDateLines();
				}
				else if (gLDDataSourceRow.RowState == DataRowState.Modified)
				{
					AddReverseDateLines();
				}
			}
			return result.ToArray();

			void AddPostDateLines()
			{
				if (postDate.IsValid)
				{
					var crAccount = transactionType == TransactionTypes.Journal ? GLControlAccounts.Instance.CFXAccount : GLControlAccounts.Instance.JobRevenueJournalControlAccount;
					var crAccountType = transactionType == TransactionTypes.Journal ? GLDAccountTypes.CFXGLAccount : GLDAccountTypes.JobRevenueJournalControlAccount;

					result.Add(CreateAndPopulateDebitCreditLine(GLControlAccounts.Instance.ARSuspenseControlAccount.PK, GLDAccountTypes.ARSuspenseControlAccount, lineAmount * multiplier, oSAmount * multiplier, postDate));
					result.Add(CreateAndPopulateDebitCreditLine(crAccount.PK, crAccountType, lineAmount, oSAmount, postDate));
				}
			}

			void AddReverseDateLines()
			{
				if (recognizedDate.IsValid)
				{
					result.Add(CreateAndPopulateDebitCreditLine((Guid)glHeaderPKofLine, GLDAccountTypes.TransactionLineGLAccount, lineAmount * multiplier, oSAmount * multiplier, recognizedDate, GLDTypeCodes.Recognition));
					result.Add(CreateAndPopulateDebitCreditLine(GLControlAccounts.Instance.ARSuspenseControlAccount.PK, GLDAccountTypes.ARSuspenseControlAccount, lineAmount, oSAmount, recognizedDate, GLDTypeCodes.Recognition));
				}
			}
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}
	}
}

using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public abstract class GeneralLedgerDataLineCreatorBase
	{
		public GeneralLedgerDataLineCreatorBase()
		{
			ReadOnlyFactory = new ReadOnlyBusinessObjectFactory();
		}

		protected readonly ReadOnlyBusinessObjectFactory ReadOnlyFactory;
		protected DebitCreditEntry DRCREntry { get; private set; }

		public DebitCreditEntry CreateDRCREntries(DataRow gLDDataSourceRow)
		{
			DRCREntry = CreateDebitCreditEntry(gLDDataSourceRow);
			using (Globals.IsUserInteractive ? null : DisposableEnvironment.ForBranch(DRCREntry.BranchPK))
			{
				DRCREntry.EntryItems = CreateDRCREntriesCore(gLDDataSourceRow);
			}

			return DRCREntry;
		}

		protected abstract DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow);

		protected abstract DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow);

		protected DebitCreditEntry CreateGeneralLedgerDataBasicBasedOnHeader(DataRow gLDDataSourceRow)
		{
			var generalLedgerDataBasic = new DebitCreditEntry();
			generalLedgerDataBasic.CompanyPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_GC];
			generalLedgerDataBasic.ExchangeRate = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_ExchangeRate];
			generalLedgerDataBasic.Currency = (string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency];

			generalLedgerDataBasic.TransactionHeaderPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.PK];
			generalLedgerDataBasic.BranchPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_GB];
			generalLedgerDataBasic.TaxBranchPK = GetGuidValueFromColumnValue(gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch]);
			generalLedgerDataBasic.DepartmentPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_GE];

			return generalLedgerDataBasic;
		}

		protected DebitCreditEntry CreateGeneralLedgerDataBasicBasedOnLine(DataRow gLDDataSourceRow)
		{
			var generalLedgerDataBasic = new DebitCreditEntry();
			generalLedgerDataBasic.CompanyPK = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GC];
			generalLedgerDataBasic.ExchangeRate = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ExchangeRate];
			generalLedgerDataBasic.Currency = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_RX_NKTransactionCurrency];

			generalLedgerDataBasic.TransactionLinePK = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.PK];
			generalLedgerDataBasic.TransactionHeaderPK = GetGuidValueFromColumnValue(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AH]);
			generalLedgerDataBasic.BranchPK = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GB];
			generalLedgerDataBasic.TaxBranchPK = GetGuidValueFromColumnValue(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GB_TaxBranch]);
			generalLedgerDataBasic.DepartmentPK = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GE];

			return generalLedgerDataBasic;
		}

		protected Guid GetGuidValueFromColumnValue(object columnValue)
		{
			if (columnValue is Guid guidValue)
			{
				return guidValue;
			}
			else
			{
				return Guid.Empty;
			}
		}

		protected DebitCreditEntryItem CreateAndPopulateDebitCreditLine(ZGuid gLHeaderPK, ZString gLDAccountType, ZDecimal localAmount, ZDecimal osAmount, ZDateTime journalDate, string gLDType = AccountingConstants.GLDTypeCodes.Posting)
		{
			var line = new DebitCreditEntryItem();
			line.AccountPK = gLHeaderPK;
			line.GLDAccountType = gLDAccountType;
			line.LocalAmount = localAmount;
			line.OSAmount = NeedCalculateOSAmountWhenEmpty && osAmount.IsEmpty ? CalculateOSAmount(localAmount) : osAmount;
			line.DRCRSign = localAmount < 0 ? DebitCredit.CR : DebitCredit.DR;
			line.JournalDate = journalDate;
			line.GLDType = gLDType;

			if (ShouldSetPeriod)
			{
				line.Period = PeriodCalculator.GetPeriodFromDate(journalDate);
			}

			return line;
		}

		protected virtual bool ShouldSetPeriod => true;

		protected virtual bool NeedCalculateOSAmountWhenEmpty => true;

		protected virtual ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return ((ICompany)TransactionCompany).ExchangeRate.LocalToForeign(localAmount, DRCREntry.ExchangeRate, DRCREntry.Currency);
		}

		#region Get Control Account

		protected AccGLHeader GetControlAccountSuspense(ZString transactionLineType)
		{
			AccGLHeader controlAccount = null;

			if (transactionLineType.Equals(TransactionLineTypes.Revenue))
			{
				controlAccount = GLControlAccounts.Instance.ARSuspenseControlAccount;
			}
			else if (transactionLineType.Equals(TransactionLineTypes.Cost))
			{
				controlAccount = GLControlAccounts.Instance.APSuspenseControlAccount;
			}

			return controlAccount;
		}

		protected AccGLHeader GetControlAccount(ZString ledger)
		{
			AccGLHeader controlAccount = null;

			if (ledger.Equals(LedgerTypes.AccountsReceivable))
			{
				controlAccount = GLControlAccounts.Instance.ARControlAccount;
			}
			else if (ledger.Equals(LedgerTypes.AccountsPayable))
			{
				controlAccount = GLControlAccounts.Instance.APControlAccount;
			}

			return controlAccount;
		}

		protected AccGLHeader GetGSTAccountForNotRecognizedLine(ZString transactionLineType, ZString taxBasis)
		{
			AccGLHeader gstAccountForNotRecognizedLine = null;

			if (transactionLineType.Equals(TransactionLineTypes.Revenue) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code))
			{
				gstAccountForNotRecognizedLine = GLControlAccounts.Instance.GSTOutputControlAccount;
			}
			else if (transactionLineType.Equals(TransactionLineTypes.Revenue) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
			{
				gstAccountForNotRecognizedLine = GLControlAccounts.Instance.PendingGSTOutputControlAccount;
			}
			else if (transactionLineType.Equals(TransactionLineTypes.Cost) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code))
			{
				gstAccountForNotRecognizedLine = GLControlAccounts.Instance.GSTInputControlAccount;
			}
			else if (transactionLineType.Equals(TransactionLineTypes.Cost) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
			{
				gstAccountForNotRecognizedLine = GLControlAccounts.Instance.PendingGSTInputControlAccount;
			}

			return gstAccountForNotRecognizedLine;
		}

		#endregion

		GlbCompany TransactionCompany => GeneralLedgerDataRetriever.GetCompanyInfo(ReadOnlyFactory, DRCREntry.CompanyPK).Company;

		protected int LocalCurrencyDecimals => TransactionCompany?.LocalCurrency?.Decimals ?? 2;

		protected AccountingPeriodCalculator PeriodCalculator => GeneralLedgerDataRetriever.GetCompanyInfo(ReadOnlyFactory, DRCREntry.CompanyPK).PeriodCalculator;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public interface IDocumentSupportableWithForcedLogging : IDocumentSupportable
	{
		BusinessObject GetTheBusinessObjectForForceLogging();
	}

	public abstract class AccountingJournal : NonPersistentBusinessObject, IDocumentSupportableWithForcedLogging, IAccountingJournal, ISourceIdentifierProvider
	{
		protected AccountingJournal(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: this(factory, reportingBook, generalLedgerTransactionData)
		{
			Argument.NotNull(transaction, "transaction");
			this.transaction = transaction;
		}

		protected AccountingJournal(ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(factory)
		{
			this.reportingBook = reportingBook;
			this.generalLedgerTransactionData = generalLedgerTransactionData;
		}

		public virtual ZString Ledger
		{
			get
			{
				return transaction != null ? transaction.AH_Ledger : ZString.Empty;
			}
		}

		public virtual ZString TransactionType
		{
			get
			{
				return transaction != null ? transaction.AH_TransactionType : ZString.Empty;
			}
		}

		public virtual ZString TransactionNumber
		{
			get
			{
				return transaction != null ? transaction.AH_TransactionNum : ZString.Empty;
			}
		}

		public virtual ZString TransactionDescription
		{
			get { return transaction != null ? transaction.AH_Desc : ZString.Empty; }
		}

		public virtual ZString TransactionReference => transaction.AH_TransactionReference;

		public virtual ZString ConsolidatedInvoiceRef => transaction.AH_ConsolidatedInvoiceRef;

		public virtual ZString ComplianceSubType => transaction.AH_ComplianceSubType;

		public virtual List<GLJournal> RelatedGLJournals
		{
			get
			{
				return transaction != null ? transaction.RelatedGLJournals : new List<GLJournal>();
			}
		}

		public virtual ZString Currency
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public virtual ZInt LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public virtual JobHeader Job
		{
			get
			{
				return transaction != null ? transaction.Job : null;
			}
		}

		public ZString JournalName
		{
			get { return Res.GetString("723c5f10-4ba0-40b1-a321-a5f7db1f404f", "{0} {1} {2}", Ledger, TransactionType, TransactionNumber); }
		}

		public virtual ZDateTime CreatedDate
		{
			get { return transaction != null ? transaction.CreatedDate : ZDateTime.MinSmallDateTimeValue; }
		}

		public virtual ZString CreatedBy
		{
			get { return transaction != null ? transaction.CreatingUser : ZString.Empty; }
		}

		public virtual ZString LastPostedRequest_RequesterFullName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public virtual ZDateTime LastPostedRequest_RequestedTime
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public virtual ZString LastPostedRequest_ApproverFullName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public virtual ZDateTime LastPostedRequest_ApprovedTime
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public virtual ZString OriginalRequest_RequesterFullName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public virtual ZString OriginalPostedRequest_ApproverFullName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public virtual bool GroupByLinesWhilePrinting
		{
			get { return true; }
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new AccountingJournalDocumentSupporter(this); }
		}

		public IEnumerable<IAccountingJournalLine> Lines
		{
			get
			{
				return fLines;
			}
		}
		IEnumerable<IAccountingJournalLine> fLines;

		public IEnumerable<IAccountingJournalTaxDetail> TaxDetails
		{
			get;
			protected set;
		}

		public DataTable generalLedgerTransactionData;

		public AccReportingBook reportingBook;

		public ZString ReportingBookCode => reportingBook?.ARB_Code ?? ZString.Empty;

		public ZString ReportingBookDescription => reportingBook?.ARB_Description ?? ZString.Empty;

		public ZString ChartCode => reportingBook?.AlternateChart.AAC_Code ?? ZString.Empty;

		public ZString ChartDescription => reportingBook?.AlternateChart.AAC_Description ?? ZString.Empty;

		public bool DisplayParentAccount
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ReportingBookAccountingJournalPrintOption>().FirstOrDefault(x => x.ReportingBook == reportingBook.PK)?.DisplayParentAccount ?? false;
			}
		}

		public abstract Dictionary<ZString, ZString> ApplicableOptionalFields { get; }

		protected abstract IEnumerable<ZString> ValidLedgerTypes { get; }

		protected abstract IEnumerable<ZString> ValidTransactionTypes { get; }

		public ZString PostToPeriod
		{
			get
			{
				var postToPeriod = ZString.Empty;
				if (reportingBook != null && Lines.FirstOrDefault() is AccountingJournalLineForReportingBook accountingJournalLineForReportingBook)
				{
					postToPeriod = accountingJournalLineForReportingBook.ReportingBookPeriod;
				}
				else
				{
					postToPeriod = transaction != null ? transaction.PostPeriod.ToString() : ZString.Empty;
				}
				return postToPeriod;
			}
		}

		public ZBool IsMissingPeriodForReportingBook
		{
			get
			{
				var isMissingPeriodForReportingBook = false;
				if (reportingBook != null && Lines.FirstOrDefault() is AccountingJournalLineForReportingBook accountingJournalLineForReportingBook)
				{
					isMissingPeriodForReportingBook = accountingJournalLineForReportingBook.IsMissingPeriodForReportingBook;
				}

				return isMissingPeriodForReportingBook;
			}
		}

		public static string PostToPeriodText
		{
			get
			{
				return Res.GetString("62332a2b-dc62-4185-8e54-4339266e3987", "POST TO PERIOD");
			}
		}

		public static string ReversePeriodText
		{
			get
			{
				return Res.GetString("7ee4f181-5f68-4722-ac1d-4cc1ef5851dc", "REVERSE PERIOD");
			}
		}

		public static string EndPeriodText
		{
			get
			{
				return Res.GetString("12925bc5-3d66-424b-a90d-f35644b0b9db", "END PERIOD");
			}
		}

		public static string DateReversedText
		{
			get
			{
				return Res.GetString("81b17f68-7c7e-44d0-954a-8cdc6f445c2a", "DATE REVERSED");
			}
		}

		public static string ReversedByText
		{
			get
			{
				return Res.GetString("5e1963be-c686-446d-a479-86824c145d34", "REVERSED BY");
			}
		}

		public static string BankCodeText
		{
			get
			{
				return Res.GetString("f51bc1a4-9c27-4622-80dd-774fe52dea6e", "BANK");
			}
		}

		public static string DebtorText
		{
			get
			{
				return Res.GetString("11e1a546-4cce-4afe-af64-b405304b19a9", "DEBTOR");
			}
		}

		public static string CreditorText
		{
			get
			{
				return Res.GetString("db4e3375-2125-4719-ad55-ba3b610a713e", "CREDITOR");
			}
		}

		public static string PresentationText
		{
			get
			{
				return Res.GetString("48c51a60-a534-4659-94fd-f18ed8ba88dd", "PRESENTATION");
			}
		}

		public static string StatusText
		{
			get
			{
				return Res.GetString("b7a7dba5-cc90-4803-9a44-253394097781", "STATUS");
			}
		}

		public static string TaxBranchText
		{
			get
			{
				return Res.GetString("23DA2F42-C96A-4748-B797-4D9481349F8B", "TAX BRANCH");
			}
		}

		public static string Filler
		{
			get { return ""; }
		}

		protected ZString GetJournalStatus()
		{
			bool isComplete = true;
			if (new ZString[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, LedgerTypes.JobCosting }.Contains(transaction.AH_Ledger)
				&& new ZString[] { TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote, TransactionTypes.Invoice, TransactionTypes.JobRevenueJournal, TransactionTypes.Journal }.Contains(transaction.AH_TransactionType))
			{
				string sql = @" SELECT	COUNT(AL_PK) AS cnt
										FROM	((	SELECT		AL.AL_PK 
													FROM		dbo.AccTransactionLines AL
																LEFT JOIN dbo.AccCashBasisVAT YC ON YC_AL_TransactionLine = AL_PK
													WHERE		AL.AL_AH = @headerPK AND 
																AL_GSTVATBasis = 'C'
													GROUP BY	AL.AL_PK, AL.AL_LineAmount, AL.AL_GSTVAT, AL.AL_GSTVATBasis
													HAVING		AL_GSTVAT <> SUM(ISNULL(YC.YC_TaxAmount, 0))

													UNION

													SELECT		AL.AL_PK 		
													FROM		dbo.AccTransactionLines AL		
													WHERE		AL.AL_AH = @headerPK AND 																 
																AL_JH IS NOT NULL AND 
																AL_ReverseDate is NULL AND
																AL_RevRecognitionType != 'IMM')) a";

				var col = new DynamicBusinessObjectCollection(Factory);
				col.Load(sql, new ZSqlParameter[] { ZSqlParameter.New("@headerPK", transaction.PK, AccTransactionLinesSchema.AL_AH) });
				isComplete = new ZInt(col[0]["cnt"]) == 0;
			}

			return isComplete ? Res.GetString("e0c30250-72ba-44aa-9a11-9272ada17334", "Completed") : Res.GetString("7423c6c0-5ae1-4247-967b-a44fe2c00ac1", "Incomplete");
		}

		#region DocumentSupporter
		BusinessObject IDocumentSupportableWithForcedLogging.GetTheBusinessObjectForForceLogging()
		{
			return transaction;
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return fDocumentSupporter ?? (fDocumentSupporter = new AccountingJournalDocumentSupporter(this)); }
		}
		DocumentSupporter fDocumentSupporter;
		#endregion

		#region Implementation

		protected void PopulateJournalLines()
		{
			if (JournalLineProvider is not null)
			{
				fLines = JournalLineProvider.GetAccountingJournalLines();

				if (AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value && fLines.IsNullOrEmpty() && journalLineProvider is AccountingJournalLineProviderForGeneralLedgerData)
				{
					throw new InvalidAccountingJournalOperationException(Res.GetString("154671e4-bed9-4fed-8adb-1d41f3dadfbe",
						"Journal entries have not been generated yet for the selected transaction."));
				}
			}
		}

		#region AccountingJournalLineProvider

		protected virtual AccountingJournalLineProvider JournalLineProvider
		{
			get
			{
				if (journalLineProvider is null)
				{
					var factory = Factory as ReadOnlyBusinessObjectFactory;
					var providerType = DetermineProviderType();

					journalLineProvider = CreateProvider(providerType, factory);
				}

				return journalLineProvider;
			}
		}

		JournalLineProviderType DetermineProviderType()
		{
			JournalLineProviderType providerType;

			if (reportingBook is not null)
			{
				providerType = JournalLineProviderType.ReportingBook;
			}
			else if (AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value)
			{
				providerType = JournalLineProviderType.GeneralLedgerData;
			}
			else if (transaction is TransactionHeaderWithLines)
			{
				providerType = JournalLineProviderType.TransactionLine;
			}
			else
			{
				providerType = JournalLineProviderType.TransactionHeader;
			}

			return providerType;
		}

		AccountingJournalLineProvider CreateProvider(JournalLineProviderType type, ReadOnlyBusinessObjectFactory factory)
		{
			AccountingJournalLineProvider provider = null;

			switch (type)
			{
				case JournalLineProviderType.ReportingBook:
					provider = new AccountingJournalLineProviderForReportingBook(factory, transaction, ValidLedgerTypes, ValidTransactionTypes, reportingBook, generalLedgerTransactionData);
					break;
				case JournalLineProviderType.GeneralLedgerData:
					provider = new AccountingJournalLineProviderForGeneralLedgerData(factory, transaction, ValidLedgerTypes, ValidTransactionTypes);
					break;
				case JournalLineProviderType.TransactionLine:
					provider = new AccountingJournalLineProviderForTransactionLine(factory, transaction as TransactionHeaderWithLines, ValidLedgerTypes, ValidTransactionTypes);
					break;
				case JournalLineProviderType.TransactionHeader:
					provider = new AccountingJournalLineProviderForTransactionHeader(factory, transaction, ValidLedgerTypes, ValidTransactionTypes);
					break;
			}

			return provider;
		}

		#endregion

		public DataTable GeneralLedgerTransactionData { get => generalLedgerTransactionData; }

		public AccReportingBook ReportingBook { get => reportingBook; }

		protected AccountingJournalLineProvider journalLineProvider;

		protected TransactionHeader transaction;

		#endregion

		public virtual ZGuid SourceIdentifier => transaction?.PK ?? ZGuid.Empty;

		public ZString JournalEntriesNumber => Lines.FirstOrDefault()?.JournalEntriesNumber ?? string.Empty;
	}

	public class AccountingJournalDocumentSupporter : DocumentSupporter
	{
		public AccountingJournalDocumentSupporter(AccountingJournal accountingJournal)
			: base(accountingJournal)
		{
		}

		protected AccountingJournal AccountingJournal
		{
			get { return (AccountingJournal)BusinessObject; }
		}

		#region Overrides

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ARTransaction; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, AccountingJournal);
			}
			else if (dataContext == Constants.DataContext.AccountingJournal)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, AccountingJournal) };
			}
			else
			{
				return null;
			}
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.AccountingJournal, Constants.DataContext.GenericFreightJob };
		}

		#endregion

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}

	public enum JournalLineProviderType
	{
		GeneralLedgerData,
		ReportingBook,
		TransactionLine,
		TransactionHeader
	}
}

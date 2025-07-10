using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccountingJournalLineForGeneralLedgerData : NonPersistentBusinessObject, IAccountingJournalLine
	{
		public AccountingJournalLineForGeneralLedgerData(AccGeneralLedgerData accGeneralLedgerData) : base(accGeneralLedgerData.Factory)
		{
			Argument.NotNull(accGeneralLedgerData, nameof(accGeneralLedgerData));
			this.accGeneralLedgerData = accGeneralLedgerData;
		}

		AccTransactionLines TransactionLine
		{
			get
			{
				if (transactionLine == null)
				{
					transactionLine = Factory.Load<AccTransactionLines>(accGeneralLedgerData.GLD_AL_TransactionLine);
				}

				return transactionLine;
			}
		}
		AccTransactionLines transactionLine;

		AccTransactionHeader TransactionHeader
		{
			get
			{
				if (transactionHeader == null)
				{
					transactionHeader = Factory.Load<AccTransactionHeader>(accGeneralLedgerData.GLD_AH_TransactionHeader);
				}

				return transactionHeader;
			}
		}
		AccTransactionHeader transactionHeader;

		readonly AccGeneralLedgerData accGeneralLedgerData;

		public ZString JournalEntriesNumber => accGeneralLedgerData.GLD_JournalEntriesNumber;

		public ZBool IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn  => AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value;

		public ZGuid AL_AC => TransactionLine?.AL_AC ?? ZGuid.Empty;

		public AccChargeCode ChargeCode => TransactionLine?.ChargeCode;

		public ZString AL_Desc
		{
			get
			{
				var description = string.Empty;

				if (TransactionHeader == null)
				{
					description = GLHeader.AG_DescriptionMultilingual;
				}
				else
				{
					switch (TransactionHeader.AH_Ledger)
					{
						case LedgerTypes.General:
							description = TransactionLine?.AL_Desc ?? string.Empty;
							break;

						case LedgerTypes.AccountsReceivable:
						case LedgerTypes.AccountsPayable:
							if (TransactionHeader.AH_TransactionType == TransactionTypes.Transfer)
							{
								description = TransactionHeader?.AH_Desc ?? string.Empty;
							}
							break;
					}
				}

				return description;
			}
		}

		public ZString AL_RevRecognitionType => TransactionLine?.AL_RevRecognitionType ?? ZString.Empty;

		public ZDecimal AL_ExchangeRate => accGeneralLedgerData.GLD_ExchangeRate;

		public ZString AL_RX_NKTransactionCurrency => accGeneralLedgerData.GLD_Currency;

		public ZGuid AL_AG => accGeneralLedgerData.GLD_AG_GLAccount;

		public ZGuid AL_GC => accGeneralLedgerData.GLD_GC_Company;

		public AccGLHeader GLHeader => accGeneralLedgerData.GLAccount;

		public GlbCompany Company => accGeneralLedgerData.Company;

		public ZGuid AL_GB => accGeneralLedgerData.GLD_GB_Branch;

		public GlbBranch Branch => accGeneralLedgerData.Branch;

		public ZGuid AL_GE => accGeneralLedgerData.GLD_GE_Department;

		public GlbDepartment Department => accGeneralLedgerData.Department;

		public ZString MultiSubAccountTypeCode => accGeneralLedgerData.SubAccount;

		public ZInt AL_PostPeriod => accGeneralLedgerData.GLD_PostPeriod;

		public ZDateTime AL_PostDate => accGeneralLedgerData.GLD_PostDate;

		public ZString CurrencyCode => accGeneralLedgerData.GLD_Currency;

		RefCurrency currency;
		public RefCurrency Currency => currency ??= Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);

		public RefCurrency LocalCurrency => GlbCompany.CurrentCompany.LocalCurrency;

		public ZString TransactionHeaderCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public string TaxBasis => TransactionLine != null ? new AccountingMasterFilesConstants.TransactionLineTaxBasisTypes().GetDescriptionFromCode(TransactionLine.AL_GSTVATBasis) : string.Empty;

		public ZGuid GLD_ATM_TaxGLMovement => accGeneralLedgerData.GLD_ATM_TaxGLMovement;

		public string GLAccountDescription => GLHeader.AG_DescriptionMultilingual;

		#region IDebitCreditAmounts

		ZDecimal IDebitCreditAmounts.OSUnsignedLineAmount
		{
			get => DebitCreditSign == DebitCreditDataEntry.CR ? accGeneralLedgerData.GLD_OSCreditAmount  : DebitCreditSign == DebitCreditDataEntry.DR ? accGeneralLedgerData.GLD_OSDebitAmount : ZDecimal.Zero;
			set
			{
			}
		}

		ZDecimal IDebitCreditAmounts.LocalUnsignedLineAmount
		{
			get => DebitCreditSign == DebitCreditDataEntry.CR ? accGeneralLedgerData.GLD_LocalCreditAmount : DebitCreditSign == DebitCreditDataEntry.DR ? accGeneralLedgerData.GLD_LocalDebitAmount : ZDecimal.Zero;
			set
			{
			}
		}

		public ZString DebitCreditSign
		{
			get => accGeneralLedgerData.GLD_LocalCreditAmount != 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
			set
			{
			}
		}

		#endregion
	}
}

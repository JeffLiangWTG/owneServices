using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLine : NonPersistentBusinessObject, IAccountingJournalLine
	{
		public AccountingJournalLine(AccTransactionLines transactionLine)
			 : base(transactionLine.Factory)
		{
			Argument.NotNull(transactionLine, "transactionLine");
			this.transactionLine = transactionLine;
			taxBasisTypes = new AccountingMasterFilesConstants.TransactionLineTaxBasisTypes();
			OSAmount = new DebitCreditDataEntry(() => AL_OSExTaxAmount, x => { });
			LocalAmount = new DebitCreditDataEntry(() => AL_LineAmount, x => { });
		}

		readonly DebitCreditDataEntry OSAmount;
		readonly DebitCreditDataEntry LocalAmount;
		readonly AccountingMasterFilesConstants.TransactionLineTaxBasisTypes taxBasisTypes;
		readonly AccTransactionLines transactionLine;

		public ZGuid AL_AC
		{
			get { return transactionLine.AL_AC; }
		}

		public AccChargeCode ChargeCode
		{
			get { return transactionLine.ChargeCode; }
		}

		public ZGuid AL_AG
		{
			get { return transactionLine.AL_AG; }
		}

		public AccGLHeader GLHeader
		{
			get { return transactionLine.GLHeader; }
		}

		public ZString AL_Desc
		{
			get { return transactionLine.AL_Desc; }
		}

		public ZString AL_RevRecognitionType
		{
			get { return transactionLine.AL_RevRecognitionType; }
		}

		public ZDecimal AL_ExchangeRate
		{
			get
			{
				return transactionLine.AL_ExchangeRate;
			}
		}

		public ZString AL_RX_NKTransactionCurrency
		{
			get
			{
				return transactionLine.AL_RX_NKTransactionCurrency;
			}
		}

		public void SetCurrency(RefCurrency currency)
		{
			innerCurrency = currency;
		}
		RefCurrency innerCurrency;

		public virtual RefCurrency Currency => innerCurrency ?? transactionLine.TransactionCurrency;

		public void SetLocalCurrency(RefCurrency localCurrency)
		{
			innerLocalCurrency = localCurrency;
		}
		RefCurrency innerLocalCurrency;

		public RefCurrency LocalCurrency => innerLocalCurrency ?? GlbCompany.CurrentCompany.LocalCurrency;

		public ZString TransactionHeaderCurrency
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public ZDecimal AL_LineAmount
		{
			get
			{
				return transactionLine.AL_LineAmount;
			}
		}

		public ZDecimal AL_OSAmount
		{
			get { return transactionLine.AL_OSAmount; }
		}

		public ZDecimal AL_GSTVAT
		{
			get { return transactionLine.AL_GSTVAT; }
		}

		public ZDecimal AL_OSExTaxAmount
		{
			get
			{
				if (AL_GSTVAT.IsEmpty)
				{
					return AL_OSAmount;
				}
				else
				{
					//has to use this boolean condition as "Foreign Currency Balance Journal" can have AL_OSAmount = 0 but AL_LineAmount != 0
					return transactionLine.AL_OSAmount != 0 ? Company.GetExchangeRate().LocalToForeign(AL_LineAmount, AL_ExchangeRate, AL_RX_NKTransactionCurrency) : 0M;
				}
			}
		}

		public ZPropertyInfo AL_OSExTaxAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AL_OSExTaxAmount));
			}
		}

		public ZPropertyInfo AL_LineAmountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AL_LineAmount));
			}
		}

		public ZDecimal AL_OSTaxAmount
		{
			get
			{
				return Company.GetExchangeRate().LocalToForeign(AL_GSTVAT, AL_ExchangeRate, AL_RX_NKTransactionCurrency);
			}
		}

		public ZGuid AL_GC
		{
			get
			{
				return transactionLine.AL_GC;
			}
		}

		public GlbCompany Company
		{
			get
			{
				return transactionLine.Company;
			}
		}

		public ZGuid AL_GB
		{
			get
			{
				return transactionLine.AL_GB;
			}
		}

		public virtual GlbBranch Branch
		{
			get
			{
				return transactionLine.Branch;
			}
		}

		public ZGuid AL_GE
		{
			get
			{
				return transactionLine.AL_GE;
			}
		}

		public virtual GlbDepartment Department
		{
			get
			{
				return transactionLine.Department;
			}
		}

		public string TaxBasis { get { return taxBasisTypes.GetDescriptionFromCode(transactionLine.AL_GSTVATBasis); } }

		public string GLAccountDescription
		{
			get { return transactionLine.GLHeader != null ? transactionLine.GLHeader.AG_DescriptionMultilingual : string.Empty; }
		}

		public ZString MultiSubAccountTypeCode
		{
			get	{ return transactionLine.MultiSubAccountTypeCode; }
		}

		public virtual ZInt AL_PostPeriod
		{
			get
			{
				return (AL_PostDate.IsValid && PeriodCalculator != null) ? PeriodCalculator.GetPeriodFromDate(AL_PostDate) : AL_PostPeriod_Original;
			}
			set
			{
				AL_PostPeriod_Original = value;
			}
		}

		public ZDateTime AL_PostDate
		{
			get
			{
				return transactionLine.AL_PostDate.IsValid ? new ZDateTime(transactionLine.AL_PostDate.Year, transactionLine.AL_PostDate.Month, transactionLine.AL_PostDate.Day) : transactionLine.AL_PostDate;
			}
			set
			{
				transactionLine.AL_PostDate = value;
			}
		}

		public ZShort Sequence
		{
			get { return transactionLine.AL_Sequence; }
		}

		public ZString JournalEntriesNumber => ZString.Empty;

		public ZBool IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn => AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value;

		protected ZString LineType
		{
			get { return transactionLine.AL_LineType; }
		}

		protected bool InvertSigns
		{
			get { return false; }
		}

		protected ZInt AL_PostPeriod_Original
		{
			get { return transactionLine.AL_PostPeriod; }
			set { transactionLine.AL_PostPeriod = value; }
		}

		AccountingPeriodCalculator fPeriodCalculator;
		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		#region IDebitCreditAmounts

		#region IDebitCreditAmounts Members

		ZDecimal IDebitCreditAmounts.OSUnsignedLineAmount
		{
			get { return OSAmount.UnsignedAmount; }
			set
			{
			}
		}

		ZDecimal IDebitCreditAmounts.LocalUnsignedLineAmount
		{
			get { return LocalAmount.UnsignedAmount; }
			set
			{
			}
		}

		#endregion

		#region Debit/Credit

		public ZString DebitCreditSign
		{
			get { return LocalAmount.DebitCreditSign; }
			set
			{
				LocalAmount.DebitCreditSign = value;
				OSAmount.DebitCreditSign = LocalAmount.DebitCreditSign;
			}
		}

		#endregion

		#endregion
	}

	public class AccountingJournalLineWithDifferentPostDateAndPeriod : AccountingJournalLine
	{
		public AccountingJournalLineWithDifferentPostDateAndPeriod(AccTransactionLines transactionLine, ZInt period)
			: base(transactionLine)
		{
			AL_PostPeriod_Original = period;
		}

		public override ZInt AL_PostPeriod
		{
			get
			{
				return AL_PostPeriod_Original;
			}
			set
			{
				AL_PostPeriod_Original = value;
			}
		}
	}
}

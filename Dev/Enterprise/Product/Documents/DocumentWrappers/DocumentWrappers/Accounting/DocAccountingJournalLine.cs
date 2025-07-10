using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingJournalLine : NonPersistentBusinessObject, IObsoleteValidation, IGenericTransactionLinePlugIn
	{
		protected DocAccountingJournalLine(IAccountingJournalLine accountingJournalLine, BusinessObjectFactory factoryToWrap)
			: base(factoryToWrap)
		{
			this.ajLines = new List<IAccountingJournalLine>
			{
				accountingJournalLine
			};
			this.accountingJournalLine = accountingJournalLine;
			SetDebitCreditSign();
		}
		readonly IAccountingJournalLine accountingJournalLine;
		internal readonly List<IAccountingJournalLine> ajLines;

		public static DocAccountingJournalLine New(IAccountingJournalLine transactionLine, BusinessObjectFactory factoryToWrap)
		{
			if (transactionLine != null)
			{
				if (transactionLine is AccountingJournalLineForReportingBook lineForReportingBook)
				{
					return new DocAccountingJournalLineForReportingBook(lineForReportingBook, factoryToWrap);
				}

				return new DocAccountingJournalLine(transactionLine, factoryToWrap);
			}
			return null;
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(accountingJournalLine.ChargeCode, Factory); }
		}

		public virtual ZString Description
		{
			get
			{
				return (GLAccount != null && GLAccount.Description.Equals(accountingJournalLine.AL_Desc)) ? ZString.Empty : accountingJournalLine.AL_Desc;
			}
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(accountingJournalLine.Currency, Factory); }
		}

		public DocCurrency LocalCurrency
		{
			get { return DocCurrency.New(accountingJournalLine.LocalCurrency, Factory); }
		}

		public ZString MultiSubAccountTypeCode
		{
			get { return accountingJournalLine.MultiSubAccountTypeCode; }
		}

		public DocGLAccount GLAccount
		{
			get { return DocGLAccount.New(accountingJournalLine.GLHeader, Factory); }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(accountingJournalLine.Branch, Factory); }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(accountingJournalLine.Department, Factory); }
		}

		public ZString RevRecognitionType
		{
			get { return accountingJournalLine.AL_RevRecognitionType; }
		}

		public ZString TaxBasis
		{
			get { return accountingJournalLine.TaxBasis; }
		}

		public virtual ZDateTime PostDate
		{
			get
			{
				return accountingJournalLine.AL_PostDate;
			}
		}

		public virtual ZString PostPeriod
		{
			get
			{
				return accountingJournalLine.AL_PostPeriod.ToString();
			}
		}

		public ZBool IsMissingPeriodForReportingBook
		{
			get
			{
				return accountingJournalLine is AccountingJournalLineForReportingBook accountingJournalLineForReportingBook ? accountingJournalLineForReportingBook.IsMissingPeriodForReportingBook : false;
			}
		}

		public virtual ZString ForeignCurrencyEquivalent
		{
			get
			{
				return (OSUnsignedAmountDecimal != 0M && TransactionHeaderCurrency != accountingJournalLine.AL_RX_NKTransactionCurrency)
						? ZString.Format("{0} {1}", OSUnsignedAmount, DebitCreditSign)
						: ZString.Empty;
			}
		}

		public ZString TransactionHeaderCurrency
		{
			get
			{
				return accountingJournalLine.TransactionHeaderCurrency;
			}
		}

		public virtual ZString AlternateGLAccount
		{
			get
			{
				return ZString.Empty;
			}
		}

		public virtual ZString ParentAccountNum
		{
			get
			{
				return ZString.Empty;
			}
		}

		#region Journal Number using GLD

		public ZString JournalEntriesNumber => accountingJournalLine.JournalEntriesNumber;

		public ZBool IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn => accountingJournalLine.IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn;

		#endregion

		#region Amounts

		protected IDebitCreditAmounts DebitCreditAmounts
		{
			get { return accountingJournalLine; }
		}

		public ZString DebitCreditSign
		{
			get { return debitCreditSign; }
		}
		ZString debitCreditSign;

		public virtual ZString DebitAmount
		{
			get
			{
				return (DebitCreditSign == DebitCreditDataEntry.DR) ? LocalUnsignedAmount : ZString.Empty;
			}
		}

		internal virtual ZDecimal DebitAmountDecimal
		{
			get
			{
				return DebitAmount.IsEmpty ? ZDecimal.Zero : LocalUnsignedAmountDecimal;
			}
		}

		public virtual ZString CreditAmount
		{
			get
			{
				return (DebitCreditSign == DebitCreditDataEntry.CR) ? LocalUnsignedAmount : ZString.Empty;
			}
		}

		internal virtual ZDecimal CreditAmountDecimal
		{
			get
			{
				return CreditAmount.IsEmpty ? ZDecimal.Zero : LocalUnsignedAmountDecimal;
			}
		}

		public ZString OSUnsignedAmount
		{
			get { return OSUnsignedAmountDecimal > 0 ? FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSUnsignedAmountDecimal, Currency) : ""; }
		}

		internal virtual ZDecimal OSUnsignedAmountDecimal
		{
			get
			{
				return ajLines.All(x => x is IDebitCreditAmounts) ? new ZDecimal(ajLines.OfType<IDebitCreditAmounts>().Sum(x => x.OSUnsignedLineAmount * GetMultiplier(x))) : ZDecimal.Zero;
			}
		}

		public ZString LocalUnsignedAmount
		{
			get { return LocalUnsignedAmountDecimal > 0 ? FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(LocalUnsignedAmountDecimal, LocalCurrency) : ""; }
		}

		protected ZDecimal LocalUnsignedAmountDecimal
		{
			get { return ajLines.All(x => x is IDebitCreditAmounts) ? new ZDecimal(ajLines.OfType<IDebitCreditAmounts>().Sum(x => x.LocalUnsignedLineAmount * GetMultiplier(x))) : ZDecimal.Zero; }
		}

		int GetMultiplier(IDebitCreditAmounts line)
		{
			return (line.DebitCreditSign == DebitCreditSign) ? 1 : -1;
		}

		#endregion

		public GenericTransactionLineSupporter LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocAccountingJournalLineSupporter(this)); }
		}
		GenericTransactionLineSupporter fGenericTransactionSupporter;

		public bool AddAccountingJournalLine(IAccountingJournalLine line)
		{
			if (MatchPK(line.ChargeCode, accountingJournalLine.ChargeCode)
				&& MatchParentAccount(line)
				&& line.AL_Desc == accountingJournalLine.AL_Desc
				&& MatchPK(line.Currency, accountingJournalLine.Currency)
				&& line.MultiSubAccountTypeCode == accountingJournalLine.MultiSubAccountTypeCode
				&& MatchPK(line.Branch, accountingJournalLine.Branch)
				&& MatchPK(line.Department, accountingJournalLine.Department)
				&& line.AL_RevRecognitionType == accountingJournalLine.AL_RevRecognitionType
				&& line.TaxBasis == accountingJournalLine.TaxBasis
				&& line.AL_PostDate == accountingJournalLine.AL_PostDate
				&& line.AL_PostPeriod == accountingJournalLine.AL_PostPeriod
				&& MatchAttributes(line))
			{
				ajLines.Add(line);
				SetDebitCreditSign();
				return true;
			}
			else
			{
				return false;
			}
		}

		internal virtual bool MatchAttributes(IAccountingJournalLine line)
		{
			return true;
		}

		internal virtual bool MatchParentAccount(IAccountingJournalLine line)
		{
			return line.AL_AG == accountingJournalLine.AL_AG;
		}

		bool MatchPK(BusinessObject bizO1, BusinessObject bizO2)
		{
			return bizO1?.PK == bizO2?.PK;
		}

		void SetDebitCreditSign()
		{
			if (ajLines.All(x => x is IDebitCreditAmounts))
			{
				var totalDrAmnt = ajLines.OfType<IDebitCreditAmounts>().Where(x => x.DebitCreditSign == DebitCreditDataEntry.DR).Sum(x => x.LocalUnsignedLineAmount);
				var totalCrAmnt = ajLines.OfType<IDebitCreditAmounts>().Where(x => x.DebitCreditSign == DebitCreditDataEntry.CR).Sum(x => x.LocalUnsignedLineAmount);
				debitCreditSign = totalDrAmnt > totalCrAmnt ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
			}
			else
			{
				debitCreditSign = ZString.Empty;
			}
		}

		class DocAccountingJournalLineSupporter : GenericTransactionLineSupporter
		{
			public DocAccountingJournalLineSupporter(DocAccountingJournalLine parent)
			{
				this.Parent = parent;
			}
			protected readonly DocAccountingJournalLine Parent;

			protected internal override DocChargeCode GetChargeCode()
			{
				return Parent.ChargeCode;
			}

			protected internal override DocGLAccount GetGLAccount()
			{
				return Parent.GLAccount;
			}

			protected internal override ZString GetDescriptionOne()
			{
				return Parent.accountingJournalLine.AL_Desc;
			}

			protected internal override ZString GetDescription()
			{
				return Parent.Description;
			}

			protected internal override DocCurrency GetCurrency()
			{
				return Parent.Currency;
			}

			protected internal override ZString GetDebitAmount()
			{
				return Parent.DebitAmount;
			}

			protected internal override ZString GetCreditAmount()
			{
				return Parent.CreditAmount;
			}

			protected internal override ZString GetForeignCurrencyEquivalentAmount()
			{
				return Parent.ForeignCurrencyEquivalent;
			}

			protected internal override ZString GetMultiSubAccountTypeCode()
			{
				return Parent.MultiSubAccountTypeCode;
			}

			protected internal override ZString GetRevenueRecognitionType()
			{
				return Parent.RevRecognitionType;
			}

			protected internal override ZString GetTaxBasis()
			{
				return Parent.TaxBasis;
			}

			protected internal override ZDateTime GetPostDate()
			{
				return Parent.PostDate;
			}

			protected internal override ZString GetPostPeriod()
			{
				return Parent.PostPeriod;
			}

			protected internal override ZBool GetIsMissingPeriodForReportingBook()
			{
				return Parent.IsMissingPeriodForReportingBook;
			}

			protected internal override DocBranch GetBranch()
			{
				return Parent.Branch;
			}

			protected internal override DocDepartment GetDepartment()
			{
				return Parent.Department;
			}

			protected internal override ZDecimal GetDebitAmountDecimal()
			{
				return Parent.DebitAmountDecimal;
			}

			protected internal override ZDecimal GetCreditAmountDecimal()
			{
				return Parent.CreditAmountDecimal;
			}

			protected internal override ZString GetOSUnsignedAmount()
			{
				return Parent.OSUnsignedAmount;
			}

			protected internal override ZString GetAlternateGLAccount()
			{
				return Parent.AlternateGLAccount;
			}

			protected internal override ZString GetParentAccountNum()
			{
				return Parent.ParentAccountNum;
			}

			protected internal override ZString GetJournalEntriesNumber()
			{
				return Parent.JournalEntriesNumber;
			}

			protected internal override ZBool GetGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOptions()
			{
				return Parent.IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn;
			}
		}
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingJournalLineForReportingBook : DocAccountingJournalLine
	{
		public DocAccountingJournalLineForReportingBook(AccountingJournalLine accountingJournalLine, BusinessObjectFactory factoryToWrap)
			: base(accountingJournalLine, factoryToWrap)
		{
			this.accountingJournalLine = (AccountingJournalLineForReportingBook)accountingJournalLine;
		}
		readonly AccountingJournalLineForReportingBook accountingJournalLine;

		public override ZString Description
		{
			get
			{
				return accountingJournalLine.AlternateGLAccountDesc;
			}
		}

		public override ZDateTime PostDate
		{
			get
			{
				return accountingJournalLine.GlPostDate;
			}
		}

		public override ZString PostPeriod
		{
			get
			{
				return accountingJournalLine.ReportingBookPeriod;
			}
		}

		public override ZString ForeignCurrencyEquivalent
		{
			get
			{
				var foreignCurrencyEquivalent = ZString.Empty;
				if (TransactionHeaderCurrency != accountingJournalLine.CurrencyCode)
				{
					foreignCurrencyEquivalent = accountingJournalLine.GLAmountOSDebit != 0 ? FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(accountingJournalLine.GLAmountOSDebit, Currency) + " DR"
						: FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(accountingJournalLine.GLAmountOSCredit, Currency) + " CR";
				}
				return foreignCurrencyEquivalent;
			}
		}

		public override ZString AlternateGLAccount
		{
			get
			{
				return accountingJournalLine.AlternateGLAccount;
			}
		}

		public override ZString ParentAccountNum
		{
			get
			{
				return accountingJournalLine.ParentAccountNum;
			}
		}

		#region Amounts

		public override ZString DebitAmount
		{
			get
			{
				return DebitAmountDecimal == ZDecimal.Zero ? ZString.Empty : FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(DebitAmountDecimal, LocalCurrency);
			}
		}

		internal override ZDecimal DebitAmountDecimal
		{
			get
			{
				ZDecimal debitAmount;
				if (ajLines.Count > 1)
				{
					debitAmount = ajLines.Cast<AccountingJournalLineForReportingBook>().Sum(x => x.LocalBalance);
					if (debitAmount < ZDecimal.Zero)
					{
						debitAmount = ZDecimal.Zero;
					}
				}
				else
				{
					debitAmount = accountingJournalLine.LocalDebitAmount;
				}
				return debitAmount;
			}
		}

		public override ZString CreditAmount
		{
			get
			{
				return CreditAmountDecimal == ZDecimal.Zero ? ZString.Empty : FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(CreditAmountDecimal, LocalCurrency);
			}
		}

		internal override ZDecimal CreditAmountDecimal
		{
			get
			{
				ZDecimal creditAmount;
				if (ajLines.Count > 1)
				{
					creditAmount = ajLines.Cast<AccountingJournalLineForReportingBook>().Sum(x => x.LocalBalance) * (-1);
					if (creditAmount < ZDecimal.Zero)
					{
						creditAmount = ZDecimal.Zero;
					}
				}
				else
				{
					creditAmount = accountingJournalLine.LocalCreditAmount;
				}
				return creditAmount;
			}
		}

		internal override ZDecimal OSUnsignedAmountDecimal
		{
			get
			{
				return accountingJournalLine.GLAmountOSDebit != ZDecimal.Zero ? accountingJournalLine.GLAmountOSDebit : accountingJournalLine.GLAmountOSCredit;
			}
		}

		#endregion

		internal override bool MatchAttributes(IAccountingJournalLine line)
		{
			if (line is AccountingJournalLineForReportingBook waitedLine)
			{
				var needMatchAttributes = AccountingMasterFilesRegistry.Instance
					.ReportingBookAccountingJournalPrintOption
					.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ReportingBookAccountingJournalPrintOption>()
					.FirstOrDefault(x => x.ReportingBook == accountingJournalLine.ReportingBookPK)?.DisplayAttribute ?? false;
				return !needMatchAttributes || accountingJournalLine.AlternateGLAccountDesc == waitedLine.AlternateGLAccountDesc;
			}
			return true;
		}

		internal override bool MatchParentAccount(IAccountingJournalLine line)
		{
			if (line is AccountingJournalLineForReportingBook waitedLine)
			{
				var needMatchParentAccounts = AccountingMasterFilesRegistry.Instance
					.ReportingBookAccountingJournalPrintOption
					.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ReportingBookAccountingJournalPrintOption>()
					.FirstOrDefault(x => x.ReportingBook == accountingJournalLine.ReportingBookPK)?.DisplayParentAccount ?? false;

				return (!accountingJournalLine.AlternateGLAccount.IsEmpty && accountingJournalLine.AlternateGLAccount == waitedLine.AlternateGLAccount
						|| accountingJournalLine.AlternateGLAccount.IsEmpty && accountingJournalLine.AlternateGLAccount == waitedLine.AlternateGLAccount && waitedLine.ParentAccountNum == accountingJournalLine.ParentAccountNum)
						&& (!needMatchParentAccounts || waitedLine.ParentAccountNum == accountingJournalLine.ParentAccountNum);
			}
			return true;
		}
	}
}

using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class GLAccountBalancesAndMovements : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T205";
		public ZString GLAccountNumber { get; set; }
		public ZString OpenBalanceDRCR { get; set; }
		public ZString EndBalanceDRCR { get; set; }
		public ZString CurrencyCode { get; set; }
		public ZString Unit { get; set; }
		public ZInt FinancialYear { get; set; }
		public ZInt Period { get; set; }
		public ZDecimal OpenQuantity { get; set; }
		public ZDecimal OpenBalanceCurrency { get; set; }
		public ZDecimal OpenBalanceLocalCurrency { get; set; }
		public ZDecimal DebitQuantity { get; set; }
		public ZDecimal DebitCurrencyAmount { get; set; }
		public ZDecimal DebitAmountLocalCurrency { get; set; }
		public ZDecimal CreditQuantity { get; set; }
		public ZDecimal CreditCurrencyAmount { get; set; }
		public ZDecimal CreditAmountLocalCurrency { get; set; }
		public ZDecimal EndQuantity { get; set; }
		public ZDecimal EndBalanceCurrency { get; set; }
		public ZDecimal EndBalanceLocalCurrency { get; set; }
		public ZString GLAccountAssistedNumber1 { get; set; }
		public ZString GLAccountAssistedNumber2 { get; set; }
		public ZString GLAccountAssistedNumber3 { get; set; }
		public ZString GLAccountAssistedNumber4 { get; set; }
		public ZString GLAccountAssistedNumber5 { get; set; }
		public ZString GLAccountAssistedNumber6 { get; set; }
		public ZString GLAccountAssistedNumber7 { get; set; }
		public ZString GLAccountAssistedNumber8 { get; set; }
		public ZString GLAccountAssistedNumber9 { get; set; }
		public ZString GLAccountAssistedNumber10 { get; set; }
		public ZString GLAccountAssistedNumber11 { get; set; }
		public ZString GLAccountAssistedNumber12 { get; set; }
		public ZString GLAccountAssistedNumber13 { get; set; }
		public ZString GLAccountAssistedNumber14 { get; set; }
		public ZString GLAccountAssistedNumber15 { get; set; }
		public ZString GLAccountAssistedNumber16 { get; set; }
		public ZString GLAccountAssistedNumber17 { get; set; }
		public ZString GLAccountAssistedNumber18 { get; set; }
		public ZString GLAccountAssistedNumber19 { get; set; }
		public ZString GLAccountAssistedNumber20 { get; set; }
		public ZString GLAccountAssistedNumber21 { get; set; }
		public ZString GLAccountAssistedNumber22 { get; set; }
		public ZString GLAccountAssistedNumber23 { get; set; }
		public ZString GLAccountAssistedNumber24 { get; set; }
		public ZString GLAccountAssistedNumber25 { get; set; }
		public ZString GLAccountAssistedNumber26 { get; set; }
		public ZString GLAccountAssistedNumber27 { get; set; }
		public ZString GLAccountAssistedNumber28 { get; set; }
		public ZString GLAccountAssistedNumber29 { get; set; }
		public ZString GLAccountAssistedNumber30 { get; set; }
	}

	public class GLAccountBalancesAndMovementsCollection : NonPersistentBusinessObjectCollection<GLAccountBalancesAndMovements>	{
		public GLAccountBalancesAndMovementsCollection(BusinessObjectFactory factory, ZInt period)
			: this(factory, period, ZGuid.Empty)
		{ }

		public GLAccountBalancesAndMovementsCollection(BusinessObjectFactory factory, ZInt period, ZGuid branchPK)
			: base(factory)
		{
			AddElements(period, branchPK);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GLAccountBalancesAndMovements();
		}

		void AddElements(ZInt period, ZGuid branchPK)
		{
			AccPeriodManagement accPeriodManagement = (new AccountingPeriodCalculator(Factory)).GetPeriodManagementFromDate((new AccountingPeriodCalculator(Factory)).GetFirstDayForPeriod(period));
			if (accPeriodManagement != null)
			{
				ZInt year = accPeriodManagement.AM_Year;
				DataTable results = RunScript(period, branchPK);
				foreach (DataRow row in results.Rows)
				{
					GLAccountBalancesAndMovements glAccountBalancesAndMovements = AddNew();
					glAccountBalancesAndMovements.GLAccountNumber = row["GLAccount"].ToString();
					glAccountBalancesAndMovements.FinancialYear = year;
					glAccountBalancesAndMovements.Period = period;
					glAccountBalancesAndMovements.CurrencyCode = row["Currency"].ToString();
					glAccountBalancesAndMovements.Unit = "";
					glAccountBalancesAndMovements.GLAccountAssistedNumber1 = row["Account"].ToString();

					glAccountBalancesAndMovements.OpenQuantity = 0;
					ZDecimal openingBalance = row["OpeningBalance"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OpeningBalance"]);
					ZDecimal osOpeningBalance = row["OsOpeningBalance"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OsOpeningBalance"]);
					glAccountBalancesAndMovements.OpenBalanceLocalCurrency = Math.Abs(openingBalance) ;
					glAccountBalancesAndMovements.OpenBalanceCurrency = Math.Abs(osOpeningBalance);
					if (openingBalance == 0m && osOpeningBalance == 0)
					{
						glAccountBalancesAndMovements.OpenBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese(row["AJ_DebitCredit"].ToString());
					}
					else if (openingBalance > 0m )
					{
						glAccountBalancesAndMovements.OpenBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese("DR");
					}
					else
					{
						glAccountBalancesAndMovements.OpenBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese("CR");
					}

					if (glAccountBalancesAndMovements.OpenBalanceCurrency == 0)
					{
						glAccountBalancesAndMovements.OpenBalanceCurrency = glAccountBalancesAndMovements.OpenBalanceLocalCurrency;
					}

					ZDecimal amount = row["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Amount"]);
					ZDecimal osAmount = row["OsAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OsAmount"]);
					glAccountBalancesAndMovements.DebitAmountLocalCurrency = amount > 0 ? amount : 0;
					glAccountBalancesAndMovements.CreditAmountLocalCurrency = amount > 0 ? 0 : Math.Abs(amount);
					glAccountBalancesAndMovements.DebitQuantity = 0;
					glAccountBalancesAndMovements.CreditQuantity = 0;
					glAccountBalancesAndMovements.DebitCurrencyAmount = osAmount > 0 ? osAmount : 0;
					glAccountBalancesAndMovements.CreditCurrencyAmount = osAmount > 0 ? 0 : Math.Abs(osAmount);
					if (glAccountBalancesAndMovements.DebitCurrencyAmount == 0)
					{
						glAccountBalancesAndMovements.DebitCurrencyAmount = glAccountBalancesAndMovements.DebitAmountLocalCurrency;
					}

					if (glAccountBalancesAndMovements.CreditCurrencyAmount == 0)
					{
						glAccountBalancesAndMovements.CreditCurrencyAmount = glAccountBalancesAndMovements.CreditAmountLocalCurrency;
					}

					glAccountBalancesAndMovements.EndQuantity = 0;
					glAccountBalancesAndMovements.EndBalanceLocalCurrency = Math.Abs(openingBalance + amount);
					glAccountBalancesAndMovements.EndBalanceCurrency = Math.Abs(osOpeningBalance + osAmount);

					if (glAccountBalancesAndMovements.EndBalanceCurrency == 0)
					{
						glAccountBalancesAndMovements.EndBalanceCurrency = glAccountBalancesAndMovements.EndBalanceLocalCurrency;
					}

					if (openingBalance + amount == 0 && osOpeningBalance + osAmount == 0)
					{
						glAccountBalancesAndMovements.EndBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese(row["AJ_DebitCredit"].ToString());
					}
					else if (openingBalance + amount > 0)
					{
						glAccountBalancesAndMovements.EndBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese("DR");
					}
					else
					{
						glAccountBalancesAndMovements.EndBalanceDRCR = ChineseUtils.ConvertDebitCreditToChinese("CR");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		DataTable RunScript(ZInt period, ZGuid branchPK)
		{
			if (branchPK == ZGuid.Invalid || branchPK == ZGuid.Empty) {
			return DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(CultureInfo.InvariantCulture, @"EXEC Report_ChinaGLAccountBalance '{0}', {1}, '{2}', '{3}'",
									GlbCompany.CurrentCompany.PK, period,  "", "Y"));
			}
			return DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(CultureInfo.InvariantCulture, @"EXEC Report_ChinaGLAccountBalance '{0}', {1}, '{2}', '{3}'",
									GlbCompany.CurrentCompany.PK, period, branchPK, "Y"));
		}
	}
}


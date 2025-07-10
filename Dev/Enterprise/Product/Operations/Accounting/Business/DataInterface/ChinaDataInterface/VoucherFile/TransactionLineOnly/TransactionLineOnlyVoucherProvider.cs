using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class TransactionLineOnlyVoucherProvider : VoucherProvider
	{
		public TransactionLineOnlyVoucherProvider(WIPAccrualDataSourceCollection wIPCollection, IControlAccountProvider controlAccountProvider)
			: base(wIPCollection.Factory, controlAccountProvider)
		{
			this.WIPAccrualCollection = wIPCollection;
		}

		public TransactionLineOnlyVoucherProvider(WIPAccrualDataSourceCollection wIPCollection, IControlAccountProvider controlAccountProvider, ZInt period)
			: base(wIPCollection.Factory, controlAccountProvider)
		{
			this.WIPAccrualCollection = wIPCollection;
			this.Period = period;
		}

		public override ZString CompanyName
		{
			get
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				return GetCompanyNameFromOrgProxy(null, orgProxy);
			}
		}

		public override ZString CurrencyCode
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public override ZDateTime PostDate
		{
			get
			{
				return PeriodCalc.GetLastDayForPeriod(Period);
			}
		}

		public override VoucherLine[] VoucherLinesWithControllerPerLine
		{
			get
			{
				if (fVoucherLines == null)
				{
					int totalNumberOfVoucherLines = (MaxVoucherLineNo - 1) * 2;

					if (totalNumberOfVoucherLines > 0)
					{
						fVoucherLines = new VoucherLine[totalNumberOfVoucherLines];
						int currentLineIndex = 0;
						for (int i = 0; i < MaxVoucherLineNo - 1; i++)
						{
							VoucherDataSource dataSource = WIPAccrualCollection.GetVoucherData(i);
							fVoucherLines[currentLineIndex] = GetVoucherLine(dataSource);
							TotalAmount = dataSource.Amount;
							fVoucherLines[currentLineIndex + 1] = GetControlAccountEntry(dataSource);
							currentLineIndex += 2;
						}
					}
					else
					{
						fVoucherLines = new VoucherLine[MaxVoucherLineNo];
					}
				}

				return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
		}

		public override VoucherLine[] VoucherLines
		{
			get
			{
				if (fVoucherLines == null)
				{
					fVoucherLines = new VoucherLine[MaxVoucherLineNo];
					for (int i = 0; i < MaxVoucherLineNo - 1; i++)
					{
						VoucherDataSource dataSource = WIPAccrualCollection.GetVoucherData(i);
						fVoucherLines[i] = GetVoucherLine(dataSource);
						TotalAmount += dataSource.Amount;
					}
					if (MaxVoucherLineNo > 0)
					{
						fVoucherLines[MaxVoucherLineNo - 1] = GetControlAccountEntry();
					}
				}

				return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
#if DEBUG
			set
			{
				fVoucherLines = value;
			}
#endif
		}

		protected abstract ZGuid GetLocalAccountPK(VoucherDataSource wIPAccrual);
		protected abstract ZString GetLocalAccountDescription(VoucherDataSource wIPAccrual);
		protected abstract ZDecimal GetCreditForControl();
		protected abstract ZDecimal GetDebitForControl();

		protected abstract ZDecimal GetCreditForVoucher(VoucherDataSource wIPAccrual);
		protected abstract ZDecimal GetOSCreditForVoucher(VoucherDataSource wIPAccrual);
		protected abstract ZDecimal GetOSCreditForController(VoucherDataSource wIPAccrual);

		protected abstract ZDecimal GetDebitForVoucher(VoucherDataSource wIPAccrual);
		protected abstract ZDecimal GetOSDebitForVoucher(VoucherDataSource wIPAccrual);
		protected abstract ZDecimal GetOSDebitForController(VoucherDataSource wIPAccrual);

		protected abstract ZString GetVoucherNumberForControl();
		protected abstract ZString GetVoucherType(VoucherDataSource wIPAccrual);
		protected abstract ZString GetVoucherTypeForControl();

		protected ZDecimal TotalAmount;
		protected readonly WIPAccrualDataSourceCollection WIPAccrualCollection;

		protected ZString GetBranchCode(VoucherDataSource wIPAccrual)
		{
			return wIPAccrual.BranchCode;
		}

		protected ZString GetDepartmentCode(VoucherDataSource wIPAccrual)
		{
			return wIPAccrual.DepartmentCode;
		}

		protected VoucherLine GetControlAccountEntry()
		{
			VoucherLine lineToReturn = new VoucherLine(Factory);
			lineToReturn.AccountPK = ControlAccountProvider.PK;
			lineToReturn.AccountDescription = GetLocalAccountNo(ControlAccountProvider.PK);
			lineToReturn.VoucherNumber = GetVoucherNumberForControl();
			lineToReturn.VoucherType = GetVoucherTypeForControl();
			lineToReturn.DebitAmount = GetDebitForControl();
			lineToReturn.CreditAmount = GetCreditForControl();
			lineToReturn.VoucherDate = GetVoucherDate();
			lineToReturn.CurrencyCode = GetCurrencyCode();
			lineToReturn.ForeignCurrencyAmount = lineToReturn.DebitAmount + lineToReturn.CreditAmount;
			lineToReturn.ExchangeRate = GetExchangeRate();
			lineToReturn.Description = GetVoucherDescription();
			if (AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.Value)
			{
				lineToReturn.fVoucherNumberPrefix = GetBranchCode(WIPAccrualCollection.GetVoucherData(0));
			}
			return lineToReturn;
		}

		protected VoucherLine GetControlAccountEntry(VoucherDataSource wIPAccrual)
		{
			VoucherLine lineToReturn = GetControlAccountEntry();
			lineToReturn.OSDebitAmount = GetOSDebitForController(wIPAccrual);
			lineToReturn.OSCreditAmount = GetOSCreditForController(wIPAccrual);
			lineToReturn.BranchCode = GetBranchCode(wIPAccrual);
			lineToReturn.DepartmentCode = GetDepartmentCode(wIPAccrual);
			return lineToReturn;
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			throw new NotImplementedException();
		}

		protected override ZDateTime GetVoucherDate()
		{
			return PeriodCalc.GetLastDayForPeriod(Period);
		}

		protected virtual ZDateTime GetVoucherDateForControl()
		{
			return PeriodCalc.GetLastDayForPeriod(Period);
		}

		protected virtual VoucherLine GetVoucherLine(VoucherDataSource wIPAccrual)
		{
			VoucherLine lineToReturn = new VoucherLine(Factory);
			lineToReturn.AccountPK = GetLocalAccountPK(wIPAccrual);
			lineToReturn.AccountDescription = GetLocalAccountDescription(wIPAccrual);
			lineToReturn.VoucherNumber = GetVoucherNumber(wIPAccrual);
			lineToReturn.VoucherType = GetVoucherType(wIPAccrual);
			lineToReturn.DebitAmount = GetDebitForVoucher(wIPAccrual);
			lineToReturn.CreditAmount = GetCreditForVoucher(wIPAccrual);
			lineToReturn.OSDebitAmount = GetOSDebitForVoucher(wIPAccrual);
			lineToReturn.OSCreditAmount = GetOSCreditForVoucher(wIPAccrual);
			lineToReturn.VoucherDate = GetVoucherDate();
			lineToReturn.Description = GetVoucherDescription();
			lineToReturn.CurrencyCode = GetCurrencyCode();
			lineToReturn.ForeignCurrencyAmount = lineToReturn.DebitAmount + lineToReturn.CreditAmount;
			lineToReturn.ExchangeRate = GetExchangeRate();
			lineToReturn.BranchCode = GetBranchCode(wIPAccrual);
			lineToReturn.DepartmentCode = GetDepartmentCode(wIPAccrual);
			if (AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.Value)
			{
				lineToReturn.fVoucherNumberPrefix = GetBranchCode(wIPAccrual);
			}
			return lineToReturn;
		}

		protected virtual ZString GetVoucherNumber(VoucherDataSource wIPAccrual)
		{
			return wIPAccrual.VoucherNumber;
		}

		protected override int MaxVoucherLineNo
		{
			get
			{
				if (fMaxVoucherLineNo == 0)
				{
					if (WIPAccrualCollection != null)
					{
						int collectionCount = WIPAccrualCollection.GetCount();
						if (collectionCount > 0)
						{
							fMaxVoucherLineNo = collectionCount + 1;
						}
						else
						{
							fMaxVoucherLineNo = 0;
						}
					}
				}
				return fMaxVoucherLineNo;
			}
		}

		protected int fMaxVoucherLineNo;

		protected AccountingPeriodCalculator PeriodCalc
		{
			get
			{
				if (fPeriodCalc == null)
				{
					fPeriodCalc = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalc;
			}
		}

		AccountingPeriodCalculator fPeriodCalc;
	}
}

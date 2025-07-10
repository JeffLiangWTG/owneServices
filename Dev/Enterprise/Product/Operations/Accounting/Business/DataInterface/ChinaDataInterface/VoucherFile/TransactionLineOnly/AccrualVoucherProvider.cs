using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class AccrualVoucherProvider : TransactionLineOnlyVoucherProvider
	{
		public AccrualVoucherProvider(WIPAccrualDataSourceCollection wIPCollection)
			: base(wIPCollection, new WIPAccrualControlAccountProvider(TransactionLineTypes.Accrual))
		{
		}

		public AccrualVoucherProvider(WIPAccrualDataSourceCollection wIPCollection, ZInt period)
			: base(wIPCollection, new WIPAccrualControlAccountProvider(TransactionLineTypes.Accrual), period)
		{
		}

		public static AccrualVoucherProvider New(BusinessObjectFactory factory, int period)
		{
			WIPAccrualDataSourceCollection accrualCollection = new WIPAccrualDataSourceCollection(factory);
			accrualCollection.LoadCollection(period, TransactionLineTypes.Accrual);
			return new AccrualVoucherProvider(accrualCollection, period);
		}

		public static AccrualVoucherProvider New(BusinessObjectFactory factory, ZDateTime startDate, ZDateTime endDate)
		{
			WIPAccrualDataSourceCollection accrualCollection = new WIPAccrualDataSourceCollection(factory);

			accrualCollection.LoadCollection(startDate, endDate, TransactionLineTypes.Accrual);

			return new AccrualVoucherProvider(accrualCollection, (new AccountingPeriodCalculator(factory)).GetPeriodFromDate(startDate));
		}

		public override ZString CompanyCode
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_Code;
			}
		}

		public override ZString BranchCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		public override ZString DepartmentCode
		{
			get
			{
				return ZString.Empty;
			}
		}

		public override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.JobCosting;
			}
		}

		protected override ZDecimal GetDebitForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.Amount);
		}

		protected override ZDecimal GetOSDebitForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.OSAmount);
		}

		protected override ZDecimal GetCreditForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.Amount);
		}

		protected override ZDecimal GetOSCreditForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.OSAmount);
		}

		protected override ZGuid GetLocalAccountPK(VoucherDataSource wIPAccrual)
		{
			return wIPAccrual.GLHeader;
		}

		protected override ZString GetLocalAccountDescription(VoucherDataSource wIPAccrual)
		{
			ZString accountNumber = "";
			accountNumber = GetLocalAccountDescription(wIPAccrual.GLHeader);
			return accountNumber;
		}

		protected override ZString GetVoucherType(VoucherDataSource accrual)
		{
			return TransactionLineTypes.Accrual;
		}

		protected override ZDecimal GetExchangeRate()
		{
			return 1M;
		}

		protected override ZDecimal GetCreditForControl()
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(TotalAmount);
		}

		protected override ZDecimal GetOSCreditForController(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.OSAmount);
		}

		protected override ZDecimal GetDebitForControl()
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(TotalAmount);
		}

		protected override ZDecimal GetOSDebitForController(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.OSAmount);
		}

		protected override ZString GetVoucherTypeForControl()
		{
			return TransactionLineTypes.Accrual;
		}

		protected override ZString GetVoucherDescription()
		{
			if (DataInterfaceUtils.GetLocalLanguage() == Core.Constants.Languages.ChineseSimplified)
			{
				return (NoResString)"预提成本";
			}
			else
			{
				return Res.GetString("d27be0af-299a-40a6-aa03-64de6b98a695", "Estimated Cost Item");
			}
		}

		protected override ZString GetVoucherNumberForControl()
		{
			return DataInterfaceUtils.GetVoucherNumberFowWIPAccrual(TransactionLineTypes.Accrual, Period);
		}
	}
}

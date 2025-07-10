using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class WIPVoucherProvider : TransactionLineOnlyVoucherProvider
	{
		public WIPVoucherProvider(WIPAccrualDataSourceCollection wIPCollection)
			: base(wIPCollection, new WIPAccrualControlAccountProvider(TransactionLineTypes.WIP))
		{
		}

		public WIPVoucherProvider(WIPAccrualDataSourceCollection wIPCollection, ZInt period)
			: base(wIPCollection, new WIPAccrualControlAccountProvider(TransactionLineTypes.WIP), period)
		{
		}

		public static WIPVoucherProvider New(BusinessObjectFactory factory, int period)
		{
			WIPAccrualDataSourceCollection wIPCollection = new WIPAccrualDataSourceCollection(factory);
			wIPCollection.LoadCollection(period, TransactionLineTypes.WIP);
			return new WIPVoucherProvider(wIPCollection, period);
		}

		public static WIPVoucherProvider New(BusinessObjectFactory factory, ZDateTime startDate, ZDateTime endDate)
		{
			WIPAccrualDataSourceCollection wIPCollection = new WIPAccrualDataSourceCollection(factory);

			wIPCollection.LoadCollection(startDate, endDate, TransactionLineTypes.WIP);

			return new WIPVoucherProvider(wIPCollection, (new AccountingPeriodCalculator(factory)).GetPeriodFromDate(startDate));
		}

		public override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.JobCosting;
			}
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

		protected override ZDecimal GetExchangeRate()
		{
			return 1M;
		}

		protected override ZDecimal GetCreditForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.Amount);
		}

		protected override ZDecimal GetOSCreditForVoucher(VoucherDataSource wIPAccrual)
		{
			return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? GetCreditForVoucher(wIPAccrual) : VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.OSAmount);
		}

		protected override ZDecimal GetCreditForControl()
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(TotalAmount);
		}

		protected override ZDecimal GetOSCreditForController(VoucherDataSource wIPAccrual)
		{
			return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? GetCreditForControl() : VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.OSAmount);
		}

		protected override ZDecimal GetDebitForControl()
		{
			return VoucherDebitCreditLookUp.ReturnNegativeOnly(TotalAmount);
		}

		protected override ZDecimal GetOSDebitForController(VoucherDataSource wIPAccrual)
		{
			return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? GetDebitForControl() : VoucherDebitCreditLookUp.ReturnNegativeOnly(wIPAccrual.OSAmount);
		}

		protected override ZDecimal GetDebitForVoucher(VoucherDataSource wIPAccrual)
		{
			return VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.Amount);
		}

		protected override ZDecimal GetOSDebitForVoucher(VoucherDataSource wIPAccrual)
		{
			return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? GetDebitForVoucher(wIPAccrual) : VoucherDebitCreditLookUp.ReturnPositiveOnly(wIPAccrual.OSAmount);
		}

		protected override ZString GetVoucherNumberForControl()
		{
			return DataInterfaceUtils.GetVoucherNumberFowWIPAccrual(TransactionLineTypes.WIP, Period);
		}

		protected override ZString GetVoucherType(VoucherDataSource wIP)
		{
			return TransactionLineTypes.WIP;
		}

		protected override ZString GetVoucherTypeForControl()
		{
			return TransactionLineTypes.WIP;
		}

		protected override ZString GetVoucherDescription()
		{
			if (DataInterfaceUtils.GetLocalLanguage() == Core.Constants.Languages.ChineseSimplified)
			{
				return (NoResString)"预提收入";
			}
			else
			{
				return Res.GetString("f4b47674-1153-44c0-84cf-fc4eef9e7f36", "Estimated Revenue Item");
			}
		}
	}
}

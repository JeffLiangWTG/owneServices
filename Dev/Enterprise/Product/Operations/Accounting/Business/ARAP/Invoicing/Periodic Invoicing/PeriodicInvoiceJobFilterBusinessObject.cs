using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceJobsFilterBusinessObject : PeriodicInvoiceBaseJobFilterBusinessObject
	{
		public override void SetupFilter(PeriodicInvoiceBase parent)
		{
			base.SetupFilter(parent);

			var parentCasted = (PeriodicInvoice)parent;
			((ModuleGuidFilter)this["Debtor"]).Property = parentCasted.DebtorPK;
			invoiceType = parentCasted.InvoiceType;
			((ModuleTextFilter)this["Invoice Type"]).Property = invoiceType;
			((ModuleGuidFilter)this["Tax Branch"]).Property = parentCasted.TaxBranch;
		}
		ZString invoiceType;

		protected override ZQuery GetCurrencyQuery(ZString value)
		{
			var result = new ZQuery();

			if (!InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType))
			{
				result = new ZQuery(JobChargeSchema.JR_RX_NKSellCurrency, value);
				result.AddToFilter(JobChargeSchema.JR_InvoiceType, SQLComparisonOperator.NotEqual, DeferredInvoiceTypesBillingInLocalCurrency);
			}
			else
			{
				result = base.GetCurrencyQuery(value);
			}

			return result;
		}

		public ZQuery ChargeFilterQueryFromBulkInvoice
		{
			get
			{
				return chargeFilterQueryFromBulkInvoice;
			}
			set
			{
				chargeFilterQueryFromBulkInvoice = value;
			}
		}
		ZQuery chargeFilterQueryFromBulkInvoice = new ZQuery();

		protected override ZQuery GetChargeQueryCore()
		{
			var chargeQuery = base.GetChargeQueryCore();
			chargeQuery.AddToFilter(ChargeFilterQueryFromBulkInvoice);

			return chargeQuery;
		}
	}
}

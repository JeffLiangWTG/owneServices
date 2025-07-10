using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsJobComInvoiceHeaderValidation : Enterprise.Customs.Business.AutoJobComInvoiceHeaderValidation
	{
		public WoolworthsJobComInvoiceHeaderValidation(WoolworthsJobComInvoiceHeader parent) : base(parent)
		{
		}

		public new WoolworthsJobComInvoiceHeader Parent
		{
			get { return (WoolworthsJobComInvoiceHeader)base.Parent; }
		}

		#region Validation for when Invoice Line/Orders not consistent

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();

			bool isInconsistent = false;
			foreach (WoolworthsJobComInvoiceLine invoiceLine in Parent.JobComInvoiceLines)
			{
				foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
				{
					if (delivery.OrderLine.Order.OrderCurrency == null || delivery.OrderLine.Order.JD_RX_NKOrderCurrency != Parent.JZ_RX_NKInvoice_Currency)
					{
						isInconsistent = true;
						break;
					}
				}
			}

			if (isInconsistent)
			{
				Parent.JZ_RX_NKInvoice_CurrencyInfo.AddWarning("Invoice Currency not consistent across Invoice Header and Order.");
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();

			bool isInconsistent = false;
			foreach (WoolworthsJobComInvoiceLine invoiceLine in Parent.JobComInvoiceLines)
			{
				foreach (OrderLineDelivery delivery in invoiceLine.OrderLineDeliveries)
				{
					if (delivery.OrderLine.Order.SupplierPK != Parent.JZ_OH_Supplier)
					{
						isInconsistent = true;
						break;
					}
				}
			}

			if (isInconsistent)
			{
				Parent.JZ_OH_SupplierInfo.AddWarning("Supplier not consistent across Invoice Header and Order.");
			}
		}

		#endregion
	}
}

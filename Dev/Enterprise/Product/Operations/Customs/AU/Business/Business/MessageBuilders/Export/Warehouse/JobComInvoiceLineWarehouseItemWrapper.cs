using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceLineWarehouseItemWrapper : WarehouseItemWrapper
	{
		public JobComInvoiceLineWarehouseItemWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		public ZString AHECCCode
		{
			get
			{
				return invoiceLine.JI_Tariff;
			}
		}

		public ZDecimal NetQuantity
		{
			get
			{
				return invoiceLine.JI_CustomsQuantity;
			}
		}

		public ZString NetQuantityUnit
		{
			get
			{
				return invoiceLine.JI_CustomsUnitQty;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				return invoiceLine.JI_Description;
			}
		}

		protected JobComInvoiceLine invoiceLine;
	}
}

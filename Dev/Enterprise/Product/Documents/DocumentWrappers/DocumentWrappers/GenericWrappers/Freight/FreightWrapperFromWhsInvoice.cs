using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	// todo - remove this class.

	public class FreightWrapperFromWhsInvoice : FreightWrapper
	{
		public FreightWrapperFromWhsInvoice(WhsInvoice whsInvoiceBO, BusinessObjectFactory factory)
			: base(whsInvoiceBO, factory)
		{
			WhsInvoiceBO = whsInvoiceBO;
		}
		readonly WhsInvoice WhsInvoiceBO;

		protected override ZDateTime GetBillingDate()
		{
			return WhsInvoiceBO.ET_BillingDate;
		}

		protected override ZDateTime GetStorageFromDate()
		{
			return WhsInvoiceBO.ET_StorageFromDate;
		}

		protected override ZDateTime GetStorageToDate()
		{
			return WhsInvoiceBO.ET_StorageToDate;
		}

		protected override ZString GetJobNumber()
		{
			return WhsInvoiceBO.ET_StorageJobNumber;
		}

		#region Warehouse Info

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WarehousePeriodicInvoiceWrapper(ARInvoice, WrappedBO as WhsInvoice, Factory);
		}

		#endregion
	}
}

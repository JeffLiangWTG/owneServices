using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsInvoice : DocBaseWrapper
	{
		protected DocWhsInvoice(WhsInvoice whsInvoice, BusinessObjectFactory factoryToWrap)
			: base(whsInvoice, factoryToWrap)
		{
		}

		public static DocWhsInvoice New(WhsInvoice whsInvoice, BusinessObjectFactory factoryToWrap)
		{
			return (whsInvoice == null) ? null : new DocWhsInvoice(whsInvoice, factoryToWrap);
		}

		WhsInvoice WhsInvoice
		{
			get { return (WhsInvoice)WrappedObject; }
		}

		public ZString InvoiceNumber
		{
			get { return (WhsInvoice != null ? WhsInvoice.ET_StorageJobNumber : ZString.Empty); }
		}

		public MultilingualString WarehouseNameAndAddress
		{
			get
			{
				if (WhsInvoice.Warehouse != null)
				{
					MultilingualString result = WhsInvoice.Warehouse.WW_WarehouseNameMultilingual;

					if (WhsInvoice.Warehouse.WarehouseAddress != null)
					{
						DocAddress addressWrapper = DocAddress.New(WhsInvoice.Warehouse.WarehouseAddress, Factory);

						result = MultilingualString.Join(System.Environment.NewLine, result, (NoResString)addressWrapper.PostalAddress);
					}

					return result;
				}
				else
				{
					return (NoResString)ZString.Empty;
				}
			}
		}

		public ZDateTime StorageFromDate
		{
			get { return WhsInvoice.ET_StorageFromDate; }
		}

		public ZDateTime StorageToDate
		{
			get { return WhsInvoice.ET_StorageToDate; }
		}
	}
}

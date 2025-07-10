using CargoWise.EntityFramework;
using CargoWise.Types;
using CoreCustoms = Enterprise.Customs.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class NZINVOICMessageBuilder : INVOICMessageBuilder
	{
		public NZINVOICMessageBuilder(JobInvoiceRecord invoiceRecord, BusinessObjectFactory factory)
			: base(invoiceRecord, factory)
		{
		}

		protected override string DutyRate(CoreCustoms.BaseJobComInvoiceLine line)
		{
			return "0.00";
		}

		protected override ZDecimal DutyAmount
		{
			get { return 0; }
		}

		protected override bool IsJobComInvoiceLinkedToShipment(CoreCustoms.BaseJobComInvoiceHeader invoice)
		{
			bool result = base.IsJobComInvoiceLinkedToShipment(invoice);

			if (Shipment.ArrivalConsol != null && Shipment.ArrivalConsol.IsBuyersConsol)
			{
				result = invoice.Bill != null &&
						invoice.Bill.IsHouseBill &&
						invoice.Bill.CU_BillNum == Shipment.JS_HouseBill;
			}

			return result;
		}
	}
}

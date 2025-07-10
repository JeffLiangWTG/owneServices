using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class JobInvoiceRecord
	{
		public JobInvoiceRecord(ForwardingShipment shipment, BaseJobDeclaration declaration, IReadOnlyList<InvoicingBase> invoices)
		{
			fShipment = shipment;
			fDeclaration = declaration;
			fInvoices = invoices;
		}

		public ForwardingShipment Shipment
		{
			get { return fShipment; }
		}

		public BaseJobDeclaration Declaration
		{
			get { return fDeclaration; }
		}

		public IReadOnlyList<InvoicingBase> Invoices
		{
			get { return fInvoices; }
		}

		readonly ForwardingShipment fShipment;
		readonly BaseJobDeclaration fDeclaration;
		readonly IReadOnlyList<InvoicingBase> fInvoices;
	}
}

#region Test
#endregion

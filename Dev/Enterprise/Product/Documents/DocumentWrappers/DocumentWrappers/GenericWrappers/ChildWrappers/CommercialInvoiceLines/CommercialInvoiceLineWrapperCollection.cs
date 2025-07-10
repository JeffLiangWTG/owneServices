using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CommercialInvoiceLineWrapperCollection : GenericWrapperCollection<CommercialInvoiceLineWrapper>
	{
		public CommercialInvoiceLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CommercialInvoiceLineWrapperCollection(CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (shipmentBO != null)
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)shipmentBO.DeclarationForDocuments;
				if (declaration != null)
				{
					AddLinesFrom(declaration.InvoiceLines);
				}
			}
		}

		public CommercialInvoiceLineWrapperCollection(BaseJobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
			: base(factory)
		{
			if (invoiceHeader != null)
			{
				AddLinesFrom(invoiceHeader.JobComInvoiceLines);
			}
		}

		public CommercialInvoiceLineWrapperCollection(BaseJobDeclaration declaration, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declaration != null)
			{
				AddLinesFrom(declaration.InvoiceLines);
			}
		}

		public void RemoveClassifiedInoviceLines()
		{
			foreach (CommercialInvoiceLineWrapper line in this.ToArray())
			{
				if (!line.TariffCode.IsEmpty)
				{
					this.Remove(line);
				}
			}
		}

		void AddLinesFrom(IEnumerable invoiceLines)
		{
			foreach (BaseJobComInvoiceLine invoiceLine in invoiceLines)
			{
				Add(new CommercialInvoiceLineWrapper(invoiceLine, Factory));
			}
			Sort<CommercialInvoiceLineWrapper>(new LineComparer());
		}

		class LineComparer : Comparer<CommercialInvoiceLineWrapper>
		{
			public override int Compare(CommercialInvoiceLineWrapper x, CommercialInvoiceLineWrapper y)
			{
				CommercialInvoiceWrapper xInvoice = x.Invoice;
				CommercialInvoiceWrapper yInvoice = y.Invoice;
				int result = Comparer.DefaultInvariant.Compare(xInvoice.InvoiceNumber, yInvoice.InvoiceNumber);
				if (result == 0)
				{
					result = Comparer.DefaultInvariant.Compare(xInvoice.WrappedObjectPK.ToString(), yInvoice.WrappedObjectPK.ToString());
				}
				if (result == 0)
				{
					result = Comparer.DefaultInvariant.Compare(x.LineNo, y.LineNo);
				}
				return result;
			}
		}
	}
}

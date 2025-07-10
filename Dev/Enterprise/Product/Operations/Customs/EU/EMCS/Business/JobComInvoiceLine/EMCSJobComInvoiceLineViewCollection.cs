using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		public EMCSJobComInvoiceLineViewCollection(EMCSJobComInvoiceHeader invoice, EMCSInvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public new EMCSJobComInvoiceLine this[int index]
		{
			get { return (EMCSJobComInvoiceLine)Elements[index]; }
		}

		public new EMCSJobComInvoiceLine AddNew()
		{
			return (EMCSJobComInvoiceLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(EMCSJobComInvoiceLine);
		}

		public new EMCSInvoiceLineViewCollection CollectionToFilter
		{
			get { return (EMCSInvoiceLineViewCollection)base.CollectionToFilter; }
		}

		protected new EMCSJobComInvoiceHeader InvoiceHeader
		{
			get { return (EMCSJobComInvoiceHeader)base.InvoiceHeader; }
		}

		protected new EMCSJobDeclaration JobDeclaration
		{
			get { return (EMCSJobDeclaration)base.JobDeclaration; }
		}
	}
}

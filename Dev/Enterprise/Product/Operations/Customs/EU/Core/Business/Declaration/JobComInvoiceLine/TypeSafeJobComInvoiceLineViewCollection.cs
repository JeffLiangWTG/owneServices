namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeJobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		protected TypeSafeJobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public TypeSafeJobComInvoiceLineViewCollection(JobComInvoiceHeader parent, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(parent, completeCollection)
		{
		}

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)Elements[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public new InvoiceLineViewCollection<JobComInvoiceLine> CollectionToFilter
		{
			get { return (InvoiceLineViewCollection<JobComInvoiceLine>)base.CollectionToFilter; }
		}

		protected new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		protected new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}
	}
}

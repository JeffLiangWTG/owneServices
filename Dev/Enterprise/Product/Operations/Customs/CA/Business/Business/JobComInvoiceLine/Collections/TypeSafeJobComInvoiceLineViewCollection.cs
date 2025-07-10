
namespace Enterprise.Customs.CA.Business
{
	partial class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)Elements[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		protected new JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)base.InvoiceHeader; }
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}
	}
}

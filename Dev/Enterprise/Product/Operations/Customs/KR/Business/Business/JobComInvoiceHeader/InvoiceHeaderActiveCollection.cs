namespace Enterprise.Customs.KR.Business
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(Bill bill)
			: base(bill)
		{
		}

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		public JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader newElement, Customs.Business.BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
		}
	}
}

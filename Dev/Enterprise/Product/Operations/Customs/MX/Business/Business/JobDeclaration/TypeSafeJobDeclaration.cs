using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business
{
	partial class JobDeclaration : BaseJobDeclaration
	{
		public new JobDeclaration Clone() => (JobDeclaration)base.Clone();

		[ChildEditable(true)]
		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		[ChildEditable(true)]
		public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders => (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.JobDeclarationValidation GetNewValidation() => new JobDeclarationValidation(this);

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection<JobComInvoiceLine>(this);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection() => new ActiveCusEntryHeaderCollection(this);
	}
}

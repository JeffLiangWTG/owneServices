using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[UniversalCopyAddInfo(JobDeclarationSchema.Constants.Prefix, EU.Business.AddInfo.Schema.Prefix)]
	public partial class JobDeclaration
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation
		[ChildEditable(true)]
		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

		[ChildEditable(true)]
		public new ICusEquipmentCollection<CusEquipment> Equipments => (ICusEquipmentCollection<CusEquipment>)base.Equipments;

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

		public new JobDeclarationDocumentSupporter DocumentSupporter => (JobDeclarationDocumentSupporter)base.DocumentSupporter;

		[ChildEditable(true)]
		public new DeclarationLevelPackingGroupCollection PackingGroups => (DeclarationLevelPackingGroupCollection)base.PackingGroups;

		public new JobComInvoiceGroupHeader TopGroupInvoice => (JobComInvoiceGroupHeader)base.TopGroupInvoice;

		#endregion

		#region Overridden 'CreateNew' methods
		JobDeclaration Declaration => this;

		protected override ICusEquipmentCollection<Customs.Business.CusEquipment> GetNewCusEquipmentCollection() => new CusEquipmentCollection<CusEquipment>(Declaration);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(Declaration);

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(Declaration);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(Declaration, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(Declaration);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection<JobComInvoiceLine>(Declaration);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(Declaration, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(Declaration);

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

		protected override BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups() => new DeclarationLevelPackingGroupCollection(this);

		#endregion
	}
}

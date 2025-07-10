using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IL.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
	{
		public new JobComInvoiceGroupHeader Clone() => (JobComInvoiceGroupHeader)base.Clone();

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

		public new InvoiceHeaderActiveCollection JobComInvoiceHeaders => (InvoiceHeaderActiveCollection)base.JobComInvoiceHeaders;

		public new JobComInvoiceGroupHeaderValidation Validation => (JobComInvoiceGroupHeaderValidation)base.Validation;

		public new JobComInvoiceGroupHeaderLookups Lookups => (JobComInvoiceGroupHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<GroupInvoiceCharge> Charges => (JobComInvChargeCollection<GroupInvoiceCharge>)base.Charges;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewDirectInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this, true);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceGroupHeaderValidation(this);

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceGroupHeaderLookups(this);

		protected override IJobComInvChargeCollection<BaseGroupInvoiceCharge> CreateGroupInvoiceChargeCollection() => new JobComInvChargeCollection<GroupInvoiceCharge>(this);
	}
}

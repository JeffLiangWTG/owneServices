using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IL.Business
{
	public partial class JobComInvoiceHeader : AutoILJobComInvoiceHeader
	{
		public new JobComInvoiceHeader Clone() => (JobComInvoiceHeader)base.Clone();

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new JobComInvoiceGroupHeader Master => (JobComInvoiceGroupHeader)base.Master;

		public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

		public new Bill Bill => (Bill)base.Bill;

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceHeaderValidation(this);

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			if (JobDeclaration != null)
			{
				return new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return null;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new JobComInvChargeCollection<InvoiceCharge>(this);

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);
	}
}

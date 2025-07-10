using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.MX.Business
{
	public partial class JobComInvoiceLine : BaseJobComInvoiceLine
	{
		public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => new JobComInvoiceLineValidation(this);

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);
	}
}

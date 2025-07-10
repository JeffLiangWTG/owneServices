using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobComInvoiceLine : AutoCNJobComInvoiceLine
	{
		public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		public new CusEntryInstruction EntryInstruction => base.EntryInstruction as CusEntryInstruction;

		public new JobComInvoiceHeader InvoiceHeader => base.InvoiceHeader as JobComInvoiceHeader;

		public new CusEntryLine CusEntryLine => base.CusEntryLine as CusEntryLine;

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => new JobComInvoiceLineValidation(this);

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);
	}
}

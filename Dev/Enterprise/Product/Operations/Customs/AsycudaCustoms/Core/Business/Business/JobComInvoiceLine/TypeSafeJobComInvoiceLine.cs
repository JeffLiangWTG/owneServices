using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class JobComInvoiceLine : BaseJobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceLine Clone()
		{
			return (JobComInvoiceLine)base.Clone();
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new JobComInvoiceLineLookups Lookups
		{
			get { return (JobComInvoiceLineLookups)base.Lookups; }
		}

		public new JobComInvoiceLineValidation Validation
		{
			get { return (JobComInvoiceLineValidation)base.Validation; }
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges; }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			return new JobComInvoiceLineValidation(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(this);
		}

		#endregion
	}
}

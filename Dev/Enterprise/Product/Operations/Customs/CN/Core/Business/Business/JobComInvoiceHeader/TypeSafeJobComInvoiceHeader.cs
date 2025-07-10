using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobComInvoiceHeader : AutoCNJobComInvoiceHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader Clone()
		{
			return (JobComInvoiceHeader)base.Clone();
		}

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

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			return new JobComInvoiceHeaderValidation(this);
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			if (JobDeclaration != null)
			{
				return new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return null;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<InvoiceCharge>(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);
		}

		#endregion

		#endregion
	}
}

using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public partial class JobComInvoiceHeader : AutoKRJobComInvoiceHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader Clone()
		{
			return (JobComInvoiceHeader)base.Clone();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		//For PID, it returns a validation class which inherits from Customs.Business.JobComInvoiceHeaderValidation
		//public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;
		public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceHeaderValidation result = null;
			if (IsExport)
			{
				result = new EXPJobComInvoiceHeaderValidation(this);
			}
			else if (IsLocalExport)
			{
				result = new LocalExportJobComInvoiceHeaderValidation(this);
			}
			else if (IsImport)
			{
				result = new IMPJobComInvoiceHeaderValidation(this);
			}
			else if (IsD87)
			{
				result = new D87JobComInvoiceHeaderValidation(this);
			}
			else if (IsPersonalItemDeclaration)
			{
				result = new PIDJobComInvoiceHeaderValidation(this);
			}
			else if (Is5SM)
			{
				result = new ValuationDeclarationJobComInvoiceHeaderValidation(this);
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			if (JobDeclaration != null)
			{
				return new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return null;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new JobComInvChargeCollection<InvoiceCharge>(this);

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		#endregion

		#endregion
	}
}

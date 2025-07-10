using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	partial class JobComInvoiceHeader
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

		public new JobComInvoiceHeaderValidation Validation
		{
			get { return (JobComInvoiceHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		public new JobComInvoiceLineViewCollection InvoiceLines
		{
			get { return JobComInvoiceLines; }
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceCharge>)base.Charges; }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new JobComInvoiceGroupHeader Master
		{
			get { return (JobComInvoiceGroupHeader)base.Master; }
		}

		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		public new IEnumerable<CusEntryInstruction> CusEntryInstructions => base.CusEntryInstructions.Cast<CusEntryInstruction>();

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			JobComInvoiceHeaderValidation result;

			if (IsImportLicense)
			{
				result = new ImportLicenseJobComInvoiceHeaderValidation(this);
			}
			else if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderValidation(this);
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

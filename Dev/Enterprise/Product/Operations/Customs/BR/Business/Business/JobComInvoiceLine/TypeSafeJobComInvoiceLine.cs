using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public partial class JobComInvoiceLine : AutoBRJobComInvoiceLine
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceLine Clone() => (JobComInvoiceLine)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			JobComInvoiceLineValidation result;
			if (IsImportSiscomex)
			{
				result = new ImportSiscomexJobComInvoiceLineValidation(this);
			}
			else if (IsImportLicense)
			{
				result = new ImportLicenseJobComInvoiceLineValidation(this);
			}
			else if (IsImport)
			{
				result = new ImportJobComInvoiceLineValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceLineValidation(this);
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}
			return result;
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);

		#endregion
	}
}

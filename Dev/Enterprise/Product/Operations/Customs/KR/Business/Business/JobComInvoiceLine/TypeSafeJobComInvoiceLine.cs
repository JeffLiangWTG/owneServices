using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public partial class JobComInvoiceLine : AutoKRJobComInvoiceLine
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

		public new JobComInvoiceLineLookups Lookups
		{
			get { return (JobComInvoiceLineLookups)base.Lookups; }
		}

		public new JobComInvoiceLineValidation Validation
		{
			get { return (JobComInvoiceLineValidation)base.Validation; }
		}

		public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		public new JobComInvChargeCollection<InvoiceLineCharge> Charges => (JobComInvChargeCollection<InvoiceLineCharge>)base.Charges;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		#endregion

		#region protected override

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			JobComInvoiceLineValidation result = null;
			if (IsExport)
			{
				result = new EXPJobComInvoiceLineValidation(this);
			}
			else if (IsLocalExport)
			{
				result = new LocalExportJobComInvoiceLineValidation(this);
			}
			else if (IsImport)
			{
				result = new IMPJobComInvoiceLineValidation(this);
			}
			else if (IsD87)
			{
				result = new D87JobComInvoiceLineValidation(this);
			}
			else if (IsPersonalItemDeclaration)
			{
				result = new PIDJobComInvoiceLineValidation(this);
			}
			else if (Is5SM)
			{
				result = new ValuationJobComInvoiceLineValidation(this);
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}
			return result;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new JobComInvChargeCollection<InvoiceLineCharge>(this);

		#endregion
	}
}

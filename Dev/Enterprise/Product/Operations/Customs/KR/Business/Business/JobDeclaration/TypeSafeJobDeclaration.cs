using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	partial class JobDeclaration
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration Clone() => (JobDeclaration)base.Clone();

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		//For PID, it returns a validation class which inherits from Customs.Business.JobDeclarationValidation
		//public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		[ChildEditable(true)]
		public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders => (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders;

		public new Bill PrimaryMasterBill => (Bill)base.PrimaryMasterBill;

		public new Bill PrimaryHouseBill => (Bill)base.PrimaryHouseBill;

		[BusinessObjectTestExclude]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
		#endregion

		#region Implementation

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			Customs.Business.JobDeclarationValidation result = null;
			if (IsExport)
			{
				result = new EXPJobDeclarationValidation(this);
			}
			else if (IsLocalExport)
			{
				result = new LEXJobDeclarationValidation(this);
			}
			else if (IsImport)
			{
				result = new IMPJobDeclarationValidation(this);
			}
			else if (IsD87)
			{
				result = new D87JobDeclarationValidation(this);
			}
			else if (IsPersonalItemDeclaration)
			{
				result = new PIDJobDeclarationValidation(this);
			}
			else if (Is5SM)
			{
				result = new ValuationJobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection<JobComInvoiceLine>(this);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection() => new ActiveCusEntryHeaderCollection(this);

		protected override IBillTypeViewCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillTypeCollection()
		{
			return new BillTypeViewCollection<Bill, JobDeclaration>(this);
		}
		#endregion
	}
}


using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class TypeSafeJobDeclaration : BaseJobDeclaration
	{
		protected TypeSafeJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		JobDeclaration JobDeclaration => this as JobDeclaration;

		protected new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		[ChildEditable(true)]
		public new BillCollection Bills => (BillCollection)base.Bills;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		public new InvoiceLineViewCollection FilteredInvoiceLines => (InvoiceLineViewCollection)base.FilteredInvoiceLines;

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(JobDeclaration);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection(JobDeclaration);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(JobDeclaration);
	}
}

using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

		public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

		protected override void SetDefaultForCommonInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			base.SetDefaultForCommonInvoiceLine(invoiceLine);
			if (invoiceLine.IsImport)
			{
				invoiceLine.JI_ZZF_NKTaxType = JobComInvoiceLineLookups.DefaultVatCode;
			}
		}
	}
}

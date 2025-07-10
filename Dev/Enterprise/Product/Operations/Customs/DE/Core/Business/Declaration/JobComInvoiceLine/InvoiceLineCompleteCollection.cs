using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

		public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

		protected override void SetDefaultForFirstInvoiceLine(BaseJobComInvoiceLine firstInvoiceLine)
		{
			base.SetDefaultForFirstInvoiceLine(firstInvoiceLine);
			firstInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			firstInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}

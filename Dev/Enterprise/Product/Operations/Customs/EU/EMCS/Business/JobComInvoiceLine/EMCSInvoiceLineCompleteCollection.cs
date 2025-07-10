using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSInvoiceLineCompleteCollection : InvoiceLineCompleteCollection
	{
		public EMCSInvoiceLineCompleteCollection(EMCSJobDeclaration declaration)
			: base(declaration)
		{
		}

		public new EMCSJobComInvoiceLine this[int index] => (EMCSJobComInvoiceLine)Elements[index];

		public new EMCSJobComInvoiceLine AddNew() => (EMCSJobComInvoiceLine)base.AddNew();

		protected new EMCSJobDeclaration JobDeclaration => (EMCSJobDeclaration)base.JobDeclaration;

		protected override void SetDefaultForFirstInvoiceLine(BaseJobComInvoiceLine firstInvoiceLine)
		{
			if (JobDeclaration.Invoices.Count > 0)
			{
				firstInvoiceLine.JI_JZ = JobDeclaration.Invoices[0].PK;
			}
			firstInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			firstInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

		public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var invLine = (JobComInvoiceLine)child;
			invLine.ZG_MethodOfPayment = JobDeclaration.ApplicationExtender.GetJobComInvoiceLineDefaultMethodOfPayment(JobDeclaration);
			JobDeclaration.ApplicationExtender.SetDefaultsForNewChildOnInvoiceLine(JobDeclaration, invLine);
		}
	}
}

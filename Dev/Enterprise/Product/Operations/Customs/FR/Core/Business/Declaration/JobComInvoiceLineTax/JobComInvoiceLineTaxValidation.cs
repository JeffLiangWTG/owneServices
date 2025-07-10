using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineTaxValidation : EU.Business.Declaration.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(JobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override void CheckJLT_MethodOfPayment()
		{
		}
		protected override void CheckJLT_MethodOfCalculation()
		{
			base.CheckJLT_MethodOfCalculation();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JLT_MethodOfCalculationInfo, Parent.Lookups.MethodOfCalculationList);
		}
	}
}

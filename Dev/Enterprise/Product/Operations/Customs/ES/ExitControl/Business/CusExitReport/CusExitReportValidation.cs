namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitReportValidation : EU.ExitControl.Business.CusExitReportValidation
	{
		public CusExitReportValidation(CusExitReport parent) : base(parent)
		{
		}

		protected override bool SupportValidationWhenCER_TransportTypeNotEmpty => false;
	}
}

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitReportValidationModesCalculator : EU.Business.Declaration.ValidationModesCalculator
	{
		public ExitReportValidationModesCalculator(CusExitReport exitReport)
			: base(exitReport)
		{ }

		protected override EU.Business.Declaration.ValidationModes RecalculateValidationModesCore() => EU.Business.Declaration.ValidationModes.None;

		protected new CusExitReport supporter => (CusExitReport)base.supporter;
	}
}

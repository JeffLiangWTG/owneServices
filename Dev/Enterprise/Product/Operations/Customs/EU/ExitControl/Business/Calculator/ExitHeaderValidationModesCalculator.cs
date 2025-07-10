namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class ExitHeaderValidationModesCalculator : EU.Business.Declaration.ValidationModesCalculator
	{
		public ExitHeaderValidationModesCalculator(CusExitHeader exitHeader)
			: base(exitHeader)
		{ }

		protected override EU.Business.Declaration.ValidationModes RecalculateValidationModesCore()
		{
			var result = EU.Business.Declaration.ValidationModes.None;
			foreach (var exitReport in supporter.CusExitReports)
			{
				result |= exitReport.ValidationModes;
			}
			return result;
		}

		protected new CusExitHeader supporter => (CusExitHeader)base.supporter;
	}
}

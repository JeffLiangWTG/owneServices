using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ExitNotificationCusExitReportValidation : CusExitReportValidation
	{
		public ExitNotificationCusExitReportValidation(CusExitReport parent)
			: base(parent)
		{
		}

		protected override void CheckCER_AdditionalDeclarationType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CER_AdditionalDeclarationTypeInfo);
		}
	}
}

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentItemValidation : EU.ExitControl.Business.CusExitConsignmentItemValidation
	{
		public CusExitConsignmentItemValidation(CusExitConsignmentItem parent) : base(parent)
		{
		}

		protected override void CheckCCI_GrossMass()
		{
			base.CheckCCI_GrossMass();
			var targetInfo = Parent.CCI_GrossMassInfo;

			CommonValidation.CheckGreaterThanZero(targetInfo);
			CommonValidation.CheckValueShouldGreaterThanOrEqualTo(targetInfo, Parent.CCI_NetMassInfo, targetInfo);
		}

		protected override void CheckCCI_NetMass()
		{
			base.CheckCCI_NetMass();
			var targetInfo = Parent.CCI_NetMassInfo;

			CommonValidation.CheckGreaterThanZero(targetInfo);
			CommonValidation.CheckValueShouldGreaterThanOrEqualTo(Parent.CCI_GrossMassInfo, targetInfo, targetInfo);
		}
	}
}

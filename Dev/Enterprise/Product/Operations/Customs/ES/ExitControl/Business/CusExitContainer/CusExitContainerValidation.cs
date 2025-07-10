namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitContainerValidation : EU.ExitControl.Business.CusExitContainerValidation
	{
		public CusExitContainerValidation(CusExitContainer parent) : base(parent)
		{
		}

		protected override void CheckCXN_Status()
		{
			base.CheckCXN_Status();

			var status = Parent.CXN_Status;
			var mandatoryLength = ExitControlBase.Business.AutoCusExitContainer.Schema.CXN_StatusMaxLength;
			var statusInfo = Parent.CXN_StatusInfo;
			if (!status.IsEmpty && status.Length != mandatoryLength)
			{
				statusInfo.AddError(Res.GetString("793C2CD6-9183-4A10-A9B5-FBE832582E10", "The length must be 0 or {0}.", mandatoryLength));
			}
		}
	}
}

namespace Enterprise.Customs.ES.ExitControl.Business
{
	sealed class CusExitConsignmentPackageValidation : EU.ExitControl.Business.CusExitConsignmentPackageValidation
	{
		public CusExitConsignmentPackageValidation(CusExitConsignmentPackage parent) : base(parent)
		{
		}

		protected override void CheckCXP_MarksAndNumbersStatus()
		{
			base.CheckCXP_MarksAndNumbersStatus();

			var status = Parent.CXP_MarksAndNumbersStatus;
			var mandatoryLength = CusExitConsignmentPackage.Schema.CXP_MarksAndNumbersStatusMaxLength;
			var statusInfo = Parent.CXP_MarksAndNumbersStatusInfo;
			if (!status.IsEmpty && status.Length != mandatoryLength)
			{
				statusInfo.AddError(Res.GetString("9B44AB04-9D73-42E0-8579-34B85230ECA4", "The length must be 0 or {0}.", mandatoryLength));
			}
		}
	}
}

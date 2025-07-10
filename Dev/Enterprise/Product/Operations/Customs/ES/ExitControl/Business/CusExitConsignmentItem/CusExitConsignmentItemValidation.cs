namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class CusExitConsignmentItemValidation : EU.ExitControl.Business.CusExitConsignmentItemValidation
	{
		public CusExitConsignmentItemValidation(CusExitConsignmentItem parent) : base(parent)
		{
		}

		protected new CusExitConsignmentItem Parent => (CusExitConsignmentItem)base.Parent;

		protected override void CheckCCI_UniqueConsignmentReferenceStatus()
		{
			base.CheckCCI_UniqueConsignmentReferenceStatus();

			var status = Parent.CCI_UniqueConsignmentReferenceStatus;
			var mandatoryLength = CusExitConsignmentItem.Schema.CCI_UniqueConsignmentReferenceStatusMaxLength;
			var statusInfo = Parent.CCI_UniqueConsignmentReferenceStatusInfo;
			if (!status.IsEmpty && status.Length != mandatoryLength)
			{
				statusInfo.AddError(Res.GetString("654EBC90-2E98-48A5-A36E-37362ADFD3DE", "The length must be 0 or {0}.", mandatoryLength));
			}
		}
	}
}

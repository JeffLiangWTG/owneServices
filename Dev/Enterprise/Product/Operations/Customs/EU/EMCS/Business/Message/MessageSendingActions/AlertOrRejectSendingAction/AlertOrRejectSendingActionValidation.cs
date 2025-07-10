using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class AlertOrRejectSendingActionValidation : ZValidation
	{
		public AlertOrRejectSendingActionValidation(AlertOrRejectSendingAction parent) : base(parent)
		{
		}

		protected AlertOrRejectSendingAction Parent => (AlertOrRejectSendingAction)ParentFilter;

		public override Type AutoValidationType => typeof(AlertOrRejectSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateRejectedFlag();
			ValidateDateOfAlertOrRejection();
		}

		public void ValidateRejectedFlag()
		{
			ValidateCalculatedProperty(Parent.RejectedFlagInfo);
		}

		public void ValidateDateOfAlertOrRejection()
		{
			ValidateCalculatedProperty(Parent.DateOfAlertOrRejectionInfo);
		}

		protected void CheckDateOfAlertOrRejection()
		{
			MandatoryValidation.CheckEntered(Parent.DateOfAlertOrRejectionInfo);
		}
	}
}

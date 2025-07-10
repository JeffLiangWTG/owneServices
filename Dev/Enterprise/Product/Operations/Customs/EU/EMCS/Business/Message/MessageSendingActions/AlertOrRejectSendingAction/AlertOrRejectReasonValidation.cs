using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class AlertOrRejectReasonValidation : ZValidation
	{
		public AlertOrRejectReasonValidation(AlertOrRejectReason parent)
			: base(parent)
		{
		}

		protected AlertOrRejectReason Parent => (AlertOrRejectReason)ParentFilter;

		public override Type AutoValidationType => typeof(AlertOrRejectReasonValidation);

		public override void ValidateAll()
		{
			ValidateReason();
			ValidateInformation();
		}

		public void ValidateReason()
		{
			ValidateCalculatedProperty(Parent.ReasonInfo);
		}

		protected void CheckReason()
		{
			MandatoryValidation.CheckEntered(Parent.ReasonInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ReasonInfo);
		}

		public void ValidateInformation()
		{
			ValidateCalculatedProperty(Parent.InformationInfo);
		}

		protected void CheckInformation()
		{
			if (Parent.Reason == EMCSAlertRejectionCodeList.Codes._0)
			{
				MandatoryValidation.CheckEntered(Parent.InformationInfo);
			}
		}
	}
}

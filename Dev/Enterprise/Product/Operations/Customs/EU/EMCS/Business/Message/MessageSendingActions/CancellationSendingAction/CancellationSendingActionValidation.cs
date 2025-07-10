using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class CancellationSendingActionValidation : ZValidation
	{
		public CancellationSendingActionValidation(CancellationSendingAction parent) : base(parent)
		{
		}

		CancellationSendingAction Parent => (CancellationSendingAction)ParentFilter;

		public override Type AutoValidationType => typeof(CancellationSendingActionValidation);

		#region ValidateReason
		public void ValidateReason()
		{
			ValidateCalculatedProperty(Parent.ReasonInfo);
		}

		protected void CheckReason()
		{
			var info = Parent.ReasonInfo;
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}
		#endregion

		#region ValidateInformation
		public void ValidateInformation()
		{
			ValidateCalculatedProperty(Parent.InformationInfo);
		}

		protected void CheckInformation()
		{
			if (Parent.Reason == EMCSCancellationReasonList.Codes._0)
			{
				MandatoryValidation.CheckEntered(Parent.InformationInfo);
			}
		}
		#endregion

		public override void ValidateAll()
		{
			ValidateReason();
			ValidateInformation();
		}
	}
}

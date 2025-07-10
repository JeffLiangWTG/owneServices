using System.Linq;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BlanketVesselChangeBillValidation : AutoBlanketVesselChangeBillValidation
	{
		public BlanketVesselChangeBillValidation(AutoBlanketVesselChangeBill parent) : base(parent)
		{
		}

		protected override void CheckJPM_Send()
		{
			base.CheckJPM_Send();
			var blanketVesselChange = Parent.BlanketVesselChange;
			if (!blanketVesselChange.JPM_BlanketChange)
			{
				var messageSendingObjects = blanketVesselChange.BlanketVesselChangeBills.Cast<BlanketVesselChangeBill>().ToArray();
				if (messageSendingObjects.All(x => !x.JPM_Send))
				{
					Parent.JPM_SendInfo.AddMessageError(ValidationConstants.MessageSending.NoBillsHaveBeenChecked);
				}
				else if (!Parent.JPM_Send)
				{
					Parent.JPM_SendInfo.AddMessageError(ValidationConstants.MessageSending.AnyBillIsLeftUnchecked);
				}
			}
		}

		protected new BlanketVesselChangeBill Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BlanketVesselChangeBill)base.Parent; }
		}
	}
}

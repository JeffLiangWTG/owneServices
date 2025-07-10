using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class GlobalTrackingShipmentVisibilityServiceEhubIDValidation : ZValidation
	{
		public GlobalTrackingShipmentVisibilityServiceEhubIDValidation(GlobalTrackingShipmentVisibilityServiceEhubID parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly GlobalTrackingShipmentVisibilityServiceEhubID parent;

		public override Type AutoValidationType => typeof(GlobalTrackingShipmentVisibilityServiceEhubIDValidation);

		public void ValidateEhubID()
		{
			ValidateCalculatedProperty(parent.EhubIDInfo);
		}

		protected void CheckEhubID()
		{
			if (parent.EhubID.IsEmpty)
			{
				parent.EhubIDInfo.AddError(Res.GetString("203fcbfc-71bb-49d8-9519-d590f87b5d24", "Please enter a eHub ID."));
			}
		}

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)parent).SuspendListChanged())
			{
				ValidateEhubID();
			}
		}
	}
}

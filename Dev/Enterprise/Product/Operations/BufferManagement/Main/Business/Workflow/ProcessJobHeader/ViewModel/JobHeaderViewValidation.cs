using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class JobHeaderViewValidation : ZValidation
	{
		public JobHeaderViewValidation(JobHeaderView parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly JobHeaderView parent;

		public override Type AutoValidationType => typeof(JobHeaderViewValidation);

		public override void ValidateAll()
		{
			ValidateEarliestStartDateLocal();
			ValidateAgreedDeliveryDateLocal();
		}

		public void ValidateEarliestStartDateLocal()
		{
			ValidateCalculatedProperty(parent.EarliestStartDateLocalInfo);
		}

		protected void CheckEarliestStartDateLocal()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(parent.EarliestStartDateLocalInfo);
			TypeValidation.CheckValidSmallDateTime(parent.EarliestStartDateLocalInfo);
		}

		public void ValidateAgreedDeliveryDateLocal()
		{
			ValidateCalculatedProperty(parent.AgreedDeliveryDateLocalInfo);
		}

		protected void CheckAgreedDeliveryDateLocal()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(parent.AgreedDeliveryDateLocalInfo);
			TypeValidation.CheckValidSmallDateTime(parent.AgreedDeliveryDateLocalInfo);
		}
	}
}

using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class MultiJobHeaderEditorViewModelValidation : ZValidation
	{
		public MultiJobHeaderEditorViewModelValidation(MultiJobHeaderEditorViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly MultiJobHeaderEditorViewModel parent;

		public override Type AutoValidationType => typeof(MultiJobHeaderEditorViewModelValidation);

		public override void ValidateAll()
		{
			ValidateEarliestStartDateLocal();
			ValidateAgreedDeliveryDateLocal();
			ValidateEarliestStartDateOffset();
			ValidateAgreedDeliveryDateOffset();
			ValidateEarliestStartDateDays();
			ValidateAgreedDeliveryDateDays();
			ValidateFH_DateAcceptability();
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

		public void ValidateEarliestStartDateOffset()
		{
			ValidateCalculatedProperty(parent.EarliestStartDateOffsetInfo);
		}

		protected void CheckEarliestStartDateOffset()
		{
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(parent.EarliestStartDateOffsetInfo);
		}

		public void ValidateAgreedDeliveryDateOffset()
		{
			ValidateCalculatedProperty(parent.AgreedDeliveryDateOffsetInfo);
		}

		protected void CheckAgreedDeliveryDateOffset()
		{
			TypeValidation.CheckValidZDateTimeOffsetWithoutRange(parent.AgreedDeliveryDateOffsetInfo);
		}

		public void ValidateEarliestStartDateDays()
		{
			ValidateCalculatedProperty(parent.EarliestStartDateDaysInfo);
		}

		protected void CheckEarliestStartDateDays()
		{
			CompareValidation.CheckLessThanOrEqualTo(parent.EarliestStartDateDaysInfo, MaxDaysValue);
			CompareValidation.CheckGreaterThanOrEqualTo(parent.EarliestStartDateDaysInfo, -MaxDaysValue);
		}

		public void ValidateAgreedDeliveryDateDays()
		{
			ValidateCalculatedProperty(parent.AgreedDeliveryDateDaysInfo);
		}

		protected void CheckAgreedDeliveryDateDays()
		{
			CompareValidation.CheckLessThanOrEqualTo(parent.AgreedDeliveryDateDaysInfo, MaxDaysValue);
			CompareValidation.CheckGreaterThanOrEqualTo(parent.AgreedDeliveryDateDaysInfo, -MaxDaysValue);
		}

		const decimal MaxDaysValue = 9999;

		public void ValidateFH_DateAcceptability()
		{
			ValidateCalculatedProperty(parent.FH_DateAcceptabilityInfo);
		}

		protected void CheckFH_DateAcceptability()
		{
			ListValidation.ErrorIfInvalidCode(parent.FH_DateAcceptabilityInfo);
		}
	}
}
